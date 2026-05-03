using System.Collections;
using UnityEngine;

public class CardFlip : MonoBehaviour
{
    [SerializeField] GameObject front;
    [SerializeField] GameObject back;
    [SerializeField] float      duration = 0.12f;

    bool _showingBack;
    bool _busy;

    public bool IsShowingBack => _showingBack;

    public void Flip()
    {
        if (_busy) return;
        StartCoroutine(DoFlip());
    }

    public void ForceShowFront()
    {
        StopAllCoroutines();
        _busy = false;
        _showingBack = false;
        front?.SetActive(true);
        back?.SetActive(false);
        var rt = GetComponent<RectTransform>();
        if (rt) rt.localScale = Vector3.one;
    }

    IEnumerator DoFlip()
    {
        _busy = true;
        var rt = GetComponent<RectTransform>();
        Vector3 origScale = rt.localScale;

        // Collapse X
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            rt.localScale = new Vector3(Mathf.Lerp(origScale.x, 0f, t / duration), origScale.y, origScale.z);
            yield return null;
        }
        rt.localScale = new Vector3(0f, origScale.y, origScale.z);

        // Swap faces
        _showingBack = !_showingBack;
        front?.SetActive(!_showingBack);
        back?.SetActive(_showingBack);

        // Expand X
        for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
        {
            rt.localScale = new Vector3(Mathf.Lerp(0f, origScale.x, t / duration), origScale.y, origScale.z);
            yield return null;
        }
        rt.localScale = origScale;
        _busy = false;
    }
}
