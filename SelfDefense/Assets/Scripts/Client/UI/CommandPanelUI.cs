using System;
using System.Collections.Generic;
using Client.Avatar;
using Client.Commands;
using Shared.Entity;
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
            CommandSensor.CommandTypesChange += OnCommandTypesChange;
        }

        private void OnDestroy()
        {
            CommandSensor.CommandTypesChange -= OnCommandTypesChange;
        }

        private void OnCommandTypesChange(Dictionary<CommandExecutor, List<CommandData>> availableCommands)
        {
            transform.RemoveAllChildGameObjects();

            foreach (CommandExecutor executor in availableCommands.Keys)
            {
                foreach (CommandData commandData in availableCommands[executor])
                {
                    var go = Instantiate(commandButtonPrefab, transform);
                    var commandButton = go.GetComponent<CommandButtonUI>();
                    commandButton.Initialise(executor, commandData);
                }
            }
        }
    }
}