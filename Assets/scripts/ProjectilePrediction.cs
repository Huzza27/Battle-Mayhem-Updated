using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ProjectilePrediction : MonoBehaviour
{
    /*
    public Transform player; // Reference to the player's position
    public int trajectorySteps = 30; // Number of points in the trajectory line
    public float timeStep = 0.1f; // Time step between trajectory points
    public float maxTossTime = 2f; // Maximum allowable time for the projectile to reach the target

    private LineRenderer lineRenderer;
    private Vector2 calculatedForce;

    public Vector2 GetCalculatedForce()
    {
        return calculatedForce;
    }

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (player == null) return;

        // Get mouse position in world space
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // Calculate force needed to hit the target
        calculatedForce = CalculateForce(mouseWorldPos, player.position);

        // Draw the trajectory if the force is valid
        if (calculatedForce != Vector2.zero)
        {
            DrawTrajectory(player.position, calculatedForce);
        }
    }

    Vector2 CalculateForce(Vector3 target, Vector3 origin)
    {
        // Calculate horizontal and vertical distances
        float dx = target.x - origin.x;
        float dy = target.y - origin.y;
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        // Try different times to find a valid velocity
        for (float t = 0.1f; t <= maxTossTime; t += 0.1f)
        {
            // Calculate the initial velocity needed to hit the target
            float vX = dx / t;
            float vY = (dy + 0.5f * gravity * t * t) / t;

            // Return the velocity if it's within reasonable limits
            if (Mathf.Abs(vX) <= 100f && Mathf.Abs(vY) <= 100f) // Adjust limits as needed
            {
                return new Vector2(vX, vY);
            }
        }

        // Return zero if no valid velocity was found
        return Vector2.zero;
    }

    void DrawTrajectory(Vector3 startPosition, Vector2 initialVelocity)
    {
        lineRenderer.positionCount = trajectorySteps;

        for (int i = 0; i < trajectorySteps; i++)
        {
            float t = i * timeStep;
            float x = startPosition.x + initialVelocity.x * t;
            float y = startPosition.y + initialVelocity.y * t - 0.5f * Mathf.Abs(Physics2D.gravity.y) * t * t;

            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        
  
    }
      */
}
