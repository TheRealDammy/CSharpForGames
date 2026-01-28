using UnityEngine;

public class DeathManager : MonoBehaviour
{
    public void Respawn()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Dungeon");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }
}
