using System;
using System.Collections.Generic;
using Server;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity.Towers
{
    [Serializable]
    public struct TowerStat
    {
        public float AttackRate;
        public float AttackDamage;
        public TowerType TowerType;
    }
    
    public class TowerAttack : NetworkBehaviour
    {
        [SerializeField]
        private TowerStat towerStat;

        [SerializeField]
        private GameObject attackProjectile;

        [SerializeField]
        private Transform projectileSpawnLocation;

        [SerializeField]
        private Transform rotator;
        
        private List<GameObject> _enemiesInRange;
        private GameObject _currentTarget;
        private float _attackCooldown;

        private void Awake()
        {
            if (!IsServer) return;
            GameManager.Instance.Tick += HandleTick;
            _enemiesInRange = new List<GameObject>();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            GameManager.Instance.Tick -= HandleTick;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsServer)
            {
                enabled = false;
                return;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer) return;
            var enemy = other.GetComponent<Enemy>();
            if (enemy == null || enemy.Status == EnemyStatus.POSITIVE) return;

            if (enemy.IsTargetedBy(towerStat.TowerType))
            {
                _enemiesInRange.Add(other.gameObject);
            }

            RecalculateTarget();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsServer) return;
            var enemy = other.GetComponent<Enemy>();
            if (enemy == null) return;

            _enemiesInRange.Remove(other.gameObject);
            RecalculateTarget();
        }

        private void RecalculateTarget()
        {
            if (_currentTarget && _currentTarget.GetComponent<Enemy>().Status == EnemyStatus.POSITIVE)
            {
                _enemiesInRange.Remove(_currentTarget);
            }
            
            if (_enemiesInRange.Count == 0)
            {
                ChangeTargetClientRpc(default);
                return;
            }
            
            if (_currentTarget == null || !_enemiesInRange.Contains(_currentTarget))
            {
                ChangeTargetClientRpc(_enemiesInRange[0].GetComponent<NetworkObject>());
                _currentTarget = _enemiesInRange[0];
            }
        }

        [ClientRpc]
        private void ChangeTargetClientRpc(NetworkObjectReference networkObjectReference)
        {
            if (networkObjectReference.TryGet(out NetworkObject networkObject))
            {
                _currentTarget = networkObject.gameObject;
            }
            else
            {
                _currentTarget = null;
            }
        }

        private void HandleTick(int tickNumber)
        {
            if (_currentTarget == null)
                return;
            
            var enemyHealth = _currentTarget.GetComponent<Health>();
            var enemy = _currentTarget.GetComponent<Enemy>();
            if (enemy.Status == EnemyStatus.POSITIVE)
            {
                RecalculateTarget();
                return;
            }

            _attackCooldown -= NetworkManager.LocalTime.FixedDeltaTime;
            if (_attackCooldown <= 0.0 && _currentTarget != null)
            {
                Debug.Log("Attack");
                AttackEnemyServerRpc(_currentTarget);
                _attackCooldown = towerStat.AttackRate;
            }
        }

        private void FixedUpdate()
        {
            if (_currentTarget == null) return;
            var vectorToTarget = _currentTarget.transform.position - rotator.position;
            var angle = (float) Math.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
            rotator.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        [ServerRpc(RequireOwnership = false)]
        private void AttackEnemyServerRpc(NetworkObjectReference currentTarget)
        {
            if (currentTarget.TryGet(out NetworkObject networkObject))
            {
                var go = Instantiate(attackProjectile);
                go.transform.position = projectileSpawnLocation.position;
                go.GetComponent<NetworkObject>().Spawn();
                go.GetComponent<ProjectileTargeting>().Initialise(networkObject);
            }
        }
    }
}