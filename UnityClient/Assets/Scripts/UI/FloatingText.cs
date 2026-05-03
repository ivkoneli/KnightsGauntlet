using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FloatingText : MonoBehaviour
{
    [SerializeField] TMP_Text label;

    public void Play(string text, Color color)
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>();
        label.text = text;
        label.color = new Color(color.r, color.g, color.b, 1f);
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        var rt = GetComponent<RectTransform>();
        float elapsed = 0f;
        const float duration = 1f;
        Vector2 start = rt.anchoredPosition;

        transform.localScale = Vector3.one * 1.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            rt.anchoredPosition = start + Vector2.up * (70f * t);
            float scale = Mathf.Lerp(1.2f, 1f, Mathf.Clamp01(t * 4f));
            transform.localScale = Vector3.one * scale;

            float alpha = t < 0.4f ? 1f : 1f - (t - 0.4f) / 0.6f;
            var c = label.color;
            c.a = Mathf.Max(0f, alpha);
            label.color = c;

            yield return null;
        }

        Destroy(gameObject);
    }
}
