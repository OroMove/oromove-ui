using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame() {

        SceneManager.LoadSceneAsync("Level 1");
    }

    public void BackToPlayList()
    {
        SceneManager.LoadSceneAsync("HillClimberPage");
    }
}
