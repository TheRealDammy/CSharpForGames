using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int healAmount = 20;

    public void OnPickup(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(healAmount);
            Debug.Log("Player healed by " + healAmount + " points.");
            Destroy(gameObject);
        }
    }

    public void Update()
    {
        transform.Rotate(0f, 0.4f, 0f * Time.time);
        transform.position = new Vector3(transform.position.x, transform.position.y + Mathf.Sin(Time.time) * 0.0005f, transform.position.z);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnPickup(collision.gameObject);
        }
    }
}
