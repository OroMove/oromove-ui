// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.TestTools;
// using UnityEngine.UI;
// using System.Collections;
// using NUnit.Framework;

// public class GameLibraryNavigationTest : MonoBehaviour
// {
//     [UnityTest]
//     public IEnumerator TestGameLibraryNavigation()
//     {
//         // Load PatientHomePage Scene
//         SceneManager.LoadScene("PatientHomePage");
//         yield return new WaitForSeconds(2f); // Wait for scene to load

//         // Find OroDodge and HillClimber buttons
//         GameObject oroDodgeButtonObj = GameObject.Find("OroDodge");
//         Assert.IsNotNull(oroDodgeButtonObj, "OroDodge button not found!");

//         GameObject hillClimberButtonObj = GameObject.Find("HillClimber");
//         Assert.IsNotNull(hillClimberButtonObj, "HillClimber button not found!");

//         // TEST 1: Click OroDodge and verify navigation to OroDodgePage
//         Button oroDodgeButton = oroDodgeButtonObj.GetComponent<Button>();
//         Assert.IsNotNull(oroDodgeButton, "Button component not found on OroDodge!");

//         Debug.Log("Clicking OroDodge button");
//         oroDodgeButton.onClick.Invoke();
//         yield return new WaitForSeconds(2f);

//         Assert.AreEqual("OroDodgePage", SceneManager.GetActiveScene().name,
//             "Navigation to OroDodgePage failed after clicking OroDodge!");

//         // Reload PatientHomePage for the next test
//         SceneManager.LoadScene("PatientHomePage");
//         yield return new WaitForSeconds(1.0f);

//         // TEST 2: Click HillClimber and verify navigation to HillClimberPage
//         hillClimberButtonObj = GameObject.Find("HillClimber");
//         Button hillClimberButton = hillClimberButtonObj.GetComponent<Button>();
//         Assert.IsNotNull(hillClimberButton, "Button component not found on HillClimber!");

//         Debug.Log("Clicking HillClimber button");
//         hillClimberButton.onClick.Invoke();
//         yield return new WaitForSeconds(2f);

//         Assert.AreEqual("HillClimberPage", SceneManager.GetActiveScene().name,
//             "Navigation to HillClimberPage failed after clicking HillClimber!");
//     }

//     [UnityTest]
//     public IEnumerator GameMainMenuNavigationTest()
//     {
//         // Load HillClimberPage Scene
//         SceneManager.LoadScene("HillClimberPage");
//         yield return new WaitForSeconds(2f); // Wait for scene to load

//         // Find Play Game button
//         GameObject playGameButtonObj = GameObject.Find("Play");
//         Assert.IsNotNull(playGameButtonObj, "Play Game button not found!");

//         Button playGameButton = playGameButtonObj.GetComponent<Button>();
//         Assert.IsNotNull(playGameButton, "Button component not found on Play Game!");

//         // Click Play Game button
//         Debug.Log("Clicking Play Game button");
//         playGameButton.onClick.Invoke();
//         yield return new WaitForSeconds(2f);

//         // Verify navigation to HillClimberGameMenu
//         Assert.AreEqual("HillClimberGameMenu", SceneManager.GetActiveScene().name,
//             "Navigation to HillClimberGameMenu failed after clicking Play Game!");

//         /*
//         // TEST 2: Uncomment when Watch Demo button is implemented
//         GameObject watchDemoButtonObj = GameObject.Find("WatchDemo");
//         Assert.IsNotNull(watchDemoButtonObj, "Watch Demo button not found!");

//         Button watchDemoButton = watchDemoButtonObj.GetComponent<Button>();
//         Assert.IsNotNull(watchDemoButton, "Button component not found on Watch Demo!");

//         Debug.Log("Clicking Watch Demo button");
//         watchDemoButton.onClick.Invoke();
//         yield return new WaitForSeconds(1.0f);

//         // Verify navigation to the demo scene (update scene name if necessary)
//         Assert.AreEqual("HillClimberDemo", SceneManager.GetActiveScene().name, 
//             "Navigation to HillClimberDemo failed after clicking Watch Demo!");
//         */
//     }
// }
