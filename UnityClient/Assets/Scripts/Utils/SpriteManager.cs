using System.Collections.Generic;
using UnityEngine;

public static class SpriteManager
{
    private static readonly Dictionary<string, Sprite> _cache = new();

    // Creates a Sprite from a row/col in a top-to-bottom sprite sheet.
    // Unity UV origin is bottom-left, so we flip the row.
    public static Sprite GetSprite(Texture2D sheet, int row, int col, int size = 32)
    {
        string key = $"{sheet.name}_{row}_{col}_{size}";
        if (_cache.TryGetValue(key, out Sprite cached) && cached != null)
            return cached;

        int totalRows = sheet.height / size;
        int flippedRow = totalRows - 1 - row;
        Rect rect = new Rect(col * size, flippedRow * size, size, size);
        Sprite s = Sprite.Create(sheet, rect, new Vector2(0.5f, 0.5f), size);
        s.name = key;
        _cache[key] = s;
        return s;
    }

    public static void ClearCache() => _cache.Clear();
}
