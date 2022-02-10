using System;
using Client.Commands;
using HelloWorld;
using Server;
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

        private StateData[] _stateBuffer;
        private ClientInputData[] _inputBuffer;
        private StateData _latestServerState;
        private StateData _lastProcessedState;
        private float _horizontalInput;
        private float _verticalInput;
        private const int BUFFER_SIZE = 1024;

        private int tick;

        private void Awake()
        {
            _networkPlayerState = GetComponent<NetworkPlayerState>();
            _mainCamera = Camera.main;
            
            _stateBuffer = new StateData[BUFFER_SIZE];
            _inputBuffer = new ClientInputData[BUFFER_SIZE];
            
            _networkPlayerState.ServerStateReceived += OnServerStateReceived;
            GameManager.Instance.Tick += HandleTick;
        }
        
        private void OnDestroy()
        {
            _networkPlayerState.ServerStateReceived -= OnServerStateReceived;
            GameManager.Instance.Tick -= HandleTick;
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
        
        private void HandleTick(int currentTick)
        {
            if (!_networkPlayerState.IsOwner) return;

            // Populate input data
            var clientInputData = new ClientInputData()
            {
                Tick = currentTick, ClientID = _networkPlayerState.OwnerClientId,
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
                    HandleServerReconciliation(currentTick);
                }

                var bufferIndex = currentTick % BUFFER_SIZE;
                _inputBuffer[bufferIndex] = clientInputData;
                var newState = _networkPlayerState.ProcessInput(clientInputData);
                transform.position =newState.Position;
                _stateBuffer[bufferIndex] = newState;

                _networkPlayerState.SubmitPlayerInputServerRpc(clientInputData);
            }

            _commandRequested = false;
        }

        private void HandleServerReconciliation(int currentTick)
        {
            _lastProcessedState = _latestServerState;

            var serverStateBufferIndex = _latestServerState.Tick % BUFFER_SIZE;
            var positionError =
                Vector3.Distance(_latestServerState.Position, _stateBuffer[serverStateBufferIndex].Position);

            if (positionError <= 0.1f) return;

            Debug.Log("Needs correction");
            transform.position = _latestServerState.Position;
            _stateBuffer[serverStateBufferIndex] = _latestServerState;

            var tickToProcess = _latestServerState.Tick + 1;

            while (tickToProcess < currentTick)
            {
                var state = _networkPlayerState.ProcessInput(_inputBuffer[tickToProcess % BUFFER_SIZE]);
                transform.position = state.Position;
                _stateBuffer[tickToProcess % BUFFER_SIZE] = state;
                tickToProcess++;
            }
        }

        void Update()
        {
            _horizontalInput = Input.GetAxis("Horizontal");
            _verticalInput = Input.GetAxis("Vertical");
        }
    }
}
