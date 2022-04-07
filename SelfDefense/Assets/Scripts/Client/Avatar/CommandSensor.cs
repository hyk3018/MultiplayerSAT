using System;
using System.Collections.Generic;
using Client.Commands;
using Unity.Netcode;
using UnityEngine;

namespace Client.Avatar
{
    public class CommandSensor : NetworkBehaviour
    {
        public static event Action<Dictionary<CommandExecutor, List<CommandData>>> CommandTypesChange;
        private ClientInputProcessor _clientInputProcessor;
        private Dictionary<CommandExecutor, List<CommandData>> _currentCommandData;

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
            var commands = GetCommandsFromCommandPoints(commandPoints);
            CommandTypesChange?.Invoke(new Dictionary<CommandExecutor, List<CommandData>>());
        }

        private static Dictionary<CommandExecutor, List<CommandData>> GetCommandsFromCommandPoints(CommandExecutor[] commandExecutors)
        {
            var executorCommands = new Dictionary<CommandExecutor, List<CommandData>>();

            foreach (CommandExecutor commandExecutor in commandExecutors)
            {
                var commands = new List<CommandData>();
                commands.AddRange(commandExecutor.GetAvailableCommands());
                executorCommands.Add(commandExecutor, commands);
            }

            return executorCommands;
        }
    }
}