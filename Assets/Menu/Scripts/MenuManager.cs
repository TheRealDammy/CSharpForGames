using TMPro;
using UnityEditor;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private TextMeshProUGUI text;   
    private bool controlsPanelOpen = false;
    
    public void ToggleContorlsPanel()
    {
        if (controlsPanelOpen)
        {
            controlsPanel.SetActive(false);
            menuPanel.SetActive(true);
            text.text = "MainMenu";
        }
        else
        {
            controlsPanel.SetActive(true);
            menuPanel.SetActive(false);
            text.text = "Controls";
        }
        controlsPanelOpen = !controlsPanelOpen;
    }

    public void LoadLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("CharacterSelection");
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
