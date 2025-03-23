using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PatientTile : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI diagnosisText;
    public Button selectButton;

    private string patientID;

    public void SetPatientData(string name, string diagnosis, string id)
    {
        Debug.Log($"Setting UI Data: Name={name}, Diagnosis={diagnosis}");

        nameText.text = name;
        diagnosisText.text = diagnosis;
        patientID = id;

        selectButton.onClick.AddListener(() => SelectPatient());
    }

    private void SelectPatient()
    {
        Debug.Log($"Selected Patient ID: {patientID}");
        PlayerPrefs.SetString("SelectedPatientID", patientID); // Store ID for retrieval in PatientBrief
        SceneManager.LoadScene("PatientBriefDetail");
    }
}
