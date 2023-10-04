using UnityEngine;

public class CombinedOvalAndVerticalMotion : MonoBehaviour
{
    // General speed multiplier
    public float speedMultiplier = 1.0f;

    // Oval Motion Parameters
    public float radiusX = 5.0f;          // Radius of the oval along the X-axis
    public float radiusZ = 3.0f;          // Radius of the oval along the Z-axis
    public float ovalRotationSpeed = 1.0f; // Speed of rotation for oval motion (in radians per second)
    private Vector3 centerPosition;       // The center around which the object will rotate

    // Vertical Oscillation Parameters
    public float amplitude = 5.0f;
    public float verticalFrequency = 1.0f; // Frequency of vertical oscillation
    private float initialYPosition;

    private void Start()
    {
        // Store the initial position as the center for the oval motion
        centerPosition = transform.position;
        // Store the initial Y position for the vertical oscillation
        initialYPosition = transform.position.y;
    }
    float newX;
    float newZ;

    // Vertical Oscillation
    float newY;

    private Transform myTransform;

    private void Awake()
    {
        myTransform = GetComponent<Transform>();
    }

    private void Update()
    {
        // Calculate the new position for the oval motion
         newX = centerPosition.x + radiusX * Mathf.Cos(Time.time * ovalRotationSpeed * speedMultiplier);
         newZ = centerPosition.z + radiusZ * Mathf.Sin(Time.time * ovalRotationSpeed * speedMultiplier);

        // Vertical Oscillation
         newY = initialYPosition + amplitude * Mathf.Sin(Time.time * verticalFrequency * speedMultiplier);

        // Combine the motions and update the object's position
        myTransform.position = new Vector3(newX, newY, newZ);
    }
}
