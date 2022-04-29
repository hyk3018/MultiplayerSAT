using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Client.UI
{
    public class TooltipTriggerUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private float minHoverDuration;

        [SerializeField]
        private GameObject toolTipPrefab;

        private GameObject _currentTooltip;
        private string _tooltipText;
        private Sprite _tooltipImage;
        private float _currentHoverDuration;
        private bool _hovering;

        public void Initialise(string text, Sprite image)
        {
            _tooltipText = text;
            _tooltipImage = image;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovering = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovering = false;
            _currentHoverDuration = 0f;
        }

        private void Update()
        {
            if (_hovering)
            {
                _currentHoverDuration += Time.deltaTime;
                
                if (_currentHoverDuration >= minHoverDuration && !_currentTooltip)
                {
                    _currentTooltip = Instantiate(toolTipPrefab);
                    _currentTooltip.transform.position = gameObject.transform.position;
                    _currentTooltip.GetComponent<TooltipUI>().Initialise(_tooltipText, _tooltipImage);
                }
            }

        }
    }
}