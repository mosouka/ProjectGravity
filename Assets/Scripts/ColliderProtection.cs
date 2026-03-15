using UnityEngine;

// Because an unknown script is disabling hand colliders, this script forces them back on.
public class ColliderProtection : MonoBehaviour
{
    void LateUpdate()
    {
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null && !col.enabled)
        {
            col.enabled = true;
            //UnityEngine.Debug.LogWarning("Hand collider forced ON: " + name, this);
        }
    }
}
