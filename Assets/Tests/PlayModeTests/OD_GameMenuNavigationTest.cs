using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using System.Collections;
using NUnit.Framework;

/// <summary>
/// OD_GameMenuNavigationTest is a PlayMode test script that verifies navigation functionality 
/// in the OD_MainMenu scene, including Play and Exit button interactions.
/// </summary>
public class OD_GameMenuNavigationTest : MonoBehaviour
{
    /// <summary>
    /// Tests whether the Exit button in the Main Menu panel correctly navigates back 
    /// to the OroDodgePage scene when clicked.
    /// </summary>
    /// <returns>IEnumerator for Unity Test Runner</returns>
    [UnityTest]
    public IEnumerator TestExitButtonNavigation()
    {
        // Load the OD_MainMenu Scene
        SceneManager.LoadScene("OD_MainMenu");
        yield return new WaitForSeconds(1.0f); // Wait for the scene to load completely

        // Find ExitButton inside the Main Menu
        GameObject exitButtonObj = GameObject.Find("Exit Button");
        Assert.IsNotNull(exitButtonObj, "ExitButton not found inside Main Menu!");

        Button exitButton = exitButtonObj.GetComponent<Button>();
        Assert.IsNotNull(exitButton, "Button component not found on ExitButton!");

        // Simulate a button click
        Debug.Log("Clicking ExitButton");
        exitButton.onClick.Invoke();
        yield return new WaitForSeconds(1.0f);

        // Verify if the scene switched to OroDodgePage
        Assert.AreEqual("OroDodgePage", SceneManager.GetActiveScene().name,
            "Navigation to OroDodgePage failed after clicking ExitButton!");
    }

    /// <summary>
    /// Tests whether clicking the Play button navigates to the Level Selection scene.
    /// </summary>
    /// <returns>IEnumerator for Unity Test Runner</returns>
    [UnityTest]
    public IEnumerator TestPlayButtonNavigation()
    {
        // Load the OD_MainMenu Scene
        SceneManager.LoadScene("OD_MainMenu");
        yield return new WaitForSeconds(1.0f); // Wait for the scene to load

        // Find the Play Button
        GameObject playButtonObj = GameObject.Find("Play Button");
        Assert.IsNotNull(playButtonObj, "Play button not found!");

        Button playButton = playButtonObj.GetComponent<Button>();
        Assert.IsNotNull(playButton, "Button component not found on PlayButton!");

        // Simulate a button click
        Debug.Log("Clicking PlayButton");
        playButton.onClick.Invoke();
        yield return new WaitForSeconds(1.0f);

        // Verify if the scene switched to OD_LevelSelection
        Assert.AreEqual("OD_LevelSelection", SceneManager.GetActiveScene().name,
            "Navigation to OD_LevelSelection failed after clicking PlayButton!");
    }
}
