using UnityEngine;

public class RayGrabManager : MonoBehaviour
{
    public static RayGrabManager Instance;

    public KinematicInteractable activeObject;
    public RayGrab primaryHand;
    public RayGrab secondaryHand;

    Vector3 initialHandDir;
    Vector3 initialObjectPosition;
    Quaternion initialObjRot;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterHand(RayGrab hand, KinematicInteractable obj)
    {
        if (activeObject == null)
        {
            activeObject = obj;
            primaryHand = hand;
            obj.BeginManipulation();
        }
        else if (activeObject == obj && secondaryHand == null && hand != primaryHand)
        {
            secondaryHand = hand;

            initialHandDir = (secondaryHand.HandPos - primaryHand.HandPos).normalized;
            initialObjRot = activeObject.transform.rotation;
            initialObjectPosition = activeObject.transform.position;
        }
    }

    public void ReleaseHand(RayGrab hand)
    {
        if (hand == primaryHand)
        {
            if (activeObject != null)
                activeObject.EndManipulation();

            activeObject = null;
            primaryHand = null;
            secondaryHand = null;
        }
        else if (hand == secondaryHand)
        {
            secondaryHand = null;
        }
    }

    void Update()
    {
        if (activeObject == null) return;

        if (secondaryHand == null)
        {
            // One hand mode
            primaryHand.UpdateSingleHand(activeObject);
        }
        else
        {
            // Two hand mode
            UpdateTwoHand();
        }
    }

    void UpdateTwoHand()
    {
        Vector3 posA = primaryHand.HandPos;
        Vector3 posB = secondaryHand.HandPos;

        Vector3 currentDir = (posB - posA).normalized;
        
        Quaternion rotDelta = Quaternion.FromToRotation(initialHandDir, currentDir);
        Quaternion targetRot = rotDelta * initialObjRot;

        activeObject.SetTargetPose(initialObjectPosition, targetRot);
    }
}

