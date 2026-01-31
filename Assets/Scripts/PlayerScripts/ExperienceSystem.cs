using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceSystem : MonoBehaviour
{
    public event Action OnStatsChanged;

    [SerializeField] private int experiencePoints;
    [SerializeField] private int level;
    [SerializeField] private int experienceToNextLevel = 100;
    [SerializeField] private int experienceGrowthRate = 50;
    [SerializeField] private int maxLevel = 40;
    [SerializeField] private int statPointsPerLevel = 5;

    private int currentStatPoints;

    [Header("UI")]
    [SerializeField] private Image expBar;
    [SerializeField] private TextMeshProUGUI levelText;

    private void Awake()
    {
        RefreshUI();
    }

    public void AddXP(int amount)
    {
        if (level >= maxLevel) return;

        experiencePoints += amount;

        while (experiencePoints >= experienceToNextLevel && level < maxLevel)
        {
            experiencePoints -= experienceToNextLevel;
            LevelUp();
        }

        RefreshUI();
    }

    private void LevelUp()
    {
        level++;
        currentStatPoints += statPointsPerLevel;
        experienceToNextLevel += experienceGrowthRate;

        OnStatsChanged?.Invoke();
    }

    public bool SpendStatPoint()
    {
        if (currentStatPoints <= 0) return false;

        currentStatPoints--;
        OnStatsChanged?.Invoke();
        return true;
    }

    public void RefundStatPoint()
    {
        currentStatPoints++;
        OnStatsChanged?.Invoke();
    }

    public int GetStatPoints() => currentStatPoints;
    public int GetLevel() => level;

    private void RefreshUI()
    {
        if (expBar)
            expBar.fillAmount = experiencePoints / (float)experienceToNextLevel;

        if (levelText)
            levelText.text = $"Level {level}";
    }
}
