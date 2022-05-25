using System;
using System.Collections.Generic;
using System.Linq;
using Client.Commands;
using Unity.Netcode;
using UnityEngine;

namespace Client.Avatar
{
    public class CommandSensor : NetworkBehaviour
    {
        public static event Action<Dictionary<CommandExecutor, List<CommandExecutionData>>> CommandTypesChange;

        private HashSet<CommandExecutor> _executorsInRange;

        private void Awake()
        {
            _executorsInRange = new HashSet<CommandExecutor>();
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsOwner) return;
            
            foreach (var commandExecutor in other.gameObject.GetComponents<CommandExecutor>())
            {
                _executorsInRange.Add(commandExecutor);
                commandExecutor.CommandsChanged += OnCommandsChanged;
            }
            
            CommandTypesChange?.Invoke(GetCommandsFromCommandPoints());
        }

        private void OnCommandsChanged()
        {
            CommandTypesChange?.Invoke(GetCommandsFromCommandPoints()); 
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsOwner) return;
            
            foreach (var commandExecutor in other.gameObject.GetComponents<CommandExecutor>())
            {
                _executorsInRange.Remove(commandExecutor);
                commandExecutor.CommandsChanged -= OnCommandsChanged;
            }
            
            CommandTypesChange?.Invoke(GetCommandsFromCommandPoints());
        }

        private  Dictionary<CommandExecutor, List<CommandExecutionData>> GetCommandsFromCommandPoints()
        {
            var executorCommands = new Dictionary<CommandExecutor, List<CommandExecutionData>>();
            
            foreach (var commandExecutor in _executorsInRange)
            {
                var commands = new List<CommandExecutionData>();
                commands.AddRange(commandExecutor.GetAvailableCommands());
                executorCommands.Add(commandExecutor, commands);
            }

            return executorCommands;
        }
    }
}