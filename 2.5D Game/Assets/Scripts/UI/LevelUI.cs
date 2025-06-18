using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{

    [Header("Level Display")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private Slider experienceBar;
    [SerializeField] private Image experienceBarFillImage;
    [SerializeField] private Gradient experienceBarColor;

    private PlayerStats playerStats;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not found!");
            return;
        }

        playerStats.OnLevelUp += UpdateLevelUI;
        playerStats.OnExperienceGained += UpdateExperienceUI;

        UpdateAllUI();
        
    }

    // Update is called once per frame
    private void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnLevelUp -= UpdateLevelUI;
            playerStats.OnExperienceGained -= UpdateExperienceUI;
        }
    }

    private void UpdateAllUI()
    {
        UpdateLevelUI(playerStats.CurrentLevel);
        UpdateExperienceUI(playerStats.CurrentExperience);
    }

    private void UpdateLevelUI(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
        
    }

    private void UpdateExperienceUI(float experience)
    {
        if (experienceText != null)
        {
            experienceText.text = $"{experience:F0}/{playerStats.ExperienceToNextLevel:F0}";
        }
        if (experienceBar != null)
        {
            experienceBar.value = experience / playerStats.ExperienceToNextLevel;
            if (experienceBarFillImage != null)
            {
                experienceBarFillImage.color = experienceBarColor.Evaluate(experienceBar.value);
            }
        }
    }


}
