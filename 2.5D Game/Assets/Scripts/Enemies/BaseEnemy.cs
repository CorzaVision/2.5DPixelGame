using UnityEngine;
using Interface;
using System.Collections;

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

    private PlayerStats playerStats;
    private MeshRenderer enemyMeshRenderer;
    private Color originalColor;
    private bool isDead;
     


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on player!");
        }

        currentHealth = maxHealth;

        enemyMeshRenderer = GetComponent<MeshRenderer>();
        if (enemyMeshRenderer != null)
        {
            originalColor = enemyMeshRenderer.material.color;
        }
        else
        {
            Debug.LogError($"No MeshRenderer found on enemy {name}!");
        }

        if (showDebug) Debug.Log($"Enemy {name} initialized with {maxHealth} health");
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        if (showDebug) Debug.Log($"Enemy {name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        StartCoroutine(DamageFlashEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator DamageFlashEffect()
    {
        if (enemyMeshRenderer != null)
        {
            enemyMeshRenderer.material.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration);

            enemyMeshRenderer.material.color = originalColor;
        }
    }

    private void Die()
    {
        isDead = true;
        if (showDebug) Debug.Log($"Enemy {gameObject.name} has died");
        
        // Calculate experience reward with multiplier
        float finalExperienceReward = experienceReward * (1 + (enemyLevel - 1) * experienceMultiplier);
        
        // Give experience to player
        if (playerStats != null)
        {
            playerStats.GainExperience(finalExperienceReward);
            if (showDebug) Debug.Log($"Gave {finalExperienceReward} experience to player");
        }
        else
        {
            Debug.LogWarning("PlayerStats not found! No experience given.");
        }
        
        if (lootTable != null)
        {
            LootDropManager.Instance.DropLootFromEnemy(lootTable, transform.position, enemyLevel);
        }
        
        if (TryGetComponent<Collider>(out var collider))
        {
            collider.enabled = false;
        }
        
        StartCoroutine(DamageFlashEffect());
        Destroy(gameObject, 0.5f);
    }

    private void OnGUI()
    {
        if (!showDebug) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 50, 100, 20), 
        $"{currentHealth}/{maxHealth}");
    }

}
