using System;
using ScriptableObjects.Player;
using ScriptableObjects.Towers;
using Server;
using Shared.Entity;
using Shared.Entity.Towers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Client.Commands.CommandPoints
{
    public class TowerCommandExecutor : CommandExecutor
    {
        [SerializeField]
        private TowerList towerList;

        [FormerlySerializedAs("_baseTowerSlot")]
        [SerializeField]
        private GameObject baseTowerSlot;

        private void Initialise(GameObject parentSlot)
        {
            baseTowerSlot = parentSlot;
        }

        [ServerRpc(RequireOwnership = false)]
        public override void ExecuteCommandServerRpc(CommandExecutionData commandExecutionData)
        {
            Debug.Log("Execute!");
            
            GameObject tower;
            switch (commandExecutionData.CommandType)
            {
                case CommandType.BUILD_TOY:
                    tower = SpawnTower(TowerType.PLAY);
                    break;
                case CommandType.UPGRADE_TOY_CHILDHOOD:
                    tower = SpawnTower(TowerType.PLAY_CHILDHOOD);
                    break;
                case CommandType.BUILD_MUSIC:
                    tower = SpawnTower(TowerType.MUSIC);
                    break;
                case CommandType.UPGRADE_MUSIC_LOVESONG:
                    tower = SpawnTower(TowerType.MUSIC_LOVESONG);
                    break;
                case CommandType.BUILD_LAUGHTER:
                    tower = SpawnTower(TowerType.LAUGHTER);
                    break;
                case CommandType.REMOVE_TOWER:
                    ActivateBaseTowerSlotClientRpc();
                    GameManager.Instance
                        .RefundAffectionPointsServerRpc(
                            GetComponent<PlayerOwnership>().OwnedPlayerId,
                            GetComponent<Refundable>().RefundAmount);
                    Destroy(gameObject);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (tower == null)
                return;
            DeactivateTowerSlotClientRpc();
        }

        [ClientRpc]
        private void ActivateBaseTowerSlotClientRpc()
        {
            baseTowerSlot.SetActive(true);
        }

        [ClientRpc]
        private void DeactivateTowerSlotClientRpc()
        {
            gameObject.SetActive(false);
        }

        private GameObject SpawnTower(TowerType towerType)
        {
            var towerPrefab = towerList.GetTowerPrefabFromType(towerType);
            if (towerPrefab == null) return null;

            var go = Instantiate(towerPrefab);
            go.transform.position = transform.position;
            
            var networkObject = go.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.Log("NetworkObject component not found in tower prefab.");
                return null;
            }
            networkObject.Spawn();
            
            InitialiseTowerClientRpc(networkObject);

            return go;
        }

        [ClientRpc]
        private void InitialiseTowerClientRpc(NetworkObjectReference networkObjectReference)
        {
            if (networkObjectReference.TryGet(out NetworkObject networkObject))
            {
                var playerOwnership = networkObject.GetComponent<PlayerOwnership>();
                if (playerOwnership == null)
                {
                    Debug.Log("PlayerOwnership component not found in tower prefab.");
                    return;
                }
                playerOwnership.OwnedPlayerIndex = PlayerOwner.OwnedPlayerIndex;
                playerOwnership.OwnedByPlayer = PlayerOwner.OwnedByPlayer;
                playerOwnership.OwnedPlayerId = PlayerOwner.OwnedPlayerId;

                var towerCommandExecutor = networkObject.GetComponent<TowerCommandExecutor>();
                if (towerCommandExecutor == null)
                {
                    Debug.Log("Tower missing command executor");
                    return;
                }

                towerCommandExecutor.Initialise(baseTowerSlot);
            }
        }
    }
}