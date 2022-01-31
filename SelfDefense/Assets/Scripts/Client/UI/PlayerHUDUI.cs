using System;
using Client.Avatar;
using Shared.Entity;
using Unity.Netcode;
using UnityEngine;

namespace Client.UI
{
    public class PlayerHUDUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject commandsUIPrefab;
        
        private void Awake()
        {
            NetworkPlayerState.PlayerSpawned += OnPlayerSpawn;
        }

        private void OnDestroy()
        {
            NetworkPlayerState.PlayerSpawned -= OnPlayerSpawn;
        }

        // When a player object spawns we need to create necessary HUD
        private void OnPlayerSpawn(NetworkObject playerObject)
        {
            if (playerObject.IsOwner)
            {
                var commandPanelUI = Instantiate(commandsUIPrefab, transform)
                    .GetComponent<CommandPanelUI>();

                commandPanelUI.Initialise(playerObject.GetComponent<CommandSensor>());
            }
        }
    }
}
