using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the health bar UI display, including visual effects for damage, healing, and death.
/// This script provides real-time health updates with various visual feedback effects.
/// </summary>
public class HealthBarUI : MonoBehaviour
{
    #region Serialized Fields

    [Header("UI Elements")]
    [Tooltip("The main health bar slider component")]
    [SerializeField] private Slider healthSlider;
    [Tooltip("The image that fills the health bar")]
    [SerializeField] private Image fillImage;
    [Tooltip("Text component to display current/max health")]
    [SerializeField] private TextMeshProUGUI healthText;
    [Tooltip("Color gradient for the health bar (left to right)")]
    [SerializeField] private Gradient healthGradient;

    [Header("Settings")]
    [Tooltip("Enable debug logs and visualizations")]
    [SerializeField] private bool showDebug = true;
    [Tooltip("Show the numerical health value")]
    [SerializeField] private bool showHealthText = true;
    [Tooltip("Format for health text (0 = current, 1 = max)")]
    [SerializeField] private string healthTextFormat = "{0:F0}/{1:F0}";

    [Header("Death Effects")]
    [Tooltip("How long the death effect lasts")]
    [SerializeField] private float deathFadeDuration = 1f;
    [Tooltip("Color to fade to on death")]
    [SerializeField] private Color deathColor = Color.red;
    [Tooltip("How much the health bar scales on death")]
    [SerializeField] private float deathScale = 1.2f;

    [Header("Damage Flash")]
    [Tooltip("Color to flash when taking damage")]
    [SerializeField] private Color damageFlashColor = Color.red;
    [Tooltip("How long each flash lasts")]
    [SerializeField] private float damageFlashDuration = 0.2f;
    [Tooltip("How many times to flash when taking damage")]
    [SerializeField] private int damageFlashCount = 2;

    [Header("Regen Effects")]
    [Tooltip("How fast the regeneration effect pulses")]
    [SerializeField] private float regenPulseSpeed = 1f;
    [Tooltip("How intense the regeneration pulse is")]
    [SerializeField] private float regenPulseScale = 1.2f;
    [Tooltip("Color to pulse during regeneration")]
    [SerializeField] private Color regenPulseColor = Color.green;
    [Tooltip("How long the regeneration effect lasts")]
    [SerializeField] private float regenEffectDuration = 1f;
    [Tooltip("How much the health bar scales during regeneration")]
    [SerializeField] private float regenScaleEffect = 0.1f;

    [Header("Testing")]
    [Tooltip("Enable test controls (T for damage, H for heal)")]
    [SerializeField] private bool enableTestControls = false;
    [Tooltip("Amount of damage to apply when testing")]
    [SerializeField] private float testDamageAmount = 10f;
    [Tooltip("Amount of health to restore when testing")]
    [SerializeField] private float testHealAmount = 10f;

    #endregion

    #region Private Fields

