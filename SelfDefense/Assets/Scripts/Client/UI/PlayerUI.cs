using Server;
using Shared.Entity;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class PlayerUI : NetworkBehaviour
    {
        [SerializeField]
        private HealthBarUI HealthBarUI;

        [SerializeField]
        private Image goalBar;

        [SerializeField]
        private TextMeshProUGUI goalText, affectionPointsText;

        public void Initialise(NetworkObject playerGo, NetworkObject childhoodSelfGo, PlayerGoal playerGoal)
        {
            HealthBarUI.Initialise(childhoodSelfGo.GetComponent<Health>());
            goalBar.fillAmount = 0;
            goalText.text = "0%";
            playerGoal.GoalIncremented += f =>
            {
                goalBar.fillAmount = f;
                goalText.text = f.ToString("P2");
            };

            var affectionPoints = playerGo.GetComponent<AffectionPoints>();
            affectionPoints.Points.OnValueChanged += (value, newValue) =>
            {
                affectionPointsText.text = newValue.ToString();
            };
        }
    }
}