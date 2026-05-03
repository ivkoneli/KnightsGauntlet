using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Attach to a root GameObject in the MainMenu scene.
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button   startButton;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button   exitButton;
    [SerializeField] private Button   musicButton;
    [SerializeField] private Button   soundButton;

    private static readonly Color OffColor = new Color(0.4f, 0.4f, 0.4f);

    void Start()
    {
        startButton.interactable = false;
        SetStatus("Loading game data...");

        AudioManager.PlayMusic("sounds/happy8bit");
        RefreshAudioButtons();

        if (GameManager.Instance.IsConfigLoaded)
            OnConfigReady();
        else
            GameManager.Instance.OnConfigLoaded += OnConfigReady;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnConfigLoaded -= OnConfigReady;
    }

    private void OnConfigReady()
    {
        startButton.interactable = true;
        SetStatus("Ready!");
    }

    public void OnStartGameClicked()
    {
        startButton.interactable = false;
        SetStatus("Starting...");

        if (PlayerPrefs.GetInt("intro_watched", 0) == 0)
            GameManager.Instance.GoToIntro();
        else
            GameManager.Instance.StartRun();
    }

    public void OnExitClicked() => Application.Quit();

    public void OnMusicClicked()
    {
        AudioManager.ToggleMusic();
        RefreshAudioButtons();
    }

    public void OnSoundClicked()
    {
        AudioManager.ToggleSound();
        RefreshAudioButtons();
    }

    private void RefreshAudioButtons()
    {
        SetButtonColor(musicButton, AudioManager.IsMusicEnabled);
        SetButtonColor(soundButton, AudioManager.IsSoundEnabled);
    }

    private static void SetButtonColor(Button btn, bool on)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = on ? Color.white : OffColor;
    }

    private void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }
}
