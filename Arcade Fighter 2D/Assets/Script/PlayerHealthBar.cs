using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthBar1;
    [SerializeField] private Slider healthBar2;

    [SerializeField] private PlayerController Player1;
    private Health HealthPlayer1 => Player1.Health;

    [SerializeField] private PlayerController Player2;
    private Health HealthPlayer2 => Player2.Health;

    void Start()
    {
        HealthPlayer1.OnHealthUpdate += UpdateCurrentHealth1;
        HealthPlayer2.OnHealthUpdate += UpdateCurrentHealth2;
    }

    private void UpdateCurrentHealth1(int currentHealth)
    {
        healthBar1.value = currentHealth;
    }
    private void UpdateCurrentHealth2(int currentHealth)
    {
        healthBar2.value = currentHealth;
    }
}
