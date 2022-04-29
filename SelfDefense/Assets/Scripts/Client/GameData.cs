using System.Collections.Generic;
using ScriptableObjects.Player;
using TK.Core.Common;
using UnityEngine;

namespace Client
{
    public class GameData : Singleton<GameData>
    {
        [SerializeField]
        private ScriptableObjects.Player.Commands Commands;
        
        private void Awake()
        {
            Commands.CommandTypeMap = new Dictionary<CommandType, Command>();
            foreach (var command in Commands.AllCommands)
            {
                Commands.CommandTypeMap[command.CommandType] = command;
            }
        }
    }
}