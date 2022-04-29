using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    internal class TooltipUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI tooltipText;

        [SerializeField]
        private Image tooltipImage;

        public void Initialise(string text, Sprite image)
        {
            tooltipText.text = text;
            tooltipImage.sprite = image;
        }
    }
}