﻿using System;
using System.Collections;
using System.Collections.Generic;
using ScriptableObjects.Player;
using Shared.Entity;
using Shared.Entity.Towers;
using Unity.Netcode;
using UnityEngine;

namespace Client.Commands.CommandPoints
{
    [RequireComponent(typeof(PlayerGoal))]
    public class GoalCommandExecutor : CommandExecutor
    {
        [SerializeField]
        private List<Animation> buildAnimations;
        
        private PlayerGoal _playerGoal;
        private bool _buildingGoal;

        public event Action BuildingGoal;
        public event Action StoppedBuildingGoal;
        
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

            foreach (Animation animation in buildAnimations)
            {
                animation.gameObject.SetActive(true);
                animation.Play();
            }

            _playerGoal.GetComponent<AudioSource>().Play();
            StartCoroutine(StopBuildingGoalAfterTime());
        }

        private IEnumerator StopBuildingGoalAfterTime()
        {
            yield return new WaitForSeconds(_playerGoal.buildTime);
            _playerGoal.IncrementGoal();
            _buildingGoal = false;
            
            foreach (Animation animation in buildAnimations)
            {
                animation.gameObject.SetActive(false);
                animation.Stop();
            }

            StoppedBuildingGoal?.Invoke();
        }
    }
}