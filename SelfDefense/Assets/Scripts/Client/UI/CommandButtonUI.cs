using System;
using System.Collections.Generic;
using Client.Avatar;
using Client.Commands;
using Server;
using Shared.Entity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Client.UI
{
    public class CommandButtonUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI buttonText, affectionCostText;

        [SerializeField]
        private GameObject affordabilityMask;

        [SerializeField]
        private ScriptableObjects.Player.Commands Commands;
        
        private Image _image;
        private Button _button;
        private CommandExecutionData _currentCommandExecutionData;
        private AffectionPoints _affectionPoints;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
        }

        private void OnDestroy()
        {
            if (_affectionPoints.Points.OnValueChanged != null)
                _affectionPoints.Points.OnValueChanged -= OnAffectionChange;
        }

        public void Initialise(CommandExecutor executor, CommandExecutionData commandExecutionData)
        {
            _button.onClick.AddListener(() =>
            {
                Debug.Log("Button pressed!");
                executor.ExecuteCommandServerRpc(_currentCommandExecutionData);

                if (_affectionPoints)
                {
                    _affectionPoints.SpendPointsServerRpc(Commands
                        .CommandTypeMap[_currentCommandExecutionData.CommandType].Cost);
                }
            });
        
            _currentCommandExecutionData = commandExecutionData;
            
            var command = Commands.CommandTypeMap[commandExecutionData.CommandType];

            _affectionPoints = GameManager.Instance.LocalPlayer.GetComponent<AffectionPoints>();

            OnAffectionChange(_affectionPoints.Points.Value, _affectionPoints.Points.Value);
            _affectionPoints.Points.OnValueChanged += OnAffectionChange;
            
            _image.sprite = command.Image;
            buttonText.text = command.Name;
            affectionCostText.text = command.Cost.ToString();

            var tooltipTrigger = GetComponent<TooltipTriggerUI>();
            tooltipTrigger.Initialise(command.Tooltip, command.Image);
        }
        
        void OnAffectionChange(int value, int newValue)
        {
            var command = Commands.CommandTypeMap[_currentCommandExecutionData.CommandType];
            if (command.Cost > newValue)
            {
                _button.enabled = false;
                affordabilityMask.SetActive(true);
            }
            else
            {
                _button.enabled = true;
                affordabilityMask.SetActive(false);
            }
        }
    }
}