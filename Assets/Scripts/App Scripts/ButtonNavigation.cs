using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonNavigation : MonoBehaviour
{
    // This method will be called by each button when clicked
    public void LoadScene(string sceneName)
    {
        // Load the scene by its name
        SceneManager.LoadScene(sceneName);
    }
}
