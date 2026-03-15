using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MyGrabXRI : MonoBehaviour
{
    // Modiefied from MyGrab.cs to use XR Interaction Toolkit and collision-based selection
    // No button presses so there is no interference with the gravity spheres
    [Header("Task References")]
    public SelectionTaskMeasure selectionTaskMeasure;
    // public GravitationalField gravitationalFieldRight;
    // public GravitationalField gravitationalFieldLeft;
    
    private bool isInCollider;
    private bool isSelected;
    private GameObject selectedObj;
    private float dwellTime;  // Time controller stays in collider
    private const float DWELL_THRESHOLD = 0.5f;  // 0.5 seconds to "select"


    void Update()
    {
        // Pure collision-based selection - NO BUTTONS!
        if (isInCollider)
        {
            dwellTime += Time.deltaTime;
            
            if (!isSelected && dwellTime > DWELL_THRESHOLD)
            {
                isSelected = true;
                if (selectedObj != null)
                {
                    // Parent to controller (same grab logic)
                    selectedObj.transform.parent.transform.parent = transform;
                    Debug.Log("Grabbed objectT via collision dwell");
                }
            }
        }
        else
        {
            dwellTime = 0f;
        }

        // Auto-release when exiting collider (no button needed)
        // if (isSelected && !isInCollider)
        // {
        //     isSelected = false;
        //     if (selectedObj != null)
        //     {
        //         selectedObj.transform.parent.transform.parent = null;
        //         Debug.Log("Released objectT");
        //     }
        // }
    }

    void OnTriggerEnter(Collider other)
    {
        // if (other.CompareTag("objectT"))
        // {
        //     isInCollider = true;
        //     selectedObj = other.gameObject;
        //     dwellTime = 0f;  // Reset dwell timer
        // }
        if (other.CompareTag("selectionTaskStart"))
        {
            if (selectionTaskMeasure != null && !selectionTaskMeasure.isCountdown)
            {
                selectionTaskMeasure.isTaskStart = true;
                selectionTaskMeasure.StartOneTask();

                // gravitationalFieldRight.isTaskActive = true;
                // gravitationalFieldLeft.isTaskActive = true;

                Debug.Log("Task started via controller collision");
            }
        }
        else if (other.CompareTag("done"))
        {
            if (selectionTaskMeasure != null)
            {
                Debug.Log("Collision with 'done' trigger detected. Ending task...");
                selectionTaskMeasure.isTaskStart = false;
                selectionTaskMeasure.EndOneTask();

                // gravitationalFieldRight.ResetTaskMode();
                // gravitationalFieldLeft.ResetTaskMode();
                
                // GravityFieldSpawner taskSpawner = FindObjectOfType<GravityFieldSpawner>(); 
                // if (taskSpawner != null && taskSpawner.isTaskController)
                // {
                //     taskSpawner.ResetTaskSphereSpawn();
                // }

                Debug.Log("Task ended via controller collision");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("objectT"))
        {
            isInCollider = false;
            selectedObj = null;
        }
    }
}
