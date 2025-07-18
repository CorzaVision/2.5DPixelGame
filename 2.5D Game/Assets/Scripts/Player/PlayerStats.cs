using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages all player statistics including health, attack, defense, speed, leveling, and equipment.
/// This script handles stat calculations, damage processing, healing, experience gain, and weapon equipping.
/// </summary>
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

    [Header("Currency")]
    [SerializeField] private int copper = 0;
    [SerializeField] private int silver = 0;
    [SerializeField] private int gold = 0;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private Dictionary<CurrencyType, int> currencyAmounts = new Dictionary<CurrencyType, int>();

    // Components
    private PlayerAttack playerAttack;
    private PlayerInventory playerInventory;

    // State Variables
    private float lastDamageTime;
    private bool isDead;

    // Equipment
    public ItemInstance CurrentWeapon { get; private set; }

    // Events
    public event Action<int> OnLevelUp;
    public event Action<float> OnExperienceGained;
    public event Action<float> OnHealthChanged;
    public event Action<float> OnAttackChanged;
    public event Action<float> OnDefenseChanged;
    public event Action<float> OnSpeedChanged;
    public event Action<float> OnCooldownChanged;
    public event Action OnDeath;
    public event Action<CurrencyType, int> OnCurrencyGained;
    public event Action<CurrencyType, int> OnCurrencySpent;
    public event Action<CurrencyType, int> OnCurrencyChanged;

    // Public Properties
    public int CurrentLevel { get; private set; }
    public float CurrentExperience { get; private set; }
    public float ExperienceToNextLevel { get; private set; }
    public float CurrentHealth { get; private set; }
    public float CurrentAttack { get; private set; }
    public float CurrentDefense { get; private set; }
    public float CurrentSpeed { get; private set; }
    public float CurrentCooldown => CalculateAttackCooldown();
    public int GetCurrency(CurrencyType currencyType)
    {
        return currencyAmounts.ContainsKey(currencyType) ? currencyAmounts[currencyType] : 0;
    }

    public bool HasCurrency(CurrencyType currencyType, int amount)
    {
        return GetCurrency(currencyType) >= amount;
    }

    #region Unity Lifecycle 

    private void Awake()
    {
        InitializeComponents();
        ValidateComponents();
        InitializeStats();
    }

    private void Start() 
    {
        LogInitialStats();
    }

    private void Update()
    {
        HandleHealthRegeneration();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Gets and caches required components.
    /// </summary>
    private void InitializeComponents()
    {
        playerAttack = GetComponent<PlayerAttack>();
        playerInventory = GetComponent<PlayerInventory>();
    }

    /// <summary>
    /// Validates that all required components are present.
    /// </summary>
    private void ValidateComponents()
    {
        if (playerAttack == null)
        {
            Debug.LogError("PlayerAttack component not found on player!");
        }

        if (playerInventory == null)
        {
            Debug.LogError("PlayerInventory component not found on player!");
        }
    }

    /// <summary>
    /// Initializes all player statistics to their starting values.
    /// </summary>
    private void InitializeStats()
    {
        CurrentLevel = initialLevel;
        UpdateStats();
        ExperienceToNextLevel = baseHealth * experienceMultiplier;
        CurrentHealth = GetMaxHealth();
        InitializeCurrency();
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    /// <summary>
    /// Logs initial stats for debugging purposes.
    /// </summary>
    private void LogInitialStats()
    {
        if (showDebug)
        {
            Debug.Log($"Player stats initialized: Level {CurrentLevel}, Health {CurrentHealth}, Attack {CurrentAttack}, Defense {CurrentDefense}, Speed {CurrentSpeed}");
        }
    }

    private void InitializeCurrency()
    {
        currencyAmounts[CurrencyType.Copper] = copper;
        currencyAmounts[CurrencyType.Silver] = silver;
        currencyAmounts[CurrencyType.Gold] = gold;
        
        // Notify UI of initial currency amounts
        foreach (var currency in currencyAmounts)
        {
            OnCurrencyChanged?.Invoke(currency.Key, currency.Value);
        }
    }

    #endregion

    #region Health Management

    /// <summary>
    /// Handles automatic health regeneration over time.
    /// </summary>
    private void HandleHealthRegeneration()
    {
        if (!isDead && Time.time - lastDamageTime >= 1f)
        {
            Heal(1f * Time.deltaTime);
        }
    }

    /// <summary>
    /// Applies damage to the player, calculating defense reduction.
    /// </summary>
    /// <param name="damage">The raw damage amount to apply.</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        float damageTaken = CalculateDamageTaken(damage);
        CurrentHealth = Mathf.Max(CurrentHealth - damageTaken, 0);
        lastDamageTime = Time.time;
        OnHealthChanged?.Invoke(CurrentHealth);

        if (showDebug) 
        {
            Debug.Log($"Player took {damageTaken} damage, health is now {CurrentHealth}");
        }

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals the player by the specified amount, respecting maximum health.
    /// </summary>
    /// <param name="amount">The amount of health to restore.</param>
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

    /// <summary>
    /// Handles player death, triggering death events and disabling functionality.
    /// </summary>
    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
        
        if (showDebug) 
        {
            Debug.Log("Player has died.");
        }
    }

    /// <summary>
    /// Returns whether the player is currently dead.
    /// </summary>
    /// <returns>True if the player is dead, false otherwise.</returns>
    public bool IsDead()
    {
        return isDead;
    }

    /// <summary>
    /// Gets the maximum health for the current level.
    /// </summary>
    /// <returns>The maximum health value.</returns>
    public float GetMaxHealth()
    {
        return baseHealth + (healthGrowth * (CurrentLevel - 1));
    }

    #endregion

    #region Equipment Management

    /// <summary>
    /// Equips a weapon, moving it from inventory to equipment slot.
    /// </summary>
    /// <param name="weapon">The weapon item to equip.</param>
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

    /// <summary>
    /// Unequips the current weapon, moving it back to inventory.
    /// </summary>
    public void UnequipWeapon()
    {
        if (CurrentWeapon == null) return;

        playerInventory.AddItemInstance(CurrentWeapon);
        CurrentWeapon = null;
        UpdateStats();
        
        Debug.Log("Weapon unequipped.");
    }

    #endregion

    #region Item Usage

    /// <summary>
    /// Uses an item, applying its effects based on item type.
    /// </summary>
    /// <param name="item">The item to use.</param>
    public void UseItem(ItemInstance item)
    {
        if (item.itemData.itemType == ItemType.Consumable)
        {
            UseConsumable(item);
        }
        else if (item.itemData.itemType == ItemType.Currency)
        {
            UseCurrency(item);
        }
    }

    /// <summary>
    /// Uses a consumable item, applying its effects.
    /// </summary>
    /// <param name="item">The consumable item to use.</param>
    public void UseConsumable(ItemInstance item)
    {
        if (item.itemData.itemType != ItemType.Consumable) return;
        
        if (item.itemData.consumableSubType == ConsumableSubType.Potion)
        {
            if (CurrentHealth < GetMaxHealth() && item.count > 0)
            {
                Heal(item.itemData.healthRestore);
                item.count--;
                Debug.Log($"Consumed {item.itemData.itemName} for {item.itemData.healthRestore} health");
            }
            else
            {
                Debug.Log("No health to restore as it is already at max or no consumable left");
            }
        }
    }

    /// <summary>
    /// Uses a currency item, adding it to the player's currency.
    /// </summary>
    /// <param name="item">The currency item to use.</param>
    private void UseCurrency(ItemInstance item)
    {
        if (item.count > 0)
        {
            // Use currencyValue if it's set, otherwise use random range
            int currencyAmount = item.itemData.currencyValue > 0
                ? item.itemData.currencyValue
                : UnityEngine.Random.Range(item.itemData.currencyMinValue, item.itemData.currencyMaxValue + 1);
            
            AddCurrency(item.itemData.currencyType, currencyAmount * item.count);
            item.count = 0; // Remove the item after use
            
            if (showDebug)
            {
                Debug.Log($"Collected {currencyAmount * item.count} {item.itemData.currencyType}");
            }
        }
    }

    #endregion

    #region Experience and Leveling

    /// <summary>
    /// Adds experience points and handles level-ups.
    /// </summary>
    /// <param name="amount">The amount of experience to gain.</param>
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

        if (showDebug) 
        {
            Debug.Log($"Player gained {amount} experience. Current experience: {CurrentExperience}");
        }
    }

    /// <summary>
    /// Handles level-up logic, increasing stats and experience requirements.
    /// </summary>
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

    #endregion

    #region Stat Calculations

    /// <summary>
    /// Updates all current stats based on level and equipment.
    /// </summary>
    private void UpdateStats()
    {
        float levelBonus = CurrentLevel - 1;
        
        CurrentHealth = baseHealth + (healthGrowth * levelBonus);
        CurrentAttack = baseAttack + (attackGrowth * levelBonus);
        CurrentDefense = baseDefense + (defenseGrowth * levelBonus);
        CurrentSpeed = baseSpeed + (speedGrowth * levelBonus);

        // Apply weapon bonus
        if (CurrentWeapon != null)
        {
            CurrentAttack += CurrentWeapon.itemData.damage;
        }

        // Notify listeners of stat changes
        float newCooldown = CalculateAttackCooldown();
        OnCooldownChanged?.Invoke(newCooldown);
        OnHealthChanged?.Invoke(CurrentHealth);
        OnAttackChanged?.Invoke(CurrentAttack);
        OnDefenseChanged?.Invoke(CurrentDefense);
        OnSpeedChanged?.Invoke(CurrentSpeed);
    }

    /// <summary>
    /// Calculates damage reduction based on defense stat.
    /// </summary>
    /// <param name="incomingDamage">The raw incoming damage.</param>
    /// <returns>The final damage after defense reduction.</returns>
    public float CalculateDamageTaken(float incomingDamage)
    {
        float damageReduction = CurrentDefense / (CurrentDefense + 100);
        float finalDamage = incomingDamage * (1 - damageReduction);
        return finalDamage;
    }

    /// <summary>
    /// Calculates attack cooldown based on speed stat.
    /// </summary>
    /// <returns>The current attack cooldown in seconds.</returns>
    private float CalculateAttackCooldown()
    {
        float speedModifier = 1f - (CurrentSpeed / speedToAttackCooldown);
        return Mathf.Max(0.1f, attackCooldown * speedModifier, 0.5f);
    }

    /// <summary>
    /// Modifies stats by the specified amounts (for temporary effects).
    /// </summary>
    /// <param name="healthModifier">Health modification amount.</param>
    /// <param name="attackModifier">Attack modification amount.</param>
    /// <param name="defenseModifier">Defense modification amount.</param>
    /// <param name="speedModifier">Speed modification amount.</param>
    public void ModifyStats(float healthModifier, float attackModifier, float defenseModifier, float speedModifier)
    {
        CurrentHealth += healthModifier;
        CurrentAttack += attackModifier;
        CurrentDefense += defenseModifier;
        CurrentSpeed += speedModifier;
        UpdateStats();
    }

    #endregion


    #region Currency Management


    public void AddCurrency(CurrencyType currencyType, int amount)
    {
        if (amount <= 0) return;

        if (!currencyAmounts.ContainsKey(currencyType))
        {
            currencyAmounts[currencyType] = 0;
        }

        currencyAmounts[currencyType] += amount;

        ConvertCurrency(currencyType);
        OnCurrencyGained?.Invoke(currencyType, amount);
        OnCurrencyChanged?.Invoke(currencyType, currencyAmounts[currencyType]);

        if (showDebug)
        {
            Debug.Log($"Player gained {amount} {currencyType} total: {currencyAmounts[currencyType]}");
        }
    }

    public bool SpendCurrency(CurrencyType currencyType, int amount)
    {
        if (amount <= 0) return false;

        if (!HasCurrency(currencyType, amount)) return false;

        currencyAmounts[currencyType] -= amount;
        OnCurrencySpent?.Invoke(currencyType, amount);
        OnCurrencyChanged?.Invoke(currencyType, currencyAmounts[currencyType]);

        if (showDebug)
        {
            Debug.Log($"Player spent {amount} {currencyType} total: {currencyAmounts[currencyType]}");
        }

        return true;
    }

    /// <summary>
    /// Converts excess currency to higher denominations (auto-conversion).
    /// </summary>
    /// <param name="currencyType">The currency type to check for conversion.</param>
    private void ConvertCurrency(CurrencyType currencyType)
    {
        // Convert Copper to Silver (100 Copper = 1 Silver)
        if (currencyType == CurrencyType.Copper && currencyAmounts[CurrencyType.Copper] >= 100)
        {
            int silverToAdd = currencyAmounts[CurrencyType.Copper] / 100;
            currencyAmounts[CurrencyType.Copper] %= 100;
            currencyAmounts[CurrencyType.Silver] += silverToAdd;
            
            // Recursively convert Silver to Gold
            ConvertCurrency(CurrencyType.Silver);
            
            if (showDebug)
            {
                Debug.Log($"Converted {silverToAdd} Silver from Copper");
            }
        }
        
        // Convert Silver to Gold (100 Silver = 1 Gold)
        if (currencyType == CurrencyType.Silver && currencyAmounts[CurrencyType.Silver] >= 100)
        {
            int goldToAdd = currencyAmounts[CurrencyType.Silver] / 100;
            currencyAmounts[CurrencyType.Silver] %= 100;
            currencyAmounts[CurrencyType.Gold] += goldToAdd;
            
            if (showDebug)
            {
                Debug.Log($"Converted {goldToAdd} Gold from Silver");
            }
        }
    }

    #endregion

    #region Debug

    /// <summary>
    /// Displays debug information on screen when debug mode is enabled.
    /// </summary>
    private void OnGUI() 
    {
        if (!showDebug) return;
        
        GUI.Box(new Rect(10, 10, 200, 280), "Player Stats");
        GUI.Label(new Rect(20, 30, 180, 20), $"Level: {CurrentLevel}");
        GUI.Label(new Rect(20, 50, 180, 20), $"Experience: {CurrentExperience}/{ExperienceToNextLevel}");
        GUI.Label(new Rect(20, 70, 180, 20), $"Health: {CurrentHealth}/{GetMaxHealth()}");
        GUI.Label(new Rect(20, 90, 180, 20), $"Attack: {CurrentAttack}");
        GUI.Label(new Rect(20, 110, 180, 20), $"Defense: {CurrentDefense}");
        GUI.Label(new Rect(20, 130, 180, 20), $"Speed: {CurrentSpeed}");
        
        GUI.Label(new Rect(20, 160, 180, 20), $"Gold: {GetCurrency(CurrencyType.Gold)}");
        GUI.Label(new Rect(20, 180, 180, 20), $"Silver: {GetCurrency(CurrencyType.Silver)}");
        GUI.Label(new Rect(20, 200, 180, 20), $"Copper: {GetCurrency(CurrencyType.Copper)}");
    }

    #endregion
}
