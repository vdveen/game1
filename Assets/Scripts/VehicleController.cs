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
    private Vector3 currentDirection;
    private RoadNetwork network;

    public Waypoint currentWaypoint;
    public Waypoint previousWaypoint;
    public Waypoint endWaypoint;
    public Queue<Waypoint> plannedRoute;
    public List<Node> vehiclePath;
    public List<Waypoint> plannedRouteDebug;

    public void SetInitialWaypoints(Waypoint spawnPoint, Waypoint endWaypoint, List<Node> pathToFollow)
    {
        // Find required components
        gameManager = GameObject.FindAnyObjectByType<GameManager>();
        network = GameObject.FindAnyObjectByType<RoadNetwork>();

        // Set speed
        baseSpeed = Mathf.Max(6f, baseSpeed);
        baseSpeed += Random.Range(-0.8f, 0.8f);
        currentSpeed = baseSpeed;

        // Set waypoints
        previousWaypoint = spawnPoint;
        this.endWaypoint = endWaypoint;

        // Store the path
        vehiclePath = pathToFollow;
        Debug.Log($"VehicleController.SetInitialWaypoints: path count = {vehiclePath?.Count ?? 0}");

        // Convert the path to planned route
        //Queue<Waypoint> plannedRoute = new Queue<Waypoint>();
        plannedRoute = ConvertNodePathToWaypoints(vehiclePath);
        if (plannedRoute != null)
        {
            currentWaypoint = plannedRoute.Dequeue();
        }
        Debug.Log($"ConvertNodePathToWaypoints route = {currentWaypoint}");

        // Set up initial movement
        if (currentWaypoint == null || currentWaypoint.connections.Count > 0)
        {
            Debug.LogError($"Waypoint {currentWaypoint?.name ?? "null"} has no connections!");
        }
    }

    private void Update()
    {
        if (!isMoving || gameManager.waypoints == null || gameManager.waypoints.Length == 0 || currentWaypoint == null) return;

        plannedRouteDebug = plannedRoute.ToList();

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
            currentWaypoint = ChooseNextWaypoint();
            isMoving = currentWaypoint != null;
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
                    }
                    else
                    {
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
                }
                else
                {
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




        if (plannedRoute != null && plannedRoute.Count > 0)
        {
            return plannedRoute.Dequeue();
        }
        else
        {
            isMoving = false; // Stop the vehicle
            Debug.Log("Reached goal");
            return null;
        }
    }


    private Queue<Waypoint> ConvertNodePathToWaypoints(List<Node> vehiclePath)
    {
        Queue<Waypoint> waypointQueue = new Queue<Waypoint>();
        if (vehiclePath == null) return waypointQueue;

        // Grab your RoadNetwork reference
        RoadNetwork network = GameObject.FindAnyObjectByType<RoadNetwork>();

        foreach (Node node in vehiclePath)
        {
            Waypoint w = network.GetWaypointForNode(node);
            if (w != null)
            {
                waypointQueue.Enqueue(w);
            }
        }
        return waypointQueue;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        // teken gizmo iets hoger dan wegennetwerk
        Gizmos.DrawRay(transform.position + Vector3.up * 0.02f, (currentDirection * detectionDistance) + Vector3.up * 0.02f);
    }
}

