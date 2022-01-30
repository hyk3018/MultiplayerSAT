using System.Collections.Generic;
using Client.Avatar;
using Client.Commands;
using TK.Core.Common;
using Unity.Netcode;
using UnityEngine;

namespace Client.UI
{
    public class CommandPanelUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject commandButtonPrefab;
    
        private void Awake()
        {
            NetworkManager.Singleton
                .LocalClient.PlayerObject.GetComponent<CommandSensor>()
                .CommandTypesChange += OnCommandTypesChange;
        }

        private void OnCommandTypesChange(List<CommandType> obj)
        {
            transform.RemoveAllChildGameObjects();

            foreach (CommandType commandType in obj)
            {
                var go = Instantiate(commandButtonPrefab, transform);
                var commandButton = go.GetComponent<CommandButtonUI>();
                commandButton.Initialise(commandType);
            }
        }
    }
}