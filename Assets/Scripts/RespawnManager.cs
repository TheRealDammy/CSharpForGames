using UnityEditor;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance;

    private PlayerHealth player;
    private Vector3 lastCheckpoint;

    [SerializeField] private GameObject deathScreen;

    private void Awake()
    {
        Instance = this;
    }

    public void SetCheckpoint(Vector3 position)
    {
        lastCheckpoint = position;
    }

    public void PlayerDied(PlayerHealth p)
    {
        player = p;

        if (deathScreen)
            deathScreen.SetActive(true);

        Time.timeScale = 0f;
    }

    private void RespawnPlayer()
    {
        Time.timeScale = 1f;

        if (deathScreen)
            deathScreen.SetActive(false);

        player.RespawnAt(lastCheckpoint);
    }

    public void Respawn()
    {
        Debug.Log("Respawning Player...");
        RespawnPlayer();
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
    }
}
