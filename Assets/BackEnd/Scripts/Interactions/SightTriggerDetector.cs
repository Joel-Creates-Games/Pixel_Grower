using UnityEngine;

public class SightTriggerDetector : MonoBehaviour
{
    private bool isPlayerSightInside = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerSight"))
        {
            isPlayerSightInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerSight"))
        {
            isPlayerSightInside = false;
        }
    }

    /// <summary>
    /// Returns true if a GameObject with the "PlayerSight" tag is currently inside the trigger.
    /// </summary>
    public bool IsPlayerSightColliding()
    {
        return isPlayerSightInside;
    }
}