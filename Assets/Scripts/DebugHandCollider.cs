using UnityEngine;

public class DebugHandCollider : MonoBehaviour
{
    private bool wasEnabledLastFrame = true;
    
    void Update()
    {
        Collider col = GetComponent<SphereCollider>();
        if (col != null && !col.enabled)
        {
            UnityEngine.Debug.LogError("Hand Collider DISABLED on " + name + " at " + Time.time, this);
        }
        
        wasEnabledLastFrame = col?.enabled ?? false;
    }
}
