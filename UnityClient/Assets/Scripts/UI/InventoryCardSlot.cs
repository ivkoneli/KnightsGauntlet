using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One of 4 equipped-move slots. Click to unequip.
public class InventoryCardSlot : MonoBehaviour
{
    [SerializeField] Image          cardImage;
    [SerializeField] Image          iconImage;
    [SerializeField] Image          cardTypeIconImage;
    [SerializeField] TMP_Text       moveName;
    [SerializeField] GameObject     emptyOverlay;
    [SerializeField] CardTypeColors typeColors;

    private string              _equippedId;
    private InventoryController _controller;

    public void Init(InventoryController controller) => _controller = controller;

    public void SetMove(MoveData move)
    {
        _equippedId = move?.id;
        bool has = move != null;

        if (cardImage != null)
        {
            cardImage.sprite = has ? LoadCardSprite(move) : null;
            cardImage.color  = has ? Color.white : new Color(0.15f, 0.15f, 0.15f);
        }
        if (iconImage != null)
        {
            iconImage.sprite         = has ? LoadIcon(move) : null;
            iconImage.preserveAspect = true;
            iconImage.color          = has ? Color.white : Color.clear;
        }

        if (cardTypeIconImage != null)
        {
            cardTypeIconImage.sprite         = has ? LoadTypeIcon(ResolveCardName(move)) : null;
            cardTypeIconImage.preserveAspect = true;
            cardTypeIconImage.color          = has
                ? (typeColors != null ? typeColors.GetColor(ResolveCardName(move)) : Color.white)
                : Color.clear;
        }
        if (moveName     != null) moveName.text = has ? move.name : "Empty";
        if (emptyOverlay != null) emptyOverlay.SetActive(!has);
    }

    // Wired to Button OnClick in Inspector.
    public void OnClick()
    {
        if (_equippedId == null) return;
        if (!GameManager.Instance.InventoryEditMode) return;
        GameManager.Instance.Hero.UnequipMove(_equippedId);
        _controller.MarkDirty();
        _controller.RefreshAll();
    }

    private static Sprite LoadIcon(MoveData move)
    {
        if (!string.IsNullOrEmpty(move.icon))
        {
            var s = Resources.Load<Sprite>($"required_icons/{move.icon}");
            if (s != null) return s;
        }
        return Resources.Load<Sprite>("required_icons/rusty_blade");
    }

    private static Sprite LoadTypeIcon(string cardType)
    {
        string fileName = cardType switch
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
        return fileName != null ? Resources.Load<Sprite>($"cards/{fileName}") : null;
    }

    private static Sprite LoadCardSprite(MoveData move)
    {
        string key = ResolveCardName(move);
        var sp  = Resources.Load<Sprite>($"cards/{key}");
        if (sp != null) return sp;
        var tex = Resources.Load<Texture2D>($"cards/{key}");
        return tex != null
            ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f))
            : null;
    }

    private static string ResolveCardName(MoveData move)
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
