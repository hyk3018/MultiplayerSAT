using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    /*
     * Used to select character and goal sprites for goal
     */
    public class ImageSelectionButtonUI : MonoBehaviour
    {
        [SerializeField]
        private Image image;

        [SerializeField]
        private TextMeshProUGUI imageName;

        [SerializeField]
        private Image background;

        [SerializeField]
        private Button button;

        [SerializeField]
        private Color selectedColor;

        private int _indexValue;
        
        public void Initialise(Sprite sprite, SpriteSelectionUI characterSelectionUI, string name)
        {
            image.sprite = sprite;
            if (imageName != null)
            {
                imageName.text = name;
            }
            
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