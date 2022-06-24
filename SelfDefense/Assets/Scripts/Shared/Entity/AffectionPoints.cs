using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity
{
    public class AffectionPoints : NetworkBehaviour
    {
        [SerializeField]
        private int affectionPointsStart;

        public NetworkVariable<int> Points;

        [ServerRpc(RequireOwnership = false)]
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