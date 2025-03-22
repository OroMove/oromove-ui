using UnityEngine;
using UnityEngine.SceneManagement;

public class OD_MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync("OD_LevelSelection");
    }

    public void ExitGame()
    {
        SceneManager.LoadSceneAsync("OroDodgePage");
    }

}
