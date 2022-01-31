using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace Client.Commands.CommandPoints
{
    public class TowerCommandPoint : CommandPoint
    {
        public override List<CommandData> GetAvailableCommands()
        {
            // Player can only do build commands for their own tower locations
            if (!IsOwner) return null;

            var commands = new List<CommandData>();
            foreach (CommandType commandType in Enum.GetValues(typeof(CommandType)))
            {
                commands.Add(new CommandData(commandType, new []{NetworkObjectId}));
            }

            return commands;
        }
    }
}