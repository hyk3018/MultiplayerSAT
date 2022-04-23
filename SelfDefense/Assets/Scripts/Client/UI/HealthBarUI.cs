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

        public void Initialise(Health healthComponent)
        {
            health = healthComponent;
            health.CurrentHealth.OnValueChanged += OnCurrentHealthChanged;
            OnCurrentHealthChanged(health.MaxHealth, health.MaxHealth);
        }
        
        private void Awake()
        {
            if (!health) return;

            health.CurrentHealth.OnValueChanged += OnCurrentHealthChanged;
        }

        private void OnCurrentHealthChanged(int value, int newValue)
        {
            healthBar.fillAmount = (float) newValue / health.MaxHealth;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}