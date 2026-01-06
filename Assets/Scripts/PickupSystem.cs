using Unity.VisualScripting;
using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private ScoreSystem m_scoreSystem;
    [SerializeField] private int m_points;

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
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnPickup();
        }
    }

    public void OnPickup()
    {
        if (m_scoreSystem != null)
        {
            m_scoreSystem.AddScore(m_points);
            Destroy(gameObject);
        }
    }
}
