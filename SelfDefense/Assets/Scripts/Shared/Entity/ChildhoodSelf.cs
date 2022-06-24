using System;
using Server;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity
{
    public class ChildhoodSelf : NetworkBehaviour
    {
        // Currently unused decay mechanic
        [SerializeField]
        private int healthDecayRate;

        [SerializeField]
        private Animation heartAnimation, sadAnimation;

        [SerializeField]
        private AudioSource heartSound, sadSound;

        public event Action Distressed; 

        private int _decayCounter;
        private Health _health;
        private AffectionPoints _affectionPoints;
        
        private void Awake()
        {
            _decayCounter = 0;
            _health = GetComponent<Health>();
            _health.HealthZero += () =>
            {
                Distressed?.Invoke();
            };
            GameManager.Instance.Tick += HandleTick;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            GameManager.Instance.Tick -= HandleTick;
        }

        public void Initialise(AffectionPoints affectionPoints)
        {
            _affectionPoints = affectionPoints;
        }

        private void HandleTick(int obj)
        {
            return;
            
            // Currently unused decay mechanic
            if (GameManager.Instance.GameState != GameState.PLAYING) return;
            
            _decayCounter++;
            if (_decayCounter < healthDecayRate) return;
            
            _health.TakeDamageServerRpc(1);
            _decayCounter = 0;
        }

        [ServerRpc]
        public void AcceptEmotionRewardServerRpc(int amount)
        {
            _affectionPoints.Points.Value += amount;
            ShowAffectionAnimationClientRpc();
        }

        [ClientRpc]
        private void ShowAffectionAnimationClientRpc()
        {
            heartAnimation.Play();
            heartSound.Play();
        }

        [ServerRpc]
        public void ReceiveDistressServerRpc()
        {
            ShowDistressAnimationClientRpc();
        }

        [ClientRpc]
        private void ShowDistressAnimationClientRpc()
        {
            sadAnimation.Play();
            sadSound.Play();
        }
    }
}