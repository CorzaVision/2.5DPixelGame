using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Interface;

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

    private PlayerController playerController;
    private PlayerStats playerStats;
    private PlayerInput playerInput;
    private InputAction attackAction;
    private float lastAttackTime;
    private bool canAttack = true;

    private Vector3 lastHitPoint;
    private float lastHitTime;
    private float hitEffectDuration = 0.5f;

    public event Action OnAttackPerformed;
    public event Action OnAttackCooldown;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerStats = GetComponent<PlayerStats>();
        playerInput = GetComponent<PlayerInput>();

        if (playerController == null) Debug.LogError("PlayerController not found on this GameObject.");
        if (playerStats == null) Debug.LogError("PlayerStats not found on this GameObject.");
        if (playerInput == null) Debug.LogError("PlayerInput not found on this GameObject.");
        
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

    private void OnEnable()
    {
        if (attackAction != null)
        {
            attackAction.performed += HandleAttackInput;
            attackAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (attackAction != null)
        {
            attackAction.performed -= HandleAttackInput;
            attackAction.Disable();
        }
    }

    private void HandleAttackInput(InputAction.CallbackContext context)
    {
        Debug.Log("Attack input received");
        if (!canAttack) return;

        if (Time.time - lastAttackTime < playerStats.CurrentCooldown)
        {
            if (showDebug) Debug.Log("Attack on cooldown");
            OnAttackCooldown?.Invoke();
            return;
        }

        PerformAttack();
        lastAttackTime = Time.time;
    }

    private void Update()
    {
        if (!showDebug) return;

        Color rayColor = (Time.time - lastAttackTime < playerStats.CurrentCooldown) ? cooldownRayColor : debugRayColor;
        Debug.DrawRay(transform.position, playerController.GetAttackDirection() * attackRange, rayColor);
    }

    private void OnDrawGizmos()
    {
        if (!showDebug) return;
        
        Gizmos.color = new Color(debugRayColor.r, debugRayColor.g, debugRayColor.b, 0.2f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (Time.time - lastHitTime < hitEffectDuration)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(lastHitPoint, 0.2f);
        }
    }

    private void PerformAttack()
    {
        if (showDebug) Debug.Log("Performing attack");

        Vector3 attackDirection = playerController.GetAttackDirection();
        RaycastHit hit;
        if (Physics.Raycast(transform.position, attackDirection, out hit, attackRange, attackableLayers))
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if(damageable != null)
            {
                float damageToDeal = playerStats.CurrentAttack;
                damageable.TakeDamage(damageToDeal);
                if(showDebug) Debug.Log($"Hit {hit.collider.name} for {damageToDeal} damage");
                
                DrawHitEffect(hit.point);
            }
        }

        Debug.DrawRay(transform.position, attackDirection * attackRange, debugRayColor, debugRayDuration);
        OnAttackPerformed?.Invoke();
    }

    private void DrawHitEffect(Vector3 hitPoint)
    {
        lastHitPoint = hitPoint;
        lastHitTime = Time.time;

        Debug.DrawRay(hitPoint, Vector3.up * 0.5f, Color.yellow, 0.5f);
        Debug.DrawRay(hitPoint, Vector3.right * 0.5f, Color.yellow, 0.5f);
        Debug.DrawRay(hitPoint, Vector3.forward * 0.5f, Color.yellow, 0.5f);
    }
}

