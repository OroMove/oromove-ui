using System.Collections.Generic;
using UnityEngine;

public class FaceLandmarkerEvaluator : MonoBehaviour
{
    private int truePositives = 0;
    private int falsePositives = 0;
    private int falseNegatives = 0;

    private List<float> groundTruthMouthOpenings = new List<float>(); // Actual values
    private List<float> predictedMouthOpenings = new List<float>();   // Predicted by MediaPipe

    private float threshold = 0.01f; // Threshold for considering the mouth open

    public void EvaluatePrediction(float predictedOpening, float actualOpening)
    {
        predictedMouthOpenings.Add(predictedOpening);
        groundTruthMouthOpenings.Add(actualOpening);

        bool predictedOpen = predictedOpening > threshold;
        bool actualOpen = actualOpening > threshold;

        if (predictedOpen && actualOpen) truePositives++;  // Correctly detected open mouth
        else if (predictedOpen && !actualOpen) falsePositives++; // False detection (mistake)
        else if (!predictedOpen && actualOpen) falseNegatives++; // Missed detection

        Debug.Log($"Evaluated: TP={truePositives}, FP={falsePositives}, FN={falseNegatives}");
    }

    public float CalculateF1Score()
    {
        float precision = truePositives / (float)(truePositives + falsePositives);
        float recall = truePositives / (float)(truePositives + falseNegatives);

        if (precision + recall == 0) return 0; // Avoid division by zero

        return 2 * (precision * recall) / (precision + recall);
    }

    public void PrintResults()
    {
        Debug.Log($"Precision: {CalculatePrecision()}");
        Debug.Log($"Recall: {CalculateRecall()}");
        Debug.Log($"F1 Score: {CalculateF1Score()}");
    }

    private float CalculatePrecision()
    {
        return truePositives / (float)(truePositives + falsePositives);
    }

    private float CalculateRecall()
    {
        return truePositives / (float)(truePositives + falseNegatives);
    }
}
