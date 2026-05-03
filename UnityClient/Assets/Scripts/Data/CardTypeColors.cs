using UnityEngine;

[CreateAssetMenu(fileName = "CardTypeColors", menuName = "Game/Card Type Colors")]
public class CardTypeColors : ScriptableObject
{
    public Color damage    = new Color(0.85f, 0.15f, 0.15f);
    public Color heal      = new Color(0.15f, 0.75f, 0.25f);
    public Color magic     = new Color(0.25f, 0.45f, 0.90f);
    public Color poison    = new Color(0.45f, 0.80f, 0.15f);
    public Color lifesteal = new Color(0.65f, 0.15f, 0.85f);
    public Color stun      = new Color(0.95f, 0.80f, 0.10f);
    public Color buff      = new Color(0.95f, 0.55f, 0.10f);
    public Color defensive = new Color(0.55f, 0.65f, 0.85f);

    public Color GetColor(string cardType) => cardType switch
    {
        "damage"    => damage,
        "heal"      => heal,
        "magic"     => magic,
        "poison"    => poison,
        "lifesteal" => lifesteal,
        "stun"      => stun,
        "buff"      => buff,
        "defensive" => defensive,
        _           => Color.white
    };
}
