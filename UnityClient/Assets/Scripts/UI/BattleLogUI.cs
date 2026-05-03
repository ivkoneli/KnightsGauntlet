using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleLogUI : MonoBehaviour
{
    [SerializeField] TMP_Text logText;
    [SerializeField] ScrollRect scroll;

    public void AddEntry(string text)
    {
        if (logText == null) return;
        logText.text += (logText.text.Length > 0 ? "\n" : "") + text;
        Canvas.ForceUpdateCanvases();
        if (scroll != null)
            scroll.verticalNormalizedPosition = 0f;
    }

    public void Clear()
    {
        if (logText != null)
            logText.text = "";
    }
}
