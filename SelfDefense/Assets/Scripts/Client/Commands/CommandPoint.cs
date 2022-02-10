using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Client.Commands
{
    public struct CommandData : INetworkSerializable
    {
        public CommandType CommandType;
        public ulong[] TargetIds;

        public CommandData(CommandType commandType, ulong[] targetIds)
        {
            CommandType = commandType;
            TargetIds = targetIds;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref CommandType);

            if (TargetIds == null) return;
            
            int length = 0;
            if (!serializer.IsReader)
            {
                length = TargetIds.Length;
            }

            serializer.SerializeValue(ref length);
            if (serializer.IsReader)
            {
                TargetIds = new ulong[length];
            }

            for (int i = 0; i < TargetIds.Length; i++)
            {
                serializer.SerializeValue(ref TargetIds[i]);
            }
        }
    }
    
    public enum CommandType
    {
        BUILD_TOY,
        BUILD_MUSIC,
        BUILD_JOKE
    }

    public class CommandPoint : NetworkBehaviour
    {
        [SerializeField]
        private List<CommandData> configuredCommandTypes;
        
        public virtual List<CommandData> GetAvailableCommands()
        {
            return configuredCommandTypes;
        } 
    }
}
