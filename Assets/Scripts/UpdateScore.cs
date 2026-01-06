using UnityEngine;
using UnityEngine.UI;

public class UpdateScore : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private ScoreSystem m_scoreSystem;
    [SerializeField] private Button m_button;

    public void Start()
    {
        m_button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        m_scoreSystem.AddScore(10);
    }
}
