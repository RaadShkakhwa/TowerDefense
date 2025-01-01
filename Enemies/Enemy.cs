using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float DamageResistance = 1f;
    public int NodeIndex;
    public float MaxHealth = 10f;
    public float Health;
    public int ID;

    public float baseSpeed = 5f;   // The original speed of the enemy
    private float currentSpeed;
    private float slowEndTime;



    private bool isDead = false;

    private void Start()
    {
        currentSpeed = baseSpeed;
    }

    public void ApplySlow(float slowPercentage, float duration)
    {
        float newSpeed = baseSpeed * (1f - slowPercentage);

        // If already slowed, check if the new slow effect is stronger
        if (currentSpeed > newSpeed)
        {
            currentSpeed = newSpeed;
            slowEndTime = Time.time + duration;
            Debug.Log($"{name} slowed to {currentSpeed} for {duration} seconds.");
        }

        // Ensure the slow effect doesn't expire prematurely
        if (Time.time + duration > slowEndTime)
        {
            slowEndTime = Time.time + duration;
        }
    }

 


    public void Init()
    {
        // Ensure MaxHealth and Speed are valid
        MaxHealth = MaxHealth > 0 ? MaxHealth : 100f;
        baseSpeed = baseSpeed > 0 ? baseSpeed : 5f;

        Health = MaxHealth;

        if (GameLoopMaster.NodePosition != null && GameLoopMaster.NodePosition.Length > 0)
        {
            transform.position = GameLoopMaster.NodePosition[0];
            NodeIndex = 0;
        }
        else
        {
            Debug.LogError("NodePosition array is null or empty!");
        }
    }

    public void ApplyDamage(float damage)
    {
        float effectiveDamage = Mathf.Max(damage - DamageResistance, 0);
        Health -= effectiveDamage;
        Debug.Log($"Enemy took {effectiveDamage} damage. Remaining Health: {Health}");

        if (Health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{name} has died.");
        Destroy(gameObject); // Ensure this is correctly called
    }

    internal void RemoveSlow(float slowPercentage)
    {
        throw new NotImplementedException();
    }
}

