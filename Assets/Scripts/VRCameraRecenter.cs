using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.InputSystem;
using System.Linq;  // For FirstOrDefault()

public class VRCameraRecenter : MonoBehaviour
{
    [Header("Transforms")]
    public Transform head;
    public Transform origin;

    [Header("Input Action")]
    public InputActionProperty recenterAction;

    public void Recenter()
    {
        // 1. Get the current head rotation on the Y-axis (yaw)
        float currentYaw = head.eulerAngles.y;

        // 2. Calculate how much we need to rotate the origin to make the head face '0'
        // If you want to face a specific 'target' direction, replace 0 with target.eulerAngles.y
        float rotationAngleY = -currentYaw;

        // 3. Rotate the origin around the head's position so the view snaps forward
        origin.RotateAround(head.position, Vector3.up, rotationAngleY);
    }

    void Update()
    {
        if (recenterAction.action != null && recenterAction.action.WasPerformedThisFrame())
        {
            Recenter();
        }
    }

}
