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

        [ServerRpc]
        private void DecreasePointsServerRpc()
        {
            Points.Value = Points.Value - 10;
        }

        [ServerRpc]
        private void IncreasePointsServerRpc()
        {
            Points.Value = Points.Value + 10;
        }

        [ServerRpc]
        public void SpendPointsServerRpc(int commandCost)
        {
            Points.Value -= commandCost;
        }

        public void Initialise()
        {
            Points.Value = affectionPointsStart;
        }
    }
}