using UnityEngine;

public class ThrowAwayGunOnSwap : MonoBehaviour
{
    [Header("Direction Settings")]
    [SerializeField] private float minAngle = 210f; // Slightly left of down
    [SerializeField] private float maxAngle = 330f; // Slightly right of down

    [Header("Force Settings")]
    [SerializeField] private float minForce = 5f;
    [SerializeField] private float maxForce = 10f;

    [Header("Rotation Settings")]
    [SerializeField] private float minRotationSpeed = -360f; // Degrees per second
    [SerializeField] private float maxRotationSpeed = 360f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody2D found on GameObject!");
            enabled = false;
            return;
        }

        TossObject();
    }

    private void TossObject()
    {
        // Generate random angle within our specified range
        float randomAngle = Random.Range(minAngle, maxAngle);

        // Convert angle to radians and calculate direction
        float angleRad = randomAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

        // Apply random force
        float randomForce = Random.Range(minForce, maxForce);
        rb.AddForce(direction * randomForce, ForceMode2D.Impulse);

        // Apply random rotation
        float randomRotation = Random.Range(minRotationSpeed, maxRotationSpeed);
        rb.angularVelocity = randomRotation;
    }
}