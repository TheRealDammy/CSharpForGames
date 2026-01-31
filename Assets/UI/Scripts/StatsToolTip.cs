using TMPro;
using UnityEngine;

public class StatTooltip : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI text;

    public void Show(string content, Vector2 position)
    {
        panel.SetActive(true);
        panel.transform.position = position;
        text.text = content;
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}
