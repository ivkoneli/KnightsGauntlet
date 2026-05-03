using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Purely visual glue. BattleManager calls these methods; no game logic here.
public class BattleUIController : MonoBehaviour
{
    [SerializeField] HealthBarUI heroHealthBar;
    [SerializeField] HealthBarUI monsterHealthBar;
    [SerializeField] HealthBarUI heroBottomBar;
    [SerializeField] HealthBarUI monsterBottomBar;
    [SerializeField] HealthBarUI heroXpBar;
    [SerializeField] MoveButtonPanel moveButtonPanel;
    [SerializeField] BattleLogUI     battleLog;
    [SerializeField] TMP_Text        monsterNameText;
    [SerializeField] TMP_Text        heroNameText;
    [SerializeField] TMP_Text        heroBottomLevelText;
    [SerializeField] TMP_Text        monsterBottomLevelText;
    [SerializeField] TMP_Text        heroLevelBadgeText;
    [SerializeField] TMP_Text        monsterLevelBadgeText;
    [SerializeField] FloatingText    floatingTextPrefab;
    [SerializeField] RectTransform   heroFloatingAnchor;
    [SerializeField] RectTransform   monsterFloatingAnchor;

    // ── Result panel ─────────────────────────────────────────────────────────
    [SerializeField] GameObject      resultPanel;
    [SerializeField] TMP_Text        winlossText;       // "Victory" / "Defeat"
    [SerializeField] TMP_Text        monsterTEXT;       // "Defeated <name>"
    [SerializeField] Image           monsterHead;       // monster portrait
    [SerializeField] TMP_Text        resultText;        // "Level Up" or "+X XP"
    [SerializeField] Image           awardedCardImage;  // full card art
    [SerializeField] Image           awardedCardIcon;   // move icon
    [SerializeField] TMP_Text        awardedMoveLabel;
    [SerializeField] GameObject      awardedCardObject;
    [SerializeField] GameObject      retryButton;
    [SerializeField] GameObject      inventoryButton;      // "Next" button
    [SerializeField] TMP_Text        inventoryButtonLabel;
    [SerializeField] GameObject      finishButton;         // shown only after dragon
    [SerializeField] GameObject      allMovesEarnedLabel;

    // ── HUD ──────────────────────────────────────────────────────────────────
    [SerializeField] Image           heroAvatarSprite;
    [SerializeField] Image           monsterAvatarSprite;
    [SerializeField] Button          spellbookHudButton;
    [SerializeField] Image           spellbookHudIcon;

    // ── Pause panel ──────────────────────────────────────────────────────────
    [SerializeField] GameObject      pausePanel;
    [SerializeField] Image           pauseMusicButtonImage;
    [SerializeField] Image           pauseSoundButtonImage;

    private static readonly Color OffColor = new Color(0.4f, 0.4f, 0.4f);

    // Result callbacks — wired by BattleManager via SetupResultCallbacks.
    private Action _onVictoryRetry;
    private Action _onVictoryNext;
    private Action _onDefeatRetry;

    void Awake()
    {
        ApplyPixelSprite(inventoryButton);
        ApplyPixelSprite(retryButton);
        if (pausePanel  != null) pausePanel.SetActive(false);
        if (finishButton != null) finishButton.SetActive(false);
    }

    public void SetupBattle(MonsterData monster, List<string> heroMoveIds,
                            Dictionary<string, MoveData> allMoves, Action<string> onMoveClicked)
    {
        if (resultPanel != null) resultPanel.SetActive(false);

        if (spellbookHudButton != null)
        {
            spellbookHudButton.gameObject.SetActive(true);
            spellbookHudButton.interactable = false;
        }

        battleLog.Clear();
        moveButtonPanel.Populate(heroMoveIds, allMoves, onMoveClicked);
        moveButtonPanel.SetInteractable(false);

        if (spellbookHudIcon != null)
        {
            var sp = Resources.Load<Sprite>("avatars/spellbook");
            if (sp != null) { spellbookHudIcon.sprite = sp; spellbookHudIcon.color = Color.white; }
        }
    }

    public void SetupResultCallbacks(Action onViewMode, Action onVictoryRetry, Action onVictoryNext, Action onDefeatRetry)
    {
        _onVictoryRetry = onVictoryRetry;
        _onVictoryNext  = onVictoryNext;
        _onDefeatRetry  = onDefeatRetry;

        if (spellbookHudButton != null)
        {
            spellbookHudButton.onClick.RemoveAllListeners();
            spellbookHudButton.onClick.AddListener(() => onViewMode?.Invoke());
        }
        if (inventoryButtonLabel != null) inventoryButtonLabel.text = "Next";
    }

    // ── Level / name display ─────────────────────────────────────────────────

    public void SetHeroNameLevel(int level)
    {
        if (heroNameText        != null) heroNameText.text        = "HERO";
        if (heroBottomLevelText != null) heroBottomLevelText.text = "HERO";
        if (heroLevelBadgeText  != null) heroLevelBadgeText.text  = level.ToString();
        heroHealthBar?.SetName("HERO");
        heroBottomBar?.SetName("HERO");
    }

