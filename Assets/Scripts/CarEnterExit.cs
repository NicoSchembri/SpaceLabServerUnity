using UnityEngine;
using Unity.Netcode;

public class CarEnterExit : MonoBehaviour
{
    public GameObject car;
    public FollowPlayer followCam;

    private GameObject dummyPlayer; 
    private bool playerInRange = false;
    private bool isDriving = false;

    void Start()
    {
        AssignLocalPlayerIfSpawned();

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void AssignLocalPlayerIfSpawned()
    {
        if (NetworkManager.Singleton.LocalClient != null &&
            NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            dummyPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            AssignLocalPlayerIfSpawned();
        }
    }

    void Update()
    {
        if (dummyPlayer == null) return;

        if (playerInRange && !isDriving && Input.GetKeyDown(KeyCode.E))
            EnterCar();
        else if (isDriving && Input.GetKeyDown(KeyCode.E))
            ExitCar();
    }

    void EnterCar()
    {
        isDriving = true;
        dummyPlayer.SetActive(false);
        car.GetComponent<PlayerController>().enabled = true;
        followCam.SetPlayer(car);
    }

    void ExitCar()
    {
        isDriving = false;
        dummyPlayer.transform.position = car.transform.position - car.transform.right * 2f;
        dummyPlayer.SetActive(true);
        car.GetComponent<PlayerController>().enabled = false;
        followCam.SetPlayer(dummyPlayer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == dummyPlayer)
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == dummyPlayer)
            playerInRange = false;
    }
}
