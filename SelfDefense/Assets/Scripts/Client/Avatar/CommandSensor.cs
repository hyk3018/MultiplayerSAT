using System;
using System.Collections.Generic;
using Client.Commands;
using UnityEngine;

namespace Client.Avatar
{
    public class CommandSensor : MonoBehaviour
    {
        public static event Action<List<CommandData>> CommandTypesChange;
        private ClientInputProcessor _clientInputProcessor;
        private List<CommandData> _currentCommandData;

        private void Awake()
        {
            _clientInputProcessor = GetComponent<ClientInputProcessor>();
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            var commandPoints = other.gameObject.GetComponents<CommandPoint>();
            var commands = GetCommandsFromCommandPoints(commandPoints);
            _currentCommandData = commands;
            CommandTypesChange?.Invoke(commands);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var commandPoints = other.gameObject.GetComponents<CommandPoint>();
            var commands = GetCommandsFromCommandPoints(commandPoints);
            CommandTypesChange?.Invoke(new List<CommandData>());
        }

        private static List<CommandData> GetCommandsFromCommandPoints(CommandPoint[] commandPoints)
        {
            var commands = new List<CommandData>();

            foreach (CommandPoint commandPoint in commandPoints)
            {
                commands.AddRange(commandPoint.GetAvailableCommands());
            }

            return commands;
        }

        public void RequestCommand(CommandData commandData) => _clientInputProcessor.RequestCommand(commandData);
    }
}