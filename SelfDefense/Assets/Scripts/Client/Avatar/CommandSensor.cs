using System;
using System.Collections.Generic;
using Client.Commands;
using Unity.Netcode;
using UnityEngine;

namespace Client.Avatar
{
    public class CommandSensor : NetworkBehaviour
    {
        public static event Action<Dictionary<CommandExecutor, List<CommandExecutionData>>> CommandTypesChange;
        private ClientInputProcessor _clientInputProcessor;
        private Dictionary<CommandExecutor, List<CommandExecutionData>> _currentCommandData;

        private void Awake()
        {
            _clientInputProcessor = GetComponent<ClientInputProcessor>();
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsOwner) return;
            
            var commandPoints = other.gameObject.GetComponents<CommandExecutor>();
            var commands = GetCommandsFromCommandPoints(commandPoints);
            _currentCommandData = commands;
            CommandTypesChange?.Invoke(_currentCommandData);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsOwner) return;
            
            var commandPoints = other.gameObject.GetComponents<CommandExecutor>();

            if (commandPoints == null || commandPoints.Length == 0) return;
            
            CommandTypesChange?.Invoke(new Dictionary<CommandExecutor, List<CommandExecutionData>>());
        }

        private static Dictionary<CommandExecutor, List<CommandExecutionData>> GetCommandsFromCommandPoints(CommandExecutor[] commandExecutors)
        {
            var executorCommands = new Dictionary<CommandExecutor, List<CommandExecutionData>>();

            foreach (CommandExecutor commandExecutor in commandExecutors)
            {
                var commands = new List<CommandExecutionData>();
                commands.AddRange(commandExecutor.GetAvailableCommands());
                executorCommands.Add(commandExecutor, commands);
            }

            return executorCommands;
        }
    }
}