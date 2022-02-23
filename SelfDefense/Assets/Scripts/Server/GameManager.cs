using System;
using Server.EnemySpawning;
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

        [SerializeField]
        private ServerEnemySpawner enemySpawner;
        
        public event Action<int> Tick;
        public event Action<ulong, ulong> GameStarted;

        private float _timer;
        private int _currentTick;
        private float _tickPeriod;

        public NetworkVariable<int> readyCount;
        
        private bool _gameStarted;

        private void Awake()
        {
            readyCount.OnValueChanged += StartGameIfReady;
        }

        private void StartGameIfReady(int value, int newValue)
        {
            if (!IsHost) return;
            if (newValue == minReady)
            {
                foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    var go = Instantiate(playerSpawnPrefab);
                    go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
                }
                
                var clientIds = NetworkManager.Singleton.ConnectedClientsIds;
                var player1IdIndex = clientIds[0] == NetworkManager.Singleton.LocalClientId ? 0 : 1;
                var player2Id = clientIds.Count == 2 ? clientIds[1 - player1IdIndex] : 0;

                StartGameAtTimeClientRpc(clientIds[player1IdIndex], player2Id);
                enemySpawner.SpawnNextWave();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetReadyServerRpc(bool ready)
        {
            readyCount.Value = Mathf.Max(0, ready ? readyCount.Value + 1 : readyCount.Value - 1);
        }

        [ClientRpc]
        private void StartGameAtTimeClientRpc(ulong player1Id, ulong player2Id)
        {
            _currentTick = NetworkManager.LocalTime.Tick;
            _gameStarted = true;
            _tickPeriod = 1f / NetworkManager.LocalTime.TickRate;

            GameStarted?.Invoke(player1Id, player2Id);
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

        [ServerRpc(RequireOwnership = false)]
        public void RetrieveStartingPlayerCountServerRpc()
        {
            SendStartingPlayerCountClientRpc();
        }

        [ClientRpc]
        private void SendStartingPlayerCountClientRpc()
        {
            readyCount.OnValueChanged?.Invoke(readyCount.Value, readyCount.Value);
        }
    }
}