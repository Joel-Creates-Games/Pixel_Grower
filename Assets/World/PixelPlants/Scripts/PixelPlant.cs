using System.Collections;
using UnityEngine;

public class PixelPlant : MonoBehaviour
{
    [Header("Plant Parts")]
    [SerializeField] private Transform leaves;
    [SerializeField] private Transform stem;
    [SerializeField] private Transform bulb;
    [SerializeField] private Transform flower;

    [Header("Plant Parts Scalars")]
    [SerializeField] private Transform leavesScalar;
    [SerializeField] private Transform stemScalar;
    [SerializeField] private Transform bulbScalar;

    [Header("Growth Settings")]
    [SerializeField] private float growDuration = 1f;
    private Collider flowerCollider;


    // Store each part’s original scale so we can return to it
    private Vector3 leavesOriginalScale;
    private Vector3 stemOriginalScale;
    private Vector3 bulbOriginalScale;
    private Vector3 flowerOriginalScale;

    private void Awake()
    {
        // cache the flower's Collider
        if (flower != null)
            flowerCollider = GetComponent<Collider>();
        else
            Debug.LogWarning("Flower GameObject not assigned on " + name);
    }

    private void Start()
    {
        // Cache original scales
        if (leaves != null) leavesOriginalScale = leavesScalar.localScale;
        if (stem != null) stemOriginalScale = stemScalar.localScale;
        if (bulb != null) bulbOriginalScale = bulbScalar.localScale;
        if (flower != null) flowerOriginalScale = flower.localScale;

        // Disable all parts at the start
        if (leaves != null) leaves.gameObject.SetActive(false);
        if (stem != null) stem.gameObject.SetActive(false);
        if (bulb != null) bulb.gameObject.SetActive(false);
        if (flower != null) flower.gameObject.SetActive(false);
    }

    public void GrowPlant()
    {
        StartCoroutine(GrowSequence());
    }

    public void EndGrowth()
    {
        // 1. Disable the trigger so it can't be clicked again
        if (flowerCollider != null)
        {
            flowerCollider.enabled = false;
        }
        else
        {
            Debug.LogWarning($"{name} has no flowerCollider assigned");
        }

        // 2. Activate the flower visual
        if (flower != null)
        {
            flower.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"{name} has no flowerVisual assigned");
        }

        // Optionally, you could also disable this script if 
        // no further growth logic is needed:
        // this.enabled = false;
    }

    public void ApplyFlowerColour(Color colour)
    {
        if (flower != null)
        {
            Renderer renderer = flower.GetComponent<Renderer>();
            Renderer renderer2 = bulb.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = colour;
                renderer2.material.color = colour;
            }
        }
    }



    private IEnumerator GrowSequence()
    {
        if (leaves != null)
            yield return StartCoroutine(GrowPart(leaves, leavesScalar, leavesOriginalScale));

        if (stem != null)
            yield return StartCoroutine(GrowPart(stem, stemScalar, stemOriginalScale));

        if (bulb != null)
            yield return StartCoroutine(GrowPart(bulb, bulbScalar, bulbOriginalScale));

        // once all parts are grown, enable the flower's Collider
        if (flowerCollider != null)
            flowerCollider.enabled = true;
        else
            Debug.LogWarning("No Collider found on flower to enable");
    }


    private IEnumerator GrowPart(Transform part, Transform partScalar, Vector3 targetScale)
    {
        // Reset to zero and enable
        partScalar.localScale = Vector3.zero;
        part.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < growDuration)
        {
            float t = elapsed / growDuration;
            partScalar.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure exact final value
        partScalar.localScale = targetScale;
    }
}