using System;
using System.Collections.Generic;
using Shared.Entity;
using Unity.Netcode;
using UnityEngine;

namespace Client.Commands
{
    [Serializable]
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
        BUILD_LAUGHTER,
        WORK_GOAL,
    }

    [RequireComponent(typeof(PlayerOwnership))]
    public class CommandExecutor : NetworkBehaviour
    {
        [SerializeField]
        private List<CommandData> configuredCommandTypes;

        protected PlayerOwnership _playerOwner;

        private void Awake()
        {
            _playerOwner = GetComponent<PlayerOwnership>();
        }

        public virtual List<CommandData> GetAvailableCommands()
        {
            if (!_playerOwner.OwnedByPlayer) return new List<CommandData>();
            return configuredCommandTypes;
        }

        [ServerRpc(RequireOwnership = false)]
        public virtual void ExecuteCommandServerRpc(CommandData commandData)
        {
            return;
        }
    }
}
