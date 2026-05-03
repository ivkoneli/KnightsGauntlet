using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveTooltip : MonoBehaviour
{
    [SerializeField] TMP_Text      titleText;
    [SerializeField] TMP_Text      typeText;
    [SerializeField] TMP_Text      damageText;
    [SerializeField] Image         damageIcon;
    [SerializeField] TMP_Text      cooldownText;
    [SerializeField] TMP_Text      descText;
    [SerializeField] CardTypeColors typeColors;

    private static BattleStats _heroStats;
    public static void SetHeroStats(BattleStats stats) => _heroStats = stats;

    public void Show(MoveData move)
    {
        if (titleText    != null) titleText.text    = move.name ?? "";
        if (typeText     != null) typeText.text     = BuildTypeLabel(move);
        if (damageText   != null) damageText.text   = BuildDamageNumber(move);
        if (damageIcon   != null)
        {
            damageIcon.sprite = LoadTypeIcon(move);
            damageIcon.color  = typeColors != null ? typeColors.GetColor(ResolveCardName(move)) : Color.white;
        }
        if (cooldownText != null) cooldownText.text = move.cooldown > 0 ? $"{move.cooldown} Turns" : "—";
        if (descText     != null) descText.text     = move.description ?? "";
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);

    static string BuildTypeLabel(MoveData move)
    {
        if (move.dot           != null) return "Poison";
        if (move.lifesteal)             return "Lifesteal";
        if (move.stunChance     > 0f)  return "Stun";
        if (move.healPercent    > 0f || move.selfHeal > 0) return "Heal";
        if (move.magMultiplier  > 0f)  return "Magic";
        if (move.atkMultiplier  > 0f)  return "Physical";
        return "Defensive";
    }

    static string BuildDamageNumber(MoveData move)
    {
        if (_heroStats == null) return "";
        if (move.atkMultiplier > 0)
            return $"~{Mathf.Max(1, Mathf.RoundToInt(_heroStats.currentAtk * move.atkMultiplier))}";
        if (move.magMultiplier > 0)
            return $"~{Mathf.Max(1, Mathf.RoundToInt(_heroStats.currentMag * move.magMultiplier))}";
        if (move.healPercent > 0 && _heroStats.maxHp > 0)
            return $"+{Mathf.RoundToInt(_heroStats.maxHp * move.healPercent)}";
        if (move.selfHeal > 0)
            return $"+{move.selfHeal}";
        return "";
    }

    internal static Sprite LoadTypeIcon(MoveData move)
    {
        string key = ResolveCardName(move) switch
        {
            "damage"    => "IconDamage",
            "heal"      => "IconHeal",
            "magic"     => "IconMagic",
            "poison"    => "IconPoison",
            "lifesteal" => "IconLifeSteal",
            "stun"      => "IconStun",
            "buff"      => "IconBuff",
            "defensive" => "IconDeffensive",
            _           => null
        };
        return key != null ? Resources.Load<Sprite>($"cards/{key}") : null;
    }

    internal static string ResolveCardName(MoveData move)
    {
        if (move.dot           != null) return "poison";
        if (move.lifesteal)             return "lifesteal";
        if (move.stunChance     > 0f)  return "stun";
        if (move.healPercent    > 0f || move.selfHeal > 0) return "heal";
        if (move.magMultiplier  > 0f)  return "magic";
        if (move.atkMultiplier  > 0f)  return "damage";
        return "defensive";
    }
}
