using UnityEngine;

// Edit the API URL in the Unity Inspector:
//   Resources/GameSettings asset → apiBaseUrl field
// This asset is gitignored so URLs never get committed.
// On a fresh clone it falls back to http://localhost:8000 automatically.
[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
public class GameSettings : ScriptableObject
{
    [Tooltip("Base URL for the FastAPI backend. No trailing slash.")]
    public string apiBaseUrl = "http://localhost:8000";
}
