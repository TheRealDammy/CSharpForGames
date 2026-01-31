using UnityEngine;
using UnityEngine.EventSystems;

public class StatButtonTooltip : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public PlayerStatType statType;
    public StatTooltip tooltip;
    public PlayerStats stats;

    public void OnPointerEnter(PointerEventData eventData)
    {
        string preview = stats.GetUpgradePreview(statType);
        tooltip.Show(preview, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.Hide();
    }
}
