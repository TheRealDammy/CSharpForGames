using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int AliveEnemies { get; private set; }
    public event Action<int> OnEnemyCountChanged;
    public event Action OnGameCompleted;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterEnemies(int count)
    {
        AliveEnemies = count;
        OnEnemyCountChanged?.Invoke(AliveEnemies);
    }

    public void EnemyKilled()
    {
        AliveEnemies = Mathf.Max(0, AliveEnemies - 1);
        OnEnemyCountChanged?.Invoke(AliveEnemies);

        if (AliveEnemies <= 0)
            CompleteGame();
    }

    private void CompleteGame()
    {
        OnGameCompleted?.Invoke();
        SceneManager.LoadScene("WinScreen");
    }
}
