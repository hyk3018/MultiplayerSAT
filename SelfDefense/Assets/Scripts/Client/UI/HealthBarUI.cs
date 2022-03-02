using System;
using Shared.Entity;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField]
        private Image healthBar;

        [SerializeField]
        private Health health;

        private void Awake()
        {
            health.CurrentHealth.OnValueChanged += (value, newValue) =>
            {
                healthBar.fillAmount = (float) newValue / health.MaxHealth;
            };
        }
    }
}