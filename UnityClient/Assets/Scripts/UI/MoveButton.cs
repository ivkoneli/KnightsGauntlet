using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MoveButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] TMP_Text      moveNameText;
    [SerializeField] TMP_Text      descText;       // kept for scene compat — hidden at runtime
    [SerializeField] Image         background;
    [SerializeField] Image         cardIconImage;
    [SerializeField] Image         cardTypeIconImage;
    [SerializeField] CardTypeColors typeColors;
    [SerializeField] MoveTooltip   tooltip;
    [SerializeField] GameObject    cooldownOverlay;
    [SerializeField] TMP_Text      cooldownText;

    public string MoveId { get; private set; }
    private Button   _button;
    private MoveData _move;
    private Vector3  _baseScale;
    private bool     _isOnCooldown;

    void Awake()
    {
        _button    = GetComponent<Button>();
        _baseScale = transform.localScale;
    }

    public void Setup(MoveData move, Action<string> onClick)
    {
        _move  = move;
        MoveId = move.id;

        if (moveNameText != null) moveNameText.text = move.name;
        if (descText     != null) descText.gameObject.SetActive(false);

        if (cardIconImage != null)
        {
            cardIconImage.sprite = LoadIcon(move);
            cardIconImage.color  = Color.white;
            cardIconImage.preserveAspect = true;
            cardIconImage.gameObject.SetActive(true);
        }

        if (cardTypeIconImage != null)
        {
            string cardType = ResolveCardName(move);
            cardTypeIconImage.sprite         = LoadTypeIcon(cardType);
            cardTypeIconImage.preserveAspect = true;
            cardTypeIconImage.color          = typeColors != null ? typeColors.GetColor(cardType) : Color.white;
        }

        if (background != null)
        {
            background.sprite = LoadCardSprite(move);
            background.color  = Color.white;
            background.type   = Image.Type.Simple;
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() => { tooltip?.Hide(); onClick?.Invoke(MoveId); });
    }

    public void SetInteractable(bool on)
    {
        if (_button != null) _button.interactable = on && !_isOnCooldown;
    }

    public void SetCooldown(int turns)
    {
        _isOnCooldown = turns > 0;
        if (_button != null) _button.interactable = !_isOnCooldown;
        if (cooldownOverlay != null) cooldownOverlay.SetActive(_isOnCooldown);
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(_isOnCooldown);
            if (_isOnCooldown) cooldownText.text = turns.ToString();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = _baseScale * 1.05f;
        if (_move != null) tooltip?.Show(_move);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = _baseScale;
        tooltip?.Hide();
    }

    private static Sprite LoadIcon(MoveData move)
    {
        // Try the move's own icon name first, then fall back to rusty_blade
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

    private static string ResolveCardName(MoveData move)
    {
        if (move.dot         != null) return "poison";
        if (move.lifesteal)           return "lifesteal";
        if (move.stunChance   > 0f)   return "stun";
        if (move.healPercent  > 0f || move.selfHeal > 0) return "heal";
        if (move.magMultiplier > 0f)  return "magic";
        if (move.atkMultiplier > 0f)  return "damage";
        return "defensive";
    }

    private static Sprite LoadCardSprite(MoveData move)
    {
        string name = ResolveCardName(move);
        var sp = Resources.Load<Sprite>($"cards/{name}");
        if (sp != null) return sp;
        var tex = Resources.Load<Texture2D>($"cards/{name}");
        if (tex != null) return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return null;
    }
}
