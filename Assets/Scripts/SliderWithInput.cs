using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SeveritySliderController : MonoBehaviour
{
    public Slider severitySlider;
    public TextMeshProUGUI severityValueText;

    void Start()
    {
        severitySlider.onValueChanged.AddListener(UpdateSeverityText);
        UpdateSeverityText(severitySlider.value); 
    }

    void UpdateSeverityText(float value)
    {
        severityValueText.text = value.ToString("0");
    }
}
