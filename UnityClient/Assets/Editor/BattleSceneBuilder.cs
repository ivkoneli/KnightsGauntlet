using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

// Tools > Build Battle Scene — run from Unity menu to rebuild the scene from scratch.
public static class BattleSceneBuilder
{
    [MenuItem("Tools/Build Battle Scene")]
    public static void Build()
    {
        // Save any unsaved changes in currently open scenes first.
        EditorSceneManager.SaveOpenScenes();

        // Use Additive so we never trigger the "save unsaved changes?" dialog.
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
        EditorSceneManager.SetActiveScene(scene);

        // ── Camera ──────────────────────────────────────────────────────────
        // Find the camera in THIS new scene, not in MainMenu (which is still loaded additively).
        GameObject camGO = null;
        GameObject lightGO = null;
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.GetComponent<Camera>()     != null) camGO   = go;
            if (go.GetComponent<Light>()      != null) lightGO = go;
        }
        if (lightGO != null) Object.DestroyImmediate(lightGO); // no directional light for 2D

        var cam = camGO.GetComponent<Camera>();
        cam.orthographic    = true;
        cam.backgroundColor = new Color(0.1f, 0.08f, 0.18f);
        cam.clearFlags      = CameraClearFlags.SolidColor;
        // PixelPerfectCam adjusts orthographicSize at runtime for crisp integer pixel scaling.
        camGO.AddComponent<PixelPerfectCam>();

        // ── Combatants (world-space SpriteRenderers, scaled 4x) ──────────────
        // Knight faces RIGHT by default in the sheet → flip X so it faces right toward the monster.
        var knightGO = new GameObject("KnightDisplay");
        knightGO.transform.position   = new Vector3(-3.5f, -0.5f, 0f);
        knightGO.transform.localScale = new Vector3(-4f, 4f, 1f); // negative X = face right
        knightGO.AddComponent<SpriteRenderer>();
        var knightDisplay = knightGO.AddComponent<CombatantDisplay>();

        // Goblin: flip X so it faces left toward the knight (confirmed working at -4X).
        var goblinGO = new GameObject("MonsterDisplay");
        goblinGO.transform.position   = new Vector3(3.5f, -0.5f, 0f);
        goblinGO.transform.localScale = new Vector3(-4f, 4f, 1f);
        goblinGO.AddComponent<SpriteRenderer>();
        var monsterDisplay = goblinGO.AddComponent<CombatantDisplay>();

        // ── EventSystem ─────────────────────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<StandaloneInputModule>();

        // ── Canvas ──────────────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Hero HP Panel (top-left) ─────────────────────────────────────────
        var heroPanelGO = new GameObject("HeroHPPanel");
        heroPanelGO.transform.SetParent(canvasGO.transform, false);
        var heroPanelRT = heroPanelGO.AddComponent<RectTransform>();
        heroPanelRT.anchorMin = heroPanelRT.anchorMax = heroPanelRT.pivot = new Vector2(0f, 1f);
        heroPanelRT.anchoredPosition = new Vector2(15f, -15f);
        heroPanelRT.sizeDelta = new Vector2(280f, 68f);
        heroPanelGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        var heroLabelGO = Spawn("HeroLabel", heroPanelGO.transform);
        SetAnchors(heroLabelGO, 0f, 1f, 1f, 1f, 0.5f, 1f, 0f, -3f, -10f, 22f);
        var heroLabelTMP  = heroLabelGO.AddComponent<TextMeshProUGUI>();
        heroLabelTMP.text = "HERO"; heroLabelTMP.fontSize = 15f;
        heroLabelTMP.color = Color.white; heroLabelTMP.fontStyle = FontStyles.Bold;
        heroLabelTMP.alignment = TextAlignmentOptions.Left;

        Slider heroSlider; TMP_Text heroHPTMP;
        BuildHPSlider(heroPanelGO.transform, new Color(0.2f, 0.85f, 0.2f), "HeroHPText", TextAlignmentOptions.Left,
                      out heroSlider, out heroHPTMP);
        var heroHealthBarUI = heroPanelGO.AddComponent<HealthBarUI>();

        // ── Monster HP Panel (top-right) ─────────────────────────────────────
        var monsterPanelGO = new GameObject("MonsterHPPanel");
        monsterPanelGO.transform.SetParent(canvasGO.transform, false);
        var monsterPanelRT = monsterPanelGO.AddComponent<RectTransform>();
        monsterPanelRT.anchorMin = monsterPanelRT.anchorMax = monsterPanelRT.pivot = new Vector2(1f, 1f);
        monsterPanelRT.anchoredPosition = new Vector2(-15f, -15f);
        monsterPanelRT.sizeDelta = new Vector2(280f, 68f);
        monsterPanelGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

        var monsterNameGO = Spawn("MonsterNameText", monsterPanelGO.transform);
        SetAnchors(monsterNameGO, 0f, 1f, 1f, 1f, 0.5f, 1f, 0f, -3f, -10f, 22f);
        var monsterNameTMP  = monsterNameGO.AddComponent<TextMeshProUGUI>();
        monsterNameTMP.text = "Monster"; monsterNameTMP.fontSize = 15f;
        monsterNameTMP.color = Color.white; monsterNameTMP.fontStyle = FontStyles.Bold;
        monsterNameTMP.alignment = TextAlignmentOptions.Right;

        Slider monsterSlider; TMP_Text monsterHPTMP;
        BuildHPSlider(monsterPanelGO.transform, new Color(0.9f, 0.2f, 0.2f), "MonsterHPText", TextAlignmentOptions.Right,
                      out monsterSlider, out monsterHPTMP);
        var monsterHealthBarUI = monsterPanelGO.AddComponent<HealthBarUI>();

        // ── Spell Cards Panel (bottom) ────────────────────────────────────────
        var cardsPanelGO = new GameObject("SpellCardsPanel");
        cardsPanelGO.transform.SetParent(canvasGO.transform, false);
        var cardsPanelRT = cardsPanelGO.AddComponent<RectTransform>();
        cardsPanelRT.anchorMin = new Vector2(0.05f, 0f);
        cardsPanelRT.anchorMax = new Vector2(0.95f, 0f);
        cardsPanelRT.pivot     = new Vector2(0.5f, 0f);
        cardsPanelRT.anchoredPosition = new Vector2(0f, 10f);
        cardsPanelRT.sizeDelta = new Vector2(0f, 175f);
        var hlg = cardsPanelGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12f; hlg.padding = new RectOffset(8, 8, 0, 0);
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = hlg.childControlHeight = true;
        hlg.childForceExpandWidth = hlg.childForceExpandHeight = true;
        var moveButtonPanel = cardsPanelGO.AddComponent<MoveButtonPanel>();

        // Placeholder names/descriptions visible in edit mode — replaced at runtime by BattleManager.
        string[] placeholderNames = { "Slash", "Shield Up", "Battle Cry", "Second Wind" };
        string[] placeholderDescs = {
            "Deal physical damage\nATK × 0.8",
            "Buff DEF +5\nfor 2 turns",
            "Buff ATK +4\nfor 2 turns",
            "Heal 30%\nof max HP"
        };
        // Color-code placeholders to show the system works in edit mode.
        Color[] placeholderColors = {
            new Color(1.00f, 0.65f, 0.30f), // physical orange
            new Color(0.30f, 1.00f, 0.50f), // buff green
            new Color(0.30f, 1.00f, 0.50f), // buff green
            new Color(0.30f, 0.90f, 0.80f), // heal teal
        };

        var moveButtons = new MoveButton[4];
        for (int i = 0; i < 4; i++)
        {
            var cardGO  = Spawn("SpellCard_" + i, cardsPanelGO.transform);
            var cardImg = cardGO.AddComponent<Image>(); cardImg.color = placeholderColors[i];
            cardGO.AddComponent<Button>();

            var nameGO = Spawn("MoveName", cardGO.transform);
            SetAnchors(nameGO, 0f, 0.5f, 1f, 1f, 0.5f, 0.5f, 0f, 0f, 0f, 0f);
            var nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.offsetMin = new Vector2(5f, 0f); nameRT.offsetMax = new Vector2(-5f, -5f);
            var nameTMP       = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text      = placeholderNames[i];
            nameTMP.fontSize  = 15f; nameTMP.color = Color.black;
            nameTMP.fontStyle = FontStyles.Bold; nameTMP.alignment = TextAlignmentOptions.Center;

            var descGO = Spawn("MoveDesc", cardGO.transform);
            SetAnchors(descGO, 0f, 0f, 1f, 0.5f, 0.5f, 0f, 0f, 0f, 0f, 0f);
            var descRT = descGO.GetComponent<RectTransform>();
            descRT.offsetMin = new Vector2(5f, 5f); descRT.offsetMax = new Vector2(-5f, 0f);
            var descTMP       = descGO.AddComponent<TextMeshProUGUI>();
            descTMP.text      = placeholderDescs[i];
            descTMP.fontSize  = 10f;
            descTMP.color     = new Color(0.15f, 0.15f, 0.15f);
            descTMP.alignment = TextAlignmentOptions.Center;

            var mb   = cardGO.AddComponent<MoveButton>();
            var mbSO = new SerializedObject(mb);
            mbSO.FindProperty("moveNameText").objectReferenceValue = nameTMP;
            mbSO.FindProperty("descText").objectReferenceValue     = descTMP;
            mbSO.FindProperty("background").objectReferenceValue   = cardImg;
            mbSO.ApplyModifiedProperties();
            moveButtons[i] = mb;
        }

        var panelSO     = new SerializedObject(moveButtonPanel);
        var buttonsProp = panelSO.FindProperty("buttons");
        buttonsProp.ClearArray();
        for (int i = 0; i < 4; i++)
        {
            buttonsProp.InsertArrayElementAtIndex(i);
            buttonsProp.GetArrayElementAtIndex(i).objectReferenceValue = moveButtons[i];
        }
        panelSO.ApplyModifiedProperties();

        // ── Battle Log (left side, above cards) ──────────────────────────────
        var logPanelGO = new GameObject("BattleLogPanel");
        logPanelGO.transform.SetParent(canvasGO.transform, false);
        var logPanelRT = logPanelGO.AddComponent<RectTransform>();
        logPanelRT.anchorMin = logPanelRT.anchorMax = logPanelRT.pivot = Vector2.zero;
        logPanelRT.anchoredPosition = new Vector2(10f, 200f);
        logPanelRT.sizeDelta        = new Vector2(320f, 340f);
        logPanelGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        var scrollGO = Spawn("Scroll", logPanelGO.transform);
        var scrollRT = scrollGO.GetComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero; scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = new Vector2(5f, 5f); scrollRT.offsetMax = new Vector2(-5f, -5f);
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        var vpGO = Spawn("Viewport", scrollGO.transform);
        var vpRT = vpGO.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = vpRT.offsetMax = Vector2.zero;
        vpGO.AddComponent<Image>().color = Color.clear;
        vpGO.AddComponent<Mask>().showMaskGraphic = false;
        // CullTransparentMesh must be OFF: a transparent Image that's culled never writes to the
        // stencil buffer, so the Mask hides all children (the log text becomes invisible).
        vpGO.GetComponent<CanvasRenderer>().cullTransparentMesh = false;
        scrollRect.viewport = vpRT;

        var contentGO = Spawn("Content", vpGO.transform);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f); contentRT.anchorMax = Vector2.one;
        contentRT.pivot     = new Vector2(0.5f, 1f);
        contentRT.offsetMin = contentRT.offsetMax = Vector2.zero;
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var logTMP            = contentGO.AddComponent<TextMeshProUGUI>();
        logTMP.text           = ""; logTMP.fontSize = 12f;
        logTMP.color          = new Color(0.9f, 0.9f, 0.9f);
        logTMP.alignment      = TextAlignmentOptions.TopLeft;
        logTMP.enableWordWrapping = true;
        scrollRect.content    = contentRT;

        var battleLogUI = logPanelGO.AddComponent<BattleLogUI>();
        var logSO       = new SerializedObject(battleLogUI);
        logSO.FindProperty("logText").objectReferenceValue = logTMP;
        logSO.FindProperty("scroll").objectReferenceValue  = scrollRect;
        logSO.ApplyModifiedProperties();

        // ── Result Panel (centered, hidden) ───────────────────────────────────
        var resultPanelGO = new GameObject("ResultPanel");
        resultPanelGO.transform.SetParent(canvasGO.transform, false);
        var resultPanelRT = resultPanelGO.AddComponent<RectTransform>();
        resultPanelRT.anchorMin = new Vector2(0.25f, 0.35f);
        resultPanelRT.anchorMax = new Vector2(0.75f, 0.65f);
        resultPanelRT.offsetMin = resultPanelRT.offsetMax = Vector2.zero;
        resultPanelGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);

        var resultTextGO = Spawn("ResultText", resultPanelGO.transform);
        SetAnchors(resultTextGO, 0f, 0f, 1f, 1f, 0.5f, 0.5f, 0f, 0f, 0f, 0f);
        var resultTMP       = resultTextGO.AddComponent<TextMeshProUGUI>();
        resultTMP.text      = "Result"; resultTMP.fontSize = 36f;
        resultTMP.color     = Color.yellow; resultTMP.alignment = TextAlignmentOptions.Center;
        resultTMP.fontStyle = FontStyles.Bold;
        resultPanelGO.SetActive(false);

        // ── BattleManager + BattleUIController ───────────────────────────────
        var bmGO          = new GameObject("BattleManager");
        var battleManager = bmGO.AddComponent<BattleManager>();
        var battleUI      = bmGO.AddComponent<BattleUIController>();

        // Wire HealthBarUI
        Wire(heroHealthBarUI,   "slider", heroSlider);
        Wire(heroHealthBarUI,   "hpText", heroHPTMP);
        Wire(monsterHealthBarUI,"slider", monsterSlider);
        Wire(monsterHealthBarUI,"hpText", monsterHPTMP);

        // Wire BattleUIController
        Wire(battleUI, "heroHealthBar",    heroHealthBarUI);
        Wire(battleUI, "monsterHealthBar", monsterHealthBarUI);
        Wire(battleUI, "moveButtonPanel",  moveButtonPanel);
        Wire(battleUI, "battleLog",        battleLogUI);
        Wire(battleUI, "monsterNameText",  monsterNameTMP);
        Wire(battleUI, "resultPanel",      resultPanelGO);
        Wire(battleUI, "resultText",       resultTMP);

        // Wire BattleManager
        Wire(battleManager, "ui",             battleUI);
        Wire(battleManager, "heroDisplay",    knightDisplay);
        Wire(battleManager, "monsterDisplay", monsterDisplay);

        // ── Save and Build Settings ───────────────────────────────────────────
        bool saved = EditorSceneManager.SaveScene(scene, "Assets/Scenes/Battle.unity");
        Debug.Log("[BattleSceneBuilder] Battle scene saved: " + saved);

        var scenes     = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool hasMain   = scenes.Exists(s => s.path.Contains("MainMenu"));
        bool hasBattle = scenes.Exists(s => s.path.Contains("Battle"));
        if (!hasMain)   scenes.Insert(0, new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true));
        if (!hasBattle) scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Battle.unity", true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("[BattleSceneBuilder] Build Settings updated.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject Spawn(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    static void SetAnchors(GameObject go, float axMin, float ayMin, float axMax, float ayMax,
                           float px, float py, float posX, float posY, float sdX, float sdY)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(axMin, ayMin);
        rt.anchorMax = new Vector2(axMax, ayMax);
        rt.pivot     = new Vector2(px, py);
        rt.anchoredPosition = new Vector2(posX, posY);
        rt.sizeDelta        = new Vector2(sdX, sdY);
    }

    static void Wire(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedProperties();
    }

    static void BuildHPSlider(Transform parent, Color fillColor, string hpTextName,
                               TextAlignmentOptions textAlign,
                               out Slider slider, out TMP_Text hpText)
    {
        var sliderGO = Spawn("HPSlider", parent);
        var sliderRT = sliderGO.GetComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0f, 0.5f); sliderRT.anchorMax = new Vector2(1f, 0.5f);
        sliderRT.pivot = new Vector2(0.5f, 0.5f);
        sliderRT.anchoredPosition = new Vector2(0f, 5f); sliderRT.sizeDelta = new Vector2(-10f, 20f);

        var bgGO = Spawn("Background", sliderGO.transform);
        var bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = bgRT.offsetMax = Vector2.zero;
        bgGO.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

        var faGO = Spawn("FillArea", sliderGO.transform);
        var faRT = faGO.GetComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero; faRT.anchorMax = Vector2.one;
        faRT.offsetMin = faRT.offsetMax = Vector2.zero;

        var fillGO = Spawn("Fill", faGO.transform);
        var fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero; fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
        fillGO.AddComponent<Image>().color = fillColor;

        slider            = sliderGO.AddComponent<Slider>();
        slider.fillRect   = fillRT;
        slider.direction  = Slider.Direction.LeftToRight;
        slider.minValue   = 0f; slider.maxValue = 100f; slider.value = 100f;
        slider.interactable = false;

        var textGO = Spawn(hpTextName, parent);
        SetAnchors(textGO, 0f, 0f, 1f, 0f, 0.5f, 0f, 0f, 4f, -10f, 20f);
        hpText           = textGO.AddComponent<TextMeshProUGUI>();
        hpText.text      = "100 / 100"; hpText.fontSize = 13f; hpText.color = Color.white;
        hpText.alignment = textAlign;
    }
}
