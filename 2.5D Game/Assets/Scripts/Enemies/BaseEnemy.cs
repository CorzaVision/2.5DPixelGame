using UnityEngine;
using Interface;
using System.Collections;

/// <summary>
/// Base class for all enemies in the game, implementing damageable interface and basic enemy behavior.
/// This script handles health, damage, death, experience rewards, and loot drops.
/// </summary>
public class BaseEnemy : MonoBehaviour, IDamageable
{
    [Header("Enemy Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool showDebug = true;
    [SerializeField] private int enemyLevel = 1;

    [Header("Visual Effects")]
    [SerializeField] private Color damageFlashColor = Color.red;
    [SerializeField] private float damageFlashDuration = 0.2f;

    [Header("Experience Settings")]
    [SerializeField] private float experienceReward = 50f;
    [SerializeField] private float experienceMultiplier = 1.1f;

    [Header("Loot Settings")]
    [SerializeField] private LootTable lootTable;

    // Components
    private PlayerStats playerStats;
    private MeshRenderer enemyMeshRenderer;

    // Visual State
    private Color originalColor;

    // Enemy State
    private bool isDead;

    #region Unity Lifecycle

    private void Start()
    {
        InitializeEnemy();
    }

    private void OnGUI()
    {
        if (!showDebug) return;
        DrawHealthDebug();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the enemy with health, components, and references.
    /// </summary>
    private void InitializeEnemy()
    {
        FindPlayerStats();
        InitializeHealth();
        InitializeVisualComponents();
        
        if (showDebug) 
        {
            Debug.Log($"Enemy {name} initialized with {maxHealth} health");
        }
    }

    /// <summary>
    /// Finds and caches the PlayerStats component.
    /// </summary>
    private void FindPlayerStats()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on player!");
        }
    }

    /// <summary>
    /// Initializes the enemy's health system.
    /// </summary>
    private void InitializeHealth()
    {
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Initializes visual components for damage effects.
    /// </summary>
    private void InitializeVisualComponents()
    {
        enemyMeshRenderer = GetComponent<MeshRenderer>();
        if (enemyMeshRenderer != null)
        {
            originalColor = enemyMeshRenderer.material.color;
        }
        else
        {
            Debug.LogError($"No MeshRenderer found on enemy {name}!");
        }
    }

    #endregion

    #region Damage System

    /// <summary>
    /// Applies damage to the enemy and triggers death if health reaches zero.
    /// </summary>
    /// <param name="damage">The amount of damage to apply.</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        ApplyDamage(damage);
        TriggerDamageEffects();
        CheckForDeath();
    }

    /// <summary>
    /// Applies the damage to the enemy's health.
    /// </summary>
    /// <param name="damage">The amount of damage to apply.</param>
    private void ApplyDamage(float damage)
    {
        currentHealth -= damage;
        
        if (showDebug) 
        {
            Debug.Log($"Enemy {name} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        }
    }

    /// <summary>
    /// Triggers visual effects when damage is taken.
    /// </summary>
    private void TriggerDamageEffects()
    {
        StartCoroutine(DamageFlashEffect());
    }

    /// <summary>
    /// Checks if the enemy should die based on current health.
    /// </summary>
    private void CheckForDeath()
    {
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// Coroutine that creates a flash effect when the enemy takes damage.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator DamageFlashEffect()
    {
        if (enemyMeshRenderer != null)
        {
            enemyMeshRenderer.material.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration);
            enemyMeshRenderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// Draws health debug information on screen.
    /// </summary>
    private void DrawHealthDebug()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 50, 100, 20), 
            $"{currentHealth}/{maxHealth}");
    }

    #endregion

    #region Death System

    /// <summary>
    /// Handles the enemy's death, including rewards and cleanup.
    /// </summary>
    private void Die()
    {
        isDead = true;
        
        if (showDebug) 
        {
            Debug.Log($"Enemy {gameObject.name} has died");
        }
        
        AwardExperience();
        DropLoot();
        DisableCollision();
        StartDeathSequence();
    }

    /// <summary>
    /// Awards experience to the player based on enemy level and multipliers.
    /// </summary>
    private void AwardExperience()
    {
        float finalExperienceReward = experienceReward * (1 + (enemyLevel - 1) * experienceMultiplier);
        
        if (playerStats != null)
        {
            playerStats.GainExperience(finalExperienceReward);
            if (showDebug) 
            {
                Debug.Log($"Gave {finalExperienceReward} experience to player");
            }
        }
        else
        {
            Debug.LogWarning("PlayerStats not found! No experience given.");
        }
    }

    /// <summary>
    /// Triggers loot drops from the enemy's loot table.
    /// </summary>
    private void DropLoot()
    {
        if (lootTable != null)
        {
            LootDropManager.Instance.DropLootFromEnemy(lootTable, transform.position, enemyLevel);
        }
    }

    /// <summary>
    /// Disables collision to prevent further interactions.
    /// </summary>
    private void DisableCollision()
    {
        if (TryGetComponent<Collider>(out var collider))
        {
            collider.enabled = false;
        }
    }

    /// <summary>
    /// Starts the death sequence with visual effects and destruction.
    /// </summary>
    private void StartDeathSequence()
    {
        StartCoroutine(DamageFlashEffect());
        Destroy(gameObject, 0.5f);
    }

    #endregion
}
