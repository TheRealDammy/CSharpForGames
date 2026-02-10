using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public void Respawn()
    {
        SceneManager.LoadSceneAsync("Dungeon");
        SceneManager.UnloadSceneAsync("GameOver");
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
