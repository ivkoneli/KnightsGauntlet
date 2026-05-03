using System.Collections;
using TMPro;
using UnityEngine;

public enum Speaker
{
    Knight,
    Princess,
    Goblin,
    Dragon
}


public class DialogueController : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 4)]
        public string text;
        public Speaker speaker;
    }

    [SerializeField] private GameObject dialogueRoot;

    [SerializeField] private DialogueLine[] lines;

    [Header("Characters")]
    [SerializeField] private GameObject knightDisplay;
    [SerializeField] private GameObject princessDisplay;
    [SerializeField] private GameObject goblinDisplay;
    [SerializeField] private GameObject dragonDisplay;

    [Header("Dialogue Boxes")]
    [SerializeField] private GameObject knightBox;
    [SerializeField] private TMP_Text   knightText;
    [SerializeField] private GameObject princessBox;
    [SerializeField] private TMP_Text   princessText;
    [SerializeField] private GameObject goblinBox;
    [SerializeField] private TMP_Text   goblinText;
    [SerializeField] private GameObject dragonBox;
    [SerializeField] private TMP_Text   dragonText;


    [Header("On Finish")]
    [Tooltip("True for IntroScene — sets intro_watched and loads Battle.")]
    [SerializeField] private bool goToBattleOnFinish;
    [Tooltip("Objects to activate when dialogue ends (e.g. restart button panel in EndingScene).")]
    [SerializeField] private GameObject[] showOnFinish;
    [Tooltip("Optional music to play when dialogue ends (Resources path, e.g. sounds/postcreditScene).")]
    [SerializeField] private string musicOnFinish;

    [SerializeField] private float CharsPerSecond = 15f;
    private TMP_Text _activeText;

    private int       _lineIndex;
    private bool      _isTyping;
    private Coroutine _typingRoutine;

    void Start() => ShowLine(0);

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter) && !Input.GetMouseButtonDown(0))
            return;

        if (_isTyping)
        {
            if (_typingRoutine != null)
                StopCoroutine(_typingRoutine);
            _activeText.text = lines[_lineIndex].text;
            _isTyping = false;

            AudioManager.StopMusic();
        }
        else
        {
            _lineIndex++;
            if (_lineIndex >= lines.Length)
                OnAllLinesDone();
            else
                ShowLine(_lineIndex);
        }
    }

    private void ShowLine(int i)
    {
        var line = lines[i];

        // temp fix cuz goblin isnt part of the final scene
        if(goblinDisplay != null)
        {
            goblinDisplay.SetActive(false);
            goblinBox.SetActive(false);
        }

        // Reset boxes 
        knightDisplay.SetActive(false);
        princessDisplay.SetActive(false);
        dragonDisplay.SetActive(false);

        knightBox.SetActive(false);
        princessBox.SetActive(false);
        dragonBox.SetActive(false);

        if (line.speaker == Speaker.Knight)
        {
            knightDisplay.SetActive(true);
            knightBox.SetActive(true);
            knightText.text = "";

            _activeText = knightText;

            _typingRoutine = StartCoroutine(TypeLine(line.text, knightText));
        }
        else if (line.speaker == Speaker.Princess)
        {
            princessDisplay.SetActive(true);
            princessBox.SetActive(true);
            princessText.text = "";

            _activeText = princessText;

            _typingRoutine = StartCoroutine(TypeLine(line.text, princessText));
        }
        else if (line.speaker == Speaker.Goblin)
        {
            goblinDisplay.SetActive(true);
            goblinBox.SetActive(true);
            goblinText.text = "";

            _activeText = goblinText;

            _typingRoutine = StartCoroutine(TypeLine(line.text, goblinText));
        }
        else if (line.speaker == Speaker.Dragon)
        {
            dragonDisplay.SetActive(true);
            dragonBox.SetActive(true);
            dragonText.text = "";

            _activeText = dragonText;

            _typingRoutine = StartCoroutine(TypeLine(line.text, dragonText));
        }
    }

    private IEnumerator TypeLine(string text, TMP_Text target)
    {
        _isTyping = true;
        float delay = 1f / CharsPerSecond;

        AudioManager.StopMusic();
        AudioManager.PlayMusic("sounds/sound_effects/typing_sound");
        foreach (char c in text)
        {
            target.text += c;
            yield return new WaitForSeconds(delay);
        }

        _isTyping = false;
        AudioManager.StopMusic();
    }

    private void OnAllLinesDone()
    {
        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        if (goToBattleOnFinish)
        {
            PlayerPrefs.SetInt("intro_watched", 1);
            PlayerPrefs.Save();
            GameManager.Instance.StartRun();
            return;
        }

        if (!string.IsNullOrEmpty(musicOnFinish))
            AudioManager.PlayMusic(musicOnFinish);

        foreach (var obj in showOnFinish)
            if (obj != null) obj.SetActive(true);
    }
}
