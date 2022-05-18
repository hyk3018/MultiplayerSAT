using System;
using System.Collections.Generic;
using Client.UI;
using ScriptableObjects.Enemy;
using Server.Movement;
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
    
    [RequireComponent(typeof(Health)), RequireComponent(typeof(ServerPathFollower))]
    public class Enemy : NetworkBehaviour
    {
        [SerializeField]
        private EnemyData enemyData;

        [SerializeField]
        private SpriteRenderer spriteRenderer;
        
        [SerializeField]
        private HealthBarUI _healthBarUI;

        public EnemyStatus Status;
        public event Action OnDeath;
        
        private Health _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _health.MaxHealth = enemyData.MaxHealth;
            _health.CurrentHealth.OnValueChanged += HandleHealthChangeClientRpc;
            GetComponent<ServerPathFollower>().ReachedPathEnd += () => { Destroy(gameObject); };
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            OnDeath?.Invoke();
            _health.HealthZero -= HandleDeathServerRpc;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsOwner) return;
            
            if (!other.CompareTag("ChildhoodSelf")) return;
            
            var damage = enemyData.StatusDamage[(int)Status];
            other.gameObject.GetComponent<Health>().TakeDamageServerRpc(damage);

            var childhoodSelf = other.gameObject.GetComponent<ChildhoodSelf>();
            if (Status == EnemyStatus.POSITIVE)
            {
                childhoodSelf.AcceptEmotionRewardServerRpc(enemyData.AffectionReward);    
            }
            else
            {
                childhoodSelf.ReceiveDistressServerRpc();
            }
            
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
            if (Status == EnemyStatus.POSITIVE)
            {
                _healthBarUI.Hide();
                var pathFollower = GetComponent<ServerPathFollower>();
                pathFollower.MoveSpeed = 1.2f;
            }
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