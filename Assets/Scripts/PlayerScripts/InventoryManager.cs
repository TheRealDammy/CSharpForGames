using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    private int statPoints;
    private int pointsSpent;
    //[SerializeField] private Image levelBar;
    [SerializeField] private TextMeshProUGUI statPointsText;


    private ExperienceSystem expSystem;

    private void Awake()
    {
        expSystem = GetComponent<ExperienceSystem>();
        UpdateStatPointsUI();
    }

    public void Update()
    {
        statPoints = expSystem.GetCurrentStatPoints();
        UpdateStatPointsUI();
    }

    private void UpdateStatPointsUI()
    {
        statPointsText.text = $"Stat Points Available: {statPoints}";
    }

    public void SpendStatPoints(int points)
    {
        pointsSpent += points;
        expSystem.SpendStatPoints(points);
        UpdateStatPointsUI();
        Debug.Log($"Spent {points} stat points. Remaining: {statPoints}");
    }

    public void AddStatPoints(int points)
    {
        pointsSpent -= points;
        statPoints += pointsSpent;
        UpdateStatPointsUI();
        Debug.Log($"Added back {points} stat points. Available: {statPoints}");
    }
}
