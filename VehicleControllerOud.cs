using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float detectionDistance = 5f;
    [SerializeField] private float currentSpeed = 5f;

    private Vector3 startPoint;
    private Vector3 endPoint;
    private bool isMoving = true;

    private void Start()
    {
        // Save initial position (start point)
        startPoint = transform.position;
        // Define a simple forward direction endpoint.
        // Adjust as needed, e.g., make it 20 units in front.
        endPoint = transform.position + Vector3.forward * 50f;
    }

    private void Update()
    {
        if (!isMoving) return;

        // Cast a ray forward to check if we're about to collide with another vehicle
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Vehicle"))
            {
                if (hit.distance < 1f)
                {
                    isMoving = false;
                    return;
                }
                currentSpeed = baseSpeed * 0.9f;
                return;
            }
        }
        //else {
        //    currentSpeed = baseSpeed;
        //}

        // Move towards endPoint
        float step = currentSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, endPoint, step);

        // Slow down to test collision 
        if (Vector3.Distance(transform.position, endPoint) < 40f)
        {
            if (currentSpeed > 2f)
            {
            currentSpeed *= 0.99f;
            }
            else 
            {
                currentSpeed = 5f;
            }
        }

        // If we've reached the end, reset to start
        if (Vector3.Distance(transform.position, endPoint) < 0.1f)
        {
            transform.position = startPoint;
            isMoving = true; // Reset movement in case we had stopped
        }
    }

    // (Optional) Draw the ray in the Scene window for debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * detectionDistance);
    }
}