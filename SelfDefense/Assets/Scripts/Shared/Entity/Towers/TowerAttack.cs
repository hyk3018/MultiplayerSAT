using System;
using System.Collections.Generic;
using System.Linq;
using Server;
using Server.Movement;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity.Towers
{
    [Serializable]
    public struct TowerStat
    {
        public float AttackRate;
        public TowerType TowerType;
    }

    public enum TowerTargetType
    {
        LAST,
        FIRST,
        CLOSE,
        STRONG
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
        
        public TowerTargetType TargetType;
        
        private List<GameObject> _enemiesInRange;
        private GameObject _currentTarget;
        private float _attackCooldown;

        private void Awake()
        {
            if (!IsServer) return;
            GameManager.Instance.Tick += HandleTick;
            _enemiesInRange = new List<GameObject>();
            TargetType = TowerTargetType.FIRST;
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
            
            GameObject currentTarget;
            switch (TargetType)
            {
                case TowerTargetType.LAST:
                    currentTarget = _enemiesInRange
                        .OrderByDescending(x => x
                            .GetComponent<ServerPathFollower>()
                            .DistanceToEnd)
                        .First();
                    break;
                case TowerTargetType.FIRST:
                    currentTarget = _enemiesInRange
                        .OrderBy(x => x
                            .GetComponent<ServerPathFollower>()
                            .DistanceToEnd)
                        .First();
                    break;
                case TowerTargetType.CLOSE:
                    currentTarget = _enemiesInRange
                        .OrderBy(x => (x.transform.position - transform.position)
                            .magnitude)
                        .First();
                    break;
                case TowerTargetType.STRONG:
                    currentTarget = _enemiesInRange
                        .OrderByDescending(x => x.GetComponent<Health>().CurrentHealth.Value)
                        .First();
                    break;
                default:
                    currentTarget = _enemiesInRange[0];
                    break;
            }
                
            ChangeTargetClientRpc(currentTarget.GetComponent<NetworkObject>());
            _currentTarget = currentTarget;
        }

        [ClientRpc]
        public void ChangeTargetTypeClientRpc(TowerTargetType type)
        {
            TargetType = type;
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
            _attackCooldown -= NetworkManager.LocalTime.FixedDeltaTime;
            if (_currentTarget == null)
                return;
            
            // If enemy is converted to positive, recalculate
            var enemy = _currentTarget.GetComponent<Enemy>();
            if (enemy.Status == EnemyStatus.POSITIVE)
            {
                RecalculateTarget();
                return;
            }

            // Fire projectile once cooldown reached
            if (_attackCooldown <= 0.0 && _currentTarget != null)
            {
                AttackEnemyServerRpc(_currentTarget);
                _attackCooldown = towerStat.AttackRate;
            }
        }

        /*
         * Rotate tower to face target every tick
         */
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