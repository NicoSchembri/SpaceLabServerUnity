using UnityEngine;
using Unity.Netcode;

public class FollowPlayer : MonoBehaviour
{
    [Header("Camera Settings")]
    public Vector3 carOffset = new Vector3(0, 5, -10);
    public Vector3 dummyOffset = new Vector3(0.5f, 2f, -4f);
    public float cameraSmoothSpeed = 10f;
    public float mouseSensitivity = 2f;
    public float verticalClampMin = -30f;
    public float verticalClampMax = 60f;

    private GameObject player; // local player or car
    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

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
            player = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            AssignLocalPlayerIfSpawned();
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Toggle cursor with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        HandleMouseRotation();
        FollowTarget();
    }

    private void HandleMouseRotation()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, verticalClampMin, verticalClampMax);
    }

    private void FollowTarget()
    {
        Vector3 offset = (player.GetComponent<DummyPlayerController>() != null) ? dummyOffset : carOffset;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = player.transform.position + rotation * offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, cameraSmoothSpeed * Time.deltaTime);

        Vector3 lookPoint = player.transform.position + Vector3.up * 2f;
        transform.LookAt(lookPoint);
    }

    public void SetPlayer(GameObject newPlayer)
    {
        player = newPlayer;
    }
}
