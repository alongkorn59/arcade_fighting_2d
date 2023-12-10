using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    private const int MAX_HP = 100;
    [SerializeField] private int currentHealth = MAX_HP;
    public int CurrentHealth { get { return currentHealth; } }
    public bool isDamageable = true;

    public Action OnDead;
    public Action<int> OnHealthUpdate;
    void Awake()
    {
        currentHealth = MAX_HP;
    }

    public void TakeDamage(int damage)
    {
        int estimateHp = currentHealth - damage;
        currentHealth = estimateHp > 0 ? estimateHp : 0;
        OnHealthUpdate?.Invoke(currentHealth);
        if (currentHealth <= 0)
        {
            OnDead?.Invoke();
        }
    }
}
