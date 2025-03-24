using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Services.CloudSave;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TherapistLeaderboardManager : MonoBehaviour
{
    public Transform therapistListContainer;
    public GameObject therapistItemPrefab;
    public Button backButton;

    private const string LEADERBOARD_ID = "TherapistLeaderboard";
    private string selectedTherapistID;

    private async void Start()
    {
        await Task.Delay(100);
        LoadTherapists();
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnEnable()
    {
        TherapistTile.OnTherapistSelected += SaveTherapistForPatient;
    }

    private void OnDisable()
    {
        TherapistTile.OnTherapistSelected -= SaveTherapistForPatient;
    }

    public async void LoadTherapists()
    {
        string selectedTherapistID = await GetSelectedTherapistID();
        List<TherapistData> therapists = await GetTherapistsForPatients();

        Debug.Log($"Therapists found: {therapists.Count}");

        DisplayTherapists(therapists);
    }

    public async Task<List<TherapistData>> GetTherapistsForPatients()
    {
        // Fetching the leaderboard data with metadata included
        var response = await LeaderboardsService.Instance.GetScoresAsync(LEADERBOARD_ID, new GetScoresOptions { IncludeMetadata = true });
        List<TherapistData> therapists = new List<TherapistData>();

        foreach (var entry in response.Results)
        {
            Debug.Log($"Processing entry: PlayerID={entry.PlayerId}, MetaData={entry.Metadata}, Score={entry.Score}");
            Debug.Log($"The type of entry is: {entry.GetType()}");

            Debug.Log($"Raw Metadata from Leaderboard: {entry.Metadata}");
            Debug.Log($"Raw Metadata from Leaderboard (Serialized): {JsonConvert.SerializeObject(entry.Metadata)}");

            string name = "Unknown";
            string specialization = "Unknown";

            // Ensure metadata exists
            if (entry.Metadata != null)
            {
                try
                {
                    JObject metadata = JObject.Parse(entry.Metadata.ToString());

                    // Extract values from the JObject
                    name = metadata["Name"]?.ToString() ?? "Unknown";
                    specialization = metadata["Specialization"]?.ToString() ?? "Unknown";

                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error deserializing metadata: {ex.Message}");
                }
            }

            // Add therapist data to the list
            therapists.Add(new TherapistData(name, specialization, (int)entry.Score, entry.PlayerId));
        }

        return therapists;
    }


    private void DisplayTherapists(List<TherapistData> therapists)
    {
        // Check for null references to avoid issues
        if (therapistItemPrefab == null)
        {
            Debug.LogError("Therapist item prefab is not assigned!");
            return;
        }

        if (therapistListContainer == null)
        {
            Debug.LogError("Therapist list container is not assigned!");
            return;
        }

        // Clear the previous entries from the container
        foreach (Transform child in therapistListContainer)
        {
            Destroy(child.gameObject);
        }

        // Instantiate new therapist tiles for each therapist
        foreach (TherapistData therapist in therapists)
        {
            GameObject newEntry = Instantiate(therapistItemPrefab, therapistListContainer);
            TherapistTile therapistItem = newEntry.GetComponent<TherapistTile>();

            if (therapistItem != null)
            {
                therapistItem.SetTherapistData(
                    therapist.Name,
                    therapist.Specialization,
                    therapist.Experience,
                    therapist.PlayerID
                );

                Button therapistButton = newEntry.GetComponentInChildren<Button>();

                if (therapist.PlayerID == selectedTherapistID)
                {
                    // Disable the selected therapist and highlight it
                    therapistButton.interactable = false;
                    newEntry.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f); // Increase size
                    newEntry.GetComponent<Image>().color = Color.green; // Change background color (optional)
                }
            }
            else
            {
                Debug.LogWarning("TherapistTile component not found on the instantiated prefab.");
            }
        }

        // Force a layout rebuild in case of dynamic changes
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            therapistListContainer.GetComponent<RectTransform>()
        );
    }

    private async Task<string> GetSelectedTherapistID()
    {
        var response = await CloudSaveService.Instance.Data.Player.LoadAsync(
            new HashSet<string> { "TherapistID" }
        );
        if (response.TryGetValue("TherapistID", out var therapistID))
        {
            return therapistID.ToString();
        }
        return null;
    }


    private async void SaveTherapistForPatient(string therapistPlayerID)
    {
        string patientPlayerID = await GetPatientPlayerID(); // Assume function to get patient ID

        // Check if therapist is already saved for the patient
        if (await IsTherapistAlreadySaved(patientPlayerID))
        {
            Debug.Log("Therapist already selected. Disabling other therapists.");
            DisableOtherTherapists();
            return;
        }

        // Save therapist data for patient
        var patientData = new Dictionary<string, object> { { "TherapistID", therapistPlayerID } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(patientData);

        // Save patient data for therapist
        var therapistData = new Dictionary<string, object> { { "PatientID", patientPlayerID } };

        await SavePatientForTherapist(therapistPlayerID, therapistData);

        Debug.Log($"Therapist {therapistPlayerID} saved for patient {patientPlayerID}");
    }

    // Check if therapist is already saved for the patient
    private async Task<bool> IsTherapistAlreadySaved(string patientPlayerID)
    {
        var response = await CloudSaveService.Instance.Data.Player.LoadAsync(
            new HashSet<string> { "TherapistID" }
        );
        if (response.TryGetValue("TherapistID", out var therapistID))
        {
            return !string.IsNullOrEmpty(therapistID.ToString());
        }
        return false;
    }

    private async Task SavePatientForTherapist(
        string therapistPlayerID,
        Dictionary<string, object> therapistData
    )
    {
        // Use a unique key for the therapist's data to store the patient ID.
        string therapistDataKey = $"Therapist_{therapistPlayerID}_PatientID"; // Unique key for therapist's data

        // Save the patient ID to the therapist's data using the unique key
        await CloudSaveService.Instance.Data.Player.SaveAsync(
            new Dictionary<string, object> { { therapistDataKey, therapistData } }
        );

        Debug.Log($"Saved patient ID for therapist {therapistPlayerID}: {therapistData}");
    }

    private async Task<string> GetPatientPlayerID()
    {
        var response = await CloudSaveService.Instance.Data.Player.LoadAsync(
            new HashSet<string> { "PlayerID" }
        );
        if (response.TryGetValue("PlayerID", out var playerID))
        {
            return playerID.ToString();
        }
        return "UnknownPatientID";
    }

    // Disable other therapist tiles when one therapist is selected
    private void DisableOtherTherapists()
    {
        foreach (Transform child in therapistListContainer)
        {
            // Assuming TherapistTile has a button component that can be disabled
            Button therapistButton = child.GetComponentInChildren<Button>();
            if (therapistButton != null)
            {
                therapistButton.interactable = false;
            }
        }
    }

    void OnBackButtonClicked()
    {
        SceneManager.LoadScene("PatientSettingsPage");
    }
}

[System.Serializable]
public class TherapistData
{
    public string Name;
    public string Specialization;
    public int Experience;
    public string PlayerID;

    // Constructor
    public TherapistData(string name, string specialization, int experience, string playerID)
    {
        Name = name;
        Specialization = specialization;
        Experience = experience;
        PlayerID = playerID;
    }

    // Default Constructor (for serialization)
    public TherapistData() { }
}
