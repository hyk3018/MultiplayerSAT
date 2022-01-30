using System;
using System.Collections.Generic;
using Client.Commands;
using UnityEngine;
using UnityEngine.UI;

internal class CommandButtonUI : MonoBehaviour
{
    [SerializeField]
    private List<Sprite> commandImages;

    private Image _image;
    private Button _button;
    private CommandType _currentCommandType;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() =>
        {
            
        });
    }

    public void Initialise(CommandType commandType)
    {
        _currentCommandType = commandType;
        
        switch (commandType)
        {
            case CommandType.BUILD_TOY:
                _image.sprite = commandImages[0];
                break;
            case CommandType.BUILD_MUSIC:
                _image.sprite = commandImages[1];
                break;
            case CommandType.BUILD_JOKE:
                _image.sprite = commandImages[2];
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(commandType), commandType, null);
        }
        
        
    }
}