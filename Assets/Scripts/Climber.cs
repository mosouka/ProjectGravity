using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Gravity;

public class Climber : MonoBehaviour
{
    public CharacterController characterController;
    public ContinuousMoveProvider continuousMovement;
    public GravityProvider gravityProvider;
    public static TrackedPoseDriver climbingHand;

    public float climbSpeed = 1.0f;

    private TrackedPoseDriver previousHand;
    private Vector3 previousPos;
    private Vector3 currentVelocity;

    
   void FixedUpdate()
    {
        if (climbingHand != null)
        {
            if (previousHand == null || climbingHand != previousHand)
            {
                previousHand = climbingHand;
                previousPos = climbingHand.positionInput.action.ReadValue<Vector3>();
            }
            continuousMovement.enabled = false;
            gravityProvider.enabled = false;
            Climb();
        }
        else
        {
            continuousMovement.enabled = true;
            gravityProvider.enabled = true;
            previousHand = null;
        }
    }

    // Climbing computation
    private void Climb()
    {
        Vector3 currentPos = climbingHand.positionInput.action.ReadValue<Vector3>();
        currentVelocity = (currentPos - previousPos) / Time.fixedDeltaTime;
        characterController.Move(-currentVelocity * climbSpeed * Time.fixedDeltaTime);
        previousPos = currentPos;
    }
}

