using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public void Respawn()
    {
        Debug.Log("Respawning Player...");
        //RespawnManager.Instance.RespawnPlayer();
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
