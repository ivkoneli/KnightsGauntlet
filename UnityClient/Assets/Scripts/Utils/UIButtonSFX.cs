using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour
{
    [SerializeField] private string clickSFX = "sounds/sound_effects/button";

    private Button _button;

    void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(PlaySound);
    }

    private void PlaySound()
    {
        AudioManager.PlaySFX(clickSFX);
    }
}