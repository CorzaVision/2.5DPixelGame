using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float jumpCooldown = 0.2f;
    [SerializeField] private float movementSmoothing = 0.1f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private Color debugRayColor = Color.yellow;
    [SerializeField] private Color debugHitColor = Color.green;
    [SerializeField] private Color debugMissColor = Color.red;

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;  // Reference to the main camera

    // Components
    private Rigidbody rb;
    private PlayerInput playerInput;
    private CapsuleCollider capsuleCollider;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction inventoryAction;
    // Movement variables
    private Vector2 moveInput;
    private bool isGrounded;
    private float currentSpeed;
    private float lastJumpTime;
    private Vector3 currentVelocity;
    private Vector3 targetRotation;

    private PlayerInventory playerInventory;    

    private void Start()
    {
        playerInventory = GetComponent<PlayerInventory>();
    }

    public Vector3 GetAttackDirection()
    {
        return transform.forward;
    }

    private void Awake()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        playerInventory = GetComponent<PlayerInventory>();

        // Validate components
        if (rb == null) Debug.LogError("Rigidbody component is missing!");
        if (playerInput == null) Debug.LogError("PlayerInput component is missing!");
        if (capsuleCollider == null) Debug.LogError("CapsuleCollider component is missing!");
        if (playerInventory == null) Debug.LogError("PlayerInventory component is missing!");

        // Get input actions
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        inventoryAction = playerInput.actions["Inventory"];

        // Initialize speed
        currentSpeed = moveSpeed;

        // Configure Rigidbody
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        // Get main camera if not set
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No main camera found! Please assign a camera in the inspector.");
            }
        }
    }

    private void OnEnable()
    {
        // Subscribe to input events
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
        jumpAction.performed += OnJumpPerformed;
        sprintAction.performed += OnSprintPerformed;
        sprintAction.canceled += OnSprintCanceled;
        inventoryAction.performed += OnInventoryPerformed;

        // Enable input actions
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        inventoryAction.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;
        jumpAction.performed -= OnJumpPerformed;
        sprintAction.performed -= OnSprintPerformed;
        sprintAction.canceled -= OnSprintCanceled;
        inventoryAction.performed -= OnInventoryPerformed;

        // Disable input actions
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        inventoryAction.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            Jump();
        }
    }

    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        currentSpeed = moveSpeed * sprintMultiplier;
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        currentSpeed = moveSpeed;
    }

    private void OnInventoryPerformed(InputAction.CallbackContext context)
    {
        playerInventory?.ToggleInventory();
    }

    private void Jump()
    {
        if (Time.time - lastJumpTime < jumpCooldown) return;
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        lastJumpTime = Time.time;
    }

    private void CheckGrounded()
    {
        if (capsuleCollider == null) return;
        Vector3 rayStart = transform.position - Vector3.up * (capsuleCollider.height * 0.5f);
        float rayDistance = groundCheckDistance + capsuleCollider.height * 0.5f;
        Debug.DrawRay(rayStart, Vector3.down * rayDistance, debugRayColor);
        RaycastHit hit;
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(rayStart, Vector3.down, out hit, rayDistance, groundLayer);
        if (isGrounded)
        {
            Debug.DrawLine(rayStart, hit.point, debugHitColor);
        }
        else
        {
            Debug.DrawLine(rayStart, rayStart + Vector3.down * rayDistance, debugMissColor);
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        CheckGrounded();
        // Calculate movement
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 targetVelocity = movement * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y;
        // Smooth movement
        rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, movementSmoothing);
        // Rotate towards movement direction
        if (movement != Vector3.zero)
        {
            // Calculate the target rotation based on movement direction
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            // Smoothly rotate towards the movement direction
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }
}