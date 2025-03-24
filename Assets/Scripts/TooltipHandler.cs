using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private GameObject tooltipPanel;  // The UI Panel
    private TMP_Text tooltipText;     // The Text inside the Panel
    private float mouthDistance;      // The distance value

    // This method is called when the pointer enters the graph bar
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Ensure the tooltip panel is active when hovering over the bar
        tooltipPanel.SetActive(true);

        // Set the tooltip text to show the mouth distance
        tooltipText.text = "Mouth Distance: " + mouthDistance.ToString("F2");

        // Position the tooltip panel near the pointer (hover location)
        Vector2 position = eventData.position;
        tooltipPanel.transform.position = position;  // Move the panel to that position
    }

    // This method is called when the pointer exits the graph bar
    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.SetActive(false);  // Hide the tooltip when pointer exits
    }

    // Method to dynamically assign the tooltip UI elements
    public void InitializeTooltip(GameObject panel, TMP_Text text)
    {
        tooltipPanel = panel;
        tooltipText = text;
    }

    // This will be called to set the mouth distance
    public void SetMouthDistance(float distance)
    {
        mouthDistance = distance;
    }
}
