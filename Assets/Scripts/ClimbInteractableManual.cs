using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem.XR;


public class ClimbInteractableManual : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
{
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);

            var interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;
            if (interactor != null)
            {
                var climbingHand = interactor.GetComponent<TrackedPoseDriver>();
                if (climbingHand != null)
                {
                    Climber.climbingHand = climbingHand;
                }
            }
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);
            
           var interactor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;

            if (interactor != null)
            {
                var climbingHand = interactor.GetComponent<TrackedPoseDriver>();
                if (climbingHand != null && Climber.climbingHand == climbingHand)
                {
                    Climber.climbingHand = null;
                }
            }
        }
}

