using UnityEngine;

[System.Serializable]
public class BackgroundBuilder
{
    public int    monsterId;
    public bool   useTilemap = true;
    public Sprite backgroundImage;

    [Tooltip("66 tiles, reading order left→right top→bottom (row 0 = top visually). " +
             "Format: 'srcRow.srcCol' where col is a letter: a=0 b=1 c=2 d=3. Example: '7.a'")]
    public string[] tiles = new string[66];
}

[RequireComponent(typeof(SpriteRenderer))]
public class BattleBackground : MonoBehaviour
{
    [SerializeField] string tileSheet = "SpriteSheets/tiles";
    [SerializeField] int    tileSize  = 32;
    [SerializeField] BackgroundBuilder[] backgrounds = new BackgroundBuilder[5];

    void Start()
    {
        int index = GameManager.Instance != null ? GameManager.Instance.CurrentMonsterIndex : 0;
        Build(index);
    }

    public void Build(int monsterId)
    {
        BackgroundBuilder map = System.Array.Find(backgrounds, b => b.monsterId == monsterId);
        if (map == null)
        {
            Debug.LogWarning($"BattleBackground: no BackgroundBuilder for monster {monsterId}");
            return;
        }

        if (map.useTilemap)
            BuildTilemap(map);
        else
            BuildImage(map);
    }

    // ── PNG image background ─────────────────────────────────────────────────

    void BuildImage(BackgroundBuilder map)
    {
        if (map.backgroundImage == null)
        {
            Debug.LogWarning($"BattleBackground: backgroundImage is null for monster {map.monsterId}");
            return;
        }

        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = map.backgroundImage;

        ScaleToFillCamera(map.backgroundImage.bounds.size);
    }

    // ── Tilemap background ───────────────────────────────────────────────────

    void BuildTilemap(BackgroundBuilder map)
    {
        var src = Resources.Load<Texture2D>(tileSheet);
        if (src == null) { Debug.LogWarning("BattleBackground: tile sheet not found"); return; }

        const int bgCols = 11, bgRows = 6;
        var bgTex = new Texture2D(bgCols * tileSize, bgRows * tileSize, TextureFormat.RGBA32, false);
        bgTex.filterMode = FilterMode.Point;
        bgTex.wrapMode   = TextureWrapMode.Clamp;

        int count = Mathf.Min(map.tiles.Length, 66);
        for (int i = 0; i < count; i++)
        {
            if (!ParseTile(map.tiles[i], out int srcRow, out int srcCol))
            {
                if (!string.IsNullOrWhiteSpace(map.tiles[i]))
                    Debug.LogWarning($"BattleBackground: bad tile '{map.tiles[i]}' at index {i} (monster {map.monsterId})");
                continue;
            }

            int bgCol = i % bgCols;
            int bgRow = bgRows - 1 - (i / bgCols);

            int srcX = srcCol * tileSize;
            int srcY = src.height - (srcRow + 1) * tileSize;
            bgTex.SetPixels(bgCol * tileSize, bgRow * tileSize, tileSize, tileSize,
                            src.GetPixels(srcX, srcY, tileSize, tileSize));
        }
        bgTex.Apply();

        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(bgTex,
            new Rect(0, 0, bgTex.width, bgTex.height),
            new Vector2(0.5f, 0.5f), tileSize);

        ScaleToFillCamera(new Vector2(bgCols, bgRows));
    }

    // ── Shared helpers ────────────────────────────────────────────────────────

    void ScaleToFillCamera(Vector2 spriteWorldSize)
    {
        var cam = Camera.main;
        if (cam == null || !cam.orthographic) return;

        float camH = cam.orthographicSize * 2f;
        float camW = camH * cam.aspect;
        transform.localScale = new Vector3(
            camW / spriteWorldSize.x,
            camH / spriteWorldSize.y, 1f);
    }

    static bool ParseTile(string code, out int srcRow, out int srcCol)
    {
        srcRow = srcCol = 0;
        if (string.IsNullOrWhiteSpace(code)) return false;
        int dot = code.IndexOf('.');
        if (dot < 1 || dot >= code.Length - 1) return false;
        if (!int.TryParse(code.Substring(0, dot), out srcRow)) return false;
        srcCol = char.ToLower(code[dot + 1]) - 'a';
        return srcCol >= 0;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        foreach (var bg in backgrounds)
        {
            if (bg == null) continue;
            if (bg.tiles.Length != 66)
                System.Array.Resize(ref bg.tiles, 66);
        }
    }
#endif
}
