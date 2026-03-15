using UnityEngine;
using UnityEngine.InputSystem;

public class RayGrab : MonoBehaviour
{
     [Header("References")]
    public Transform rayOrigin;
    public LineRenderer rayLine;
    public RaySpawn raySpawn;

    [Header("Input")]
    public InputActionProperty gripAction;

    [Header("Ray Settings")]
    public float maxDistance = 10f;
    public LayerMask interactableMask;

    [Header("Audio")]
    public AudioSource raySound;  // looping hum while ray active

    Vector3 grabOffsetPos;
    Quaternion grabOffsetRot;

    public Vector3 HandPos => rayOrigin.position;

    KinematicInteractable grabbed;

    void OnEnable()
    {
        gripAction.action?.Enable();
    }

    void OnDisable()
    {
        gripAction.action?.Disable();
    }

    void Update()
    {
        bool gripHeld = gripAction.action != null && gripAction.action.ReadValue<float>() > 0.5f;

        if(raySpawn != null)
            raySpawn.interactionActive = gripHeld;

        DrawRay(gripHeld);

        UpdateRaySound(gripHeld);

        if (gripHeld && grabbed == null && gripAction.action.WasPressedThisFrame())
        {
            Debug.Log("Trying to grab...");
            TryGrab();
        }
            

        if (!gripHeld && grabbed != null)
            Release();
    }

    void DrawRay(bool visible)
    {
        if (rayLine == null) return;

        rayLine.enabled = visible;

        if (!visible) 
        {
            return;
        }

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        float interactionMaxDistance = maxDistance;

        if (raySpawn != null)
        {
            // keep interaction ray slightly shorter than spawn preview
            interactionMaxDistance = Mathf.Min(maxDistance, raySpawn.CurrentDistance - 0.1f);
        }

        Vector3 end = rayOrigin.position + rayOrigin.forward * interactionMaxDistance;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionMaxDistance))
            end = hit.point;

        rayLine.SetPosition(0, rayOrigin.position);
        rayLine.SetPosition(1, end);
    }

    void TryGrab()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableMask))
            return;

        KinematicInteractable obj = hit.collider.GetComponentInParent<KinematicInteractable>();
        if (obj == null) return;

        Debug.Log("Grabbed: " + obj.name);

        grabbed = obj;

        grabOffsetPos = Quaternion.Inverse(rayOrigin.rotation) * (obj.transform.position - rayOrigin.position);
        grabOffsetRot = Quaternion.Inverse(rayOrigin.rotation) * obj.transform.rotation;

        RayGrabManager.Instance.RegisterHand(this, obj);
    }

    public void UpdateSingleHand(KinematicInteractable obj)
    {
        Vector3 targetPos = rayOrigin.position + rayOrigin.rotation * grabOffsetPos;
        Quaternion targetRot = rayOrigin.rotation * grabOffsetRot;

        obj.SetTargetPose(targetPos, targetRot);
    }

    void Release()
    {
        RayGrabManager.Instance.ReleaseHand(this);
        grabbed = null;
    }

    void UpdateRaySound(bool rayActive)
    {
        if (raySound == null) return;

        if (rayActive && !raySound.isPlaying)
        {
            raySound.Play();  // start hum when ray activates
        }
        else if (!rayActive && raySound.isPlaying)
        {
            raySound.Stop();  // stop hum when ray deactivates
        }
    }
}
