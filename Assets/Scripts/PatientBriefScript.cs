using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Leaderboards;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;

public class PatientBriefManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI ageText;
    public TextMeshProUGUI diagnosisText;
    public TextMeshProUGUI GenderText;
    public TextMeshProUGUI SeverityLevelText;
    public Button backButton;

    private string leaderboardId = "PatientLeaderboard";

    private async void Start()
    {
        string selectedPatientID = PlayerPrefs.GetString("SelectedPatientID", null);

        if (string.IsNullOrEmpty(selectedPatientID))
        {
            Debug.LogError("No Patient ID selected!");
            return;
        }

        await LoadPatientData(selectedPatientID);
        backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private async Task LoadPatientData(string playerId)
    {
        var response = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, new GetScoresOptions { IncludeMetadata = true });

        foreach (var entry in response.Results)
        {
            if (entry.PlayerId == playerId)
            {
                Debug.Log($"Found patient data for ID: {playerId}");

                if (entry.Metadata != null)
                {
                    try
                    {
                        JObject metadata = JObject.Parse(entry.Metadata.ToString());

                        // Extracting metadata values
                        nameText.text = metadata["Name"]?.ToString() ?? "Unknown";
                        ageText.text = metadata["Age"]?.ToString() ?? "Unknown";
                        diagnosisText.text = metadata["Diagnosis"]?.ToString() ?? "Unknown";
                        GenderText.text = metadata["Gender"]?.ToString() ?? "Unknown";
                        SeverityLevelText.text = metadata["SeverityLevel"]?.ToString() ?? "Unknown";
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Error parsing metadata: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogError("No metadata found for the selected patient!");
                }
                return;
            }
        }

        Debug.LogError($"Patient with ID {playerId} not found in the leaderboard!");
    }

    void OnBackButtonClicked()
    {
        SceneManager.LoadScene("TherapistUserProfile");
    }
}
