using UnityEngine;
using UnityEngine.InputSystem;

public class GravityFieldSpawner : MonoBehaviour
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

    // [Header("Task Auto-Sphere")]
    // public bool isTaskController = false;  // ✓ TRUE on RIGHT HAND SPAWNER ONLY
    // public GameObject taskSpherePrefab;    // Same as gravityFieldPrefab
    // public GameObject currentTaskSphere;   // To track the spawned task sphere (if any)
    // public float taskTriggerRadius = 1.5f;
    // private bool taskSphereActive = false;
    // private bool taskSphereSpawned = false;  // To prevent the sphere from spawning too soon

    private float currentRadius = 1f;

    [Header("Input")]
    public InputActionProperty triggerAction;
    public InputActionProperty moveAction;

    private bool previewActive = false;

    private float currentDistance;

    private void OnEnable()
    {
        if (triggerAction.action != null) triggerAction.action.Enable();
        if (moveAction.action != null) moveAction.action.Enable();

        currentDistance = (minDistance + maxDistance) / 2f;

        currentRadius = minRadius;
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

        // spawn task sphere if close to target
        // if (isTaskController && !taskSphereSpawned)
        // {
        //     GameObject objectT = GameObject.FindWithTag("objectT");
        //     GameObject targetT = GameObject.FindWithTag("targetT");

        //     if (objectT != null && targetT != null)
        //     {
        //         float distToTarget = Vector3.Distance(objectT.transform.position, targetT.transform.position);
        //         if (distToTarget <= taskTriggerRadius)
        //         {
        //             //taskSphereSpawned = true;  // Prevent multiple spawns
        //             SpawnTaskSphereAtTargetT(targetT.transform.position);
        //         }
        //     }
        // }
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
    }

    // called from Update when we want to auto-spawn
    // private void SpawnTaskSphereAtTargetT(Vector3 targetPos)
    // {
    //     // guard: if already spawned, do nothing
    //     if (taskSphereSpawned) return;

    //     taskSphereSpawned = true;        // mark as spawned
    //     taskSphereActive  = true;        // optional, if you use this elsewhere

    //     currentTaskSphere = Instantiate(taskSpherePrefab, targetPos, Quaternion.identity);

    //     GravitationalField field = currentTaskSphere.GetComponent<GravitationalField>();
    //     if (field != null)
    //     {
    //         field.taskSphereActive = true;
    //         field.SetRadius(3f);
    //         field.isTaskActive = false;
    //         field.SetOwner(this);
    //     }
    // }


    private void PlaceField()
    {
        if (gravityFieldPrefab == null || previewSphere == null) return;

        // GameObject field = Instantiate(gravityFieldPrefab, previewSphere.transform.position, Quaternion.identity);
        // field.GetComponent<GravitationalField>().SetRadius(currentRadius);

        // activeField = field;
        // previewSphere.SetActive(false);
        // Compute final position (same as preview)
        Vector3 spawnPos = previewSphere.transform.position;
        GameObject field = Instantiate(gravityFieldPrefab, spawnPos, Quaternion.identity);
        GravitationalField gField = field.GetComponent<GravitationalField>();
        if (gField != null)
            gField.SetRadius(currentRadius);

        activeField = field;
        placedPosition = spawnPos;  // Store
        placedRadius = currentRadius;
        previewSphere.SetActive(false);
    }

    public void DespawnField()
    {
        if (activeField != null)
        {
            Destroy(activeField);
            activeField = null;
            previewActive = false;
        }
    }

    // public void DespawnTaskSphere()
    // {
    //     if (currentTaskSphere != null)
    //     {
    //         Destroy(currentTaskSphere);
    //         currentTaskSphere = null;
    //         //taskSphereActive = false;
    //         // Gravity will naturally restore via the GravitationalField's OnDestroy
    //     }
    //     taskSphereActive = false;
    // }

    // // Called only once per completed task (e.g. after SelectionTaskMeasure.EndOneTask)
    // public void ResetTaskSphereSpawn()
    // {
    //     taskSphereSpawned = false;
    // }
}
