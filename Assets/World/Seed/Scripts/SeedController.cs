using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedController : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float throwForce = 6f;
    [SerializeField] private float disableDelay = 5f;

    private void OnEnable()
    {
        // Reset velocity for pooled seeds
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void ThrowSeed()
    {
        // Apply an instantaneous forward impulse
        rb.AddForce(transform.forward * throwForce, ForceMode.VelocityChange);

        // Schedule deactivation so seed returns to pool
        StartCoroutine(DisableAfterTime());
    }

    public void DisableObject()
    {
        StopCoroutine(DisableAfterTime());
        gameObject.SetActive(false);
    }

    private IEnumerator DisableAfterTime()
    {
        yield return new WaitForSeconds(disableDelay);
        gameObject.SetActive(false);
    }
}

