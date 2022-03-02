using System;
using Server;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity.Towers
{
    public class ProjectileTargeting : NetworkBehaviour
    {
        [SerializeField]
        private float projectileSpeed;

        [SerializeField]
        private int damage;

        [SerializeField]
        private TowerType towerType;
        
        private GameObject _currentTarget;

        private void Awake()
        {
            if (!IsServer)
                return;
            GameManager.Instance.Tick += HandleTick;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            GameManager.Instance.Tick -= HandleTick;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsServer) return;
            if (other.gameObject == _currentTarget)
            {
                var finalDamage = damage;
                
                var enemy = other.GetComponent<Enemy>();
                if (enemy != null)
                {
                    finalDamage = Mathf.CeilToInt(damage * enemy.GetEffectiveness(towerType));
                }
                
                var health = _currentTarget.GetComponent<Health>();
                health.TakeDamageServerRpc(finalDamage);
                Destroy(gameObject);
            }
        }

        private void HandleTick(int tickNumber)
        {
            var targetDirection = _currentTarget.transform.position - transform.position;
            transform.position += targetDirection.normalized * projectileSpeed;
        }

        public void Initialise(NetworkObject currentTarget)
        {
            _currentTarget = currentTarget.gameObject;
        }

        private void FixedUpdate()
        {
            transform.Rotate(Vector3.forward, 9f);
        }
    }
}