using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private ScoreSystem m_scoreSystem;

    public TMPro.TextMeshProUGUI m_scoreText;

    void Awake()
    {
        if (m_scoreSystem == null)
        {
            GameObject scoreSystemObj = GameObject.Find("ScoreSystem");
            if (scoreSystemObj != null)
            {
                m_scoreSystem = scoreSystemObj.GetComponent<ScoreSystem>();
            }
        }
    }

    void Update()
    {
        if (m_scoreSystem != null && m_scoreText != null)
        {
            m_scoreText.text = "Score: " + m_scoreSystem.m_score;
        }
    }
}
