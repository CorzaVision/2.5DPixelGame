using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class HealthBarUI : MonoBehaviour
{

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

    private PlayerStats playerStats;
    private PlayerInput playerInput;
    private InputAction testDamageAction;
    private InputAction testHealAction;
    private float lastHealth;
    private bool isRegenerating;
    private Coroutine currentRegenEffect;

    private void Awake()
    {
        // Find and validate the PlayerController
        playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not found in the scene.");
            return;
        }

        // Get and validate the PlayerInput component
        playerInput = playerStats.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component not found on PlayerStats.");
            return;
        }

        // Get and validate UI components
        if (healthSlider == null)
        {
            Debug.LogError("Health Slider not assigned in the inspector.");
            return;
        }

        if (fillImage == null)
        {
            Debug.LogError("Fill Image not assigned in the inspector.");
            return;
        }

        // Initialize health values
        healthSlider.maxValue = playerStats.CurrentHealth;
        healthSlider.value = playerStats.CurrentHealth;
        fillImage.color = healthGradient.Evaluate(healthSlider.normalizedValue);

        // Initialize health text if enabled
        if (showHealthText && healthText != null)
        {
            healthText.text = string.Format(healthTextFormat, 
                playerStats.CurrentHealth, 
                playerStats.CurrentHealth);
        }

        lastHealth = playerStats.CurrentHealth;
        isRegenerating = false;

        // Get input actions
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

    private void Start()
    {
        // Subscribe to events only if we have a valid player stats
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateHealthUI;
          //  playerStats.OnDeath += OnPlayerDeath;
        }
    }

    private void OnEnable()
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

    private void OnDisable()
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

    private void OnTestDamagePerformed(InputAction.CallbackContext context)
    {
        if (!enableTestControls) return;
        playerStats.ModifyStats(0, 0, 0, -testDamageAmount);
        if (showDebug) Debug.Log($"Test: Took {testDamageAmount} damage");
    }

    private void OnTestHealPerformed(InputAction.CallbackContext context)
    {
        if (!enableTestControls) return;
        playerStats.ModifyStats(testHealAmount, 0, 0, 0);
        if (showDebug) Debug.Log($"Test: Healed {testHealAmount} health");
    }

    // Update the health UI
    void UpdateHealthUI(float currentHealth)
    {
        // Check if health decreased (damage)
        if (currentHealth < lastHealth)
        {
            StartCoroutine(DamageFlashEffect());
        }

        // Check if health increased (regeneration)
        if (currentHealth > lastHealth)
        {
            if (!isRegenerating)
            {
                // Stop any existing regeneration effect
                if (currentRegenEffect != null)
                {
                    StopCoroutine(currentRegenEffect);
                }
                
                isRegenerating = true;
                currentRegenEffect = StartCoroutine(RegenerationEffect());
                
                if (showDebug)
                {
                    Debug.Log($"Started regeneration effect. Health: {currentHealth}/{playerStats.CurrentHealth}");
                }
            }
        }

        lastHealth = currentHealth;

        // Update the health slider value
       healthSlider.value = currentHealth;

       // Update the fill color based on the health percentage
       fillImage.color = healthGradient.Evaluate(healthSlider.normalizedValue);

       // Update the health text if enabled
       if(showHealthText && healthText != null)
       {
        healthText.text = string.Format(healthTextFormat, 
        currentHealth, 
        playerStats.CurrentHealth);
       }

       if(showDebug)
       {
        Debug.Log($"Health UI Updated: {currentHealth}/{playerStats.CurrentHealth}, Regenerating: {isRegenerating}");
       }

    }

    private System.Collections.IEnumerator DamageFlashEffect()
    {
        Color originalColor = fillImage.color;
        
        for (int i = 0; i < damageFlashCount; i++)
        {
            // Flash to damage color
            fillImage.color = damageFlashColor;
            yield return new WaitForSeconds(damageFlashDuration / 2);
            
            // Return to original color
            fillImage.color = originalColor;
            yield return new WaitForSeconds(damageFlashDuration / 2);
        }
    }

    private System.Collections.IEnumerator RegenerationEffect()
    {
        Color originalColor = fillImage.color;
        float elapsedTime = 0f;
        isRegenerating = true;

        while (elapsedTime < regenEffectDuration)
        {
            elapsedTime += Time.deltaTime * regenPulseSpeed;
            
            // Pulse between original color and regen color
            float pulse = Mathf.Sin(elapsedTime * Mathf.PI * 2) * regenPulseScale;
            Color targetColor = Color.Lerp(originalColor, regenPulseColor, pulse);
            fillImage.color = targetColor;
            
            if (showDebug)
            {
                Debug.DrawRay(transform.position, Vector3.up * pulse, regenPulseColor);
            }
            
            yield return null;
        }

        // Reset to original state
        fillImage.color = healthGradient.Evaluate(healthSlider.normalizedValue);
        isRegenerating = false;
        currentRegenEffect = null;

        if (showDebug)
        {
            Debug.Log("Regeneration effect completed");
        }
    }

    private void OnPlayerDeath()
    {
        if (showDebug) Debug.Log("Player Death Event Triggered");
        StartCoroutine(DeathEffects()); // Start the death effects
    }

    private System.Collections.IEnumerator DeathEffects() // Coroutine for death effects
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

        if (showDebug) Debug.Log("Death Effects Completed");
    }

}