    public void SetMonsterNameLevel(string name, int level)
    {
        if (monsterNameText        != null) monsterNameText.text        = name;
        if (monsterBottomLevelText != null) monsterBottomLevelText.text = name;
        if (monsterLevelBadgeText  != null) monsterLevelBadgeText.text  = level.ToString();
        monsterHealthBar?.SetName(name);
        monsterBottomBar?.SetName(name);
    }

    // ── HP ───────────────────────────────────────────────────────────────────

    public void SetHeroMaxHP(int max)
    {
        heroHealthBar?.SetMax(max);
        heroBottomBar?.SetMax(max);
    }

    public void SetMonsterMaxHP(int max)
    {
        monsterHealthBar?.SetMax(max);
        monsterBottomBar?.SetMax(max);
    }

    public void UpdateHeroHP(int cur, int max)
    {
        heroHealthBar?.UpdateHP(cur, max);
        heroBottomBar?.UpdateHP(cur, max);
    }

    public void UpdateMonsterHP(int cur, int max)
    {
        monsterHealthBar?.UpdateHP(cur, max);
        monsterBottomBar?.UpdateHP(cur, max);
    }

    // ── XP bar ──────────────────────────────────────────────────────────────

    public void InitHeroXpBar(int xp, int xpMax)
    {
        if (heroXpBar == null) return;
        heroXpBar.SetMax(xpMax);
        heroXpBar.UpdateHP(xp, xpMax);
    }

    public void UpdateHeroXp(int xp, int xpMax) => heroXpBar?.UpdateHP(xp, xpMax);

    // ── Avatars ──────────────────────────────────────────────────────────────

    public void SetHeroAvatar(Sprite sprite)
    {
        if (heroAvatarSprite == null) return;
        heroAvatarSprite.sprite = sprite;
        heroAvatarSprite.color  = sprite != null ? Color.white : Color.clear;
    }

    public void SetMonsterAvatar(Sprite sprite)
    {
        if (monsterAvatarSprite == null) return;
        monsterAvatarSprite.sprite = sprite;
        monsterAvatarSprite.color  = sprite != null ? Color.white : Color.clear;
    }

    // ── Floating text ────────────────────────────────────────────────────────

    public void ShowFloatingText(string text, Color color, bool onHero)
    {
        if (floatingTextPrefab == null) return;
        var anchor = onHero ? heroFloatingAnchor : monsterFloatingAnchor;
        if (anchor == null) return;
        float xJitter = UnityEngine.Random.Range(-25f, 25f);
        var instance = Instantiate(floatingTextPrefab, anchor.parent);
        instance.gameObject.SetActive(true);
        var rt = instance.GetComponent<RectTransform>();
        rt.anchoredPosition = anchor.anchoredPosition + new Vector2(xJitter, 0);
        instance.Play(text, color);
    }

    // ── Log ─────────────────────────────────────────────────────────────────

    public void LogEntry(string text)
    {
        if (battleLog == null) { Debug.LogWarning("[BattleUIController] battleLog not wired: " + text); return; }
        battleLog.AddEntry(text);
    }

    public void LockMoveButtons()   => moveButtonPanel.SetInteractable(false);
    public void UnlockMoveButtons() => moveButtonPanel.SetInteractable(true);

    public void RefreshMoveCooldowns(Dictionary<string, int> cooldowns)
        => moveButtonPanel.RefreshCooldowns(cooldowns);

    // ── Result panel ─────────────────────────────────────────────────────────

