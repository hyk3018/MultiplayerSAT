using System;
using System.Collections.Generic;
using Client.Avatar;
using Client.Commands;
using Server;
using Server.Movement;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Shared.Entity
{
    public class NetworkPlayerState : NetworkBehaviour
    {
        public float MoveSpeed;
        public static event Action<NetworkObject> PlayerSpawned;
        public event Action<StateData> ServerStateReceived;

        private NetworkTransform _networkTransform;
        private Queue<ClientInputData> _inputQueue;
        private StateData[] _stateBuffer;
        private const int BUFFER_SIZE = 1024;

        private void Awake()
        {
            _inputQueue = new Queue<ClientInputData>();
            _stateBuffer = new StateData[BUFFER_SIZE];
            GameManager.Instance.Tick += HandleTick;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            if (GameManager.Instance != null)
                GameManager.Instance.Tick -= HandleTick;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            _networkTransform = GetComponent<NetworkTransform>();
            
            if (IsOwnedByServer)
            {
                _networkTransform.enabled = true;
            }
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
        }

        private void HandleTick(int tick)
        {
            int bufferIndex = -1;
            while (_inputQueue.Count > 0)
            {
                ClientInputData clientInput = _inputQueue.Dequeue();

                bufferIndex = clientInput.Tick % BUFFER_SIZE;

                StateData stateData = ProcessInput(clientInput);
                transform.position = stateData.Position;
                _stateBuffer[bufferIndex] = stateData;
            }

            if (bufferIndex != -1)
            {
                ReturnResultStateClientRpc(_stateBuffer[bufferIndex]);
            }
        }

        public StateData ProcessInput(ClientInputData input)
        {
            FaceMovementDirectionClientRpc(input.MoveTarget.x);
            var bounds = GameManager.Instance.mapBounds.bounds;
            var newPos = transform.position + input.MoveTarget * MoveSpeed;
            newPos.x = Mathf.Clamp(newPos.x, bounds.min.x, bounds.max.x);
            newPos.y = Mathf.Clamp(newPos.y, bounds.min.y, bounds.max.y);
            
            return new StateData()
            {
                Tick = input.Tick,
                Position = newPos
            };
        }

        [ClientRpc]
        private void FaceMovementDirectionClientRpc(float moveTargetX)
        {
            if (moveTargetX > 0)
            {
                GetComponent<SpriteRenderer>().flipX = false;
            }
            else if (moveTargetX < 0)
            {
                GetComponent<SpriteRenderer>().flipX = true;
            }
        }
    }
}

