using UnityEngine;

public class ShopController : MonoBehaviour
{
    [Header("Seed Settings")]
    public int seedPrice = 10;

    [Header("Reference to Player Inventory")]
    public PlayerInventory playerInventory;

    [Header("Reference to Player Inventory")]
    public SightTriggerDetector sightTriggerDetector;


    public void BuySeeds()
    {
        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory reference not set.");
            return;
        }

        if (playerInventory.gold >= seedPrice && sightTriggerDetector.IsPlayerSightColliding())
        {
            playerInventory.gold -= seedPrice;
            playerInventory.GiveSeed();
            Debug.Log("Seed purchased successfully.");
        }
        else
        {
            Debug.Log("Not enough gold to buy a seed.");
        }
    }
}