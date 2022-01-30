using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Client.Commands
{
    public enum CommandType
    {
        BUILD_TOY,
        BUILD_MUSIC,
        BUILD_JOKE
    }

    public class CommandPoint : NetworkBehaviour
    {
        [SerializeField]
        private List<CommandType> configuredCommandTypes;
        
        public virtual List<CommandType> GetAvailableCommands()
        {
            return configuredCommandTypes;
        } 
    }
}
