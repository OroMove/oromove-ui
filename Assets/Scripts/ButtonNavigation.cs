using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonNavigation : MonoBehaviour
{
    [SerializeField] private string targetScene;

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            Debug.LogWarning("Target scene is not set!");
        }
    }
}
