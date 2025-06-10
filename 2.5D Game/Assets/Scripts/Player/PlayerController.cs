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

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float healthRegenRate = 1f;
    [SerializeField] private float healthRegenDelay = 1f;

    

    // Components
    private Rigidbody rb;
    private PlayerInput playerInput;
    private CapsuleCollider capsuleCollider;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    // Movement variables
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isSprinting;
    private float currentSpeed;
    private float lastJumpTime;
    private Vector3 currentVelocity;
    private Vector3 targetRotation;

    // Events
    public event Action OnJumped;
    public event Action<bool> OnSprintChanged;
    public event Action<bool> OnGroundedChanged;

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    private float lastDamageTime;
    private bool isDead;



    private void Start()
    {
        currentHealth = maxHealth;
        if (showDebug) Debug.Log($"PlayerController initialized with maxHealth: {maxHealth}");
    }

    public Vector3 GetAttackDirection()
    {
        return transform.forward;
    }



    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth = Mathf.Max(currentHealth - damage, 0);
        lastDamageTime = Time.time;

        if (showDebug) Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (showDebug) Debug.Log($"Player healed {amount} health. Current health: {currentHealth}");
        OnHealthChanged?.Invoke(currentHealth);
    }
    

    private void Update()
    {
        // Check if enough time has passed since last damage
        if (Time.time - lastDamageTime >= healthRegenDelay)
        {
            RegenerateHealth();
        }
    }

    private void RegenerateHealth()
    {
        if (currentHealth < maxHealth && !isDead)
        {
            float regenAmount = healthRegenRate * Time.deltaTime;
            Heal(regenAmount);
        }
    }

    private void Die()
    {
        isDead = true;
        if (showDebug) Debug.Log("Player died!");
        OnDeath?.Invoke();
    }

    public bool IsDead()
    {
        return isDead;
    }

    // Add this method to get health percentage
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    // Add this method to get current health
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    // Add this method to get max health
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    private void Awake()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        // Validate components
        if (rb == null) Debug.LogError("Rigidbody component is missing!");
        if (playerInput == null) Debug.LogError("PlayerInput component is missing!");
        if (capsuleCollider == null) Debug.LogError("CapsuleCollider component is missing!");

        // Get input actions
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];

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

        // Enable input actions
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;
        jumpAction.performed -= OnJumpPerformed;
        sprintAction.performed -= OnSprintPerformed;
        sprintAction.canceled -= OnSprintCanceled;

        // Disable input actions
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (showDebug) Debug.Log($"Movement input: {moveInput}, Magnitude: {moveInput.magnitude}");
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
        isSprinting = true;
        currentSpeed = moveSpeed * sprintMultiplier;
        OnSprintChanged?.Invoke(true);
        if (showDebug) Debug.Log($"Sprint: {isSprinting}, Speed: {currentSpeed}");
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        isSprinting = false;
        currentSpeed = moveSpeed;
        OnSprintChanged?.Invoke(false);
        if (showDebug) Debug.Log($"Sprint: {isSprinting}, Speed: {currentSpeed}");
    }

    private void Jump()
    {
        if (Time.time - lastJumpTime < jumpCooldown) return;
        
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        lastJumpTime = Time.time;
        OnJumped?.Invoke();
        
        if (showDebug) Debug.Log("Jump!");
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
            if (showDebug) Debug.Log($"Ground hit at distance: {hit.distance}");
        }
        else
        {
            Debug.DrawLine(rayStart, rayStart + Vector3.down * rayDistance, debugMissColor);
            if (showDebug) Debug.Log("No ground detected!");
        }

        if (wasGrounded != isGrounded)
        {
            OnGroundedChanged?.Invoke(isGrounded);
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

        // Add debug logging
        if (showDebug)
        {
            Debug.Log($"Move Input: {moveInput}, Movement: {movement}, Target Velocity: {targetVelocity}");
            Debug.Log($"Current Velocity: {rb.linearVelocity}, Grounded: {isGrounded}");
        }

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