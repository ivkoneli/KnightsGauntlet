using UnityEngine;

public class EndingSceneController : MonoBehaviour
{

    public void Start()
    {
        AudioManager.StopMusic();
    }
    public void OnRestartClicked()
    {
        GameManager.Instance.ResetProgress();
        GameManager.Instance.GoToMainMenu();
    }
}
