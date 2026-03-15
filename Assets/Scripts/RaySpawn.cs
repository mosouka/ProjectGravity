using UnityEngine;
using UnityEngine.InputSystem;

public class RaySpawn : MonoBehaviour
{
    [Header("References")]
    public Transform rayOrigin;
    public GameObject gravityFieldPrefab;
    public GameObject previewSphere;
    public LayerMask placementMask = ~0;

    [Space]
    public GameObject activeField;  // Tracks the spawned sphere (null = none)

    [Header("Ray Settings")]
    public float minDistance = 0.5f;
    public float maxDistance = 10f;
    public float distanceAdjustSpeed = 1f;
    private Vector3 placedPosition;  // Add this field to store spawn pos

    [Header("Sphere Size Control")]
    public float minRadius = 0.5f;
    public float maxRadius = 3f;
    public float radiusAdjustSpeed = 1f;
    private float placedRadius;      // Add this to store spawn radius

    private float currentRadius = 1f;

    [Header("Shoot Animation")]
    public Transform startOffset; // Start slightly in front of the gun
    public float shootDuration = 0.3f;     // time to travel from gun to target
    public AnimationCurve shootCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float spawnStartScaleFactor = 0.05f;  // start at 5% of final radius
    public AudioSource shootSound; // Add this to play the sound



    [Header("Input")]
    public InputActionProperty triggerAction;
    public InputActionProperty moveAction;

    [Space]
    public bool interactionActive = false;

    private float currentDistance;

    public float CurrentDistance => currentDistance;

    private void OnEnable()
    {
        if (triggerAction.action != null) triggerAction.action.Enable();
        if (moveAction.action != null) moveAction.action.Enable();

        currentDistance = (minDistance + maxDistance) / 2f;
    }

    private void OnDisable()
    {
        if (triggerAction.action != null) triggerAction.action.Disable();
        if (moveAction.action != null) moveAction.action.Disable();
    }

    private void Update()
    {
        // get trigger value
        if (triggerAction.action != null && triggerAction.action.WasPressedThisFrame())
        {
            if (activeField == null)
            {
                PlaceField();
            }
                
            else
                DespawnField();
        }

        if (activeField == null)
        {
            UpdateDistanceFromStick();
            UpdateRadiusFromStick();
            UpdatePreview();
        }
    }

    // ADD THIS NEW METHOD
    private void UpdatePlacedField()
    {
        if (previewSphere == null || rayOrigin == null || activeField == null) return;

        // Recompute position along current ray + distance (with surface snap)
        Vector3 basePosition = rayOrigin.position + rayOrigin.forward * currentDistance;
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, placementMask))
        {
            if (hit.distance < currentDistance)
                basePosition = hit.point;
        }

        activeField.transform.position = basePosition;
        activeField.transform.localScale = Vector3.one * currentRadius * 2f;  // Assuming sphere scales this way

        // Update the field script too
        GravitationalField field = activeField.GetComponent<GravitationalField>();
        if (field != null)
            field.SetRadius(currentRadius);
    }

    private void UpdateDistanceFromStick()
    {
        if (moveAction.action == null) return;

        Vector2 stickInput = moveAction.action.ReadValue<Vector2>();
        currentDistance += stickInput.y * distanceAdjustSpeed * Time.deltaTime;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }

    private void UpdateRadiusFromStick()
    {
        if (moveAction.action == null) return;

        Vector2 stickInput = moveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
        
        // Use X axis for radius (Y already used for distance)
        float radiusDelta = stickInput.x * radiusAdjustSpeed * Time.deltaTime;
        currentRadius = Mathf.Clamp(currentRadius + radiusDelta, minRadius, maxRadius);
    }

    private void UpdatePreview()
    {
        if (previewSphere == null || rayOrigin == null) return;

        Vector3 basePosition = rayOrigin.position + rayOrigin.forward * currentDistance;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, placementMask))
        {
            if (hit.distance < currentDistance)
                basePosition = hit.point;
        }
            
        previewSphere.SetActive(true);
        previewSphere.transform.position = basePosition;

        previewSphere.transform.localScale = Vector3.one * currentRadius * 2f;

        // fade when interacting
        Renderer r = previewSphere.GetComponent<Renderer>();
        if (r != null)
        {
            Color c = r.material.color;
            c.a = interactionActive ? 0.0f : 0.5f;
            r.material.color = c;
        }
    }


    private void PlaceField()
    {
        if (gravityFieldPrefab == null || previewSphere == null || rayOrigin == null) return;

        shootSound.Play(); // Play the shooting sound when placing the field

        // Target data (where we WANT the sphere to end up)
        Vector3 targetPos = previewSphere.transform.position;
        float finalRadius = currentRadius;

        // Spawn at gun tip, tiny
        GameObject field = Instantiate(gravityFieldPrefab, rayOrigin.position, Quaternion.identity);
        GravitationalField gField = field.GetComponent<GravitationalField>();
        if (gField != null)
        {
            gField.SetRadius(minRadius); // optional: start with small radius for physics
        }

        // start small visually
        float startRadius = finalRadius * spawnStartScaleFactor;
        field.transform.localScale = Vector3.one * (startRadius * 2f);

        activeField = field;
        placedPosition = targetPos;
        placedRadius = finalRadius;

        // hide preview while the shot animates
        previewSphere.SetActive(false);

        // animate to target
        StartCoroutine(ShootFieldCoroutine(field, targetPos, startRadius, finalRadius));
    }

    private System.Collections.IEnumerator ShootFieldCoroutine(GameObject field, Vector3 targetPos, float startRadius, float finalRadius)
    {
        float elapsed = 0f;

        while (elapsed < shootDuration && field != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shootDuration);
            float curvedT = shootCurve != null ? shootCurve.Evaluate(t) : t;

            // position along ray
            field.transform.position = Vector3.Lerp(startOffset.position, targetPos, curvedT);

            // scale from small to final
            float radius = Mathf.Lerp(startRadius, finalRadius, curvedT);
            field.transform.localScale = Vector3.one * (radius * 2f);

            yield return null;
        }

        if (field != null)
        {
            field.transform.position = targetPos;
            field.transform.localScale = Vector3.one * (finalRadius * 2f);

            // ensure the GravitationalField is using the final radius for physics
            var gField = field.GetComponent<GravitationalField>();
            if (gField != null)
            {
                gField.SetRadius(finalRadius);
                gField.canDespawnOnPlayer = true; // Allow despawn on player collision after fully spawned
            }
                
        }
    }



    public void DespawnField()
    {
        if (activeField != null)
        {
            Destroy(activeField);
            activeField = null;
        }
    }
}
