using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Player Resources")]
    public int gold = 50;
    public int seedCount = 0;

    public void GiveSeed()
    {
        seedCount++;
        Debug.Log("Received a seed. Total seeds: " + seedCount);
    }
}