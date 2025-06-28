using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Handles player movement, jumping, and input processing for the 2.5D RPG character.
/// This script manages WASD movement, sprinting, jumping, and ground detection.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float movementSmoothing = 0.1f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private Color debugRayColor = Color.yellow;
    [SerializeField] private Color debugHitColor = Color.green;
    [SerializeField] private Color debugMissColor = Color.red;

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;

    // Components
    private Rigidbody rb;
    private PlayerInput playerInput;
    private CapsuleCollider capsuleCollider;
    private PlayerInventory playerInventory;

    // Input Actions
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction inventoryAction;

    // Movement State
    private Vector2 moveInput;
    private bool isGrounded;
    private float currentSpeed;
    private Vector3 currentVelocity;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        ValidateComponents();
        SetupInputActions();
        ConfigureRigidbody();
        SetupCamera();
    }

    private void OnEnable()
    {
        SubscribeToInputEvents();
        EnableInputActions();
    }

    private void OnDisable()
    {
        UnsubscribeFromInputEvents();
        DisableInputActions();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        
        CheckGrounded();
        HandleMovement();
        HandleRotation();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Gets and caches all required components.
    /// </summary>
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        playerInventory = GetComponent<PlayerInventory>();
    }

    /// <summary>
    /// Validates that all required components are present.
    /// </summary>
    private void ValidateComponents()
    {
        if (rb == null) Debug.LogError("Rigidbody component is missing!");
        if (playerInput == null) Debug.LogError("PlayerInput component is missing!");
        if (capsuleCollider == null) Debug.LogError("CapsuleCollider component is missing!");
        if (playerInventory == null) Debug.LogError("PlayerInventory component is missing!");
    }

    /// <summary>
    /// Sets up input actions from the Input System.
    /// </summary>
    private void SetupInputActions()
    {
        moveAction = playerInput.actions["Move"];
        sprintAction = playerInput.actions["Sprint"];
        inventoryAction = playerInput.actions["Inventory"];

        currentSpeed = moveSpeed;
    }

    /// <summary>
    /// Configures the Rigidbody for optimal movement behavior.
    /// </summary>
    private void ConfigureRigidbody()
    {
        if (rb == null) return;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    /// <summary>
    /// Sets up camera reference if not already assigned.
    /// </summary>
    private void SetupCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No main camera found! Please assign a camera in the inspector.");
            }
        }
    }

    #endregion

    #region Input Management

    /// <summary>
    /// Subscribes to all input events.
    /// </summary>
    private void SubscribeToInputEvents()
    {
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
        sprintAction.performed += OnSprintPerformed;
        sprintAction.canceled += OnSprintCanceled;
        inventoryAction.performed += OnInventoryPerformed;
    }

    /// <summary>
    /// Unsubscribes from all input events.
    /// </summary>
    private void UnsubscribeFromInputEvents()
    {
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;
        sprintAction.performed -= OnSprintPerformed;
        sprintAction.canceled -= OnSprintCanceled;
        inventoryAction.performed -= OnInventoryPerformed;
    }

    /// <summary>
    /// Enables all input actions.
    /// </summary>
    private void EnableInputActions()
    {
        moveAction.Enable();
        sprintAction.Enable();
        inventoryAction.Enable();
    }

    /// <summary>
    /// Disables all input actions.
    /// </summary>
    private void DisableInputActions()
    {
        moveAction.Disable();
        sprintAction.Disable();
        inventoryAction.Disable();
    }

    #endregion

    #region Input Event Handlers

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
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

    #endregion

    #region Movement Logic

    /// <summary>
    /// Handles the main movement calculation and application.
    /// </summary>
    private void HandleMovement()
    {
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        Vector3 targetVelocity = movement * currentSpeed;
        targetVelocity.y = rb.linearVelocity.y;

        // Apply smooth movement
        rb.linearVelocity = Vector3.SmoothDamp(
            rb.linearVelocity, 
            targetVelocity, 
            ref currentVelocity, 
            movementSmoothing
        );
    }

    /// <summary>
    /// Handles character rotation towards movement direction.
    /// </summary>
    private void HandleRotation()
    {
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }


    /// <summary>
    /// Checks if the player is grounded using a raycast.
    /// </summary>
    private void CheckGrounded()
    {
        if (capsuleCollider == null) return;

        Vector3 rayStart = transform.position - Vector3.up * (capsuleCollider.height * 0.5f);
        float rayDistance = groundCheckDistance + capsuleCollider.height * 0.5f;
        
        Debug.DrawRay(rayStart, Vector3.down * rayDistance, debugRayColor);
        
        RaycastHit hit;
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

    #endregion

    #region Public Interface

    /// <summary>
    /// Returns the current forward direction of the player for attack calculations.
    /// </summary>
    /// <returns>The forward direction vector of the player.</returns>
    public Vector3 GetAttackDirection()
    {
        return transform.forward;
    }

    #endregion
}