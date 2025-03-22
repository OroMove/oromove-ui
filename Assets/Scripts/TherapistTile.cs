using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class TherapistTile : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI specializationText;
    public TextMeshProUGUI experienceText;
    public Button selectButton;

    private string therapistPlayerID;

    public static event Action<string> OnTherapistSelected; // Event for selection

    public void SetTherapistData(string name, string specialization, int experience, string playerID)
    {
        Debug.Log($"Setting UI Data: Name={name}, Specialization={specialization}, Experience={experience}");

        nameText.text = name;
        specializationText.text = specialization;
        experienceText.text = experience.ToString();
        therapistPlayerID = playerID;

        selectButton.onClick.AddListener(() => SelectTherapist());
    }

    private void SelectTherapist()
    {
        Debug.Log($"Selected Therapist ID: {therapistPlayerID}");
        OnTherapistSelected?.Invoke(therapistPlayerID);
    }
}
