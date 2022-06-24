using Server;
using Shared.Entity;
using Unity.Netcode;
using UnityEngine;

namespace Client.Avatar
{
    /*
     * Capture input data to send across network
     */
    public struct ClientInputData : INetworkSerializable
    {
        public int Tick;
        public ulong ClientId;
        public bool DoCommand;
        public Vector3 MoveTarget;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Tick);
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref DoCommand);
            serializer.SerializeValue(ref MoveTarget);
        }
    }

    /*
     * State to send and receive
     */
    public struct StateData
    {
        public int Tick;
        public Vector3 Position;
    }
    
    public class ClientInputProcessor : MonoBehaviour
    {
        private bool _commandRequested;
        private NetworkPlayerState _networkPlayerState;

        private StateData[] _stateBuffer;
        private ClientInputData[] _inputBuffer;
        private StateData _latestServerState;
        private StateData _lastProcessedState;
        private float _horizontalInput;
        private float _verticalInput;
        private const int BUFFER_SIZE = 1024;

        private bool _inputBlocked;

        private void Awake()
        {
            _networkPlayerState = GetComponent<NetworkPlayerState>();

            _inputBlocked = false;
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
        
        /*
         * On each server tick, update to latest state and handle input
         */
        private void HandleTick(int currentTick)
        {
            // Do nothing if script exists on non-player client or input is blocked
            if (!_networkPlayerState.IsOwner) return;
            if (_inputBlocked) return;

            // Populate input data
            var clientInputData = new ClientInputData()
            {
                Tick = currentTick, ClientId = _networkPlayerState.OwnerClientId,
                MoveTarget = new Vector3(_horizontalInput, _verticalInput, transform.position.z),
                DoCommand = _commandRequested
            };
            
            // If we are client, do client prediction and server reconciliation
            if (NetworkManager.Singleton.IsHost)
            {
                _networkPlayerState.SubmitPlayerInputServerRpc(clientInputData);
            }
            else
            {
                // If we receive a new state we haven't processed, reconcile
                if (!_latestServerState.Equals(default(StateData)) &&
                    (_lastProcessedState.Equals(default(StateData)) ||
                     !_latestServerState.Equals(_lastProcessedState)))
                {
                    HandleServerReconciliation(currentTick);
                }

                // Store input into buffer
                var bufferIndex = currentTick % BUFFER_SIZE;
                _inputBuffer[bufferIndex] = clientInputData;
                
                // Client prediction
                var newState = _networkPlayerState.ProcessInput(clientInputData);
                
                // Store predicted state into buffer
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

            // Only bother reconcile if difference in state is significant
            if (positionError <= 0.1f) return;

            // Set state to server state and apply consequent inputs
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

        public void BlockInput()
        {
            _inputBlocked = true;
        }

        public void UnblockInput()
        {
            _inputBlocked = false;
        }
    }
}
