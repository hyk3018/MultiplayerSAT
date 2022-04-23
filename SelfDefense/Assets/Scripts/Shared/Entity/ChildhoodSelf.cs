using System;
using Server;
using Shared.Entity.Towers;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity
{
    public class ChildhoodSelf : NetworkBehaviour
    {
        [SerializeField]
        private int healthDecayRate;

        private int _decayCounter;
        private Health _health;
        
        private void Awake()
        {
            _decayCounter = 0;
            _health = GetComponent<Health>();
            GameManager.Instance.Tick += HandleTick;
        }

        private void HandleTick(int obj)
        {
            if (GameManager.Instance.GameState != GameState.PLAYING) return;
            
            _decayCounter++;
            if (_decayCounter < healthDecayRate) return;
            
            _health.TakeDamageServerRpc(1);
            _decayCounter = 0;
        }
    }
}