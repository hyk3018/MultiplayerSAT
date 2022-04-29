using System;
using System.Collections.Generic;
using ScriptableObjects.Player;
using ScriptableObjects.Towers;
using Shared.Entity;
using Shared.Entity.Towers;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Client.Commands.CommandPoints
{
    public class TowerCommandExecutor : CommandExecutor
    {
        [SerializeField]
        private TowerList towerList;

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
                case CommandType.BUILD_MUSIC:
                    tower = SpawnTower(TowerType.MUSIC);
                    break;
                case CommandType.BUILD_LAUGHTER:
                    tower = SpawnTower(TowerType.LAUGHTER);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (tower == null)
                return;
            DeactivateTowerSlotClientRpc();
        }

        [ClientRpc]
        private void DeactivateTowerSlotClientRpc()
        {
            gameObject.SetActive(false);
        }

        private GameObject SpawnTower(TowerType towerType)
        {
            var toyPrefab = towerList.GetTowerPrefabFromType(towerType);
            if (toyPrefab == null) return null;

            var go = Instantiate(toyPrefab);
            go.transform.position = transform.position;
            
            var playerOwnership = go.GetComponent<PlayerOwnership>();
            if (playerOwnership == null)
            {
                Debug.Log("PlayerOwnership component not found in tower prefab.");
                return null;
            }
            playerOwnership.OwnedPlayerIndex = _playerOwner.OwnedPlayerIndex;
            playerOwnership.OwnedByPlayer = _playerOwner.OwnedByPlayer;
            
            var networkObject = go.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.Log("NetworkObject component not found in tower prefab.");
                return null;
            }
            networkObject.Spawn();

            return go;
        }
    }
}