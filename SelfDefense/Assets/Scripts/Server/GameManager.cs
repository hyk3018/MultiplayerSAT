using System;
using TK.Core.Common;
using Unity.Netcode;
using UnityEngine;

namespace Server
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField]
        private GameObject playerSpawnPrefab;

        [SerializeField]
        private int minReady;
        
        public event Action<int> Tick;
        public event Action GameStarted;

        private float _timer;
        private int _currentTick;
        private float _tickPeriod;

        public NetworkVariable<int> readyCount;
        
        private bool _gameStarted;

        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(bool ready)
        {
            readyCount.Value = ready ? readyCount.Value + 1 : readyCount.Value - 1;
            readyCount.Value = Mathf.Max(0, readyCount.Value);
            if (readyCount.Value == minReady)
            {
                foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    var go = Instantiate(playerSpawnPrefab);
                    go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
                }
                StartGameAtTimeClientRpc(NetworkManager.ServerTime.Tick);
            }
        }

        [ClientRpc]
        private void StartGameAtTimeClientRpc(int tick)
        {
            _currentTick = NetworkManager.LocalTime.Tick;
            _gameStarted = true;
            _tickPeriod = 1f / NetworkManager.LocalTime.TickRate;
            GameStarted?.Invoke();
        }

        private void Update()
        {
            if (!_gameStarted) return;

            _timer += Time.deltaTime;
            while (_timer >= _tickPeriod)
            {
                _timer -= _tickPeriod;
                Tick?.Invoke(++_currentTick);
            }
        }
    }
}