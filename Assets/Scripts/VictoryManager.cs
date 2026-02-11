using TMPro;
using UnityEditor;
using UnityEngine;

public class VictoryManager : MonoBehaviour
{
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI damageText;

    private void OnEnable()
    {
        GameManager.Instance.OnGameCompleted += ShowVictory;
    }

    private void ShowVictory()
    {
        Time.timeScale = 0f;

        var stats = RunStats.Instance;

        timeText.text = $"Time: {stats.runTime:F1}s\n";
        killsText.text = $"Kills: {stats.enemiesKilled}\n";
        damageText.text = $"Damage Dealt: {stats.damageDealt}\n";

        victoryUI.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Dungeon");
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
        #else
            Application.Quit();
        #endif
    }
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameCompleted -= ShowVictory;
        }           
    }
}
