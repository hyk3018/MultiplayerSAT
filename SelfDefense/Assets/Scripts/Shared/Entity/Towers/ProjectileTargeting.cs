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

        [SerializeField]
        private AudioSource hitSound;
        
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
            if (other.gameObject != _currentTarget) return;
            
            var finalDamage = damage;
                
            var enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                finalDamage = Mathf.CeilToInt(damage * enemy.GetEffectiveness(towerType));
                PlayHitSoundClientRpc();
            }

                
            var health = _currentTarget.GetComponent<Health>();
            health.TakeDamageServerRpc(finalDamage);
            Destroy(gameObject);
        }

        [ClientRpc]
        private void PlayHitSoundClientRpc()
        {
            hitSound.Play();
        }

        private void HandleTick(int tickNumber)
        {
            if (_currentTarget)
            {
                var targetDirection = _currentTarget.transform.position - transform.position;
                transform.position += targetDirection.normalized * projectileSpeed;
            }
            else
            {
                Destroy(gameObject);
            }
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