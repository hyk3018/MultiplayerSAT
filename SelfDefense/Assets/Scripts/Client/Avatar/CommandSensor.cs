using System;
using System.Collections.Generic;
using Client.Commands;
using UnityEngine;

namespace Client.Avatar
{
    public class CommandSensor : MonoBehaviour
    {
        public event Action<List<CommandType>> CommandTypesChange;
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            var commandPoints = other.gameObject.GetComponents<CommandPoint>();
            var commandTypes = new List<CommandType>();

            foreach (CommandPoint commandPoint in commandPoints)
            {
                commandTypes.AddRange(commandPoint.GetAvailableCommands());
            }
            
            CommandTypesChange?.Invoke(commandTypes);
        }
    }
}