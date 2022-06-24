using System;
using System.Collections.Generic;
using UnityEngine;

namespace Client.UI
{
    public class SpriteSelectionUI : MonoBehaviour
    {
        [SerializeField]
        private Transform spriteButtonsParent;

        [SerializeField]
        private GameObject spriteButtonsPrefab;

        public event Action<int> SelectionChanged; 

        public int selectedIndex;

        public void Initialise(List<Sprite> selectionOptions, List<string> names)
        {
            for (int i = 0; i < selectionOptions.Count; i++)
            {
                var go = Instantiate(spriteButtonsPrefab, spriteButtonsParent);

                var button = go.GetComponent<ImageSelectionButtonUI>();
                button.Initialise(selectionOptions[i], this, names == null ? "" : names[i]);
                button.SetSelectionValue(i);
                button.RegisterOnClicked(value =>
                {
                    selectedIndex = value;
                    SelectionChanged?.Invoke(selectedIndex);
                });
            }
        }
    }
}