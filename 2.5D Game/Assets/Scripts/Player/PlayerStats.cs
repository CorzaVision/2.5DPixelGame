using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{

    [Header("Leveling")]
    [SerializeField] private int initialLevel = 1;
    [SerializeField] private float experienceMultiplier = 1.1f;
    
    [Header("Base Stats")]
    [SerializeField] private float baseHealth = 100f;
    [SerializeField] private float baseAttack = 10f;
    [SerializeField] private float baseDefense = 0f;
    [SerializeField] private float baseSpeed = 10f;

    [Header("Stat Growth")]
    [SerializeField] private float healthGrowth = 10f;
    [SerializeField] private float attackGrowth = 1f;
    [SerializeField] private float defenseGrowth = 0.5f;
    [SerializeField] private float speedGrowth = 0.5f;

    [Header("Cooldown Settings")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float speedToAttackCooldown = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private PlayerAttack playerAttack;
    private PlayerInventory playerInventory;
    private float lastDamageTime;
    private bool isDead;

    public ItemInstance CurrentWeapon { get; private set; }

    public event Action<int> OnLevelUp;
    public event Action<float> OnExperienceGained;
    public event Action<float> OnHealthChanged;
    public event Action<float> OnAttackChanged;
    public event Action<float> OnDefenseChanged;
    public event Action<float> OnSpeedChanged;
    public event Action<float> OnCooldownChanged;
    public event Action OnDeath;

    // --- Public Properties for Stats ---
    public int CurrentLevel { get; private set; }
    public float CurrentExperience { get; private set; }
    public float ExperienceToNextLevel { get; private set; }
    public float CurrentHealth { get; private set; }
    public float CurrentAttack { get; private set; }
    public float CurrentDefense { get; private set; }
    public float CurrentSpeed { get; private set; }
    public float CurrentCooldown => CalculateAttackCooldown();


    private void Awake()
    {
        playerAttack = GetComponent<PlayerAttack>();
        playerInventory = GetComponent<PlayerInventory>();

        if (playerAttack == null)
        {
            Debug.LogError("PlayerAttack component not found on player!");
        }

        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory component not found on player!");
        }

        // Initialize public properties from serialized fields
        CurrentLevel = initialLevel;
        
        UpdateStats(); // This will set up attack, defense, etc.
        ExperienceToNextLevel = baseHealth * experienceMultiplier; // Initial EXP to level
        CurrentHealth = GetMaxHealth(); // Start with full health
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    private void Start() 
    {
        if (showDebug)
        {
            Debug.Log($"Player stats initialized: Level {CurrentLevel}, Health {CurrentHealth}, Attack {CurrentAttack}, Defense {CurrentDefense}, Speed {CurrentSpeed}");
        }
    }

    private void Update()
    {
        // Handle health regeneration
        if (!isDead && Time.time - lastDamageTime >= 1f) // Using a fixed 1s delay
        {
            Heal(1f * Time.deltaTime); // Using a fixed 1hp/sec regen rate
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        float damageTaken = CalculateDamageTaken(damage);
        CurrentHealth = Mathf.Max(CurrentHealth - damageTaken, 0);
        lastDamageTime = Time.time;
        OnHealthChanged?.Invoke(CurrentHealth);

        if (showDebug) Debug.Log($"Player took {damageTaken} damage, health is now {CurrentHealth}");

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        float maxHealth = GetMaxHealth();
        if (CurrentHealth < maxHealth)
        {
            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(CurrentHealth);
        }
    }
    
    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
        if (showDebug) Debug.Log("Player has died.");
        // You can disable player input or trigger other death effects here
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void EquipWeapon(ItemInstance weapon)
    {
        if (CurrentWeapon != null)
        {
            playerInventory.AddItemInstance(CurrentWeapon);
        }

        playerInventory.RemoveItemInstance(weapon);

        CurrentWeapon = weapon;
        UpdateStats();
    }

    public void UnequipWeapon()
    {
        if (CurrentWeapon == null) return;

        playerInventory.AddItemInstance(CurrentWeapon);

        CurrentWeapon = null;
        UpdateStats();
        Debug.Log("Weapon unequipped.");
    }

    public void UseConsumable(ItemInstance item)
    {
        if (item.itemData.itemType == ItemType.Consumable)
        {
            if (item.itemData.consumableSubType == ConsumableSubType.Potion)
            {
                if (CurrentHealth < GetMaxHealth() && item.count > 0)
                {
                    Heal(item.itemData.healthRestore);
                    item.count--;
                    Debug.Log("Consumed " + item.itemData.itemName + " for " + item.itemData.healthRestore + " health");
                }
                else
                {
                    Debug.Log("No health to restore as it is already at max or no consumable left");
                }
            }
        }
    }

    public void GainExperience(float amount)
    {
        float remainingExperience = CurrentExperience + amount;
        while (remainingExperience >= ExperienceToNextLevel)
        {
            remainingExperience -= ExperienceToNextLevel;
            LevelUp();
        }
        CurrentExperience = remainingExperience;
        OnExperienceGained?.Invoke(CurrentExperience);

        if (showDebug) Debug.Log($"Player gained {amount} experience. Current experience: {CurrentExperience}");
    }

    private void UpdateStats()
    {
        float levelBonus = CurrentLevel - 1;
        CurrentHealth = baseHealth + (healthGrowth * levelBonus);
        CurrentAttack = baseAttack + (attackGrowth * levelBonus);

        if (CurrentWeapon != null)
            CurrentAttack += CurrentWeapon.itemData.damage;

        CurrentDefense = baseDefense + (defenseGrowth * levelBonus);
        CurrentSpeed = baseSpeed + (speedGrowth * levelBonus);

        float newCooldown = CalculateAttackCooldown();
        OnCooldownChanged?.Invoke(newCooldown);

        OnHealthChanged?.Invoke(CurrentHealth);
        OnAttackChanged?.Invoke(CurrentAttack);
        OnDefenseChanged?.Invoke(CurrentDefense);
        OnSpeedChanged?.Invoke(CurrentSpeed);
    }

    public float GetMaxHealth()
    {
        return baseHealth + (healthGrowth * (CurrentLevel - 1));
    }

    public float CalculateDamageTaken(float incomingDamage)
    {
        float damageReduction = CurrentDefense / (CurrentDefense + 100);
        float finalDamage = incomingDamage * (1 - damageReduction);
        return finalDamage;
    }
    
    public void ModifyStats(float HealthModifier, float AttackModifier, float DefenseModifier, float SpeedModifier)
    {
        CurrentHealth += HealthModifier;
        CurrentAttack += AttackModifier;
        CurrentDefense += DefenseModifier;
        CurrentSpeed += SpeedModifier;
        UpdateStats();
    }

    private void OnGUI() 
    {
        if (!showDebug) return;
        GUI.Box(new Rect(10, 10, 200, 220), "Player Stats");
        GUI.Label(new Rect(20, 30, 180, 20), $"Level: {CurrentLevel}");
        GUI.Label(new Rect(20, 50, 180, 20), $"Experience: {CurrentExperience}/{ExperienceToNextLevel}");
        GUI.Label(new Rect(20, 70, 180, 20), $"Health: {CurrentHealth}/{GetMaxHealth()}");
        GUI.Label(new Rect(20, 90, 180, 20), $"Attack: {CurrentAttack}");
        GUI.Label(new Rect(20, 110, 180, 20), $"Defense: {CurrentDefense}");
        GUI.Label(new Rect(20, 130, 180, 20), $"Speed: {CurrentSpeed}");
    }

    private void LevelUp()
    {
        CurrentLevel++;
        ExperienceToNextLevel *= experienceMultiplier;
        OnLevelUp?.Invoke(CurrentLevel);
        UpdateStats();
        if (showDebug)
        { 
            Debug.Log($"Player leveled up to level {CurrentLevel}");
            Debug.Log($"New stats: Health {CurrentHealth}, Attack {CurrentAttack}, Defense {CurrentDefense}, Speed {CurrentSpeed}");
        }
    }

    private float CalculateAttackCooldown()
    {
       float speedModifier = 1f - (CurrentSpeed / speedToAttackCooldown);
       return Mathf.Max(0.1f, attackCooldown * speedModifier, 0.5f);
    }
}
