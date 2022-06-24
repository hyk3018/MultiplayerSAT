using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Player;
using Shared.Entity;
using Unity.Netcode;
using UnityEngine;

namespace Client.Commands.CommandPoints
{
    /*
     * Implements working on goal behaviour with animations
     */
    [RequireComponent(typeof(PlayerGoal))]
    public class GoalCommandExecutor : CommandExecutor
    {
        [SerializeField]
        private List<Animation> buildAnimations;
        
        public event Action BuildingGoal;
        public event Action StoppedBuildingGoal;
        
        private PlayerGoal _playerGoal;
        private bool _buildingGoal;

        
        private void Start()
        {
            _playerGoal = GetComponent<PlayerGoal>();
        }

        public override List<CommandExecutionData> GetAvailableCommands()
        {
            if (_buildingGoal || !_playerGoal.CanWorkOnGoal)
                return new List<CommandExecutionData>();
            
            return base.GetAvailableCommands();
        }

        [ServerRpc(RequireOwnership = false)]
        public override void ExecuteCommandServerRpc(CommandExecutionData commandExecutionData)
        {
            switch (commandExecutionData.CommandType)
            {
                case CommandType.WORK_GOAL:
                    WorkOnGoalClientRPC();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [ClientRpc]
        private void WorkOnGoalClientRPC()
        {
            BuildingGoal?.Invoke();
            _buildingGoal = true;
            ChangeCommand();

            foreach (Animation anim in buildAnimations)
            {
                anim.gameObject.SetActive(true);
                anim.Play();
            }

            _playerGoal.GetComponent<AudioSource>().Play();
            StartCoroutine(StopBuildingGoalAfterTime());
        }

        /*
         * Only update goal variables after animation time finishes
         */
        private IEnumerator StopBuildingGoalAfterTime()
        {
            yield return new WaitForSeconds(_playerGoal.buildTime);
            _playerGoal.IncrementGoal();
            _buildingGoal = false;
            ChangeCommand();
            
            foreach (Animation anim in buildAnimations)
            {
                anim.gameObject.SetActive(false);
                anim.Stop();
            }

            StoppedBuildingGoal?.Invoke();
        }
    }
}