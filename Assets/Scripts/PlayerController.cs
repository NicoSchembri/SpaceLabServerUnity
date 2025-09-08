using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    public float speed = 35.0f;
    public float turnSpeed = 70.0f;
    public float driftTurnMultiplier = 3.5f;
    public float mouseSensitivity = 100f;

    private float horizontalInput;
    private float forwardInput;

    void Update()
    {
        if (!IsOwner) return;

        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        bool isDrifting = Input.GetKey(KeyCode.LeftShift);

        transform.Translate(Vector3.forward * Time.deltaTime * speed * forwardInput);

        if (Mathf.Abs(forwardInput) > 0.01f)
        {
            float currentTurnSpeed = turnSpeed;
            if (isDrifting) currentTurnSpeed *= driftTurnMultiplier;

            transform.Rotate(Vector3.up, currentTurnSpeed * horizontalInput * Time.deltaTime);
        }
    }
}
