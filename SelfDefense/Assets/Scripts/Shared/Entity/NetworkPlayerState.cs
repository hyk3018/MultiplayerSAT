using System;
using System.Collections.Generic;
using Client.Commands;
using Server.Movement;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity
{
    [RequireComponent(typeof(ServerPathFollower))]
    public class NetworkPlayerState : NetworkBehaviour
    {
        public static event Action<NetworkObject> PlayerSpawned;
        
        private ServerPathFollower m_serverPathFollower;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            PlayerSpawned?.Invoke(GetComponent<NetworkObject>());
        }

        private void Awake()
        {
            m_serverPathFollower = GetComponent<ServerPathFollower>();
        }

        [ServerRpc]
        public void SubmitPositionRequestServerRpc(Vector3 newPosition)
        {
            m_serverPathFollower.SetNewPath(new List<Vector3>() {newPosition});
        }

        [ServerRpc]
        public void SubmitCommandRequestServerRpc(CommandData requestedCommand)
        {
            switch (requestedCommand.CommandType)
            {
                case CommandType.BUILD_TOY:
                    Debug.Log("Build toy command requested");
                    break;
                case CommandType.BUILD_MUSIC:
                    break;
                case CommandType.BUILD_JOKE:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

