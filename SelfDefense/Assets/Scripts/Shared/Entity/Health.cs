using System;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity
{
    public class Health : NetworkBehaviour
    {
        public event Action HealthZero;
        
        public int MaxHealth;
        public NetworkVariable<int> CurrentHealth;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
                CurrentHealth.Value = MaxHealth;
        }

        [ServerRpc]
        public void TakeDamageServerRpc(int amount)
        {
            CurrentHealth.Value = Math.Max(0, CurrentHealth.Value - amount);
            if (CurrentHealth.Value == 0)
            {
                HealthZero?.Invoke();
            }
        }
    }
}