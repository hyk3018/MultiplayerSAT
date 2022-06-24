using System;
using System.Collections.Generic;
using ScriptableObjects.Player;
using Shared.Entity.Towers;
using Unity.Netcode;

namespace Client.Commands.CommandPoints
{
    /*
     * Implements change tower targetting behaviour
     */
    public class TowerTargetCommandExecutor : CommandExecutor
    {
        private TowerAttack _towerAttack;

        private void Start()
        {
            _towerAttack = GetComponentInChildren<TowerAttack>();
        }

        public override List<CommandExecutionData> GetAvailableCommands()
        {
            if (!PlayerOwner.OwnedByPlayer) return new List<CommandExecutionData>();
            
            switch (_towerAttack.TargetType)
            {
                case TowerTargetType.LAST:
                    return new List<CommandExecutionData>()
                    {
                        new CommandExecutionData(CommandType.RANGE_FIRST, null)
                    };
                case TowerTargetType.FIRST:
                    return new List<CommandExecutionData>()
                    {
                        new CommandExecutionData(CommandType.RANGE_CLOSE, null)
                    };
                case TowerTargetType.CLOSE:
                    return new List<CommandExecutionData>()
                    {
                        new CommandExecutionData(CommandType.RANGE_STRONG, null)
                    };
                case TowerTargetType.STRONG:
                    return new List<CommandExecutionData>()
                    {
                        new CommandExecutionData(CommandType.RANGE_LAST, null)
                    };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public override void ExecuteCommandServerRpc(CommandExecutionData commandExecutionData)
        {
            switch (commandExecutionData.CommandType)
            {
                case CommandType.RANGE_LAST:
                    _towerAttack.ChangeTargetTypeClientRpc(TowerTargetType.LAST);
                    break;
                case CommandType.RANGE_FIRST:
                    _towerAttack.ChangeTargetTypeClientRpc(TowerTargetType.FIRST);
                    break;
                case CommandType.RANGE_CLOSE:
                    _towerAttack.ChangeTargetTypeClientRpc(TowerTargetType.CLOSE);
                    break;
                case CommandType.RANGE_STRONG:
                    _towerAttack.ChangeTargetTypeClientRpc(TowerTargetType.STRONG);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            ChangedTargetClientRpc();
        }

        [ClientRpc]
        private void ChangedTargetClientRpc()
        {
            ChangeCommand();
        }
    }
}