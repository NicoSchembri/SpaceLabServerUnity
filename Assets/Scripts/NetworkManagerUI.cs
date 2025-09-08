using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Button hostButton;
    private void Awake()
    {
        if (serverButton != null)
            serverButton.onClick.AddListener(() => NetworkManager.Singleton.StartServer());
        else
            Debug.LogError("Server Button is not assigned!");

        if (clientButton != null)
            clientButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
        else
            Debug.LogError("Client Button is not assigned!");

        if (hostButton != null)
            hostButton.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
        else
            Debug.LogError("Host Button is not assigned!");
    }
}
