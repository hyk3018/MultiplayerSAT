using System.Collections.Generic;
using Server.Movement;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity
{
    [RequireComponent(typeof(ServerPathFollower))]
    public class NetworkPlayerState : NetworkBehaviour
    {
        private ServerPathFollower m_serverPathFollower;

        private void Awake()
        {
            m_serverPathFollower = GetComponent<ServerPathFollower>();
        }

        [ServerRpc]
        public void SubmitPositionRequestServerRpc(Vector3 newPosition)
        {
            m_serverPathFollower.SetNewPath(new List<Vector3>() {newPosition});
        }
    }
}

