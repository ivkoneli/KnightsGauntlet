using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameConfig   Config { get; private set; }
    public HeroProgress Hero   { get; private set; }
    public bool IsConfigLoaded => Config != null;

    public int  CurrentMonsterIndex { get; private set; } = 0;
    public bool IsLastMonster       => Config != null && CurrentMonsterIndex >= Config.monsters.Count - 1;

    public void AdvanceMonster()
    {
        if (!IsLastMonster) { CurrentMonsterIndex++; SaveProgress(); }
    }

    public void RevertMonster()
    {
        if (CurrentMonsterIndex > 0) { CurrentMonsterIndex--; SaveProgress(); }
    }

    public event Action OnConfigLoaded;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        StartCoroutine(LoadConfig());
    }

    private IEnumerator LoadConfig()
    {
        yield return StartCoroutine(ApiClient.GetConfig(config =>
        {
            Config = config;
            if (config != null)
            {
                Hero = new HeroProgress();
                Hero.Initialize(config.hero.starterMoves);

                if (PlayerPrefs.GetInt("pg_exists", 0) == 1)
                    LoadProgress();

                Debug.Log("[GameManager] Config loaded successfully.");
                Debug.Log($"  Hero: {config.hero.name} — starter moves: {string.Join(", ", config.hero.starterMoves)}");
                Debug.Log($"  Monsters ({config.monsters.Count}): {string.Join(", ", config.monsters.ConvertAll(m => m.name))}");
                Debug.Log($"  Total moves: {config.moves.Count}");
            }
            else
            {
                Debug.LogError("[GameManager] Config failed to load — check backend connection.");
            }
            OnConfigLoaded?.Invoke();
        }));
    }

    // Awards XP + unlocks a random monster move the hero doesn't have yet.
    // Returns the awarded move id, or null if nothing new to give.
    public string AwardVictory(MonsterData monster)
    {
        Hero.AddXp(monster.xpReward);

        var candidates = new List<string>();
        foreach (string id in monster.moves)
            if (!Hero.IsUnlocked(id)) candidates.Add(id);

        if (candidates.Count == 0) { SaveProgress(); return null; }

        string awarded = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        Hero.UnlockMove(awarded);
        SaveProgress();
        return awarded;
    }

    // ── Persistence ──────────────────────────────────────────────────────────

    public void SaveProgress()
    {
        PlayerPrefs.SetInt("pg_exists",  1);
        PlayerPrefs.SetInt("pg_level",   Hero.level);
        PlayerPrefs.SetInt("pg_xp",      Hero.xp);
        PlayerPrefs.SetInt("pg_monster", CurrentMonsterIndex);
        PlayerPrefs.SetString("pg_unlocked", JoinIds(Hero.UnlockedMoveIds));
        PlayerPrefs.SetString("pg_equipped", JoinIds(Hero.EquippedMoveIds));
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        int          level    = PlayerPrefs.GetInt("pg_level",   1);
        int          xp       = PlayerPrefs.GetInt("pg_xp",      0);
        List<string> unlocked = SplitIds(PlayerPrefs.GetString("pg_unlocked", ""));
        List<string> equipped = SplitIds(PlayerPrefs.GetString("pg_equipped", ""));

        // Only restore equipped moves the hero actually has unlocked
        equipped.RemoveAll(id => !unlocked.Contains(id));

        Hero.LoadFrom(level, xp, unlocked, equipped);
        CurrentMonsterIndex = PlayerPrefs.GetInt("pg_monster", 0);

        // Clamp to valid range in case config changed
        if (Config != null)
            CurrentMonsterIndex = Mathf.Clamp(CurrentMonsterIndex, 0, Config.monsters.Count - 1);

        Debug.Log($"[GameManager] Progress loaded — Lv.{level} XP:{xp} Monster:{CurrentMonsterIndex}");
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("pg_exists");
        PlayerPrefs.DeleteKey("pg_level");
        PlayerPrefs.DeleteKey("pg_xp");
        PlayerPrefs.DeleteKey("pg_monster");
        PlayerPrefs.DeleteKey("pg_unlocked");
        PlayerPrefs.DeleteKey("pg_equipped");
        PlayerPrefs.Save();

        CurrentMonsterIndex = 0;
        if (Config != null)
        {
            Hero = new HeroProgress();
            Hero.Initialize(Config.hero.starterMoves);
        }
    }

    private static string JoinIds(IEnumerable<string> ids) => string.Join(",", ids);

    private static List<string> SplitIds(string s) =>
        string.IsNullOrEmpty(s) ? new List<string>() : new List<string>(s.Split(','));

    // ── Scene navigation ─────────────────────────────────────────────────────

    public void StartRun()
    {
        if (!IsConfigLoaded) { Debug.LogWarning("[GameManager] StartRun called before config was loaded."); return; }
        Debug.Log("[GameManager] StartRun — loading Battle scene.");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
    }

    public bool InventoryEditMode { get; private set; }

    public void GoToInventory(bool editMode = false)
    {
        InventoryEditMode = editMode;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Inventory");
    }

    public void GoToIntro()
    {
        AudioManager.StopMusic();
        UnityEngine.SceneManagement.SceneManager.LoadScene("IntroScene");
    }
    public void GoToBattle()    => UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
    public void GoToMainMenu()  => UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    public void GoToEnding()    => UnityEngine.SceneManagement.SceneManager.LoadScene("EndingScene");
}
