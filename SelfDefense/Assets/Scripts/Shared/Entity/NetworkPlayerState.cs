using System;
using System.Collections.Generic;
using Client.Avatar;
using Client.Commands;
using Server.Movement;
using Unity.Netcode;
using UnityEngine;

namespace Shared.Entity
{
    public class NetworkPlayerState : NetworkBehaviour
    {
        public float MoveSpeed;
        public static event Action<NetworkObject> PlayerSpawned;
        public event Action<StateData> ServerStateReceived;
        
        private bool _inputReceived;
        private Queue<ClientInputData> _inputQueue;
        private StateData[] _stateBuffer;
        private const int BUFFER_SIZE = 1024;

        private void Awake()
        {
            _inputQueue = new Queue<ClientInputData>();
            _stateBuffer = new StateData[BUFFER_SIZE];
            NetworkManager.Singleton.NetworkTickSystem.Tick += HandleTick;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.NetworkTickSystem.Tick -= HandleTick;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            PlayerSpawned?.Invoke(GetComponent<NetworkObject>());
        }
        
        [ClientRpc]
        private void ReturnResultStateClientRpc(StateData stateData, ClientRpcParams clientRpcParams = default)
        {
            ServerStateReceived?.Invoke(stateData);
        }

        [ServerRpc]
        public void SubmitPlayerInputServerRpc(ClientInputData clientInputData)
        {
            _inputQueue.Enqueue(clientInputData);
            return;
            

            if (!clientInputData.DoCommand) return;
            switch (clientInputData.RequestedCommand.CommandType)
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

        private void HandleTick()
        {
            int bufferIndex = -1;
            while (_inputQueue.Count > 0)
            {
                ClientInputData clientInput = _inputQueue.Dequeue();

                bufferIndex = clientInput.Tick % BUFFER_SIZE;

                StateData stateData = ProcessInput(clientInput);
                _stateBuffer[bufferIndex] = stateData;
            }

            if (bufferIndex != -1)
            {
                var clientRpcParams = new ClientRpcParams()
                {
                    Send = new ClientRpcSendParams()
                    {
                        TargetClientIds = new ulong[]{OwnerClientId}
                    }
                };
                ReturnResultStateClientRpc(_stateBuffer[bufferIndex], clientRpcParams);
            }
        }

        private StateData ProcessInput(ClientInputData input)
        {
            transform.position += input.MoveTarget * MoveSpeed;

            return new StateData()
            {
                Tick = input.Tick,
                Position = transform.position
            };
        }
    }
}

