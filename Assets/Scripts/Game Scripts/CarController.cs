using UnityEngine;
using UnityEngine.UI; // For UI components like Image and Text
using System.Collections.Generic; // For storing distances and speeds in lists

public class CarController : MonoBehaviour
{
    public float fuelLevel = 1f; // Assuming max is 1
    public float fuelConsumptionRate;

    public Image fuelBar; // Image component for fuel bar
    public Gradient fuelGradient; // Gradient for color change

    public float torqueForce = 150f;
    public float baseSpeed = 150f; // Initial speed
    private float mouthOpeningDistance = 0f;
    public float MouthOpeningDistance => mouthOpeningDistance; // Read-only property
    public float maxSpeedFromMouthOpening = 0f; // Track max speed from mouth opening distance

    private List<float> mouthOpeningDistances = new List<float>(); // Store all mouth opening distances
    private List<float> speeds = new List<float>(); // Store all speeds corresponding to mouth opening distances

    public List<float> GetNonZeroMouthOpeningDistances() => mouthOpeningDistances; // Get mouth opening distances
    public List<float> GetSpeeds() => speeds; // Get all corresponding speeds

    public Rigidbody2D frontWheel;
    public Rigidbody2D rearWheel;
    public Rigidbody2D carBody;

    public Text speedText; // Text component to display speed
    public GameObject finishFlag; // Reference to the finish line flag

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == finishFlag) // When colliding with the finish line
        {
            FindObjectOfType<GameManager>().LevelComplete(); // Trigger level completion
        }
    }

    void Update()
    {
        fuelBar.fillAmount = fuelLevel;
        fuelBar.color = fuelGradient.Evaluate(fuelLevel); // Set color based on fuel level
    }

    private void FixedUpdate()
    {
        if (fuelLevel > 0)
        {
            MouthDetectionMovement();
        }
        ManageFuelConsumption();
    }

    void MouthDetectionMovement()
    {
        if (mouthOpeningDistance > 0f) // Even the slightest opening accelerates
        {
            float speedOfCar = Mathf.Min(400f, 50f + 25f * (mouthOpeningDistance / 0.01f));

            Debug.Log("Mouth Opening Distance: " + mouthOpeningDistance + ", Speed: " + speedOfCar);

            // Store the maximum speed reached based on mouth opening distance
            if (speedOfCar > maxSpeedFromMouthOpening)
            {
                maxSpeedFromMouthOpening = speedOfCar;
            }

            // Store all mouth opening distances and their corresponding speeds
            mouthOpeningDistances.Add(mouthOpeningDistance);
            speeds.Add(speedOfCar);

            speedText.text = "Speed: " + Mathf.Round(speedOfCar).ToString();

            float adjustedTorque = speedOfCar * Time.fixedDeltaTime;
            rearWheel.AddTorque(-adjustedTorque);
            frontWheel.AddTorque(-adjustedTorque);
        }
        else // Mouth fully closed, apply smooth braking
        {
            float brakeForce = 3f; // Smooth braking factor

            // Gradually reduce velocity
            rearWheel.linearVelocity = Vector2.Lerp(rearWheel.linearVelocity, Vector2.zero, Time.fixedDeltaTime * brakeForce);
            frontWheel.linearVelocity = Vector2.Lerp(frontWheel.linearVelocity, Vector2.zero, Time.fixedDeltaTime * brakeForce);

            // Force stop when speed is very low
            if (rearWheel.linearVelocity.magnitude < 0.1f)
            {
                rearWheel.linearVelocity = Vector2.zero;
                frontWheel.linearVelocity = Vector2.zero;
            }

            // Update UI with actual speed
            float currentSpeed = rearWheel.linearVelocity.magnitude * 10f; // Scale for readability
            speedText.text = "Speed: " + Mathf.Round(currentSpeed).ToString();
        }
    }

    void ManageFuelConsumption()
    {
        // Calculate the distance the car moves (magnitude of the velocity vector)
        float distanceMoved = carBody.linearVelocity.magnitude * Time.deltaTime; // Speed multiplied by Time.deltaTime gives distance

        // If the car moves forward or backward, reduce fuel by 2% per meter
        if (distanceMoved > 0f)
        {
            fuelLevel -= 0.01f * distanceMoved; // 2% fuel reduction per meter
        }

        // Ensure fuel stays within 0 to 1 range
        fuelLevel = Mathf.Clamp01(fuelLevel);
    }

    public void SetMouthOpeningDistance(float distance)
    {
        mouthOpeningDistance = distance;
    }
}
