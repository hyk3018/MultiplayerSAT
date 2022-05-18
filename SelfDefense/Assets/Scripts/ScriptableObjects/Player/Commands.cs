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
        UPGRADE_TOY_CHILDHOOD,
        UPGRADE_MUSIC_LOVESONG
    }
    
    [Serializable]
    public struct Command
    {
        public CommandType Type;
        public string Name;
        public Sprite Image;
        public string Tooltip;
        public int Cost;
    }
}