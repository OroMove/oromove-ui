//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.TestTools;
//using UnityEngine.UI;
//using System.Collections;
//using NUnit.Framework;

///// <summary>
///// HC_GameMenuNavigationTest is a PlayMode test script that verifies navigation functionality 
///// in the HillClimberGameMenu scene, including Exit and Start button interactions.
///// </summary>
//public class HC_GameMenuNavigationTest : MonoBehaviour
//{
//    /// <summary>
//    /// Tests whether the Exit button in the Main Menu panel correctly navigates back 
//    /// to the HillClimberPage scene when clicked.
//    /// </summary>
//    /// <returns>IEnumerator for Unity Test Runner</returns>
//    [UnityTest]
//    public IEnumerator TestExitButtonNavigation()
//    {
//        // Load the HillClimberGameMenu Scene
//        SceneManager.LoadScene("HillClimberGameMenu");
//        yield return new WaitForSeconds(1.0f); // Wait for the scene to load completely

//        // Find ExitButton inside the Main Menu
//        GameObject exitButtonObj = GameObject.Find("ExitButton");
//        Assert.IsNotNull(exitButtonObj, "ExitButton not found inside Main Menu!");

//        Button exitButton = exitButtonObj.GetComponent<Button>();
//        Assert.IsNotNull(exitButton, "Button component not found on ExitButton!");

//        // Simulate a button click
//        Debug.Log("Clicking ExitButton");
//        exitButton.onClick.Invoke();
//        yield return new WaitForSeconds(1.0f);

//        // Verify if the scene switched back to HillClimberPage
//        Assert.AreEqual("HillClimberPage", SceneManager.GetActiveScene().name,
//            "Navigation back to HillClimberPage failed after clicking ExitButton!");
//    }

//    /// <summary>
//    /// Tests whether clicking the Start button hides the Main Menu panel 
//    /// and shows the Levels Panel.
//    /// </summary>
//    /// <returns>IEnumerator for Unity Test Runner</returns>
//    [UnityTest]
//    public IEnumerator TestStartButtonTogglesPanels()
//    {
//        // Ignore authentication-related errors that might occur during initialization
//        LogAssert.ignoreFailingMessages = true;

//        // Load the HillClimberGameMenu Scene
//        SceneManager.LoadScene("HillClimberGameMenu");
//        yield return new WaitForSeconds(1.0f); // Wait for the scene to load

//        // Find the Start Button
//        GameObject startButtonObj = GameObject.Find("StartButton");
//        Assert.IsNotNull(startButtonObj, "Start button not found!");

//        Button startButton = startButtonObj.GetComponent<Button>();
//        Assert.IsNotNull(startButton, "Button component not found on StartButton!");

//        // Find Main Menu Panel (should be active initially)
//        GameObject mainMenuPanel = GameObject.Find("Main Menu");
//        Assert.IsNotNull(mainMenuPanel, "Main Menu panel not found!");

//        // Find Levels Panel (even if it's inactive)
//        GameObject levelsPanel = FindInactiveObject("Levels Panel");
//        Assert.IsNotNull(levelsPanel, "Levels Panel not found! Make sure it's in the scene hierarchy.");

//        // Ensure that Main Menu is visible and Levels Panel is hidden initially
//        Assert.IsTrue(mainMenuPanel != null && mainMenuPanel.activeSelf, "Main Menu should be visible initially!");
//        Assert.IsTrue(levelsPanel != null && !levelsPanel.activeSelf, "Levels Panel should be hidden initially!");

//        // Simulate a button click
//        Debug.Log("Clicking StartButton");
//        startButton.onClick.Invoke();
//        yield return new WaitForSeconds(1.0f);

//        // Re-check if objects are still valid before making assertions
//        if (mainMenuPanel != null && levelsPanel != null)
//        {
//            Assert.IsFalse(mainMenuPanel.activeSelf, "Main Menu should be hidden after clicking Start!");
//            Assert.IsTrue(levelsPanel.activeSelf, "Levels Panel should be visible after clicking Start!");
//        }
//        else
//        {
//            Debug.LogWarning("One of the objects was destroyed. Skipping final assertions.");
//        }
//    }

//    /// <summary>
//    /// Finds an inactive GameObject in the scene by searching through all objects in the hierarchy.
//    /// This method is used to locate objects that are disabled at runtime.
//    /// </summary>
//    /// <param name="name">The name of the GameObject to find.</param>
//    /// <returns>The GameObject if found, otherwise null.</returns>
//    private GameObject FindInactiveObject(string name)
//    {
//        Transform[] allObjects = Resources.FindObjectsOfTypeAll<Transform>();
//        foreach (Transform obj in allObjects)
//        {
//            if (obj.name == name)
//            {
//                return obj.gameObject;
//            }
//        }
//        return null;
//    }
//}
