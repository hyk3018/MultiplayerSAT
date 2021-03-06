using Server;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Client.UI
{
    public class ConnectionManagerUI : MonoBehaviour
    {
        [SerializeField]
        private Button hostButton, joinButton;

        [SerializeField]
        private TMP_InputField joinCodeInput;

        [SerializeField]
        private TMP_Text roomCodeText, playersReadyText;

        [SerializeField]
        private GameObject readyStatusBar, charSelectPanel, goalSelectPanel, promptPanel, backgroundMaskPanel;

        [SerializeField]
        private SpriteSelectionUI charSelectionUI, goalSelectionUI;

        [SerializeField]
        private GameManager gameManager;

        [SerializeField]
        private ReadyListener joinGameReadyListener;

        [SerializeField]
        private Image playerColourImage;

        [SerializeField]
        private Sprite player1Colour, player2Colour;

        public void Start()
        {
            hostButton.onClick.AddListener(StartHost);
            joinButton.onClick.AddListener(Join);
            joinGameReadyListener.readyCount.OnValueChanged +=
                (oldValue, newValue) => playersReadyText.text = newValue.ToString();
            gameManager.GameStarted += (_,__) =>
            {
                readyStatusBar.SetActive(false);
                gameObject.SetActive(false);
                
            };

            charSelectionUI.SelectionChanged += _ =>
            {
                gameManager.SetPlayerCustomizationServerRpc(NetworkManager.Singleton.LocalClientId,
                    charSelectionUI.selectedIndex,
                    goalSelectionUI.selectedIndex);
            };
            
            goalSelectionUI.SelectionChanged += _ =>
            {
                gameManager.SetPlayerCustomizationServerRpc(NetworkManager.Singleton.LocalClientId,
                    charSelectionUI.selectedIndex,
                    goalSelectionUI.selectedIndex);
            };
        }

        private async void StartHost()
        {
            if (RelayConnectionManager.Instance.IsRelayEnabled)
            {
                var data = await RelayConnectionManager.Instance.SetupRelay();
                roomCodeText.text = data.JoinCode;
            }
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started");
            }
            else
                Debug.Log("Failed to host");
        
        }

        public void Disconnect()
        {
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadSceneAsync("MainScene");
        }

        private async void Join()
        {
            if (RelayConnectionManager.Instance.IsRelayEnabled)
                await RelayConnectionManager.Instance.JoinRelay(joinCodeInput.text);
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started");
                roomCodeText.text = joinCodeInput.text;
            }
            else
                Debug.Log("Failed to start client");
        }
        
        private void OnClientConnected(ulong clientId)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            
            gameManager.RegisterListenToReadyUp();
            joinButton.transform.parent.gameObject.SetActive(false);
            hostButton.gameObject.SetActive(false);
            roomCodeText.gameObject.SetActive(true);
            charSelectPanel.SetActive(true);
            goalSelectPanel.SetActive(true);
            readyStatusBar.SetActive(true);
            promptPanel.SetActive(true);

            var colour = NetworkManager.Singleton.IsHost ? player1Colour : player2Colour;
            playerColourImage.sprite = colour;
        }
    }
}
