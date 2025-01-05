using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [SerializeField] private float baseSpeed = 8f;
    [SerializeField] private float detectionDistance = 20f;
    [SerializeField] float currentSpeed;
    private bool isMoving = true;
    private GameManager gameManager; 
    //private int currentWaypointIndex = 0; 
    private Vector3 currentDirection;
    public Waypoint currentWaypoint;
    public Waypoint previousWaypoint;
    
    private void Start()
    {
        baseSpeed = Mathf.Max(6f, baseSpeed);
        baseSpeed += Random.Range(-0.8f, 0.8f);
        currentSpeed = baseSpeed;
        
        // Find the GameManager
        gameManager = GameObject.FindAnyObjectByType<GameManager>();
        if (gameManager != null && gameManager.waypoints.Length > 0)
        {
            // Set initial waypoint
            int randomIndex = Random.Range(0, gameManager.waypoints.Length);
            currentWaypoint = gameManager.waypoints[randomIndex].GetComponent<Waypoint>();
            
            // Choose next waypoint immediately to get moving
            if (currentWaypoint != null)
            {
                previousWaypoint = currentWaypoint;
                currentWaypoint = ChooseNextWaypoint();
            }
        } else
        {
            Debug.LogError("GameManager not found or no waypoints available!");
        }
    }

    private void Update()
    {
        if (!isMoving || gameManager.waypoints == null || gameManager.waypoints.Length == 0 || currentWaypoint == null) return;

        // Calculate direction to current waypoint
        Vector3 direction = (currentWaypoint.transform.position - transform.position).normalized;

        // Store that direction for gizmos
        currentDirection = direction;   

        // Move toward the waypoint
        float step = currentSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.transform.position, step);

        // Face the waypoint
        transform.LookAt(transform.position + direction);

        // Check if reached waypoint
        if (Vector3.Distance(transform.position, currentWaypoint.transform.position) < 0.2f)
        {
            ChooseNextWaypoint();
        }

        //RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out var hit, detectionDistance))
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
                            //isMoving = false;
                        }
                        else
                        {
                            currentSpeed = Mathf.MoveTowards(currentSpeed, baseSpeed, 1f * Time.deltaTime); 
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

    }

    private Waypoint ChooseNextWaypoint()
        {
            if (currentWaypoint == null || currentWaypoint.connections.Count == 0)
                return null;

            // Filter out the previous waypoint to avoid going backwards (unless it's the only option)
            List<Waypoint> availableConnections = currentWaypoint.connections
                .Where(w => w != previousWaypoint || currentWaypoint.connections.Count == 1)
                .ToList();

            if (availableConnections.Count == 0)
                availableConnections = currentWaypoint.connections; // If no other options, allow backtracking

            // Choose a random connection from available ones
            int randomIndex = Random.Range(0, availableConnections.Count);
            return availableConnections[randomIndex];
        }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, currentDirection * detectionDistance);
    }
}
    
