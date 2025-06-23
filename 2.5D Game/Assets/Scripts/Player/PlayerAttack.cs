using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Interface;

/// <summary>
/// Handles player attack mechanics, including input processing, damage calculation, and visual effects.
/// This script manages attack cooldowns, range detection, and damage application to damageable targets.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask attackableLayers;

    [Header("Visual Effects")]
    [SerializeField] private bool showDebug = true;
    [SerializeField] private Color debugRayColor = Color.red;
    [SerializeField] private float debugRayDuration = 0.5f;
    [SerializeField] private Color cooldownRayColor = Color.gray;

    // Components
    private PlayerController playerController;
    private PlayerStats playerStats;
    private PlayerInput playerInput;
    private InputAction attackAction;

    // Attack State
    private float lastAttackTime;
    private bool canAttack = true;

    // Visual Effects
    private Vector3 lastHitPoint;
    private float lastHitTime;
    private float hitEffectDuration = 0.5f;

    // Events
    public event Action OnAttackPerformed;
    public event Action OnAttackCooldown;

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        ValidateComponents();
        SetupInputActions();
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

    private void Update()
    {
        if (showDebug)
        {
            DrawDebugRays();
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebug) return;
        
        DrawDebugGizmos();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Gets and caches all required components.
    /// </summary>
    private void InitializeComponents()
    {
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
        playerInput = GetComponent<PlayerInput>();
    }

    /// <summary>
    /// Validates that all required components are present.
    /// </summary>
    private void ValidateComponents()
    {
        if (playerController == null) 
        {
            Debug.LogError("PlayerController not found on this GameObject.");
        }
        
        if (playerStats == null) 
        {
            Debug.LogError("PlayerStats not found on this GameObject.");
        }
        
        if (playerInput == null) 
        {
            Debug.LogError("PlayerInput not found on this GameObject.");
        }
    }

    /// <summary>
    /// Sets up input actions from the Input System.
    /// </summary>
    private void SetupInputActions()
    {
        Debug.Log("Available Action Maps:");
        foreach (var actionMap in playerInput.actions.actionMaps)
        {
            Debug.Log($"Action Map: {actionMap.name}");
            foreach (var action in actionMap.actions)
            {
                Debug.Log($"- Action: {action.name}");
            }
        }
        
        var playerMap = playerInput.actions.FindActionMap("Player");
        if (playerMap != null)
        {
            playerMap.Enable();
            attackAction = playerMap.FindAction("Attack");
            
            if (attackAction != null)
            {
                Debug.Log("Attack action found and set up");
            }
            else
            {
                Debug.LogError("Attack action not found in Player action map");
            }
        }
        else
        {
            Debug.LogError("Player action map not found");
        }
    }

    #endregion

    #region Input Management

    /// <summary>
    /// Subscribes to input events.
    /// </summary>
    private void SubscribeToInputEvents()
    {
        if (attackAction != null)
        {
            attackAction.performed += HandleAttackInput;
        }
    }

    /// <summary>
    /// Unsubscribes from input events.
    /// </summary>
    private void UnsubscribeFromInputEvents()
    {
        if (attackAction != null)
        {
            attackAction.performed -= HandleAttackInput;
        }
    }

    /// <summary>
    /// Enables input actions.
    /// </summary>
    private void EnableInputActions()
    {
        if (attackAction != null)
        {
            attackAction.Enable();
        }
    }

    /// <summary>
    /// Disables input actions.
    /// </summary>
    private void DisableInputActions()
    {
        if (attackAction != null)
        {
            attackAction.Disable();
        }
    }

    #endregion

    #region Attack Logic

    /// <summary>
    /// Handles attack input and performs attack if conditions are met.
    /// </summary>
    /// <param name="context">The input action callback context.</param>
    private void HandleAttackInput(InputAction.CallbackContext context)
    {
        Debug.Log("Attack input received");
        
        if (!canAttack) return;

        if (Time.time - lastAttackTime < playerStats.CurrentCooldown)
        {
            if (showDebug) 
            {
                Debug.Log("Attack on cooldown");
            }
            OnAttackCooldown?.Invoke();
            return;
        }

        PerformAttack();
        lastAttackTime = Time.time;
    }

    /// <summary>
    /// Performs the actual attack, checking for targets and applying damage.
    /// </summary>
    private void PerformAttack()
    {
        if (showDebug) 
        {
            Debug.Log("Performing attack");
        }

        Vector3 attackDirection = playerController.GetAttackDirection();
        RaycastHit hit;
        
        if (Physics.Raycast(transform.position, attackDirection, out hit, attackRange, attackableLayers))
        {
            HandleHitTarget(hit);
        }

        Debug.DrawRay(transform.position, attackDirection * attackRange, debugRayColor, debugRayDuration);
        OnAttackPerformed?.Invoke();
    }

    /// <summary>
    /// Handles damage application to a hit target.
    /// </summary>
    /// <param name="hit">The raycast hit information.</param>
    private void HandleHitTarget(RaycastHit hit)
    {
        IDamageable damageable = hit.collider.GetComponent<IDamageable>();
        
        if (damageable != null)
        {
            float damageToDeal = playerStats.CurrentAttack;
            damageable.TakeDamage(damageToDeal);
            
            if (showDebug) 
            {
                Debug.Log($"Hit {hit.collider.name} for {damageToDeal} damage");
            }
            
            DrawHitEffect(hit.point);
        }
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// Draws debug rays for attack visualization.
    /// </summary>
    private void DrawDebugRays()
    {
        Color rayColor = (Time.time - lastAttackTime < playerStats.CurrentCooldown) ? cooldownRayColor : debugRayColor;
        Debug.DrawRay(transform.position, playerController.GetAttackDirection() * attackRange, rayColor);
    }

    /// <summary>
    /// Draws debug gizmos for attack range and hit effects.
    /// </summary>
    private void DrawDebugGizmos()
    {
        // Draw attack range
        Gizmos.color = new Color(debugRayColor.r, debugRayColor.g, debugRayColor.b, 0.2f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw hit effect
        if (Time.time - lastHitTime < hitEffectDuration)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(lastHitPoint, 0.2f);
        }
    }

    /// <summary>
    /// Draws visual effects at the hit point.
    /// </summary>
    /// <param name="hitPoint">The point where the attack hit.</param>
    private void DrawHitEffect(Vector3 hitPoint)
    {
        lastHitPoint = hitPoint;
        lastHitTime = Time.time;

        Debug.DrawRay(hitPoint, Vector3.up * 0.5f, Color.yellow, 0.5f);
        Debug.DrawRay(hitPoint, Vector3.right * 0.5f, Color.yellow, 0.5f);
        Debug.DrawRay(hitPoint, Vector3.forward * 0.5f, Color.yellow, 0.5f);
    }

    #endregion
}

