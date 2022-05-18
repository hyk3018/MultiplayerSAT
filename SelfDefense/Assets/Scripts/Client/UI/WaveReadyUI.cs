using System;
using Server;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class WaveReadyUI : MonoBehaviour
    {
        [SerializeField]
        private Button readyButton;

        [SerializeField]
        private TextMeshProUGUI readyText;

        [SerializeField]
        private ReadyListener readyListener;

        private void Start()
        {
            readyListener.readyCount.OnValueChanged += (value, newValue) =>
            {
                if (newValue < GameManager.Instance.MinReady)
                {
                    readyText.text = newValue + " out of 2 players ready";
                }
                else 
                {
                    gameObject.SetActive(false);
                    if (NetworkManager.Singleton.IsServer)
                        GameManager.Instance.StartWaveServerRpc();
                }
            };

            readyButton.onClick.AddListener(() =>
            {
                readyListener.SetReadyServerRpc(true);
                readyButton.enabled = false;
            });
        }

        public void ReadyUpForNewWave()
        {
            gameObject.SetActive(true);
            readyButton.enabled = true;
            
            if (NetworkManager.Singleton.IsServer)
                readyListener.readyCount.Value = 0;
        }
    }
}