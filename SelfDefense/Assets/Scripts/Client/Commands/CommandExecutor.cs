using System;
using System.Collections.Generic;
using ScriptableObjects.Player;
using Shared.Entity;
using Unity.Netcode;
using UnityEngine;

namespace Client.Commands
{
    [Serializable]
    public struct CommandExecutionData : INetworkSerializable
    {
        public CommandType CommandType;
        public ulong[] TargetIds;

        public CommandExecutionData(CommandType commandType, ulong[] targetIds)
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

    [RequireComponent(typeof(PlayerOwnership))]
    public class CommandExecutor : NetworkBehaviour
    {
        [SerializeField]
        private List<CommandExecutionData> configuredCommandTypes;
        
        public event Action CommandsChanged;
        
        protected PlayerOwnership PlayerOwner;

        private void Awake()
        {
            PlayerOwner = GetComponent<PlayerOwnership>();
        }

        protected void ChangeCommand()
        {
            CommandsChanged?.Invoke();
        }

        public virtual List<CommandExecutionData> GetAvailableCommands()
        {
            return !PlayerOwner.OwnedByPlayer ? new List<CommandExecutionData>() : configuredCommandTypes;
        }

        [ServerRpc(RequireOwnership = false)]
        public virtual void ExecuteCommandServerRpc(CommandExecutionData commandExecutionData)
        {
            return;
        }

    }
}
