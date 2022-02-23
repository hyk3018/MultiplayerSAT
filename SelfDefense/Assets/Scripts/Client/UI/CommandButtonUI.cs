using System;
using System.Collections.Generic;
using Client.Avatar;
using Client.Commands;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class CommandButtonUI : MonoBehaviour
    {
        [SerializeField]
        private List<Sprite> commandImages;

        private Image _image;
        private Button _button;
        private CommandData _currentCommandData;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
        }

        public void Initialise(CommandExecutor executor, CommandData commandData)
        {
            _button.onClick.AddListener(() =>
            {
                Debug.Log("Button pressed!");
                executor.ExecuteCommandServerRpc(_currentCommandData);
            });
        
            _currentCommandData = commandData;
            switch (commandData.CommandType)
            {
                case CommandType.BUILD_TOY:
                    _image.sprite = commandImages[0];
                    break;
                case CommandType.BUILD_MUSIC:
                    _image.sprite = commandImages[1];
                    break;
                case CommandType.BUILD_LAUGHTER:
                    _image.sprite = commandImages[2];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(commandData), commandData, null);
            }
        }
    }
}