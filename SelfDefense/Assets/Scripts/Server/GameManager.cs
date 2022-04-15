using System;
using System.Collections.Generic;
using Client.Avatar;
using Client.Commands.CommandPoints;
using Client.UI;
using ScriptableObjects.Player;
using Server.EnemySpawning;
using Shared.Entity;
using TK.Core.Common;
using Unity.Netcode;
using UnityEngine;

namespace Server
{
    public struct PlayerSpawnData : INetworkSerializable
    {
        public ulong PlayerId;
        public int SpriteIndex;
        public int GoalIndex;
        public bool IsHostPlayer;

        public PlayerSpawnData(ulong playerId, int spriteIndex, int goalIndex, bool isHostPlayer)
        {
            PlayerId = playerId;
            SpriteIndex = spriteIndex;
            GoalIndex = goalIndex;
            IsHostPlayer = isHostPlayer;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerId);
            serializer.SerializeValue(ref SpriteIndex);
            serializer.SerializeValue(ref GoalIndex);
            serializer.SerializeValue(ref IsHostPlayer);
        }
    }
    
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField]
        private GameObject playerSpawnPrefab, childhoodSelfSpawnPrefab;

        [SerializeField]
        private int minReady;

        [SerializeField]
        private ServerEnemySpawner enemySpawner;

        [SerializeField]
        private ReadyListener joinGameReadyListener;

        [SerializeField]
        private Transform player1ChildhoodSelfSpawn, player2ChildhoodSelfSpawn;

        [SerializeField]
        private PlayerGoal player1Goal, player2Goal;

        [SerializeField]
        private PlayableCharacters playableCharacters;

        public BoxCollider2D mapBounds;
        public event Action<int> Tick;
        public event Action<ulong, ulong> GameStarted;

        private float _timer;
        private int _currentTick;
        private float _tickPeriod;
        private bool _gameStarted;
        private Dictionary<ulong, Tuple<int, int>> _playerCustomization;

        private void Awake()
        {
            _playerCustomization = new Dictionary<ulong, Tuple<int, int>>();
        }

        public void RegisterListenToReadyUp()
        {
            if (!IsHost) return;
            joinGameReadyListener.readyCount.OnValueChanged += StartGameIfReadyServerRpc;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void StartGameIfReadyServerRpc(int value, int newValue)
        {
            if (!IsHost) return;
            Debug.Log("Start game if ready");
            if (newValue == minReady)
            {
                var clientIds = NetworkManager.Singleton.ConnectedClientsIds;
                var player1IdIndex = clientIds[0] == NetworkManager.Singleton.LocalClientId ? 0 : 1;
                var player1Id = clientIds[player1IdIndex];
                var player2Id = clientIds.Count == 2 ? clientIds[1 - player1IdIndex] : 0;

                var idToPlayerData = new Dictionary<ulong, PlayerSpawnData>();
                idToPlayerData[player2Id] = new PlayerSpawnData(player2Id, _playerCustomization[player2Id].Item1,
                    _playerCustomization[player2Id].Item2, false);
                idToPlayerData[player1Id] = new PlayerSpawnData(player1Id, _playerCustomization[player1Id].Item1,
                    _playerCustomization[player1Id].Item2, true);
            
                
                foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    InitialisePlayerGame(idToPlayerData[clientId]);
                }

                StartGameAtTimeClientRpc(player1Id, player2Id);
                enemySpawner.SpawnNextWave();
            }
        }

        private void InitialisePlayerGame(PlayerSpawnData spawnData)
        {
            var playerGo = Instantiate(playerSpawnPrefab);
            playerGo.GetComponent<NetworkObject>().SpawnAsPlayerObject(spawnData.PlayerId);

            var childhoodSelfGo = SpawnChildhoodSelf(spawnData.PlayerId);

            InitialisePlayerClientRpc(spawnData, playerGo.GetComponent<NetworkObject>(),
                childhoodSelfGo.GetComponent<NetworkObject>());
        }

        [ClientRpc]
        private void InitialisePlayerClientRpc(PlayerSpawnData spawnData,
            NetworkObjectReference playerGoRef, NetworkObjectReference childhoodSelfGoRef)
        {
            if (!playerGoRef.TryGet(out var playerGo) 
                || !childhoodSelfGoRef.TryGet(out var childhoodSelfGo)) return;
            
            playerGo.GetComponent<SpriteRenderer>().sprite =
                playableCharacters.Sprites[spawnData.SpriteIndex].Idle[0];
            childhoodSelfGo.GetComponent<SpriteRenderer>().sprite =
                playableCharacters.Sprites[spawnData.SpriteIndex].Idle[0];
            
            playerGo.transform.position =
                spawnData.IsHostPlayer ? player1ChildhoodSelfSpawn.position : player2ChildhoodSelfSpawn.position;
            childhoodSelfGo.transform.position =
                spawnData.IsHostPlayer ? player1ChildhoodSelfSpawn.position : player2ChildhoodSelfSpawn.position;
            
            // Customise goal sprite
            var playerGoal = spawnData.IsHostPlayer ? player1Goal : player2Goal;
            playerGoal.Initialise(playableCharacters.Goals[spawnData.GoalIndex]);

            if (!playerGo.IsOwner) return;
            
            var commandExecutor = playerGoal.GetComponent<GoalCommandExecutor>();
            commandExecutor.BuildingGoal += () =>
            {
                var clientInputProcessor = playerGo.GetComponent<ClientInputProcessor>();
                clientInputProcessor.BlockInput();
            };
            commandExecutor.StoppedBuildingGoal += () =>
            {
                var clientInputProcessor = playerGo.GetComponent<ClientInputProcessor>();
                clientInputProcessor.UnblockInput();
            };
        }

        private GameObject SpawnChildhoodSelf(ulong clientId)
        {
            var childhoodSelfGo = Instantiate(childhoodSelfSpawnPrefab);
            childhoodSelfGo.GetComponent<NetworkObject>().Spawn();
            return childhoodSelfGo;
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetPlayerCustomizationServerRpc(ulong playerId, int spriteDataIndex, int goalDataIndex)
        {
            Debug.Log("Customise player ID" + playerId);
            _playerCustomization[playerId] = new Tuple<int, int>(spriteDataIndex, goalDataIndex);
        }

        [ClientRpc]
        private void StartGameAtTimeClientRpc(ulong player1Id, ulong player2Id)
        {
            Debug.Log("Game start");
            
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

        // [ServerRpc(RequireOwnership = false)]
        // public void RetrieveStartingPlayerCountServerRpc()
        // {
        //     SendStartingPlayerCountClientRpc();
        // }
        //
        // [ClientRpc]
        // private void SendStartingPlayerCountClientRpc()
        // {
        //     joinGameReadyListener.readyCount.OnValueChanged?.Invoke(readyCount.Value, readyCount.Value);
        // }
    }
}