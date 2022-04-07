using System;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class ImageSelectionButtonUI : MonoBehaviour
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private Image background;

        [SerializeField]
        private Button button;

        [SerializeField]
        private Color selectedColor;

        private int _indexValue;
        
        public void Initialise(Sprite sprite, SpriteSelectionUI characterSelectionUI)
        {
            image.sprite = sprite;
            characterSelectionUI.SelectionChanged += value =>
            {
                background.color = value == _indexValue ? selectedColor : Color.white;
            };
        }

        public void RegisterOnClicked(Action<int> callback)
        {
            button.onClick.AddListener(() => { callback(_indexValue); });
        }

        public void SetSelectionValue(int value)
        {
            _indexValue = value;
        }
    }
}