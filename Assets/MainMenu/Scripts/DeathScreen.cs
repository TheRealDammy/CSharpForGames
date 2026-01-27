using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public void Respawn()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("DungeonScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
