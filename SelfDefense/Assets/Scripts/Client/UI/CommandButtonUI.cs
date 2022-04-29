using System;
using System.Collections.Generic;
using Client.Avatar;
using Client.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Client.UI
{
    public class CommandButtonUI : MonoBehaviour
    {
        [SerializeField]
        private List<Sprite> commandImages;

        [SerializeField]
        private TextMeshProUGUI buttonText;
        
        private Image _image;
        private Button _button;
        private CommandExecutionData _currentCommandExecutionData;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
        }

        public void Initialise(CommandExecutor executor, CommandExecutionData commandExecutionData)
        {
            _button.onClick.AddListener(() =>
            {
                Debug.Log("Button pressed!");
                executor.ExecuteCommandServerRpc(_currentCommandExecutionData);
            });
        
            _currentCommandExecutionData = commandExecutionData;
            switch (commandExecutionData.CommandType)
            {
                case CommandType.BUILD_TOY:
                    _image.sprite = commandImages[0];
                    buttonText.text = "TOY TOWER";
                    break;
                case CommandType.BUILD_MUSIC:
                    _image.sprite = commandImages[1];
                    buttonText.text = "MUSIC TOWER";
                    break;
                case CommandType.BUILD_LAUGHTER:
                    _image.sprite = commandImages[2];
                    buttonText.text = "LAUGHTER TOWER";
                    break;
                case CommandType.WORK_GOAL:
                    _image.sprite = commandImages[3];
                    buttonText.text = "WORK ON GOAL";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(commandExecutionData), commandExecutionData, null);
            }
        }
    }
}