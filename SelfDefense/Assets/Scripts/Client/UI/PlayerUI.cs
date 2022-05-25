using System;
using Server;
using Shared.Entity;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class PlayerUI : MonoBehaviour
    {
        [SerializeField]
        private HealthBarUI HealthBarUI;

        [SerializeField]
        private Image goalBar;

        [SerializeField]
        private TextMeshProUGUI goalText, affectionPointsText;

        private PlayerGoal _playerGoal;
        private AffectionPoints _affectionPoints;

        public void Initialise(NetworkObject playerGo, NetworkObject childhoodSelfGo, PlayerGoal playerGoal)
        {
            _playerGoal = playerGoal;
            
            HealthBarUI.Initialise(childhoodSelfGo.GetComponent<Health>());
            goalBar.fillAmount = 0;
            goalText.text = "0%";
            playerGoal.GoalIncremented += OnGoalIncremented;

            var affectionPoints = playerGo.GetComponent<AffectionPoints>();
            _affectionPoints = affectionPoints;
            affectionPoints.Points.OnValueChanged += OnAffectionPointsChanged;
        }

        private void OnAffectionPointsChanged(int value, int newValue)
        {
            affectionPointsText.text = newValue.ToString();
        }

        private void OnGoalIncremented(float f)
        {
            goalBar.fillAmount = f;
            goalText.text = f.ToString("P2");
        }

        private void OnDestroy()
        {
            if (_playerGoal)
                _playerGoal.GoalIncremented -= OnGoalIncremented;
            _affectionPoints.Points.OnValueChanged -= OnAffectionPointsChanged;
        }
    }
}