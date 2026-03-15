using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KinematicInteractable : MonoBehaviour
{
    public Rigidbody rb { get; private set; }

    public float positionLerp = 25f;
    public float rotationLerp = 25f;

    private Vector3 targetPos;
    private Quaternion targetRot;
    private bool isActive;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    public void BeginManipulation()
    {
        isActive = true;
    }

    public void EndManipulation()
    {
        isActive = false;
    }

    public void SetTargetPose(Vector3 pos, Quaternion rot)
    {
        targetPos = pos;
        targetRot = rot;
    }

    void FixedUpdate()
    {
        if (!isActive) return;

        float posT = 1f - Mathf.Exp(-positionLerp * Time.fixedDeltaTime);
        float rotT = 1f - Mathf.Exp(-rotationLerp * Time.fixedDeltaTime);

        rb.MovePosition(Vector3.Lerp(rb.position, targetPos, posT));
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotT));
    }
}