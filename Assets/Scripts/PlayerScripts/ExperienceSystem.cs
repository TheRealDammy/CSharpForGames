using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceSystem : MonoBehaviour
{
    private int experiencePoints = 0;
    private int level = 0;
    [SerializeField] private int experienceToNextLevel = 100;
    [SerializeField] private int experienceGrowthRate = 50;
    [SerializeField] private int maxLevel = 40;
    [SerializeField] private int statPointsPerLevel = 5;
    private int currentStatPoints = 0;

    [SerializeField] private Image expBar;
    [SerializeField] private TextMeshProUGUI levelText;

    public void Awake()
    {
        expBar.fillAmount = experiencePoints / (float)experienceToNextLevel;
        levelText.text = $"Level {level}";
    }

    public void AddExperience(int amount)
    {
        if (level >= maxLevel)
            return;
        experiencePoints += amount;
        while (experiencePoints >= experienceToNextLevel && level < maxLevel)
        {
            experiencePoints -= experienceToNextLevel;
            LevelUp();
        }

        expBar.fillAmount = experiencePoints / (float)experienceToNextLevel;

        Debug.Log($"Gained {amount} XP. Current XP: {experiencePoints}/{experienceToNextLevel}");
    }

    public void LevelUp()
    {
        level++;
        currentStatPoints += statPointsPerLevel;
        experienceToNextLevel += experienceGrowthRate;
        levelText.text = $"Level {level}";
        Debug.Log($"Leveled up to {level}! Stat points available: {currentStatPoints}");
    }

    public bool SpendStatPoints(int points)
    {
        if (points <= currentStatPoints)
        {
            currentStatPoints -= points;
            return true;
        }
        return false;
    }

    public void ResetExperience()
    {
        experiencePoints = 0;
        level = 0;
        experienceToNextLevel = 100;
        currentStatPoints = 0;
    }

    public int GetCurrentStatPoints()
    {
        return currentStatPoints;
    }
}
