using System;
using System.Collections.Generic;
using UnityEngine;

namespace Client.UI
{
    public class HelpUI : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> HelpPanels;

        private int _currentActiveIndex;

        private void Start()
        {
            _currentActiveIndex = 0;
            HelpPanels[_currentActiveIndex].SetActive(true);
        }

        public void Previous()
        {
            HelpPanels[_currentActiveIndex].SetActive(false);
            _currentActiveIndex = (_currentActiveIndex - 1) % HelpPanels.Count;
            if (_currentActiveIndex < 0)
            {
                _currentActiveIndex += HelpPanels.Count;
            }
            HelpPanels[_currentActiveIndex].SetActive(true);
        }
        
        public void Next()
        {
            HelpPanels[_currentActiveIndex].SetActive(false);
            _currentActiveIndex = (_currentActiveIndex + 1) % HelpPanels.Count;
            HelpPanels[_currentActiveIndex].SetActive(true);
        }
    }
}