using UnityEngine;

public class GravitationalField : MonoBehaviour
{
    [Header("Gravity")]
    public float baseMass = 20f;
    public float innerRadius = 0.1f;
    public float forceAmplifier = 2f;
    
    private float currentRadius;
    public float currentMass;

    [Header("Softening")]
    public float softeningLength = 0.5f;

    private static int activeFields = 0;
    private static Rigidbody playerRigidbody;

    private bool isInfluencing = false;
    private RaySpawn ownerSpawner;
    public bool canDespawnOnPlayer = false;

    public void SetRadius(float radius)
    {
        currentRadius = radius;
        currentMass = baseMass * (radius * radius);
        UpdateVisualSize();
    }

    private void Awake()
    {
        if(playerRigidbody == null)
            // find Rigidbody player
            playerRigidbody = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();

        if (currentRadius == 0f)
            SetRadius(1f);

        // if(objects == null)
        //     // find Rigidbody objects
        //     objects = GameObject.FindWithTag("debris").GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        StartInfluence();
    }

    public void SetOwner(RaySpawn owner)
    {
        ownerSpawner = owner;
    }

    private void StartInfluence()
    {
        if (isInfluencing)
            return;

        activeFields++;
        isInfluencing = true;

        if (activeFields == 1)
        {
            playerRigidbody.useGravity = false;
        }
            
    }

    private void OnDisable()
    {
        StopInfluence();
    }

    private void OnDestroy()
    {
        StopInfluence();
    }

    private void StopInfluence()
    {
        if (!isInfluencing)
            return;

        activeFields = Mathf.Max(0, activeFields - 1);
        isInfluencing = false;

        if (activeFields == 0)
        {
            playerRigidbody.useGravity = true;
        }
    }

    private void FixedUpdate()
    {        
        Vector3 toCenter = transform.position - playerRigidbody.position;
        float r2_player = toCenter.sqrMagnitude;
        float dist_player = toCenter.magnitude;
        float eps2_player = softeningLength * softeningLength;

        if (r2_player < 1e-6f)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            return;
        }

        // Plummer-like softening: a ∝ r / (r^2 + eps^2)^(3/2)
        float denom_player = Mathf.Pow(r2_player + eps2_player, 1.5f);
        playerRigidbody.AddForce(toCenter * (currentMass * forceAmplifier / denom_player), ForceMode.Acceleration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!canDespawnOnPlayer)
            {
                return; 
            }
            
            // Find ANY spawner and tell it to despawn THIS specific field
            RaySpawn[] spawners = FindObjectsByType<RaySpawn>(FindObjectsSortMode.None);
            foreach (var spawner in spawners)
            {
                if (spawner.activeField == gameObject)
                {
                    spawner.DespawnField();
                    return;  // Done!
                }
            }
        }
    }

    void UpdateVisualSize()
    {
        transform.localScale = Vector3.one * (currentRadius * 2f);
    }
}