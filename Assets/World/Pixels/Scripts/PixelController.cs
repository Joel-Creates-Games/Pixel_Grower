using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PixelController : MonoBehaviour
{
    [Header("Assign the Plant component that has GrowPlant()")]
    [Tooltip("Drag & drop the Plant script (on this GameObject or another) here.")]
    [SerializeField] private PixelPlant plant;

    private Collider pixelCollider;

    private void Awake()
    {
        pixelCollider = GetComponent<Collider>();

        // Ensure the collider is set as a trigger
        if (!pixelCollider.isTrigger)
        {
            Debug.LogWarning($"[{name}] Collider is not marked as Trigger. Please enable 'Is Trigger'.");
            pixelCollider.isTrigger = true;
        }
    }

    private void Start()
    {
        //plant.GrowPlant();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only react to objects tagged "seed"
        if (!other.CompareTag("seed"))
            return;

        // Disable this pixel's collider so it can't trigger again
        pixelCollider.enabled = false;
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.enabled = false;
        if (other.GetComponent<SeedController>() != null)
        {
            other.GetComponent<SeedController>().DisableObject();
        }

        // Invoke GrowPlant on the connected Plant
        if (plant != null)
        {
            plant.GrowPlant();
        }
        else
        {
            Debug.LogWarning($"[{name}] No Plant assigned in PixelController.");
        }
    }
}