using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Drives the full turn loop.
// Flow: show spell cards → wait for click → execute hero move →
//       API call for monster move → execute monster move → repeat.
public class BattleManager : MonoBehaviour
{
    [SerializeField] BattleUIController ui;
    [SerializeField] CombatantDisplay   heroDisplay;
    [SerializeField] CombatantDisplay   monsterDisplay;
    [SerializeField] GameObject         spellCardsPanel;

    private MonsterData  _monster;
    private BattleStats  _heroStats    = new();
    private BattleStats  _monsterStats = new();
    private List<string> _heroMoveIds;

    private int    _currentTurn;
    private string _lastMonsterMove = "";
    private bool   _heroMoveChosen;
    private string _chosenMoveId;
    private bool   _isBattleOver;
    private bool   _heroSkippedTurn;
    private bool   _monsterAdvanced;
    private bool   _isPaused;
    private readonly Dictionary<string, int> _moveCooldowns        = new();
    private readonly Dictionary<string, int> _monsterMoveCooldowns = new();

    const string HeroColor    = "yellow";
    const string MonsterColor = "#CC7744";

    void Start()
    {
        GameConfig config = GameManager.Instance?.Config;
        if (config == null)
        {
            Debug.LogError("[BattleManager] Config not loaded — return to MainMenu first.");
            return;
        }

        _monster     = config.monsters[GameManager.Instance.CurrentMonsterIndex];
        _heroMoveIds = new List<string>(GameManager.Instance.Hero.EquippedMoveIds);

        Texture2D rogues   = Resources.Load<Texture2D>("SpriteSheets/rogues");
        Texture2D monsters = Resources.Load<Texture2D>("SpriteSheets/monsters");

        if (rogues != null)
        {
            rogues.filterMode = FilterMode.Point;
            ui.SetHeroAvatar(SpriteManager.GetSprite(rogues, 1, 0, 32));
        }
        else Debug.LogWarning("[BattleManager] rogues.png not found in Resources/SpriteSheets/");

        if (monsters != null)
        {
            monsters.filterMode = FilterMode.Point;
            ui.SetMonsterAvatar(SpriteManager.GetSprite(monsters, _monster.spriteRow, _monster.spriteCol, 32));
        }
        else Debug.LogWarning("[BattleManager] monsters.png not found in Resources/SpriteSheets/");

        bool heroAnimOk = heroDisplay.SetupAnimation(
            "char animations/KnightIdleAnim", 6f,
            "char animations/KnightAttackAnim", 8f);
        if (!heroAnimOk && rogues != null)
            heroDisplay.SetSprite(rogues, 1, 0);

        string monsterAnimPath = GetMonsterAnimPath(_monster.id);
        bool monsterAnimOk = monsterAnimPath != null &&
            monsterDisplay.SetupAnimation(monsterAnimPath, 6f);
        if (!monsterAnimOk && monsters != null)
            monsterDisplay.SetSprite(monsters, _monster.spriteRow, _monster.spriteCol);

        bool isDragon = _monster.id == "dragon";
        AudioManager.PlayMusic(isDragon ? "sounds/DragonFight" : "sounds/3");

        StartBattle();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !_isBattleOver)
        {
            if (_isPaused) ResumeGame();
            else           PauseGame();
        }
    }

    public void StartBattle()
    {
        GameConfig   config = GameManager.Instance.Config;
        HeroProgress hero   = GameManager.Instance.Hero;

        int lvl = hero.level;
        var scaled = new BaseStats
        {
            hp  = config.hero.baseStats.hp  + config.hero.statsPerLevel.hp  * (lvl - 1),
            atk = config.hero.baseStats.atk + config.hero.statsPerLevel.atk * (lvl - 1),
            def = config.hero.baseStats.def + config.hero.statsPerLevel.def * (lvl - 1),
            mag = config.hero.baseStats.mag + config.hero.statsPerLevel.mag * (lvl - 1),
        };

        _heroStats.Reset(scaled);
        _monsterStats.Reset(_monster.stats);
        _heroMoveChosen  = false;
        _isBattleOver    = false;
        _isPaused        = false;
        _currentTurn     = 1;
        _lastMonsterMove = "";
        _moveCooldowns.Clear();
        _monsterMoveCooldowns.Clear();

        ui.ShowPause(false);
        ui.SetHeroMaxHP(_heroStats.maxHp);
        ui.SetMonsterMaxHP(_monsterStats.maxHp);
        ui.SetHeroNameLevel(hero.level);
        ui.SetMonsterNameLevel(_monster.name, _monster.difficulty);
        MoveTooltip.SetHeroStats(_heroStats);
        ui.InitHeroXpBar(hero.xp, hero.XpToNextLevel);
        ui.SetupResultCallbacks(GoToInventoryView, RetryToInventory, GoToInventoryEdit, RetryBattle);
        ui.SetupBattle(_monster, _heroMoveIds, config.moves, OnHeroMoveSelected);
        _monsterAdvanced = false;

        StartCoroutine(BattleTurnLoop());
    }

    // ── Pause ────────────────────────────────────────────────────────────────

    private void PauseGame()
    {
        _isPaused = true;
        ui.ShowPause(true);
    }

    public void ResumeGame()
    {
        _isPaused = false;
        ui.ShowPause(false);
    }

    public void RestartBattle()
    {
        _isPaused = false;
        GameManager.Instance.GoToBattle();
    }

    public void GoToMainMenuFromPause()
    {
        _isPaused = false;
        GameManager.Instance.GoToMainMenu();
    }

    // ── Turn loop ────────────────────────────────────────────────────────────

    private IEnumerator BattleTurnLoop()
    {
        while (!_isBattleOver)
        {
            _heroMoveChosen  = false;
            _heroSkippedTurn = false;
            ui.LogEntry($"<color=#555555><size=80%>── Turn {_currentTurn} ──</size></color>");

            yield return new WaitWhile(() => _isPaused);
            yield return StartCoroutine(StartHeroTurn());
            if (_isBattleOver) break;

            if (!_heroSkippedTurn)
            {
                spellCardsPanel?.SetActive(true);
                heroDisplay.SetOutline(true, Color.white);
                ui.RefreshMoveCooldowns(_moveCooldowns);
                yield return new WaitUntil(() => _heroMoveChosen);
                heroDisplay.SetOutline(false, Color.white);
                spellCardsPanel?.SetActive(false);
                yield return new WaitWhile(() => _isPaused);
                yield return StartCoroutine(ExecuteHeroMove(_chosenMoveId));
                if (_isBattleOver) break;
            }

            string monsterMoveId = null;
            yield return StartCoroutine(
                ApiClient.GetMonsterMove(BuildRequest(), id => monsterMoveId = id));
            yield return new WaitWhile(() => _isPaused);
            monsterDisplay.SetOutline(true, Color.red);
            yield return StartCoroutine(ExecuteMonsterMove(monsterMoveId));
            monsterDisplay.SetOutline(false, Color.red);

            _currentTurn++;
        }
    }

    private IEnumerator StartHeroTurn()
    {
        int dotDmg = _heroStats.activeDot?.damage ?? 0;
        if (dotDmg > 0)
        {
            _heroStats.TickDot();
            heroDisplay.FlashOnHit();
            ui.ShowFloatingText($"Poison (-{dotDmg})", new Color(0.67f, 0.17f, 0.67f), onHero: true);
            ui.LogEntry($"<color={HeroColor}>HERO</color> suffers <color=#AA44AA>{dotDmg} Poison</color>");
            ui.UpdateHeroHP(_heroStats.currentHp, _heroStats.maxHp);
            yield return new WaitForSeconds(0.6f);
            yield return new WaitWhile(() => _isPaused);
            CheckBattleEnd();
            if (_isBattleOver) yield break;
        }

        if (_heroStats.isStunned)
        {
            int turns = _heroStats.stunTurnsRemaining;
            _heroStats.stunTurnsRemaining--;
            _heroSkippedTurn = true;
            ui.ShowFloatingText($"Stunned ({turns} Turn{(turns > 1 ? "s" : "")})", Color.red, onHero: true);
            ui.LogEntry($"<color={HeroColor}>HERO</color> is <color=gray>STUNNED</color> — skips turn!");
            yield return new WaitForSeconds(0.8f);
            yield return new WaitWhile(() => _isPaused);
        }
    }

    public void OnHeroMoveSelected(string moveId)
    {
        if (_heroMoveChosen || _isBattleOver) return;
        _chosenMoveId   = moveId;
        _heroMoveChosen = true;
        ui.LockMoveButtons();
    }

    // ── Move execution ───────────────────────────────────────────────────────

    private IEnumerator ExecuteHeroMove(string moveId)
    {
        GameConfig config = GameManager.Instance.Config;
        if (!config.moves.TryGetValue(moveId, out MoveData move))
        {
            Debug.LogWarning($"[BattleManager] Hero move not found: {moveId}");
            yield break;
        }

        bool isSlashMove = moveId == "slash" || moveId == "rusty_blade";

        if (isSlashMove)
        {
            heroDisplay.SwitchToAttack();
            AudioManager.PlaySFX("sounds/sound_effects/knight_slash");
        }
        else if (moveId == "second_wind")
        {
            AudioManager.PlaySFX("sounds/sound_effects/healing");
        }

        if (move.target == "enemy")
            yield return StartCoroutine(heroDisplay.LungeForward(monsterDisplay.transform.position));

        ApplyMove(move, _heroStats, _monsterStats, isHeroAttacking: true);
        if (move.cooldown > 0) _moveCooldowns[moveId] = move.cooldown;

        if (move.target == "enemy")
            yield return StartCoroutine(heroDisplay.LungeReturn());

        if (isSlashMove) heroDisplay.ResumeIdle();

        ui.UpdateHeroHP(_heroStats.currentHp, _heroStats.maxHp);
        ui.UpdateMonsterHP(_monsterStats.currentHp, _monsterStats.maxHp);

        yield return new WaitForSeconds(0.55f);
        yield return new WaitWhile(() => _isPaused);
        CheckBattleEnd();
    }

    private IEnumerator ExecuteMonsterMove(string moveId)
    {
        if (_isBattleOver) yield break;

        GameConfig config = GameManager.Instance.Config;

        if (_monsterStats.isStunned)
        {
            _monsterStats.stunTurnsRemaining--;
            ui.LogEntry($"<color={MonsterColor}>{_monster.name.ToUpper()}</color> is <color=gray>STUNNED</color> — skips turn!");
        }
        else
        {
            Debug.Log($"[Battle T{_currentTurn}] SERVER chose move [{moveId}] for {_monster.name}");
            if (config.moves.TryGetValue(moveId, out MoveData move))
            {
                if (move.target == "enemy")
                    yield return StartCoroutine(monsterDisplay.LungeForward(heroDisplay.transform.position));

                _lastMonsterMove = moveId;
                ApplyMove(move, _monsterStats, _heroStats, isHeroAttacking: false);
                if (move.cooldown > 0) _monsterMoveCooldowns[moveId] = move.cooldown;

                if (move.target == "enemy")
                    yield return StartCoroutine(monsterDisplay.LungeReturn());
            }
            else
            {
                Debug.LogWarning($"[BattleManager] Monster move not found: {moveId}");
            }
        }

        int monsterDotDmg = _monsterStats.activeDot?.damage ?? 0;
        _monsterStats.TickDot();
        _heroStats.TickModifiers();
        _monsterStats.TickModifiers();
        TickMoveCooldowns();

        if (monsterDotDmg > 0)
        {
            monsterDisplay.FlashOnHit();
            ui.ShowFloatingText($"Poison (-{monsterDotDmg})", new Color(0.67f, 0.17f, 0.67f), onHero: false);
            ui.LogEntry($"<color={MonsterColor}>{_monster.name.ToUpper()}</color> suffers <color=#AA44AA>{monsterDotDmg} Poison</color>");
        }

        ui.UpdateHeroHP(_heroStats.currentHp, _heroStats.maxHp);
        ui.UpdateMonsterHP(_monsterStats.currentHp, _monsterStats.maxHp);

        yield return new WaitForSeconds(1.0f);
        yield return new WaitWhile(() => _isPaused);
        CheckBattleEnd();
    }

    // ── Shared move resolution ───────────────────────────────────────────────

    private void ApplyMove(MoveData move, BattleStats attacker, BattleStats defender, bool isHeroAttacking)
    {
        string atkName    = isHeroAttacking ? "HERO" : _monster.name.ToUpper();
        string atkCol     = isHeroAttacking ? HeroColor : MonsterColor;
        var    parts      = new List<string>();
        int    damage     = 0;
        bool   onDefender = !isHeroAttacking;
        bool   onAttacker = isHeroAttacking;

        if (move.atkMultiplier > 0)
        {
            damage = MoveEffectProcessor.ResolvePhysicalDamage(move, attacker, defender);
            defender.currentHp = Mathf.Max(0, defender.currentHp - damage);
            string c = move.dot != null ? "#AA44AA" : "red";
            parts.Add($"<color={c}>{damage} DMG</color>");
            ui.ShowFloatingText(damage.ToString(), Color.white, onDefender);
        }
        else if (move.magMultiplier > 0)
        {
            damage = MoveEffectProcessor.ResolveMagicDamage(move, attacker);
            defender.currentHp = Mathf.Max(0, defender.currentHp - damage);
            string c = move.dot != null ? "#AA44AA" : "#4488FF";
            parts.Add($"<color={c}>{damage} DMG</color>");
            ui.ShowFloatingText(damage.ToString(), Color.white, onDefender);
        }

        if (damage > 0)
            (isHeroAttacking ? monsterDisplay : heroDisplay).FlashOnHit();

        if (move.healPercent > 0)
        {
            int heal = MoveEffectProcessor.ResolveHeal(move, attacker);
            attacker.currentHp = Mathf.Min(attacker.maxHp, attacker.currentHp + heal);
            parts.Add($"<color=#44DD44>+{heal} HP</color>");
            ui.ShowFloatingText($"Heal (+{heal})", new Color(0.27f, 0.87f, 0.27f), onAttacker);
            (isHeroAttacking ? heroDisplay : monsterDisplay).FlashHeal();
        }

        if (move.selfHeal > 0)
        {
            attacker.currentHp = Mathf.Min(attacker.maxHp, attacker.currentHp + move.selfHeal);
            parts.Add($"<color=#44DD44>+{move.selfHeal} HP</color>");
            ui.ShowFloatingText($"Heal (+{move.selfHeal})", new Color(0.27f, 0.87f, 0.27f), onAttacker);
            (isHeroAttacking ? heroDisplay : monsterDisplay).FlashHeal();
        }

        if (move.selfDamage > 0)
        {
            attacker.currentHp = Mathf.Max(0, attacker.currentHp - move.selfDamage);
            parts.Add($"<color=red>-{move.selfDamage} self</color>");
            (isHeroAttacking ? heroDisplay : monsterDisplay).FlashOnHit();
        }

        if (move.lifesteal && damage > 0)
        {
            attacker.currentHp = Mathf.Min(attacker.maxHp, attacker.currentHp + damage);
            parts.Add($"<color=#CC44AA>+{damage} stolen</color>");
            ui.ShowFloatingText($"Heal (+{damage})", new Color(0.27f, 0.87f, 0.27f), onAttacker);
            (isHeroAttacking ? heroDisplay : monsterDisplay).FlashLifesteal();
        }

        if (move.dot != null)
        {
            defender.activeDot = new DotEffect
                { damage = move.dot.damage, duration = move.dot.duration, type = move.dot.type };
            parts.Add($"<color=#AA44AA>Poison {move.dot.duration}t</color>");
        }

        BattleStats statTarget = move.target == "self" ? attacker : defender;
        if (move.statChanges != null && move.statChanges.Count > 0)
        {
            MoveEffectProcessor.ApplyStatChanges(move, statTarget);
            var buffParts = new List<string>();
            foreach (var sc in move.statChanges)
                buffParts.Add($"{sc.stat.ToUpper()} {(sc.amount >= 0 ? "+" : "")}{sc.amount}");
            parts.Add($"<color=#CC9944>{string.Join(" ", buffParts)}</color>");
            if (move.target == "self")
                (isHeroAttacking ? heroDisplay : monsterDisplay).FlashBuff();
        }

        if (MoveEffectProcessor.RollStun(move))
        {
            defender.stunTurnsRemaining = 1;
            parts.Add("<color=gray>STUNNED</color>");
            ui.ShowFloatingText("Stunned (1 Turn)", Color.red, onDefender);
        }

        string fx = parts.Count > 0 ? ", " + string.Join(", ", parts) : "";
        ui.LogEntry($"<color={atkCol}>{atkName}</color> used [{move.name}]{fx}");
    }

    // ── Battle end ───────────────────────────────────────────────────────────

    private void CheckBattleEnd()
    {
        if (_isBattleOver) return;

        if (_monsterStats.currentHp <= 0)
        {
            _isBattleOver = true;

            int          oldLevel  = GameManager.Instance.Hero.level;
            string       awardedId = GameManager.Instance.AwardVictory(_monster);
            HeroProgress hero      = GameManager.Instance.Hero;
            bool         leveledUp = hero.level > oldLevel;
            bool         isLast    = GameManager.Instance.IsLastMonster;

            bool allMovesEarned = awardedId == null && AllCurrentMonsterMovesUnlocked();

            MoveData awardedMove = null;
            if (awardedId != null)
                GameManager.Instance.Config.moves.TryGetValue(awardedId, out awardedMove);

            ui.UpdateHeroXp(hero.xp, hero.XpToNextLevel);
            if (leveledUp) ui.SetHeroNameLevel(hero.level);

            _monsterAdvanced = !isLast;
            if (!isLast) GameManager.Instance.AdvanceMonster();

            if (isLast)
                ui.LogEntry("<color=yellow>CONGRATULATIONS! All enemies defeated!</color>");
            else
                ui.LogEntry($"<color=yellow>Victory! {_monster.name} defeated!</color>");

            ui.ShowResult(
                victory:      true,
                leveledUp:    leveledUp,
                xpReward:     _monster.xpReward,
                monsterName:  _monster.name,
                monsterId:    _monster.id,
                awardedMove:  awardedMove,
                awardedMoveId: awardedId,
                allMovesEarned: allMovesEarned,
                isFinalBoss:  isLast);
        }
        else if (_heroStats.currentHp <= 0)
        {
            _isBattleOver = true;
            ui.LogEntry("<color=red>Defeat! The hero has fallen...</color>");
            ui.ShowResult(
                victory:     false,
                leveledUp:   false,
                xpReward:    0,
                monsterName: _monster.name,
                monsterId:   _monster.id);
        }
    }

    private bool AllCurrentMonsterMovesUnlocked()
    {
        if (_monster.moves == null || _monster.moves.Count == 0) return false;
        foreach (string id in _monster.moves)
            if (!GameManager.Instance.Hero.IsUnlocked(id)) return false;
        return true;
    }

    public void GoToInventoryView() => GameManager.Instance.GoToInventory(editMode: false);
    public void GoToInventoryEdit() => GameManager.Instance.GoToInventory(editMode: true);

    public void RetryToInventory()
    {
        if (_monsterAdvanced) GameManager.Instance.RevertMonster();
        GameManager.Instance.GoToInventory(editMode: true);
    }

    public void RetryBattle() => GameManager.Instance.GoToBattle();

    private void TickMoveCooldowns()
    {
        TickCooldownDict(_moveCooldowns);
        TickCooldownDict(_monsterMoveCooldowns);
    }

    private static void TickCooldownDict(Dictionary<string, int> dict)
    {
        var keys = new List<string>(dict.Keys);
        foreach (var key in keys)
        {
            if (--dict[key] <= 0)
                dict.Remove(key);
        }
    }

    private static string GetMonsterAnimPath(string monsterId) => monsterId switch
    {
        "goblin_warrior" => "char animations/GoblinMonsterIdleAnim",
        "goblin_mage"    => "char animations/goblinMageIdleAnim",
        "giant_spider"   => "char animations/caveSpiderIdleAnim",
        "witch"          => "char animations/hagWitchIdleAnim",
        "dragon"         => "char animations/dragonIdleAnim",
        _                => null
    };

    private MonsterMoveRequest BuildRequest() => new()
    {
        monsterId     = _monster.id,
        monsterHp     = _monsterStats.currentHp,
        monsterMaxHp  = _monsterStats.maxHp,
        heroHp        = _heroStats.currentHp,
        heroMaxHp     = _heroStats.maxHp,
        heroAtk       = _heroStats.currentAtk,
        heroDef       = _heroStats.currentDef,
        heroMag       = _heroStats.currentMag,
        lastMove      = _lastMonsterMove,
        turn          = _currentTurn,
        cooldownMoves = new List<string>(_monsterMoveCooldowns.Keys)
    };
}
