using System;
using Client.Commands;
using HelloWorld;
using Server.Movement;
using Shared.Entity;
using Shared.Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Client.Avatar
{
    public struct ClientInputData : INetworkSerializable
    {
        public int Tick;
        public ulong ClientID;
        public bool DoCommand;
        public Vector3 MoveTarget;
        public CommandData RequestedCommand;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref ClientID);
            serializer.SerializeValue(ref DoCommand);
            serializer.SerializeValue(ref MoveTarget);
            RequestedCommand.NetworkSerialize(serializer);
        }
    }

    public struct StateData
    {
        public int Tick;
        public Vector3 Position;
    }
    
    public class ClientInputProcessor : MonoBehaviour
    {
        private bool _moveRequested;
        private bool _commandRequested;
        private CommandData _currentRequestedCommand;
        private NetworkPlayerState _networkPlayerState;
        private Camera _mainCamera;

        private int _currentTick;
        private StateData[] _stateBuffer;
        private ClientInputData[] _inputBuffer;
        private StateData _latestServerState;
        private StateData _lastProcessedState;
        private float _horizontalInput;
        private float _verticalInput;
        private const int BUFFER_SIZE = 1024;

        private void Awake()
        {
            _networkPlayerState = GetComponent<NetworkPlayerState>();
            _mainCamera = Camera.main;
            
            _stateBuffer = new StateData[BUFFER_SIZE];
            _inputBuffer = new ClientInputData[BUFFER_SIZE];
            
            _networkPlayerState.ServerStateReceived += OnServerStateReceived;
            NetworkManager.Singleton.NetworkTickSystem.Tick += HandleTick;
        }
        
        private void OnDestroy()
        {
            _networkPlayerState.ServerStateReceived -= OnServerStateReceived;
            NetworkManager.Singleton.NetworkTickSystem.Tick -= HandleTick;
        }
        
        private void OnServerStateReceived(StateData serverState)
        {
            _latestServerState = serverState;
        }

        public void RequestCommand(CommandData requestedCommand)
        {
            if (_commandRequested) return;

            _commandRequested = true;
            _currentRequestedCommand = requestedCommand;
        }
        
        private void HandleTick()
        {
            if (!_networkPlayerState.IsOwner) return;

            // Populate input data
            var clientInputData = new ClientInputData()
            {
                Tick = _currentTick, ClientID = _networkPlayerState.OwnerClientId,
                MoveTarget = new Vector3(_horizontalInput, _verticalInput, transform.position.z),
                DoCommand = _commandRequested, RequestedCommand = _currentRequestedCommand
            };
            
            if (NetworkManager.Singleton.IsHost)
            {
                
                _networkPlayerState.SubmitPlayerInputServerRpc(clientInputData);
            }
            else
            {
                if (!_latestServerState.Equals(default(StateData)) &&
                    (_lastProcessedState.Equals(default(StateData)) ||
                     !_latestServerState.Equals(_lastProcessedState)))
                {
                    HandleServerReconciliation();
                }

                var bufferIndex = _currentTick % BUFFER_SIZE;
                _inputBuffer[bufferIndex] = clientInputData;
                _stateBuffer[bufferIndex] = ProcessInput(clientInputData);
                
                _networkPlayerState.SubmitPlayerInputServerRpc(clientInputData);
                _currentTick++;
            }

            _commandRequested = false;
        }

        private void HandleServerReconciliation()
        {
            _lastProcessedState = _latestServerState;

            var serverStateBufferIndex = _latestServerState.Tick % BUFFER_SIZE;
            var positionError =
                Vector3.Distance(_latestServerState.Position, _stateBuffer[serverStateBufferIndex].Position);

            if (positionError <= 0.1f) return;
            
            transform.position = _latestServerState.Position;
            _stateBuffer[serverStateBufferIndex] = _latestServerState;

            var tickToProcess = _latestServerState.Tick + 1;

            while (tickToProcess < _currentTick)
            {
                var state = ProcessInput(_inputBuffer[tickToProcess % BUFFER_SIZE]);
                _stateBuffer[tickToProcess % BUFFER_SIZE] = state;
                tickToProcess++;
            }
        }

        private StateData ProcessInput(ClientInputData clientInputData)
        {
            transform.position += _networkPlayerState.MoveSpeed * clientInputData.MoveTarget.normalized;
            return new StateData()
            {
                Tick = clientInputData.Tick,
                Position = transform.position
            };
        }

        void Update()
        {
            _horizontalInput = Input.GetAxis("Horizontal");
            _verticalInput = Input.GetAxis("Vertical");
        }
    }
}
