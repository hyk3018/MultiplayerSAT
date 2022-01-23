using System;
using System.Collections;
using System.Collections.Generic;
using HelloWorld.Shared.Relay;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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

    public void Start()
    {
        Debug.Log("hello");
        startButton.onClick.AddListener(StartHost);
        joinButton.onClick.AddListener(Join);
    }

    private async void StartHost()
    {
        if (RelayConnectionManager.Instance.IsRelayEnabled)
        {
            var data = await RelayConnectionManager.Instance.SetupRelay();
            roomCodeText.text = data.JoinCode;
        }

        if (NetworkManager.Singleton.StartHost())
            Debug.Log("Host started");
        else
            Debug.Log("Failed to host");
        
    }

    private async void Join()
    {
        if (RelayConnectionManager.Instance.IsRelayEnabled)
            await RelayConnectionManager.Instance.JoinRelay(joinCodeInput.text);
        
        if (NetworkManager.Singleton.StartClient())
            Debug.Log("Client started");
        else
            Debug.Log("Failed to start client");
    }
}
