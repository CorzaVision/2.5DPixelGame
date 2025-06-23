using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the level and experience UI display for the player.
/// This script shows the current level, experience progress, and experience bar.
/// </summary>
public class LevelUI : MonoBehaviour
{
    [Header("Level Display")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private Slider experienceBar;
    [SerializeField] private Image experienceBarFillImage;
    [SerializeField] private Gradient experienceBarColor;

    private PlayerStats playerStats;

    #region Unity Lifecycle

    private void Start()
    {
        InitializePlayerStats();
        SubscribeToEvents();
        UpdateAllUI();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the player stats reference and validates it.
    /// </summary>
    private void InitializePlayerStats()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not found!");
        }
    }

    /// <summary>
    /// Subscribes to player stats events for UI updates.
    /// </summary>
    private void SubscribeToEvents()
    {
        if (playerStats != null)
        {
            playerStats.OnLevelUp += UpdateLevelUI;
            playerStats.OnExperienceGained += UpdateExperienceUI;
        }
    }

    /// <summary>
    /// Unsubscribes from player stats events to prevent memory leaks.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (playerStats != null)
        {
            playerStats.OnLevelUp -= UpdateLevelUI;
            playerStats.OnExperienceGained -= UpdateExperienceUI;
        }
    }

    #endregion

    #region UI Updates

    /// <summary>
    /// Updates all UI elements with current player stats.
    /// </summary>
    private void UpdateAllUI()
    {
        if (playerStats == null) return;

        UpdateLevelUI(playerStats.CurrentLevel);
        UpdateExperienceUI(playerStats.CurrentExperience);
    }

    /// <summary>
    /// Updates the level display text.
    /// </summary>
    /// <param name="level">The current player level.</param>
    private void UpdateLevelUI(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
    }

    /// <summary>
    /// Updates the experience display text and progress bar.
    /// </summary>
    /// <param name="experience">The current experience value.</param>
    private void UpdateExperienceUI(float experience)
    {
        UpdateExperienceText(experience);
        UpdateExperienceBar(experience);
    }

    /// <summary>
    /// Updates the experience text display.
    /// </summary>
    /// <param name="experience">The current experience value.</param>
    private void UpdateExperienceText(float experience)
    {
        if (experienceText != null)
        {
            experienceText.text = $"{experience:F0}/{playerStats.ExperienceToNextLevel:F0}";
        }
    }

    /// <summary>
    /// Updates the experience progress bar and its color.
    /// </summary>
    /// <param name="experience">The current experience value.</param>
    private void UpdateExperienceBar(float experience)
    {
        if (experienceBar == null) return;

        if (playerStats.ExperienceToNextLevel > 0)
        {
            float progress = experience / playerStats.ExperienceToNextLevel;
            experienceBar.value = progress;
            UpdateExperienceBarColor(progress);
        }
        else
        {
            experienceBar.value = 0;
            UpdateExperienceBarColor(0);
        }
    }

    /// <summary>
    /// Updates the experience bar fill color based on progress.
    /// </summary>
    /// <param name="progress">The progress value (0-1).</param>
    private void UpdateExperienceBarColor(float progress)
    {
        if (experienceBarFillImage != null)
        {
            experienceBarFillImage.color = experienceBarColor.Evaluate(progress);
        }
    }

    #endregion
}