    private PlayerStats playerStats;
    private PlayerInput playerInput;
    private InputAction testDamageAction;
    private InputAction testHealAction;
    private float lastHealth;
    private bool isRegenerating;
    private Coroutine currentRegenEffect;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        InitializeComponents();
        SetupInputActions();
        InitializeHealthUI();
    }

    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes and validates all required components.
    /// </summary>
    private void InitializeComponents()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not found in the scene.");
            return;
        }

        playerInput = playerStats.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component not found on PlayerStats.");
            return;
        }

        ValidateUIComponents();
    }

    /// <summary>
    /// Validates that all UI components are properly assigned.
    /// </summary>
    private void ValidateUIComponents()
    {
        if (healthSlider == null)
        {
            Debug.LogError("Health Slider not assigned in the inspector.");
        }

        if (fillImage == null)
        {
            Debug.LogError("Fill Image not assigned in the inspector.");
        }
    }

    /// <summary>
    /// Sets up input actions for testing.
    /// </summary>
    private void SetupInputActions()
    {
        var playerMap = playerInput.actions.FindActionMap("Player");
        if (playerMap != null)
        {
            playerMap.Enable();
            testDamageAction = playerMap.FindAction("TestDamage");
            testHealAction = playerMap.FindAction("TestHeal");
        }
        else
        {
            Debug.LogError("Player action map not found in Input Actions.");
        }
    }

    /// <summary>
    /// Initializes the health UI with current player stats.
    /// </summary>
    private void InitializeHealthUI()
    {
        if (playerStats == null) return;

        healthSlider.maxValue = playerStats.CurrentHealth;
        healthSlider.value = playerStats.CurrentHealth;
        fillImage.color = healthGradient.Evaluate(healthSlider.normalizedValue);

        if (showHealthText && healthText != null)
        {
            healthText.text = string.Format(healthTextFormat, 
                playerStats.CurrentHealth, 
                playerStats.CurrentHealth);
        }

        lastHealth = playerStats.CurrentHealth;
        isRegenerating = false;
    }

    #endregion

    #region Event Management

    /// <summary>
    /// Subscribes to player stats events.
    /// </summary>
    private void SubscribeToEvents()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateHealthUI;
            // playerStats.OnDeath += OnPlayerDeath;
        }
    }

    /// <summary>
    /// Enables input actions for testing.
    /// </summary>
    private void EnableInputActions()
    {
        if (testDamageAction != null)
        {
            testDamageAction.performed += OnTestDamagePerformed;
            testDamageAction.Enable();
        }
        if (testHealAction != null)
        {
            testHealAction.performed += OnTestHealPerformed;
            testHealAction.Enable();
        }
    }

    /// <summary>
    /// Disables input actions for testing.
    /// </summary>
    private void DisableInputActions()
    {
        if (testDamageAction != null)
        {
            testDamageAction.performed -= OnTestDamagePerformed;
            testDamageAction.Disable();
        }
        if (testHealAction != null)
        {
            testHealAction.performed -= OnTestHealPerformed;
            testHealAction.Disable();
        }
    }

    #endregion

    #region Input Event Handlers

    /// <summary>
    /// Handles test damage input.
    /// </summary>
    /// <param name="context">The input action callback context.</param>
    private void OnTestDamagePerformed(InputAction.CallbackContext context)
    {
        if (!enableTestControls) return;
        playerStats.ModifyStats(0, 0, 0, -testDamageAmount);
        if (showDebug) 
        {
            Debug.Log($"Test: Took {testDamageAmount} damage");
        }
    }

    /// <summary>
    /// Handles test heal input.
    /// </summary>
    /// <param name="context">The input action callback context.</param>
    private void OnTestHealPerformed(InputAction.CallbackContext context)
    {
        if (!enableTestControls) return;
        playerStats.ModifyStats(testHealAmount, 0, 0, 0);
        if (showDebug) 
        {
            Debug.Log($"Test: Healed {testHealAmount} health");
        }
    }

    #endregion

    #region Health UI Updates

    /// <summary>
    /// Updates the health UI with the current health value.
    /// </summary>
    /// <param name="currentHealth">The current health value.</param>
    private void UpdateHealthUI(float currentHealth)
    {
        HandleHealthChange(currentHealth);
        UpdateHealthDisplay(currentHealth);
        LogHealthUpdate(currentHealth);
    }

    /// <summary>
    /// Handles health changes and triggers appropriate effects.
    /// </summary>
    /// <param name="currentHealth">The current health value.</param>
    private void HandleHealthChange(float currentHealth)
    {
        // Check if health decreased (damage)
        if (currentHealth < lastHealth)
        {
            StartCoroutine(DamageFlashEffect());
        }

        // Check if health increased (regeneration)
        if (currentHealth > lastHealth)
        {
            HandleRegeneration();
        }

        lastHealth = currentHealth;
    }

    /// <summary>
    /// Handles regeneration effects when health increases.
    /// </summary>
    private void HandleRegeneration()
    {
        if (!isRegenerating)
        {
            if (currentRegenEffect != null)
            {
                StopCoroutine(currentRegenEffect);
            }
            
            isRegenerating = true;
            currentRegenEffect = StartCoroutine(RegenerationEffect());
            
            if (showDebug)
            {
                Debug.Log($"Started regeneration effect. Health: {lastHealth}/{playerStats.CurrentHealth}");
            }
        }
    }

    /// <summary>
    /// Updates the visual health display elements.
    /// </summary>
    /// <param name="currentHealth">The current health value.</param>
    private void UpdateHealthDisplay(float currentHealth)
    {
        healthSlider.maxValue = playerStats.CurrentHealth;
        healthSlider.value = currentHealth;
        fillImage.color = healthGradient.Evaluate(healthSlider.normalizedValue);

        if (showHealthText && healthText != null)
        {
            healthText.text = string.Format(healthTextFormat, 
                currentHealth, 
                playerStats.CurrentHealth);
        }
    }

    /// <summary>
    /// Logs health updates for debugging.
    /// </summary>
    /// <param name="currentHealth">The current health value.</param>
    private void LogHealthUpdate(float currentHealth)
    {
        if (showDebug)
        {
            Debug.Log($"Health UI Updated: {currentHealth}/{playerStats.CurrentHealth}, Regenerating: {isRegenerating}");
        }
    }

    #endregion

    #region Visual Effects

    /// <summary>
    /// Coroutine that creates a flash effect when taking damage.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator DamageFlashEffect()
    {
        Color originalColor = fillImage.color;
        
        for (int i = 0; i < damageFlashCount; i++)
        {
            fillImage.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration / 2);
            
            fillImage.color = originalColor;
            yield return new WaitForSeconds(damageFlashDuration / 2);
        }
    }

    /// <summary>
    /// Coroutine that creates a pulsing effect during regeneration.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator RegenerationEffect()
    {
        Color originalColor = fillImage.color;
        float elapsedTime = 0f;
        isRegenerating = true;

        while (elapsedTime < regenEffectDuration)
        {
            elapsedTime += Time.deltaTime * regenPulseSpeed;
            
            float pulse = Mathf.Sin(elapsedTime * Mathf.PI * 2) * regenPulseScale;
            Color targetColor = Color.Lerp(originalColor, regenPulseColor, pulse);
            fillImage.color = targetColor;
            
            if (showDebug)
            {
                Debug.DrawRay(transform.position, Vector3.up * pulse, regenPulseColor);
            }
            
            yield return null;
        }

        fillImage.color = healthGradient.Evaluate(healthSlider.normalizedValue);
        isRegenerating = false;
        currentRegenEffect = null;

        if (showDebug)
        {
            Debug.Log("Regeneration effect completed");
        }
    }

    #endregion

    #region Death Effects

    /// <summary>
    /// Handles player death events.
    /// </summary>
    private void OnPlayerDeath()
    {
        if (showDebug) 
        {
            Debug.Log("Player Death Event Triggered");
        }
        StartCoroutine(DeathEffects());
    }

    /// <summary>
    /// Coroutine that creates death visual effects.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator DeathEffects()
    {
        float elapsedTime = 0f;
        Color startColor = fillImage.color;

        while (elapsedTime < deathFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / deathFadeDuration;

            fillImage.color = Color.Lerp(startColor, deathColor, t);

            float scale = Mathf.Lerp(1f, deathScale, t);
            healthSlider.transform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        fillImage.color = deathColor;
        healthSlider.transform.localScale = Vector3.one;

        if (showDebug) 
        {
            Debug.Log("Death Effects Completed");
        }
    }

    #endregion
}
