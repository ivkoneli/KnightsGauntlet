using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One card in the 5×5 inventory grid.
// Unlocked = full color, click to equip/unequip.
// Locked   = dark overlay, click does nothing.
public class InventoryCardDisplay : MonoBehaviour
{
    [SerializeField] Image          cardImage;
    [SerializeField] Image          iconImage;
    [SerializeField] Image          cardTypeIconImage;
    [SerializeField] TMP_Text       moveName;
    [SerializeField] GameObject     lockOverlay;
    [SerializeField] GameObject     equippedBadge;
    [SerializeField] CardTypeColors typeColors;
    [SerializeField] CardFlip       cardFlip;
    [SerializeField] CardInfoBack   cardInfoBack;

    private MoveData            _move;
    private InventoryController _controller;

    public void Setup(MoveData move, InventoryController controller)
    {
        _move       = move;
        _controller = controller;
        Refresh();
    }

    public void Refresh()
    {
        if (_move == null) return;
        HeroProgress hero     = GameManager.Instance.Hero;
        bool         unlocked = hero.IsUnlocked(_move.id);
        bool         equipped = hero.IsEquipped(_move.id);

        if (cardImage != null)
        {
            cardImage.sprite = LoadCardSprite(_move);
            cardImage.color  = unlocked ? Color.white : new Color(0.2f, 0.2f, 0.2f);
        }
        if (iconImage != null)
        {
            iconImage.sprite          = LoadIcon(_move);
            iconImage.preserveAspect  = true;
            iconImage.color           = unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f);
        }

        if (cardTypeIconImage != null)
        {
            string cardType = ResolveCardName(_move);
            cardTypeIconImage.sprite         = LoadTypeIcon(cardType);
            cardTypeIconImage.preserveAspect = true;
            Color baseColor = typeColors != null ? typeColors.GetColor(cardType) : Color.white;
            cardTypeIconImage.color = unlocked ? baseColor : baseColor * new Color(0.35f, 0.35f, 0.35f);
        }
        if (moveName      != null) moveName.text = _move.name;
        if (lockOverlay   != null) lockOverlay.SetActive(!unlocked);
        if (equippedBadge != null) equippedBadge.SetActive(equipped);
    }

    // Wired to Button OnClick in Inspector.
    public void OnClick()
    {
        if (_move == null) return;
        if (!GameManager.Instance.InventoryEditMode) return;
        HeroProgress hero = GameManager.Instance.Hero;
        if (!hero.IsUnlocked(_move.id)) return;

        if (hero.IsEquipped(_move.id))
        {
            hero.UnequipMove(_move.id);
            cardFlip?.ForceShowFront();
        }
        else if (hero.HasFreeSlot)
            hero.EquipMove(_move.id);
        else
        {
            // All 4 slots full — flip card to show quick info
            cardInfoBack?.Populate(_move);
            cardFlip?.Flip();
            return;
        }

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
