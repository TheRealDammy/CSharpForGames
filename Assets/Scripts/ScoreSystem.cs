using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int m_score;

    public void AddScore(int scoreToAdd)
    {
        m_score += scoreToAdd;
    }
}
