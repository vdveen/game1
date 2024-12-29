using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 8f;
    [SerializeField] private float detectionDistance = 20f;
    [SerializeField] float currentSpeed;
    private Vector3 startPoint;
    private Vector3 endPoint; 
    private bool isMoving = true;
    private GameManager gameManager; 
    private int currentWaypointIndex = 0; 
    private Vector3 currentDirection;
    
    private void Start()
    {
        baseSpeed += Random.Range(-0.8f, 0.8f);
        baseSpeed = Mathf.Max(1f, baseSpeed);
        currentSpeed = baseSpeed;
        startPoint = transform.position;
        endPoint = transform.position + Vector3.forward * 600f;
        gameManager = GameObject.FindAnyObjectByType<GameManager>();
        //currentWaypointIndex = 0;
        currentWaypointIndex = Random.Range(0, gameManager.numberOfNodes);
    }

    private void Update()
    {
        if (!isMoving) return;
        if (gameManager.waypoints == null || gameManager.waypoints.Length == 0) return;

        // 1. Identify the current waypoint
        Transform targetWaypoint = gameManager.waypoints[currentWaypointIndex];

        // 2. Calculate the direction from you to the waypoint
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;

        // 3. Store that direction for gizmos
        currentDirection = direction;   

        // 4. Move toward the waypoint
        float step = currentSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, step);

        // 5. Face the waypoint
        transform.LookAt(transform.position + direction);

        // 6. Check if close enough to switch to next waypoint
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.2f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % gameManager.waypoints.Length;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Vehicle"))
            {
                // Get the other vehicle's script component
                VehicleController otherVehicle = hit.collider.GetComponent<VehicleController>();
                if (otherVehicle != null)
                {
                    // Compare movement directions
                    float angleToOther = Vector3.Angle(direction, -otherVehicle.currentDirection);
                    if (angleToOther < 20f)
                    {
                    currentSpeed = baseSpeed; //aka do nothing
                    } else {                    
                        if (hit.distance < 6f)
                        {
                            currentSpeed = 2f;
                        }
                        else if (hit.distance < 4f)
                        {
                            currentSpeed = 1f;
                        }
                        else if (hit.distance < 2f)
                        {
                            currentSpeed = 0f;
                        }
                        else
                        {
                            currentSpeed = Mathf.MoveTowards(currentSpeed, baseSpeed * 0.5f, 1f * Time.deltaTime); 
                        }
                    }
                } else {
                    currentSpeed = baseSpeed; 
                }
            }
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, baseSpeed, 1f * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, endPoint) < 0.1f)
        {
            transform.position = startPoint;
            currentSpeed = baseSpeed;
        }
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, currentDirection * detectionDistance);
    }
}
    
