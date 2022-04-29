using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects.Player
{
    [CreateAssetMenu(fileName = "Commands", menuName = "MultiplayerSAT/Commands", order = 0)]
    public class Commands : ScriptableObject
    {
        public List<Command> AllCommands;
        
        public Dictionary<CommandType, Command> CommandTypeMap;
    }
    
    public enum CommandType
    {
        BUILD_TOY,
        BUILD_MUSIC,
        BUILD_LAUGHTER,
        WORK_GOAL,
    }
    
    [Serializable]
    public struct Command
    {
        public CommandType CommandType;
        public string CommandName;
        public Sprite CommandImage;
        public string CommandTooltip;
        public int CommandCost;
    }
}