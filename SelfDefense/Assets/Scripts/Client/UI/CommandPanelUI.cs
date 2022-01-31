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

        private CommandSensor m_commandSensor;

        private void Awake()
        {
            CommandSensor.CommandTypesChange += OnCommandTypesChange;
        }

        private void OnDestroy()
        {
            CommandSensor.CommandTypesChange -= OnCommandTypesChange;
        }

        private void OnCommandTypesChange(List<CommandData> availableCommands)
        {
            transform.RemoveAllChildGameObjects();

            foreach (CommandData commandData in availableCommands)
            {
                var go = Instantiate(commandButtonPrefab, transform);
                var commandButton = go.GetComponent<CommandButtonUI>();
                commandButton.Initialise(m_commandSensor, commandData);
            }
        }

        public void Initialise(CommandSensor commandSensor)
        {
            m_commandSensor = commandSensor;
        }
    }
}