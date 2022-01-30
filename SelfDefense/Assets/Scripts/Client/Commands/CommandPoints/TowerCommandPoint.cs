using System.Collections.Generic;
using Unity.Netcode;

namespace Client.Commands.CommandPoints
{
    public class TowerCommandPoint : CommandPoint
    {
        public override List<CommandType> GetAvailableCommands()
        {
            // Player can only do build commands for their own tower locations
            if (IsOwner) return null;
            
            var commands = new List<CommandType>()
            {
                CommandType.BUILD_TOY,
                CommandType.BUILD_JOKE,
                CommandType.BUILD_MUSIC
            };

            return commands;
        }
    }
}