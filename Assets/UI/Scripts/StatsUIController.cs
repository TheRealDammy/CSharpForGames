using TMPro;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using System.Collections;

public class StatsUIController : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI pointsText;

    [Header("Health")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBar;

    [Header("Stamina")]
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private Image staminaBar;

    [Header("Strength")]
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private Image strengthBar;

    [Header("Durability")]
    [SerializeField] private TextMeshProUGUI durabilityText;
    [SerializeField] private Image durabilityBar;

    private PlayerStats playerStats;
    private PlayerHealth playerHealth;
    private TopDownCharacterController controller;
    private Coroutine bindRoutine;

    [SerializeField] private ExperienceSystem experienceSystem;
    [SerializeField] private float barAnimSpeed = 6f;


    private void Start()
    {
        RefreshUI();
    }

    private void OnEnable()
    {
        RefreshUI();
        PlayerStats.Instance.OnStatChanged += _ => RefreshUI();
        PlayerStats.Instance.OnStatsLoaded += RefreshUI;
        bindRoutine = StartCoroutine(BindWhenPlayerExists());
    }

    private void OnDisable()
    {
        RefreshUI();

        if (PlayerStats.Instance == null) return;
        PlayerStats.Instance.OnStatChanged -= _ => RefreshUI();
        PlayerStats.Instance.OnStatsLoaded -= RefreshUI;

        if (bindRoutine != null)
            StopCoroutine(bindRoutine);

        Unbind();
    }

    private IEnumerator BindWhenPlayerExists()
    {
        while (true)
        {
            var player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                playerStats = player.GetComponent<PlayerStats>();
                experienceSystem = player.GetComponent<ExperienceSystem>();
                playerHealth = player.GetComponent<PlayerHealth>();
                controller = player.GetComponent<TopDownCharacterController>();

                if (playerStats != null && experienceSystem != null)
                {
                    // subscribe once
                    playerStats.OnStatChanged += _ => RefreshUI();
                    experienceSystem.OnStatsChanged += RefreshUI;

                    Debug.Log("StatsUI: Successfully bound to player");
                    RefreshUI();
                    yield break; // IMPORTANT: stop coroutine
                }
            }

            yield return null; // try again next frame
        }
    }

    private void Unbind()
    {
        if (playerStats != null)
            playerStats.OnStatChanged -= _ => RefreshUI();

        if (experienceSystem != null)
            experienceSystem.OnStatsChanged += RefreshUI;

        playerStats = null;
        experienceSystem = null;
    }

    private void Update()
    {
        if (PlayerStats.Instance == null) return;
        // Late-bind player safely (handles dungeon spawn)
        if (playerHealth == null || controller == null)
            TryBindPlayer();
    }

    private void OnDestroy()
    {
        if (experienceSystem != null)
            experienceSystem.OnStatsChanged -= RefreshUI;
    }

    private void TryBindPlayer()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;

        playerHealth = player.GetComponent<PlayerHealth>();
        controller = player.GetComponent<TopDownCharacterController>();

        // Find ExperienceSystem in the scene if not already assigned
        if (experienceSystem == null)
            experienceSystem = FindFirstObjectByType<ExperienceSystem>();

        RefreshUI();
    }

    // =============================
    // UPGRADE / REFUND
    // =============================
    public void Upgrade(PlayerStatType type)
    {
        if (!experienceSystem.SpendStatPoint()) return;
        if (!PlayerStats.Instance.TryIncreaseStat(type)) return;

        ApplyStats();
        RefreshUI();
    }

    public void Refund(PlayerStatType type)
    {
        if (!PlayerStats.Instance.TryDecreaseStat(type)) return;

        experienceSystem.RefundStatPoint();
        ApplyStats();
        RefreshUI();
    }

    // =============================
    // BUTTON CALLBACKS
    // =============================
    public void UpgradeHealth() => Upgrade(PlayerStatType.Health);
    public void UpgradeStamina() => Upgrade(PlayerStatType.Stamina);
    public void UpgradeStrength() => Upgrade(PlayerStatType.Strength);
    public void UpgradeDurability() => Upgrade(PlayerStatType.Durability);

    public void RefundHealth() => Refund(PlayerStatType.Health);
    public void RefundStamina() => Refund(PlayerStatType.Stamina);
    public void RefundStrength() => Refund(PlayerStatType.Strength);
    public void RefundDurability() => Refund(PlayerStatType.Durability);

    // =============================
    private void ApplyStats()
    {
        if (playerHealth != null)
            playerHealth.ApplyStats(fullHeal: false);

        if (controller != null)
            controller.ApplyStats();
    }

    private void RefreshUI()
    {
        if (playerStats == null || experienceSystem == null)
        {
            Debug.LogWarning("StatsUI: Missing playerStats or experienceSystem");
            return;
        }

        UpdateRow(PlayerStatType.Health, healthText, healthBar);
        UpdateRow(PlayerStatType.Stamina, staminaText, staminaBar);
        UpdateRow(PlayerStatType.Strength, strengthText, strengthBar);
        UpdateRow(PlayerStatType.Durability, durabilityText, durabilityBar);

        pointsText.text = $"Stat Points: {experienceSystem.GetStatPoints()}";
    }

    private void UpdateRow(PlayerStatType type, TextMeshProUGUI text, Image bar)
    {
        if (playerStats == null)
        {
            Debug.LogWarning("StatsUI: playerStats missing");
            return;
        }

        if (text == null || bar == null)
        {
            Debug.LogWarning($"StatsUI: UI reference missing for {type}");
            return;
        }

        var data = playerStats.GetStatData(type);

        int level = data.level;
        int soft = data.softCap;
        int hard = data.hardCap;

        float targetFill = (float)level / hard;
        bar.fillAmount = targetFill;

        if (level >= hard)
            bar.color = Color.red;
        else if (level >= soft)
            bar.color = Color.yellow;
        else
            bar.color = Color.white;

        text.text = $"{type}: {level}/{hard}";
    }
}
