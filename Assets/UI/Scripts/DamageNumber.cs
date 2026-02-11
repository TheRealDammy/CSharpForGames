using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float lifetime = 1f;

    private TextMeshPro text;
    private float timer;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshPro>();
    }

    public void Init(int amount, Color color)
    {
        text.text = amount.ToString();
        text.color = color;
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        // Fade out
        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
        var c = text.color;
        c.a = alpha;
        text.color = c;

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
