using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{

    [Header("Leveling")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private float currentExperience = 0f;
    [SerializeField] private float experienceToNextLevel = 100f;
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

    private PlayerController playerController;
    private PlayerAttack playerAttack;

    private float currentHealth;
    private float currentAttack;
    private float currentDefense;
    private float currentSpeed;

    public event Action<int> OnLevelUp;
    public event Action<float> OnExperienceGained;
    public event Action<float> OnHealthChanged;
    public event Action<float> OnAttackChanged;
    public event Action<float> OnDefenseChanged;
    public event Action<float> OnSpeedChanged;
    public event Action<float> OnCooldownChanged;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();

        if (playerController == null)
        {
            Debug.LogError("PlayerController component not found on player!");
        }

        if (playerAttack == null)
        {
            Debug.LogError("PlayerAttack component not found on player!");
        }
    }

    private void Start() 
    {
        UpdateStats();
        if (showDebug)
        {
            Debug.Log($"Player stats initialized: Level {currentLevel}, Health {currentHealth}, Attack {currentAttack}, Defense {currentDefense}, Speed {currentSpeed}");
        }
    }

    public void GainExperience(float amount)
    {
        currentExperience += amount;
        OnExperienceGained?.Invoke(currentExperience);
        if (showDebug) Debug.Log($"Player gained {amount} experience. Current experience: {currentExperience}");
        while (currentExperience >= experienceToNextLevel)
        {
            currentExperience -= experienceToNextLevel;
            LevelUp();
        }
    }

    private void UpdateStats()
    {
        float levelBonus = currentLevel - 1;
        currentHealth = baseHealth + (healthGrowth * levelBonus);
        currentAttack = baseAttack + (attackGrowth * levelBonus);
        currentDefense = baseDefense + (defenseGrowth * levelBonus);
        currentSpeed = baseSpeed + (speedGrowth * levelBonus);

        // Calculate and notify of cooldown changes
        float newCooldown = CalculateAttackCooldown();
        OnCooldownChanged?.Invoke(newCooldown);

        OnHealthChanged?.Invoke(currentHealth);
        OnAttackChanged?.Invoke(currentAttack);
        OnDefenseChanged?.Invoke(currentDefense);
        OnSpeedChanged?.Invoke(currentSpeed);
    }

    public float CalculateDamageTaken(float incomingDamage)
    {
        float damageReduction = currentDefense / (currentDefense + 100);
        float finalDamage = incomingDamage * (1 - damageReduction);
        return finalDamage;
        
    }
    
    public void ModifyStats(float HealthModifier, float AttackModifier, float DefenseModifier, float SpeedModifier)
    {
        currentHealth += HealthModifier;
        currentAttack += AttackModifier;
        currentDefense += DefenseModifier;
        currentSpeed += SpeedModifier;
        UpdateStats();
        
    }

    private void OnGUI() // Debug UI
    {
        if (!showDebug) return;
        // Player Stats Box
        GUI.Box(new Rect(10, 10, 200, 220), "Player Stats");
        // Level, Experience, Health, Attack, Defense, Speed
        GUI.Label(new Rect(20, 30, 180, 20), $"Level: {currentLevel}");
        GUI.Label(new Rect(20, 50, 180, 20), $"Experience: {currentExperience}/{experienceToNextLevel}");
        GUI.Label(new Rect(20, 70, 180, 20), $"Health: {currentHealth}/{baseHealth}");
        GUI.Label(new Rect(20, 90, 180, 20), $"Attack: {currentAttack}");
        GUI.Label(new Rect(20, 110, 180, 20), $"Defense: {currentDefense}");
        GUI.Label(new Rect(20, 130, 180, 20), $"Speed: {currentSpeed}");
    }

    private void LevelUp()
    {
        currentLevel++;
        experienceToNextLevel *= experienceMultiplier;
        OnLevelUp?.Invoke(currentLevel);
        UpdateStats();
        if (showDebug)
        { 
            Debug.Log($"Player leveled up to level {currentLevel}");
            Debug.Log($"New stats: Health {currentHealth}, Attack {currentAttack}, Defense {currentDefense}, Speed {currentSpeed}");
        
        }
    }

    private float CalculateAttackCooldown()
    {
       float speedModifier = 1f - (currentSpeed / speedToAttackCooldown);
       return Mathf.Max(0.1f, attackCooldown * speedModifier, 0.5f);
    }

    public int CurrentLevel => currentLevel;
    public float CurrentExperience => currentExperience;
    public float ExperienceToNextLevel => experienceToNextLevel;
    public float CurrentHealth => currentHealth;
    public float CurrentAttack => currentAttack;
    public float CurrentDefense => currentDefense;
    public float CurrentSpeed => currentSpeed;
    public float CurrentCooldown => CalculateAttackCooldown();
}
