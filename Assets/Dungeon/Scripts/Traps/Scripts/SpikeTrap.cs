using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private int damage = 15;
    [SerializeField] private float cooldown = 1f;

    private float lastHitTime;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (Time.time < lastHitTime + cooldown) return;

        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage, transform.position, transform.position);
            lastHitTime = Time.time;
        }
    }
}
