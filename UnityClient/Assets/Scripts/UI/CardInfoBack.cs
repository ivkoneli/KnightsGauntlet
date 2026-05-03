using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardInfoBack : MonoBehaviour
{
    [SerializeField] Image    typeIcon;
    [SerializeField] TMP_Text damageText;
    [SerializeField] Image    clockIcon;
    [SerializeField] TMP_Text cooldownText;
    [SerializeField] TMP_Text descText;

    public void Populate(MoveData move)
    {
        if (typeIcon != null)
        {
            typeIcon.sprite         = MoveTooltip.LoadTypeIcon(move);
            typeIcon.preserveAspect = true;
            typeIcon.color          = typeIcon.sprite != null ? Color.white : Color.clear;
        }
        if (damageText   != null) damageText.text   = BuildDamage(move);
        if (clockIcon    != null) clockIcon.color   = Color.white;
        if (cooldownText != null) cooldownText.text = move.cooldown > 0 ? $"{move.cooldown}" : "0";
        if (descText     != null) descText.text     = move.description ?? "";
    }

    static string BuildDamage(MoveData move)
    {
        if (move.atkMultiplier > 0) return $"×{move.atkMultiplier:0.##}";
        if (move.magMultiplier > 0) return $"×{move.magMultiplier:0.##}";
        if (move.healPercent   > 0) return $"+{move.healPercent * 100:0}%";
        if (move.selfHeal      > 0) return $"+{move.selfHeal}";
        return "—";
    }
}
