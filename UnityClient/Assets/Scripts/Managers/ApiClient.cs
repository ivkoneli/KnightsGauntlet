using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

// Static utility — callers start coroutines on their own MonoBehaviour.
public static class ApiClient
{
    // Reads from Resources/GameSettings.asset (gitignored).
    // Falls back to localhost so a fresh clone works out of the box.
    private static string _baseUrl;
    public static string BASE_URL
    {
        get
        {
            if (_baseUrl != null) return _baseUrl;
            var settings = Resources.Load<GameSettings>("GameSettings");
            _baseUrl = settings != null ? settings.apiBaseUrl.TrimEnd('/') : "http://localhost:8000";
            return _baseUrl;
        }
    }

    public static IEnumerator GetConfig(Action<GameConfig> callback)
    {
        yield return GetConfigInternal(callback, 0);
    }

    private static IEnumerator GetConfigInternal(Action<GameConfig> callback, int attempt)
    {
        string url = BASE_URL + "/run/config";
        Debug.Log($"[ApiClient] GET {url} (attempt {attempt + 1})");

        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            if (attempt < 1)
            {
                Debug.LogWarning("[ApiClient] GetConfig failed, retrying in 1s... Error: " + req.error);
                yield return new WaitForSeconds(1f);
                yield return GetConfigInternal(callback, attempt + 1);
                yield break;
            }
            Debug.LogError("[ApiClient] GetConfig failed after retry: " + req.error);
            callback?.Invoke(null);
            yield break;
        }

        GameConfig config = null;
        try
        {
            config = JsonConvert.DeserializeObject<GameConfig>(req.downloadHandler.text);
        }
        catch (Exception e)
        {
            Debug.LogError("[ApiClient] Failed to parse config JSON: " + e.Message);
        }

        callback?.Invoke(config);
    }

    public static IEnumerator GetMonsterMove(MonsterMoveRequest request, Action<string> callback)
    {
        yield return GetMonsterMoveInternal(request, callback, 0);
    }

    private static IEnumerator GetMonsterMoveInternal(MonsterMoveRequest request, Action<string> callback, int attempt)
    {
        string url = BASE_URL + "/monster/move" + request.ToQueryString();
        Debug.Log($"[ApiClient] GET {url} (attempt {attempt + 1})");

        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            if (attempt < 1)
            {
                Debug.LogWarning("[ApiClient] GetMonsterMove failed, retrying in 1s... Error: " + req.error);
                yield return new WaitForSeconds(1f);
                yield return GetMonsterMoveInternal(request, callback, attempt + 1);
                yield break;
            }
            Debug.LogError("[ApiClient] GetMonsterMove failed after retry, using fallback. Error: " + req.error);
            callback?.Invoke(FallbackMonsterMove(request.monsterId));
            yield break;
        }

        Debug.Log($"[ApiClient] Monster move raw response: {req.downloadHandler.text}");

        string moveId = null;
        try
        {
            var response = JsonConvert.DeserializeObject<MonsterMoveResponse>(req.downloadHandler.text);
            moveId = response?.moveId;
        }
        catch (Exception e)
        {
            Debug.LogError("[ApiClient] Failed to parse monster move response: " + e.Message);
        }

        if (string.IsNullOrEmpty(moveId))
            moveId = FallbackMonsterMove(request.monsterId);

        callback?.Invoke(moveId);
    }

    // Weighted-random fallback using cached config when the backend is unreachable.
    private static string FallbackMonsterMove(string monsterId)
    {
        GameConfig config = GameManager.Instance?.Config;
        if (config != null)
        {
            MonsterData monster = config.monsters.Find(m => m.id == monsterId);
            if (monster?.moves?.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, monster.moves.Count);
                string fallback = monster.moves[idx];
                Debug.LogWarning($"[ApiClient] Fallback move for {monsterId}: {fallback}");
                return fallback;
            }
        }
        return "rusty_blade";
    }
}
