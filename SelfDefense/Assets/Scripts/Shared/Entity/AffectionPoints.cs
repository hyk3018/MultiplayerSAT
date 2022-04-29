using System;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity
{
    public class AffectionPoints : NetworkBehaviour
    {
        [SerializeField]
        private int affectionPointsStart;

        public NetworkVariable<int> Points;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Points.Value = affectionPointsStart;
        }
        
        private void Update()
        {
            if (!IsOwner) return;
            
            if (Input.GetKeyDown(KeyCode.U))
            {
                IncreasePointsServerRpc();
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                DecreasePointsServerRpc();
            }
        }

        [ServerRpc]
        private void DecreasePointsServerRpc()
        {
            Points.Value = Points.Value - 1;
        }

        [ServerRpc]
        private void IncreasePointsServerRpc()
        {
            Points.Value = Points.Value + 1;
        }

        [ServerRpc]
        public void SpendPointsServerRpc(int commandCost)
        {
            Points.Value -= commandCost;
        }
    }
}