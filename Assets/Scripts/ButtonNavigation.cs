using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphManager : MonoBehaviour
{
    public RectTransform graphContainer; // The container for the graph
    public GameObject linePrefab; // The Image prefab used for drawing lines
    private Dictionary<int, List<float>> levelMaxMouthOpeningData = new Dictionary<int, List<float>>();

    void Start()
    {
        // Sample data for demonstration purposes
        levelMaxMouthOpeningData[1] = new List<float> { 50f, 100f, 150f, 200f, 250f };

        // Draw graph for level 1 (or any level)
        DrawGraph(1);
    }

    void DrawGraph(int level)
    {
        if (levelMaxMouthOpeningData.ContainsKey(level))
        {
            List<float> maxMouthOpenings = levelMaxMouthOpeningData[level];

            if (maxMouthOpenings.Count == 0)
            {
                Debug.LogWarning($"⚠️ No data to plot for Level {level}");
                return;
            }

            float xStart = -500f;
            float yStart = -1000f;
            float xSpacing = 100f;
            float yScale = 500f;

            // Loop through the data and draw lines
            for (int i = 0; i < maxMouthOpenings.Count - 1; i++)
            {
                // Create a new line segment
                GameObject lineSegment = Instantiate(linePrefab, graphContainer);

                RectTransform rt = lineSegment.GetComponent<RectTransform>();

                // Calculate the start and end positions
                Vector3 startPos = new Vector3(xStart + (i * xSpacing), yStart + (maxMouthOpenings[i] * yScale), 0);
                Vector3 endPos = new Vector3(xStart + ((i + 1) * xSpacing), yStart + (maxMouthOpenings[i + 1] * yScale), 0);

                // Position the line segment (Image) in the graph container
                rt.position = startPos;

                // Adjust the size of the line segment
                rt.sizeDelta = new Vector2(Vector3.Distance(startPos, endPos), 5f); // Set line thickness (5px)

                // Rotate the line segment to point from start to end position
                rt.right = endPos - startPos;
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ No data found for Level {level} in the graph.");
        }
    }
}
