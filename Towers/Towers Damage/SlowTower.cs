using UnityEngine;

public class SlowTower : MonoBehaviour
{
    public float slowPercentage = 0.5f; // Slow enemies by 50%
    public float slowDuration = 2f;    // Effect lasts for 2 seconds
    public float range = 5f;           // Tower's effective range

    private SphereCollider rangeCollider;

    private void Start()
    {
        // Add a SphereCollider to detect enemies within range
        rangeCollider = gameObject.AddComponent<SphereCollider>();
        rangeCollider.isTrigger = true;
        rangeCollider.radius = range;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to an enemy
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.ApplySlow(slowPercentage, slowDuration);
        }
    }
}
