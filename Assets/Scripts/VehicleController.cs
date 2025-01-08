using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
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
    public Waypoint startWaypoint;
    public Waypoint endWaypoint;
    private RoadNetwork network;
    public List<Waypoint> waypoints;
    public Queue<Waypoint> plannedRoute;
    public List<Node> path;
    public void SetInitialWaypoint(Waypoint waypoint)
    {
        currentWaypoint = waypoint;
    }

    private void Start()
    {
        baseSpeed = Mathf.Max(6f, baseSpeed);
        baseSpeed += Random.Range(-0.8f, 0.8f);
        currentSpeed = baseSpeed;

        // Find the GameManager and RoadNetwork
        gameManager = GameObject.FindAnyObjectByType<GameManager>();
        network = GameObject.FindAnyObjectByType<RoadNetwork>();

        
        // Make sure the current waypoint has connections
        if (currentWaypoint != null && currentWaypoint.connections.Count > 0)
        {
            // Choose a random connection as the next waypoint
            previousWaypoint = currentWaypoint;
            currentWaypoint = currentWaypoint.connections[Random.Range(0, currentWaypoint.connections.Count)];
        }
        else
        {
            Debug.LogError($"Waypoint {currentWaypoint?.name ?? "null"} has no connections!");
        }

        
        startWaypoint = currentWaypoint; // Store the initial waypoint for pathfinding
        // Convert Transform array to List<Waypoint>
        waypoints = new List<Waypoint>();
        foreach (Transform waypointTransform in gameManager.waypoints)
        {
            Waypoint waypoint = waypointTransform.GetComponent<Waypoint>();
            if (waypoint != null)
            {
                waypoints.Add(waypoint);
            }
        }
        endWaypoint = waypoints[Random.Range(0, waypoints.Count)];
        // Convert to Node
        Node startNode = network.GetNodeForWaypoint(startWaypoint);
        Node goalNode = network.GetNodeForWaypoint(endWaypoint);

        List<Node> path = Pathfinding.FindPath(startNode, goalNode);

        // Convert Node path back to Waypoint path
        plannedRoute = ConvertNodePathToWaypoints(path);
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
            Waypoint nextWaypoint = ChooseNextWaypoint();
            if (nextWaypoint == null)
            {
                currentWaypoint = null;
                isMoving = false; // Stop at dead end
            }
            else
            {
                currentWaypoint = nextWaypoint;
            }
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

        // Store the current waypoint before changing it
        Waypoint oldWaypoint = currentWaypoint;

        // Filter out the previous waypoint to avoid going backwards
        List<Waypoint> availableConnections = currentWaypoint.connections
            .Where(w => w != previousWaypoint)
            .ToList();

        // If there are no valid connections (dead end)
        if (availableConnections.Count == 0)
        {
            isMoving = false; // Stop the vehicle
            Debug.Log("Reached dead end");
            return null;
        }

        // Choose a random connection from available ones
        int randomIndex = Random.Range(0, availableConnections.Count);
        previousWaypoint = oldWaypoint;
        return availableConnections[randomIndex];
    }

    private Queue<Waypoint> ConvertNodePathToWaypoints(List<Node> nodePath)
    {
        Queue<Waypoint> route = new Queue<Waypoint>();
        foreach (Node node in nodePath)
        {
            Waypoint wp = network.GetWaypointForNode(node); 
            route.Enqueue(wp);
        }
        return route;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // teken gizmo iets hoger dan wegennetwerk
        Gizmos.DrawRay(transform.position + Vector3.up * 0.02f, (currentDirection * detectionDistance)+ Vector3.up * 0.02f);
    }
}
    
