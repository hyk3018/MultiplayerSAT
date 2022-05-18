using System;
using System.Collections;
using System.Collections.Generic;
using Client.Avatar;
using Client.Commands.CommandPoints;
using Client.UI;
using ScriptableObjects.Player;
using Server.EnemySpawning;
using Shared.Entity;
using TK.Core.Common;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

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

    public enum GameState
    {
        PREP,
        PLAYING
    }
    
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField]
        private GameObject playerSpawnPrefab, childhoodSelfSpawnPrefab;

        [SerializeField]
        private ServerEnemySpawner enemySpawner;

        [SerializeField]
        private WaveReadyUI waveReadyUI;

        [SerializeField]
        private Animator countdownAnimation, wavePromptAnimation;

        [SerializeField]
        private TextMeshProUGUI wavePrompt;

        [SerializeField]
        private AudioSource countdownSound;

        [SerializeField]
        private ReadyListener joinGameReadyListener;

        [SerializeField]
        private GameObject gameBackgroundMask;
        
        [SerializeField]
        private Transform player1ChildhoodSelfSpawn, player2ChildhoodSelfSpawn;

        [SerializeField]
        private PlayerUI player1UI, player2UI;
        
        [SerializeField]
        private PlayerGoal player1Goal, player2Goal;

        [SerializeField]
        private GameObject firstWinScreen, firstLoseScreen, winWinScreen, loseLoseScreen, winLoseScreen;
        
        [SerializeField]
        private PlayableCharacters playableCharacters;


        [FormerlySerializedAs("minReady")]
        [SerializeField]
        public int MinReady;
        
        public BoxCollider2D mapBounds;
        public event Action<int> Tick;
        public event Action<ulong, ulong> GameStarted;
        public GameState GameState;
        public NetworkObject LocalPlayer;

        private float _timer;
        private int _currentTick;
        private float _tickPeriod;
        private bool _gameStarted;
        private Dictionary<ulong, Tuple<int, int>> _playerCustomization;
        private Dictionary<ulong, bool> _finishedStatuses;
        private ulong _player1Id, _player2Id;
        private Dictionary<ulong, GameObject>  _playerGos, _childhoodSelves;
        private static readonly int PlayCountdown = Animator.StringToHash("PlayCountdown");
        private static readonly int Show = Animator.StringToHash("Show");
        private static readonly int Hide = Animator.StringToHash("Hide");

        private void Awake()
        {
            _playerCustomization = new Dictionary<ulong, Tuple<int, int>>();
            _finishedStatuses = new Dictionary<ulong, bool>();
            _playerGos = new Dictionary<ulong, GameObject>();
            _childhoodSelves = new Dictionary<ulong, GameObject>();
            
            GameState = GameState.PREP;

            enemySpawner.NoEnemiesRemaining += OnNoEnemiesRemainClientRpc;
        }

        #region Start Game

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
            if (newValue == MinReady)
            {
                var clientIds = NetworkManager.Singleton.ConnectedClientsIds;
                var player1IdIndex = clientIds[0] == NetworkManager.Singleton.LocalClientId ? 0 : 1;
                _player1Id = clientIds[player1IdIndex];
                _player2Id = clientIds.Count == 2 ? clientIds[1 - player1IdIndex] : 0;

                var idToPlayerData = new Dictionary<ulong, PlayerSpawnData>();
                idToPlayerData[_player2Id] = new PlayerSpawnData(_player2Id, _playerCustomization[_player2Id].Item1,
                    _playerCustomization[_player2Id].Item2, false);
                idToPlayerData[_player1Id] = new PlayerSpawnData(_player1Id, _playerCustomization[_player1Id].Item1,
                    _playerCustomization[_player1Id].Item2, true);
            
                
                foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    InitialisePlayerGame(idToPlayerData[clientId]);
                }

                player1Goal.SetGoalEnabledClientRpc(false);
                player2Goal.SetGoalEnabledClientRpc(false);
                
                player1Goal.GoalReached += () =>
                {
                    OnGameEnd(_player1Id, true);
                };
                player2Goal.GoalReached += () =>
                {
                    OnGameEnd(_player2Id, true);
                };
                
                StartGameAtTimeClientRpc(_player1Id, _player2Id);
            }
        }

        private void OnGameEnd(ulong playerId, bool goalReached)
        {
            Destroy(_playerGos[playerId]);
            Destroy(_childhoodSelves[playerId]);
            
            ulong otherId = playerId == _player1Id ? _player2Id : _player1Id;

            DestroyPlayerUIClientRpc(playerId);
            
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[]{playerId}
                }
            };
            
            if (_finishedStatuses.TryGetValue(otherId, out var otherReached))
            {
                if (goalReached)
                {
                    ShowEndScreenClientRpc(otherReached ? "ww" : "wl");
                }
                else
                {
                    ShowEndScreenClientRpc(otherReached ? "wl" : "ll");
                }
            }
            else
            {
                _finishedStatuses[playerId] = goalReached;
                ShowEndScreenClientRpc(goalReached ? "w" : "l", 
                    clientRpcParams);
            }
        }

        [ClientRpc]
        private void DestroyPlayerUIClientRpc(ulong playerId)
        {
            Destroy(playerId == _player1Id ? player1UI.gameObject : player2UI.gameObject);
        }

        [ClientRpc]
        private void ShowEndScreenClientRpc(String endStatus, ClientRpcParams clientRpcParams = default)
        {
            gameBackgroundMask.SetActive(true);
            switch (endStatus)
            {
                case "w":
                    firstWinScreen.SetActive(true);
                    break;
                case "l":
                    firstLoseScreen.SetActive(true);
                    break;
                case "ww":
                    winWinScreen.SetActive(true);
                    break;
                case "wl":
                    winLoseScreen.SetActive(true);
                    break;
                case "ll":
                    loseLoseScreen.SetActive(true);
                    break;
                default:
                    return;
            }
        }

        private void InitialisePlayerGame(PlayerSpawnData spawnData)
        {
            var playerGo = Instantiate(playerSpawnPrefab);
            playerGo.GetComponent<NetworkObject>().SpawnAsPlayerObject(spawnData.PlayerId);
            _playerGos[spawnData.PlayerId] = playerGo;
            
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
                playableCharacters.Sprites[spawnData.SpriteIndex].Right;
            childhoodSelfGo.GetComponent<SpriteRenderer>().sprite =
                playableCharacters.Sprites[spawnData.SpriteIndex].Right;

            var playerAffectionPoints = playerGo.GetComponent<AffectionPoints>();
            childhoodSelfGo.GetComponent<ChildhoodSelf>().Initialise(playerAffectionPoints);
            
            playerGo.transform.position =
                spawnData.IsHostPlayer ? player1ChildhoodSelfSpawn.position : player2ChildhoodSelfSpawn.position;
            childhoodSelfGo.transform.position =
                spawnData.IsHostPlayer ? player1ChildhoodSelfSpawn.position : player2ChildhoodSelfSpawn.position;
            
            // Customise goal sprite
            var playerGoal = spawnData.IsHostPlayer ? player1Goal : player2Goal;
            playerGoal.Initialise(playableCharacters.Goals[spawnData.GoalIndex]);

            var playerUI = spawnData.IsHostPlayer ? player1UI : player2UI;
            playerUI.Initialise(playerGo, childhoodSelfGo, playerGoal);
            
            player1UI.gameObject.SetActive(true);
            player2UI.gameObject.SetActive(true);
            
            // Code specific to server

            if (IsServer)
            {
                playerAffectionPoints.Initialise();
            }
            
            // Code specific to owner object
            
            if (!playerGo.IsOwner) return;

            LocalPlayer = playerGo;
            var commandExecutor = playerGoal.GetComponent<GoalCommandExecutor>();
            commandExecutor.BuildingGoal += () =>
            {
                if (!playerGo) return;
                var clientInputProcessor = playerGo.GetComponent<ClientInputProcessor>();
                clientInputProcessor.BlockInput();
            };
            commandExecutor.StoppedBuildingGoal += () =>
            {
                if (!playerGo) return;
                var clientInputProcessor = playerGo.GetComponent<ClientInputProcessor>();
                clientInputProcessor.UnblockInput();
            };
            
            waveReadyUI.ReadyUpForNewWave();
            wavePrompt.text = enemySpawner.GetNextSpawnPrompt();
            wavePromptAnimation.SetTrigger(Show);
        }
        
        private GameObject SpawnChildhoodSelf(ulong clientId)
        {
            var childhoodSelfGo = Instantiate(childhoodSelfSpawnPrefab);
            _childhoodSelves[clientId] = childhoodSelfGo;
            
            childhoodSelfGo.GetComponent<NetworkObject>().Spawn();
            childhoodSelfGo.GetComponent<ChildhoodSelf>().Distressed += () =>
            {
                OnGameEnd(clientId, false);
            };
            
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

            gameBackgroundMask.SetActive(false);
            GameStarted?.Invoke(player1Id, player2Id);
        }
        
        #endregion

        #region Wave Ready Management

        [ClientRpc]
        private void OnNoEnemiesRemainClientRpc(bool finishedGame)
        {
            GameState = GameState.PREP;
            if (finishedGame) return;

            waveReadyUI.ReadyUpForNewWave();
            Debug.Log("Show prompt");
            wavePrompt.text = enemySpawner.GetNextSpawnPrompt();
            wavePromptAnimation.SetTrigger(Show);

            if (!IsHost) return;
            
            player1Goal.SetGoalEnabledClientRpc(false);
            player2Goal.SetGoalEnabledClientRpc(false);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void StartNextWaveServerRpc()
        {
            player1Goal.SetGoalEnabledClientRpc(true);
            player2Goal.SetGoalEnabledClientRpc(true);
            enemySpawner.SpawnNextWave();
            GameState = GameState.PLAYING;
        }
                
        [ServerRpc]
        public void StartWaveServerRpc()
        {
            StartCoroutine(StartGameCountdown());
        }

        [ClientRpc]
        private void StartWaveCountdownClientRpc()
        {
            wavePromptAnimation.SetTrigger(Hide);
            countdownAnimation.SetTrigger(PlayCountdown);
            countdownSound.Play();
        }

        private IEnumerator StartGameCountdown()
        {
            StartWaveCountdownClientRpc();
            yield return new WaitForSeconds(3);
            StartNextWaveServerRpc();
        }
        
        #endregion
        
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