    public void ShowResult(bool victory, bool leveledUp, int xpReward,
                           string monsterName, string monsterId,
                           MoveData awardedMove = null, string awardedMoveId = null,
                           bool allMovesEarned = false, bool isFinalBoss = false)
    {


        if (victory)
            AudioManager.PlaySFX("sounds/sound_effects/win");

        LockMoveButtons();
        if (resultPanel != null) resultPanel.SetActive(true);

        // Win / loss label
        if (winlossText != null)
            winlossText.text = victory ? "Victory" : "Defeat";

        // Monster name label — only on victory
        if (monsterTEXT != null)
        {
            monsterTEXT.gameObject.SetActive(victory);
            if (victory) monsterTEXT.text = $"Defeated {monsterName}";
        }

        // Portrait logic: monster on win, hero on lose
        if (monsterHead != null)
        {
            string headName;

            if (victory)
            {
                headName = GetMonsterHeadSpriteName(monsterId);
            }
            else
            {
                headName = "knightHead"; // 👈 tvoj player sprite
            }

            if (!string.IsNullOrEmpty(headName))
            {
                var sp = Resources.Load<Sprite>($"avatars/{headName}");
                if (sp != null)
                {
                    monsterHead.sprite = sp;
                    monsterHead.color = Color.white;
                }
            }
        }

        // Result text — XP / level up (victory only)
        if (resultText != null)
        {
            resultText.gameObject.SetActive(victory);
            if (victory)
                resultText.text = leveledUp ? "Level Up" : $"+{xpReward} XP";
        }

        // Spellbook HUD
        if (spellbookHudButton != null)
            spellbookHudButton.interactable = victory;

        bool showCard      = victory && awardedMove != null;
        bool showAllEarned = victory && awardedMove == null && allMovesEarned;

        if (awardedCardImage != null)
        {
            awardedCardImage.gameObject.SetActive(showCard);
            if (showCard) awardedCardImage.sprite = LoadCardSprite(awardedMove);
        }

        if (awardedCardIcon != null)
        {
            awardedCardIcon.gameObject.SetActive(showCard);
            if (showCard)
            {
                Sprite iconSp = null;
                if (awardedMoveId != null) iconSp = Resources.Load<Sprite>($"required_icons/{awardedMoveId}");
                if (iconSp == null) iconSp = LoadCardSprite(awardedMove);
                awardedCardIcon.sprite = iconSp;
            }
        }

        if (awardedMoveLabel != null)
        {
            awardedMoveLabel.gameObject.SetActive(showCard);
            awardedCardObject.gameObject.SetActive(showCard);
            if (showCard) awardedMoveLabel.text = "NEW: " + awardedMove.name;
        }

        if (allMovesEarnedLabel != null)
        {
            allMovesEarnedLabel.SetActive(showAllEarned);
        }


        // Buttons: final boss hides Next + Retry and shows Finish; defeat hides Next
        if (isFinalBoss && victory)
        {
            if (retryButton     != null) retryButton.SetActive(false);
            if (inventoryButton != null) inventoryButton.SetActive(false);
            if (finishButton    != null)
            {
                finishButton.SetActive(true);
                var btn = finishButton.GetComponent<Button>();
                if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(GoToEndingScene); }
            }
        }
        else if (victory)
        {
            if (finishButton != null) finishButton.SetActive(false);

            if (retryButton != null)
            {
                retryButton.SetActive(true);
                var btn = retryButton.GetComponent<Button>();
                if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => _onVictoryRetry?.Invoke()); }
            }
            if (inventoryButton != null)
            {
                inventoryButton.SetActive(true);
                var btn = inventoryButton.GetComponent<Button>();
                if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => _onVictoryNext?.Invoke()); }
            }
        }
        else
        {
            if (finishButton    != null) finishButton.SetActive(false);
            if (inventoryButton != null) inventoryButton.SetActive(false);

            if (retryButton != null)
            {
                retryButton.SetActive(true);
                var btn = retryButton.GetComponent<Button>();
                if (btn != null) { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(() => _onDefeatRetry?.Invoke()); }
            }
        }
    }

    // ── Pause panel ──────────────────────────────────────────────────────────

    public void ShowPause(bool show)
    {
        if (pausePanel != null) pausePanel.SetActive(show);
        if (show) RefreshPauseAudioButtons();
    }

    public void OnPauseMusicClicked()
    {
        AudioManager.ToggleMusic();
        RefreshPauseAudioButtons();
    }

    public void OnPauseSoundClicked()
    {
        AudioManager.ToggleSound();
        RefreshPauseAudioButtons();
    }

    private void RefreshPauseAudioButtons()
    {
        if (pauseMusicButtonImage != null)
            pauseMusicButtonImage.color = AudioManager.IsMusicEnabled ? Color.white : OffColor;
        if (pauseSoundButtonImage != null)
            pauseSoundButtonImage.color = AudioManager.IsSoundEnabled ? Color.white : OffColor;
    }

    // ── Finish / ending ──────────────────────────────────────────────────────

    public void GoToEndingScene() => GameManager.Instance.GoToEnding();

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string GetMonsterHeadSpriteName(string monsterId) => monsterId switch
    {
        "goblin_warrior" => "goblinHead",
        "goblin_mage"    => "goblinHead",
        "giant_spider"   => "spiderHead",
        "witch"          => "witchHead",
        "dragon"         => "dragonHead",
        _                => null
    };

    private static Sprite LoadCardSprite(MoveData move)
    {
        string key = ResolveCardName(move);
        var sp  = Resources.Load<Sprite>($"cards/{key}");
        if (sp != null) return sp;
        var tex = Resources.Load<Texture2D>($"cards/{key}");
        return tex != null
            ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f))
            : null;
    }

    private static string ResolveCardName(MoveData move)
    {
        if (move.dot          != null) return "poison";
        if (move.lifesteal)             return "lifesteal";
        if (move.stunChance    > 0f)   return "stun";
        if (move.healPercent   > 0f || move.selfHeal > 0) return "heal";
        if (move.magMultiplier > 0f)   return "magic";
        if (move.atkMultiplier > 0f)   return "damage";
        return "defensive";
    }

    private static void ApplyPixelSprite(GameObject buttonGo)
    {
        if (buttonGo == null) return;
        var img = buttonGo.GetComponent<Image>();
        if (img == null) return;
        var tex = Resources.Load<Texture2D>("avatars/ButtonDefault");
        if (tex == null) return;
        tex.filterMode = FilterMode.Point;
        img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        img.type   = Image.Type.Simple;
    }
}
