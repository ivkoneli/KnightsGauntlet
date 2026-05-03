using UnityEngine;
using TMPro;
using System.Collections;

public class CreditsScroller : MonoBehaviour
{
    [SerializeField] private RectTransform textTransform;
    [SerializeField] private float scrollSpeed = 50f;
    [SerializeField] private float fastScrollSpeed = 200f;

    [Header("Fade")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeStartY = 300f;
    [SerializeField] private float fadeEndY = 600f;

    [Header("End")]
    [SerializeField] private float endY = 1200f;
    [SerializeField] private GameObject restartButton;

    [Header("Restart Fade")]
    [SerializeField] private CanvasGroup restartCanvasGroup;
    [SerializeField] private float restartFadeSpeed = 2f;

    private bool _finished;

    private void Start()
    {
        canvasGroup.alpha = 0f;

        if (restartButton != null)
            restartButton.SetActive(false);

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = t;
            yield return null;
        }
    }

    void Update()
    {
        if (_finished) return;

        // SPEED CONTROL (hold or click)
        float speed = Input.GetMouseButton(0) ? fastScrollSpeed : scrollSpeed;

        textTransform.anchoredPosition += Vector2.up * speed * Time.deltaTime;

        float y = textTransform.anchoredPosition.y;

        //  fade out
        if (y > fadeStartY)
        {
            float t = Mathf.InverseLerp(fadeStartY, fadeEndY, y);
            canvasGroup.alpha = 1f - t;
        }

        //  END OF CREDITS
        if (y >= endY)
        {
            _finished = true;
            ShowRestart();
        }
    }

    void ShowRestart()
    {
        if (restartButton != null)
            restartButton.SetActive(true);  

        StartCoroutine(FadeInRestart());
    }

    IEnumerator FadeInRestart()
    {
        float t = 0;

        restartCanvasGroup.alpha = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * restartFadeSpeed;
            restartCanvasGroup.alpha = t;

            yield return null;
        }

        restartCanvasGroup.alpha = 1f;
    }

}