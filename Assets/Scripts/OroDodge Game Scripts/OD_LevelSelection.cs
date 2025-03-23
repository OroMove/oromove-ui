using UnityEngine;
using UnityEngine.SceneManagement;

public class OD_LevelSelection : MonoBehaviour
{
    public void OpenLevel(int levelID)
    {
        string levelName = "OD_Level_0" + levelID;
        SceneManager.LoadScene(levelName);
    }

    public void CloseLevelSelection()
    {
        SceneManager.LoadSceneAsync("OD_MainMenu");
    }
}
