using System;
using System.Collections.Generic;
using ScriptableObjects.Enemy;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity.Towers
{
    [RequireComponent(typeof(Health))]
    public class Enemy : NetworkBehaviour
    {
        [SerializeField]
        private EnemyData enemyData;
        
        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _health.MaxHealth = enemyData.MaxHealth;
            _health.HealthZero += HandleDeathServerRpc;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _health.HealthZero -= HandleDeathServerRpc;
        }

        public bool IsTargetedBy(TowerType towerType)
        {
            return enemyData.IsTargetedBy(towerType);
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