using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button disconnectButton;

    [Header("UI Input/Status")]
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TMP_Text statusText;

    private void Awake()
    {
        if (serverButton != null)
            serverButton.onClick.AddListener(() => {
                NetworkManager.Singleton.StartServer();
                UpdateStatus("Server running on 0.0.0.0:7778");
                ToggleButtons(false);
            });

        if (clientButton != null)
            clientButton.onClick.AddListener(() => {
                string ip = string.IsNullOrWhiteSpace(ipInputField.text) ? "127.0.0.1" : ipInputField.text;
                var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
                if (transport != null)
                    transport.SetConnectionData(ip, 7778);

                if (NetworkManager.Singleton.StartClient())
                {
                    UpdateStatus($"Client connecting to {ip}:7778");
                    ToggleButtons(false);
                }
                else
                {
                    UpdateStatus("Client failed to start.");
                }
            });

        if (hostButton != null)
            hostButton.onClick.AddListener(() => {
                NetworkManager.Singleton.StartHost();
                UpdateStatus("Host running on 0.0.0.0:7778");
                ToggleButtons(false);
            });

        if (disconnectButton != null)
        {
            disconnectButton.onClick.AddListener(() => {
                if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
                {
                    NetworkManager.Singleton.Shutdown();
                    UpdateStatus("Disconnected.");
                    ToggleButtons(true);
                }
            });
            disconnectButton.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        else
        {
            Debug.LogError("No NetworkManager found in the scene! Make sure one is placed.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            UpdateStatus("Connected as Client/Host");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            UpdateStatus("Disconnected from server");
            ToggleButtons(true);
        }
    }

    private void UpdateStatus(string message)
    {
        Debug.Log(message);
        if (statusText != null)
            statusText.text = message;
    }

    private void ToggleButtons(bool showMain)
    {
        serverButton.interactable = showMain;
        clientButton.interactable = showMain;
        hostButton.interactable = showMain;
        disconnectButton.gameObject.SetActive(!showMain);
    }
}
