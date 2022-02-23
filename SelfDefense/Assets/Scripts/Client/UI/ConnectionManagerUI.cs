using System.Collections;
using System.Threading.Tasks;
using Server;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class ConnectionManagerUI : MonoBehaviour
    {
        [SerializeField]
        private Button startButton;

        [SerializeField]
        private Button joinButton;

        [SerializeField]
        private TMP_InputField joinCodeInput;

        [SerializeField]
        private TMP_Text roomCodeText;

        [SerializeField]
        private GameObject readyStatusBar;

        [SerializeField]
        private TMP_Text playersReadyText;
        
        [SerializeField]
        private GameManager gameManager;

        public void Start()
        {
            startButton.onClick.AddListener(StartHost);
            joinButton.onClick.AddListener(Join);
            gameManager.readyCount.OnValueChanged +=
                (oldValue, newValue) => playersReadyText.text = newValue.ToString();
            gameManager.GameStarted += (_,__) =>
            {
                readyStatusBar.SetActive(false);
                gameObject.SetActive(false);
                
            };
        }

        private async void StartHost()
        {
            if (RelayConnectionManager.Instance.IsRelayEnabled)
            {
                var data = await RelayConnectionManager.Instance.SetupRelay();
                roomCodeText.text = data.JoinCode;
            }

            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started");
                readyStatusBar.SetActive(true);
            }
            else
                Debug.Log("Failed to host");
        
        }

        private async void Join()
        {
            if (RelayConnectionManager.Instance.IsRelayEnabled)
                await RelayConnectionManager.Instance.JoinRelay(joinCodeInput.text);
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started");
            }
            else
                Debug.Log("Failed to start client");
        }

        private void OnClientConnected(ulong clientId)
        {
            gameManager.RetrieveStartingPlayerCountServerRpc();
            readyStatusBar.SetActive(true);
            roomCodeText.text = joinCodeInput.text;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        private IEnumerator SyncLobbyDataOnStart()
        {
            yield return new WaitForSeconds(0.5f);
            
            readyStatusBar.SetActive(true);
            roomCodeText.text = joinCodeInput.text;
        }
    }
}
