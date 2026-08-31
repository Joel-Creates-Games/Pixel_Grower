using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridSize = 32;
    [SerializeField] private float spacing = 1f;

    [Header("Points Container")]
    [Tooltip("Assign a Transform whose children will be laid out on the grid.")]
    [SerializeField] private Transform pointsParent;

    [Header("Generated Plants")]
    [SerializeField] private List<PixelPlant> plants = new List<PixelPlant>();



    private void Awake()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (pointsParent == null)
        {
            Debug.LogWarning("No pointsParent assigned. Nothing to position.");
            return;
        }

        int requiredCount = gridSize * gridSize;
        int actualCount = pointsParent.childCount;
        float halfExtent = (gridSize - 1) * 0.5f * spacing;

        // Position and activate the first N = requiredCount children
        int usedCount = Mathf.Min(requiredCount, actualCount);
        for (int i = 0; i < usedCount; i++)
        {
            Transform child = pointsParent.GetChild(i);
            int x = i % gridSize;
            int z = i / gridSize;

            Vector3 localPos = new Vector3(
                x * spacing - halfExtent,
                0f,
                z * spacing - halfExtent
            );

            child.localPosition = localPos;
            child.gameObject.SetActive(true);
            plants.Add(child.GetChild(0).GetChild(0).GetComponent<PixelPlant>());
        }

        // Disable any extra children beyond the grid count
        for (int i = requiredCount; i < actualCount; i++)
        {
            pointsParent.GetChild(i).gameObject.SetActive(false);
        }

        // Warn if there aren’t enough children to fill the grid
        if (actualCount < requiredCount)
        {
            Debug.LogWarning(
                $"GridGenerator needs {requiredCount} children, " +
                $"but only {actualCount} were found."
            );
        }
    }
    public void ColourPlants(Color[] pixelColours)
    {
        int count = Mathf.Min(pixelColours.Length, plants.Count);

        for (int i = 0; i < count; i++)
        {
            PixelPlant plant = plants[i];
            Color colour = pixelColours[i];
            plant.ApplyFlowerColour(colour);
        }
    }



#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        float size = (gridSize - 1) * spacing;
        Vector3 half = new Vector3(size, 0, size) * 0.5f;
        Vector3 center = transform.position;

        Gizmos.DrawLine(center + new Vector3(-half.x, 0, -half.z),
                        center + new Vector3(-half.x, 0, half.z));
        Gizmos.DrawLine(center + new Vector3(-half.x, 0, half.z),
                        center + new Vector3(half.x, 0, half.z));
        Gizmos.DrawLine(center + new Vector3(half.x, 0, half.z),
                        center + new Vector3(half.x, 0, -half.z));
        Gizmos.DrawLine(center + new Vector3(half.x, 0, -half.z),
                        center + new Vector3(-half.x, 0, -half.z));
    }
#endif
}