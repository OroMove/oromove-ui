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

public class PatientLeaderboardManager : MonoBehaviour
{
    public Transform patientListContainer;
    public GameObject patientItemPrefab;
    public Button backButton;

    private const string LEADERBOARD_ID = "PatientLeaderboard"; // You can change this to your actual leaderboard ID.

    private async void Start()
    {
        await Task.Delay(100);
        LoadPatients();
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    public async void LoadPatients()
    {
        List<PatientData> patients = await GetPatientsData();

        Debug.Log($"Patients found: {patients.Count}");

        DisplayPatients(patients);
    }

    public async Task<List<PatientData>> GetPatientsData()
    {
        // Fetching the leaderboard data with metadata included
        var response = await LeaderboardsService.Instance.GetScoresAsync(LEADERBOARD_ID, new GetScoresOptions { IncludeMetadata = true });
        List<PatientData> patients = new List<PatientData>();

        foreach (var entry in response.Results)
        {
            Debug.Log($"Processing entry: PlayerID={entry.PlayerId}, MetaData={entry.Metadata}, Score={entry.Score}");

            string name = "Unknown";
            string diagnosis = "Unknown";

            // Ensure metadata exists
            if (entry.Metadata != null)
            {
                try
                {
                    JObject metadata = JObject.Parse(entry.Metadata.ToString());

                    // Extract values from the JObject
                    name = metadata["Name"]?.ToString() ?? "Unknown";
                    diagnosis = metadata["Diagnosis"]?.ToString() ?? "Unknown";
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error deserializing metadata: {ex.Message}");
                }
            }

            // Add patient data to the list
            patients.Add(new PatientData(name, diagnosis, entry.PlayerId));
        }

        return patients;
    }

    private void DisplayPatients(List<PatientData> patients)
    {
        // Check for null references to avoid issues
        if (patientItemPrefab == null)
        {
            Debug.LogError("Patient item prefab is not assigned!");
            return;
        }

        if (patientListContainer == null)
        {
            Debug.LogError("Patient list container is not assigned!");
            return;
        }

        // Clear the previous entries from the container
        foreach (Transform child in patientListContainer)
        {
            Destroy(child.gameObject);
        }

        // Instantiate new patient tiles for each patient
        foreach (PatientData patient in patients)
        {
            GameObject newEntry = Instantiate(patientItemPrefab, patientListContainer);
            PatientTile patientItem = newEntry.GetComponent<PatientTile>();

            if (patientItem != null)
            {
                patientItem.SetPatientData(
                    patient.Name,
                    patient.Diagnosis,
                    patient.PlayerID
                );
            }
            else
            {
                Debug.LogWarning("PatientTile component not found on the instantiated prefab.");
            }
        }

        // Force a layout rebuild in case of dynamic changes
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            patientListContainer.GetComponent<RectTransform>()
        );
    }

    void OnBackButtonClicked()
    {
        SceneManager.LoadScene("TherapistHomePage");
    }
}

[System.Serializable]
public class PatientData
{
    public string Name;
    public string Diagnosis;
    public string PlayerID;

    // Constructor
    public PatientData(string name, string diagnosis, string playerID)
    {
        Name = name;
        Diagnosis = diagnosis;
        PlayerID = playerID;
    }

    // Default Constructor (for serialization)
    public PatientData() { }
}
