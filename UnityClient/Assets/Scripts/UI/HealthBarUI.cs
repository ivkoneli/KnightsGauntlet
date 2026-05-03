using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] Slider   slider;
    [SerializeField] TMP_Text hpText;
    [SerializeField] TMP_Text nameText;   // optional label (Lv.X HERO / Lv.X Monster)

    public void SetMax(int max)
    {
        slider.maxValue = max;
        slider.value    = max;
        RefreshText(max, max);
    }

    public void UpdateHP(int current, int max)
    {
        slider.maxValue = max;   // also update max in case it changed (level up)
        slider.value    = current;
        RefreshText(current, max);
    }

    public void SetName(string label)
    {
        if (nameText != null) nameText.text = label;
    }

    private void RefreshText(int current, int max)
    {
        if (hpText != null) hpText.text = $"{current} / {max}";
    }
}
