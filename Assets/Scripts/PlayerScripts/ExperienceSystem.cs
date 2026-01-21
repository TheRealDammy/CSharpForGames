using UnityEngine;

public class ExperienceSystem : MonoBehaviour
{
    [SerializeField] private int experiencePoints = 0;
    [SerializeField] private int level = 1;
    [SerializeField] private int experienceToNextLevel = 100;
    [SerializeField] private int experienceGrowthRate = 50;
    [SerializeField] private int maxLevel = 40;
    [SerializeField] private int statPointsPerLevel = 5;
    [SerializeField] private int currentStatPoints = 0;

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
    }

    public void LevelUp()
    {
        level++;
        currentStatPoints += statPointsPerLevel;
        experienceToNextLevel += experienceGrowthRate;
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
        level = 1;
        experienceToNextLevel = 100;
        currentStatPoints = 0;
    }

    public int GetCurrentLevel()
    {
        return level;
    }
    public int GetCurrentExperience()
    {
        return experiencePoints;
    }
    public int GetExperienceToNextLevel()
    {
        return experienceToNextLevel;
    }
    public int GetCurrentStatPoints()
    {
        return currentStatPoints;
    }
    public int GetExperienceGrowthRate() {
        return experienceGrowthRate;
    }
    public int GetMaxLevel()
    {
        return maxLevel;
    }
    public int GetStatPointsPerLevel()
    {
        return statPointsPerLevel;
    }
}
