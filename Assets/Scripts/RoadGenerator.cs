using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoadGenerator : MonoBehaviour
{
    public Waypoint waypointPrefab;
    public int mainChainLength = 20;
    public int branchLength = 10;
    public float stepDistance = 5f;
    public float maxAngleOffset = 10f;
    public float nearbyNodeRadius = 2.5f;
    public int expansionIterations = 1000; // Adjust as needed
    public float spawnDistance = 10f;
    public Transform waypointContainer;
    public List<Waypoint> waypoints = new();

    // Start is called before the first execution of Update
    void Start()
    {
        if (waypointPrefab == null)
        {
            Debug.LogError("Waypoint prefab not assigned to RoadGenerator!");
            return;
        }
        waypointContainer = new GameObject("Waypoints").transform;
        waypointContainer.SetParent(transform);
        GenerateRoadNetwork();
        ValidateNetwork();
    }


    private void GenerateRoadNetwork()
    {
        // Step 1: Initial chain
        Vector3 startPosition = Vector3.zero;
        Vector3 startDirection = Vector3.forward;
        
        Waypoint firstWaypoint = Instantiate(waypointPrefab, startPosition, Quaternion.identity, waypointContainer);
        waypoints.Add(firstWaypoint);
        GenerateBranch(firstWaypoint, startDirection, 20);

        // Step 2 & 3: Middle branches
        int middleIndex = waypoints.Count / 2;
        Waypoint middleNode = waypoints[middleIndex];

        Vector3 mainChainDirection = waypoints[middleIndex + 1].transform.position - middleNode.transform.position;
        mainChainDirection.Normalize();

        Vector3 upDirection = Quaternion.Euler(0f, 90f, 0f) * mainChainDirection;
        Vector3 downDirection = Quaternion.Euler(0f, -90f, 0f) * mainChainDirection;

        GenerateBranch(middleNode, upDirection, 10);
        GenerateBranch(middleNode, downDirection, 10);

        // Step 4: Random expansions

        for (int i = 0; i < expansionIterations; i++)
        {
            Waypoint randomWaypoint = waypoints[Random.Range(0, waypoints.Count)];
            Vector3 chosenDirection = GetFreeDirection(randomWaypoint);
            Vector3 spawnPos = randomWaypoint.transform.position + chosenDirection * spawnDistance;

            Waypoint nearbyWaypoint = FindNearbyWaypoint(spawnPos, spawnDistance * 0.5f);

            if (nearbyWaypoint != null)
            {
                // Check if connecting to the nearby waypoint would cause intersection
                if (!IsIntersectingExistingRoad(randomWaypoint.transform.position, nearbyWaypoint.transform.position))
                {
                    randomWaypoint.connections.Add(nearbyWaypoint);
                    nearbyWaypoint.connections.Add(randomWaypoint);
                }
            }
            else
            {
                // Check if creating new waypoint would cause intersection
                if (!IsIntersectingExistingRoad(randomWaypoint.transform.position, spawnPos))
                {
                    Waypoint newWaypoint = Instantiate(waypointPrefab, spawnPos, Quaternion.identity, waypointContainer);
                    waypoints.Add(newWaypoint);
                    randomWaypoint.connections.Add(newWaypoint);
                    newWaypoint.connections.Add(randomWaypoint);
                }
            }
        }
    }

    private void GenerateBranch(Waypoint startNode, Vector3 direction, int numberOfNodes)
    {
        Waypoint currentNode = startNode;
        Vector3 currentDirection = direction.normalized;

        for (int i = 0; i < numberOfNodes; i++)
        {
            // Apply random angle offset
            float angleOffset = Random.Range(-10f, 10f);
            Quaternion rotation = Quaternion.Euler(0f, angleOffset, 0f);
            currentDirection = rotation * currentDirection;

            float stepDistance = 5f;
            Vector3 spawnPos = currentNode.transform.position + currentDirection * stepDistance;
            if (IsIntersectingExistingRoad(currentNode.transform.position, spawnPos))
            {
                break; // Stop the branch if we would intersect another road
            }
            Waypoint newWaypoint = Instantiate(waypointPrefab, spawnPos, Quaternion.identity, waypointContainer);
            waypoints.Add(newWaypoint);

            currentNode.connections.Add(newWaypoint);
            newWaypoint.connections.Add(currentNode);

            currentNode = newWaypoint;
        }
    }

    private Vector3 GetFreeDirection(Waypoint reference)
    {
        const float minAngleBetweenConnections = 60f;
        
        // Get all current connection angles
        List<float> usedAngles = new List<float>();
        foreach (Waypoint connection in reference.connections)
        {
            Vector3 direction = connection.transform.position - reference.transform.position;
            float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            usedAngles.Add(angle);
        }

        // Try to find a free angle (max 10 attempts)
        for (int i = 0; i < 10; i++)
        {
            float newAngle = Random.Range(0f, 360f);
            bool isFree = true;

            foreach (float usedAngle in usedAngles)
            {
                if (Mathf.Abs(Mathf.DeltaAngle(newAngle, usedAngle)) < minAngleBetweenConnections)
                {
                    isFree = false;
                    break;
                }
            }

            if (isFree || usedAngles.Count == 0)
            {
                return Quaternion.Euler(0f, newAngle, 0f) * Vector3.forward;
            }
        }

        // If no free angle found, return random direction as fallback
        return Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * Vector3.forward;
    }

    private Waypoint FindNearbyWaypoint(Vector3 position, float radius)
    {
        // Simple brute-force check: check all waypoints 
        foreach (Waypoint wp in waypoints)
        {
            float dist = Vector3.Distance(wp.transform.position, position);
            if (dist <= radius)
            {
                return wp;
            }
        }
        return null;
    }

    private bool IsIntersectingExistingRoad(Vector3 startPos, Vector3 endPos, float threshold = 0.5f)
    {
        // Check against all existing road segments
        foreach (Waypoint wp in waypoints)
        {
            foreach (Waypoint connection in wp.connections)
            {
                // Skip checking against the start point itself
                if (Vector3.Distance(wp.transform.position, startPos) < 0.1f || 
                    Vector3.Distance(connection.transform.position, startPos) < 0.1f)
                    continue;

                if (DoLinesIntersect(startPos, endPos, 
                                wp.transform.position, connection.transform.position, 
                                threshold))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool DoLinesIntersect(Vector3 line1Start, Vector3 line1End, 
                                Vector3 line2Start, Vector3 line2End, 
                                float threshold)
    {
        // Convert to 2D (assuming roads are on xz plane)
        Vector2 l1s = new Vector2(line1Start.x, line1Start.z);
        Vector2 l1e = new Vector2(line1End.x, line1End.z);
        Vector2 l2s = new Vector2(line2Start.x, line2Start.z);
        Vector2 l2e = new Vector2(line2End.x, line2End.z);

        // Calculate intersection
        Vector2 line1 = l1e - l1s;
        Vector2 line2 = l2e - l2s;
        
        float cross = line1.x * line2.y - line1.y * line2.x;
        
        // Lines are parallel
        if (Mathf.Abs(cross) < 0.0001f)
            return false;

        Vector2 delta = l2s - l1s;
        float t1 = (delta.x * line2.y - delta.y * line2.x) / cross;
        float t2 = (delta.x * line1.y - delta.y * line1.x) / cross;

        // Check if intersection point lies within both line segments
        if (t1 >= -threshold && t1 <= 1 + threshold && 
            t2 >= -threshold && t2 <= 1 + threshold)
        {
            return true;
        }

        return false;
    }

    private void ValidateNetwork()
    {
        foreach (Waypoint wp in waypoints)
        {
            foreach (Waypoint connection in wp.connections)
            {
                if (!connection.connections.Contains(wp))
                {
                    Debug.LogWarning($"Found one-way connection between waypoints!");
                }
            }
        }
    }
}

