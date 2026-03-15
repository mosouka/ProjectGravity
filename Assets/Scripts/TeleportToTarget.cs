using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.InputSystem;
using System.Linq;  // For FirstOrDefault()

public class TeleportToTarget : MonoBehaviour
{
    [Header("Transforms")]
    public Transform head;
    public Transform origin;
    public Transform target;

    [Header("Input Action")]
    public InputActionProperty recenterAction;

    public void Recenter()
    {
        Vector3 offset = head.position - origin.position;
        offset.y = 0; // Ignore vertical offset
        origin.position = target.position - offset;

        Vector3 targetForward = target.forward;
        targetForward.y = 0;
        Vector3 cameraForward = head.forward;
        cameraForward.y = 0;

        float angle = Vector3.SignedAngle(cameraForward, targetForward, Vector3.up);

        origin.RotateAround(head.position, Vector3.up, angle);
    }

    void Update()
    {
        if (recenterAction.action != null && recenterAction.action.WasPerformedThisFrame())
        {
            Recenter();
        }
    }

}
