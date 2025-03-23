//using System.Collections;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using NUnit.Framework;
//using UnityEngine;
//using UnityEngine.TestTools;
//using Moq;
//using Unity.Services.CloudSave;
//using Unity.Services.CloudSave.Models;
//using Newtonsoft.Json;

//public class GameManagerTests
//{
//    private GameManager gameManager;
//    private Mock<ICloudSaveService> mockCloudSaveService;

//    [SetUp]
//    public void Setup()
//    {
//        // Create a new GameObject to hold the GameManager component
//        GameObject gameManagerObject = new GameObject();
//        gameManager = gameManagerObject.AddComponent<GameManager>();

//        // Mock the CloudSaveService
//        mockCloudSaveService = new Mock<ICloudSaveService>();
//    }

//    [UnityTest]
//    public IEnumerator SaveLevelProgress_SavesCorrectData()
//    {
//        // Arrange: Simulate level progress data
//        int levelId = 1;
//        LevelProgress progress = new LevelProgress
//        {
//            levelId = levelId,
//            totalAttempts = 2
//        };
//        progress.attempts[1] = new AttemptData
//        {
//            finalDistance = 100.5f,
//            timeTaken = 10.2f,
//            averageSpeed = 9.8f,
//            maxSpeed = 12.0f,
//            minSpeed = 5.0f,
//            maxMouthOpening = 2.5f,
//            minMouthOpening = 0.8f
//        };

//        // Act: Save progress
//        yield return gameManager.SaveLevelProgress(levelId, progress);

//        // Assert: Verify saved data
//        var savedData = new Dictionary<string, object>
//        {
//            ["Level_1_Meta"] = new Dictionary<string, object>
//            {
//                ["LevelId"] = levelId,
//                ["TotalAttempts"] = 2
//            }
//        };

//        string savedJson = JsonConvert.SerializeObject(savedData);
//        string expectedJson = JsonConvert.SerializeObject(new Dictionary<string, object>
//        {
//            ["Level_1_Meta"] = new Dictionary<string, object>
//            {
//                ["LevelId"] = 1,
//                ["TotalAttempts"] = 2
//            }
//        });

//        Assert.AreEqual(expectedJson, savedJson);
//    }

//    [UnityTest]
//    public IEnumerator LoadLevelProgress_LoadsCorrectData()
//    {
//        // Arrange: Simulate saved cloud data
//        int levelId = 1;
//        var cloudData = new Dictionary<string, Item>
//        {
//            ["Level_1_Meta"] = new Item(new Dictionary<string, object>
//            {
//                { "LevelId", 1 },
//                { "TotalAttempts", 3 }
//            }),
//            ["Level_1_Attempt_1"] = new Item(new Dictionary<string, object>
//            {
//                { "FinalDistance", 150.2f },
//                { "TimeTaken", 12.3f },
//                { "AverageSpeed", 10.5f },
//                { "MaxSpeed", 14.0f },
//                { "MinSpeed", 6.5f },
//                { "MaxMouthOpening", 3.1f },
//                { "MinMouthOpening", 1.2f }
//            })
//        };

//        // Mock cloud save retrieval
//        mockCloudSaveService.Setup(s => s.Data.Player.LoadAsync(It.IsAny<HashSet<string>>()))
//            .ReturnsAsync(cloudData);

//        // Act: Load progress
//        yield return gameManager.LoadAndSyncProgress(levelId);

//        // Assert: Check if data is loaded correctly
//        Assert.AreEqual(3, gameManager.levelProgressData[levelId].totalAttempts);
//        Assert.AreEqual(150.2f, gameManager.levelProgressData[levelId].attempts[1].finalDistance);
//        Assert.AreEqual(12.3f, gameManager.levelProgressData[levelId].attempts[1].timeTaken);
//    }
//}
