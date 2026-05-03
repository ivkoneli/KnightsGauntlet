using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Scene controller for Inventory.unity.
// Left panel: hero stats. Right top: 4 equipped slots. Right bottom: 5×5 card grid.
public class InventoryController : MonoBehaviour
{
    [Header("Stats Panel")]
    [SerializeField] TMP_Text    levelText;
    [SerializeField] TMP_Text    xpText;
    [SerializeField] HealthBarUI xpBar;
    [SerializeField] TMP_Text    atkText;
    [SerializeField] TMP_Text    defText;
    [SerializeField] TMP_Text    magText;
    [SerializeField] TMP_Text    hpText;

    [Header("Hero Portrait")]
    [SerializeField] Image heroImage;

    [Header("Equipped Slots (4)")]
    [SerializeField] List<InventoryCardSlot> equippedSlots;

    [Header("Card Grid")]
    [SerializeField] Transform  gridContainer;
    [SerializeField] GameObject cardDisplayPrefab;

    [Header("Edit Mode")]
    [SerializeField] Button     saveButton;
    [SerializeField] GameObject unsavedChangesPopup;

    private bool         _isEditMode;
    private bool         _isDirty;
    private List<string> _snapshotEquipped;

    private readonly List<InventoryCardDisplay> _gridCards = new();

    void Start()
    {
        GameConfig config = GameManager.Instance?.Config;
        if (config == null)
        {
            Debug.LogError("[InventoryController] GameManager config not ready.");
            return;
        }

        _isEditMode = GameManager.Instance.InventoryEditMode;
        _isDirty    = false;

        // Snapshot the loadout so we can restore it if the player exits without saving.
        _snapshotEquipped = new List<string>(GameManager.Instance.Hero.EquippedMoveIds);

        // Load hero portrait
        if (heroImage != null)
        {
            var rogues = Resources.Load<Texture2D>("SpriteSheets/rogues");
            if (rogues != null)
            {
                rogues.filterMode        = FilterMode.Point;
                heroImage.sprite         = SpriteManager.GetSprite(rogues, 1, 0, 32);
                heroImage.preserveAspect = true;
                heroImage.color          = Color.white;
            }
        }

        if (saveButton != null) saveButton.interactable = _isEditMode;
        if (unsavedChangesPopup != null) unsavedChangesPopup.SetActive(false);

        foreach (var slot in equippedSlots)
            slot.Init(this);

        BuildGrid(config);
        RefreshAll();
    }

    private void BuildGrid(GameConfig config)
    {
        for (int i = gridContainer.childCount - 1; i >= 0; i--)
            Destroy(gridContainer.GetChild(i).gameObject);

        foreach (var move in config.moves.Values)
        {
            var go      = Instantiate(cardDisplayPrefab, gridContainer);
            var display = go.GetComponent<InventoryCardDisplay>();
            display.Setup(move, this);
            _gridCards.Add(display);
        }
    }

    // Called by card grid / slots after any equip/unequip change.
    public void RefreshAll()
    {
        RefreshStats();
        RefreshSlots();
        RefreshGrid();
    }

    // Called by card grid / slots after any equip/unequip change to mark the loadout dirty.
    public void MarkDirty() => _isDirty = true;

    private void RefreshStats()
    {
        GameConfig   config = GameManager.Instance.Config;
        HeroProgress hero   = GameManager.Instance.Hero;
        BaseStats    @base  = config.hero.baseStats;
        BaseStats    perLvl = config.hero.statsPerLevel;
        int lvl = hero.level;

        if (levelText != null) levelText.text = $"Level {lvl}";
        if (xpText    != null) xpText.text    = $"XP  {hero.xp} / {hero.XpToNextLevel}";
        if (xpBar     != null) { xpBar.SetMax(hero.XpToNextLevel); xpBar.UpdateHP(hero.xp, hero.XpToNextLevel); }
        if (atkText   != null) atkText.text   = $"ATK {@base.atk + perLvl.atk * (lvl - 1)}";
        if (defText   != null) defText.text   = $"DEF {@base.def + perLvl.def * (lvl - 1)}";
        if (magText   != null) magText.text   = $"MAG {@base.mag + perLvl.mag * (lvl - 1)}";
        if (hpText    != null) hpText.text    = $"HP  {@base.hp  + perLvl.hp  * (lvl - 1)}";
    }

    private void RefreshSlots()
    {
        GameConfig   config   = GameManager.Instance.Config;
        HeroProgress hero     = GameManager.Instance.Hero;
        var          equipped = hero.EquippedMoveIds;

        for (int i = 0; i < equippedSlots.Count; i++)
        {
            MoveData move = null;
            if (i < equipped.Count && config.moves.TryGetValue(equipped[i], out MoveData m))
                move = m;
            equippedSlots[i].SetMove(move);
        }
    }

    private void RefreshGrid()
    {
        foreach (var card in _gridCards)
            card.Refresh();
    }

    // ── Button handlers ──────────────────────────────────────────────────────

    // Wired to Save button — saves current loadout and returns to battle.
    public void OnSaveButton()
    {
        GameManager.Instance.SaveProgress();
        GameManager.Instance.GoToBattle();
    }

    // Wired to Back button — asks for confirmation if there are unsaved changes.
    public void OnBackButton()
    {
        if (_isEditMode && _isDirty && unsavedChangesPopup != null)
            unsavedChangesPopup.SetActive(true);
        else
            GameManager.Instance.GoToBattle();
    }

    // Wired to popup Save button — same as OnSaveButton.
    public void OnPopupSave()
    {
        GameManager.Instance.SaveProgress();
        GameManager.Instance.GoToBattle();
    }

    // Wired to popup Exit button — restores the snapshot loadout, then leaves.
    public void OnPopupExit()
    {
        var hero    = GameManager.Instance.Hero;
        var current = new List<string>(hero.EquippedMoveIds);
        foreach (var id in current)           hero.UnequipMove(id);
        foreach (var id in _snapshotEquipped) { if (hero.IsUnlocked(id)) hero.EquipMove(id); }
        GameManager.Instance.GoToBattle();
    }
}
