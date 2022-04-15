using System;
using System.Collections.Generic;
using ScriptableObjects.Enemy;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity.Towers
{
    public enum EnemyStatus
    {
        NEGATIVE = 0,
        NEUTRAL = 1,
        POSITIVE = 2
    }
    
    [RequireComponent(typeof(Health))]
    public class Enemy : NetworkBehaviour
    {
        [SerializeField]
        private EnemyData enemyData;

        [SerializeField]
        private SpriteRenderer spriteRenderer;

        public EnemyStatus Status;
        
        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _health.MaxHealth = enemyData.MaxHealth;
            _health.CurrentHealth.OnValueChanged += HandleHealthChangeClientRpc;
            //_health.HealthZero += HandleDeathServerRpc;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _health.HealthZero -= HandleDeathServerRpc;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsOwner) return;

            if (!other.CompareTag("ChildhoodSelf")) return;
            
            var damage = enemyData.StatusDamage[(int)Status];
            other.gameObject.GetComponent<Health>().TakeDamageServerRpc(damage);
            Destroy(gameObject);
        }

        public bool IsTargetedBy(TowerType towerType)
        {
            return enemyData.IsTargetedBy(towerType);
        }

        [ClientRpc]
        private void HandleHealthChangeClientRpc(int oldHealth, int newHealth)
        {
            for (int i = 0; i < enemyData.StatusThreshold.Count; i++)
            {
                if (newHealth < enemyData.StatusThreshold[i])
                {
                    Status = (EnemyStatus) i;
                }
            }

            spriteRenderer.sprite = enemyData.StatusSprites[(int)Status];
        }

        [ServerRpc]
        private void HandleDeathServerRpc()
        {
            Destroy(gameObject);
        }

        public float GetEffectiveness(TowerType towerType)
        {
            return enemyData.GetEffectiveness(towerType);
        }
    }
}