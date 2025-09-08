using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkAnimator))]
public class DummyPlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float rotationSpeed = 10f;
    public float jumpVelocity = 7f;

    [Header("References")]
    public Animator animator;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private NetworkAnimator netAnimator;

    private bool wasGroundedLastFrame;
    private bool jumpRequested;

    public override void OnNetworkSpawn()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        netAnimator = GetComponent<NetworkAnimator>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        capsule = GetComponent<CapsuleCollider>();
        wasGroundedLastFrame = IsGrounded();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetButtonDown("Jump") && IsGrounded())
            jumpRequested = true;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector3 moveDir = GetInputDirection();
        HandleMovement(moveDir);
        HandleRotation(moveDir);
        ApplyJump();
        UpdateAnimator(moveDir);

        rb.angularVelocity = Vector3.zero;
    }

    private Vector3 GetInputDirection()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0;
        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0;

        Vector3 moveDir = camRight.normalized * moveX + camForward.normalized * moveZ;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();
        return moveDir;
    }

    private void HandleMovement(Vector3 moveDir)
    {
        float speed = (Input.GetKey(KeyCode.LeftShift) && moveDir.sqrMagnitude > 0.01f) ? runSpeed : walkSpeed;
        Vector3 horizontalVelocity = moveDir * speed;

        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
    }

    private void HandleRotation(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void ApplyJump()
    {
        if (!jumpRequested) return;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
        netAnimator.SetTrigger("Jump"); 

        jumpRequested = false;
    }

    private void UpdateAnimator(Vector3 moveDir)
    {
        bool grounded = IsGrounded();
        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        if (IsOwner)
        {
            animator.SetFloat("Speed", horizontalSpeed);
            animator.SetBool("IsRunning", horizontalSpeed > walkSpeed + 0.1f);
            animator.SetBool("IsJumping", !grounded && rb.linearVelocity.y > 0);
            animator.SetBool("IsFalling", !grounded && rb.linearVelocity.y < 0);

            if (grounded && !wasGroundedLastFrame)
            {
                netAnimator.SetTrigger("Land");
            }

            // Sync animation to the other clients
            UpdateAnimatorServerRpc(horizontalSpeed, horizontalSpeed > walkSpeed + 0.1f, !grounded && rb.linearVelocity.y > 0, !grounded && rb.linearVelocity.y < 0);
        }

        wasGroundedLastFrame = grounded;
    }

    [ServerRpc]
    private void UpdateAnimatorServerRpc(float speed, bool isRunning, bool isJumping, bool isFalling)
    {
        UpdateAnimatorClientRpc(speed, isRunning, isJumping, isFalling);
    }

    [ClientRpc]
    private void UpdateAnimatorClientRpc(float speed, bool isRunning, bool isJumping, bool isFalling)
    {
        if (IsOwner) return;

        animator.SetFloat("Speed", speed);
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsJumping", isJumping);
        animator.SetBool("IsFalling", isFalling);
    }

    private bool IsGrounded()
    {
        float rayLength = capsule.bounds.extents.y + 0.1f;
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        return Physics.Raycast(origin, Vector3.down, rayLength);
    }
}
