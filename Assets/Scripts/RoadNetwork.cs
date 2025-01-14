using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector3 position;
    public List<Edge> edges = new List<Edge>();

    // For pathfinding
    public float gCost;
    public float hCost;
    public Node parent;

    public float fCost => gCost + hCost;
}

public class Edge
{
    public Node startNode;
    public Node endNode;
    public float weight = 1f;   // Could be distance, time, or some other cost
}


public class RoadNetwork : MonoBehaviour
{
    public List<Node> nodes = new List<Node>();
    private Dictionary<Waypoint, Node> waypointNodeMap;
    private Dictionary<Node, Waypoint> nodeWaypointMap;
    [SerializeField] private GameObject roadSegmentPrefab;
    public Transform roadsegContainer;

    public void BuildNetworkFromWaypoints(List<Waypoint> waypoints)

    {
        // Create a lookup from Waypoint to Node
        waypointNodeMap = new Dictionary<Waypoint, Node>();
        nodeWaypointMap = new Dictionary<Node, Waypoint>();

        // First pass: create nodes
        foreach (var wp in waypoints)
        {
            Node node = new Node { position = wp.transform.position };
            waypointNodeMap[wp] = node;
            nodeWaypointMap[node] = wp;
            nodes.Add(node);
        }

        // Second pass: create edges
        roadsegContainer = new GameObject("Road Segments").transform;
        foreach (var wp in waypoints)
        {
            Node node = waypointNodeMap[wp];
            foreach (var conn in wp.connections)
            {
                Node connectedNode = waypointNodeMap[conn];
                Edge edge = new Edge
                {
                    startNode = node,
                    endNode = connectedNode,
                    // For a basic distance-based cost:
                    weight = Vector3.Distance(node.position, connectedNode.position),
                };
                node.edges.Add(edge);

                // For bidirectional roads, you might want to add the opposite edge too:
                // (Only do this if your roads are truly bidirectional)
                Edge oppositeEdge = new Edge
                {
                    startNode = connectedNode,
                    endNode = node,
                    weight = edge.weight,
                };
                connectedNode.edges.Add(oppositeEdge);

                if (wp == null)
                {
                    Debug.LogWarning("wp is null");
                    continue;
                }

                if (!waypointNodeMap.ContainsKey(wp))
                {
                    Debug.LogWarning($"waypointNodeMap doesn't contain {wp.name}");
                    continue;
                }

                if (roadSegmentPrefab != null)
                {
                    GameObject segmentGO = Instantiate(roadSegmentPrefab);
                    RoadSegment roadSegment = segmentGO.GetComponent<RoadSegment>();
                    // Position, rotate, and scale the segment
                    roadSegment.SetDimensions(node.position, connectedNode.position);
                    segmentGO.transform.SetParent(roadsegContainer);
                }
                else
                {
                    Debug.LogWarning("No roadSegmentPrefab is assigned in RoadNetwork.");
                }
            }
        }

    }
    public Node GetNodeForWaypoint(Waypoint wp)
    {
        return waypointNodeMap[wp];
    }

    public Waypoint GetWaypointForNode(Node node)
    {
        return nodeWaypointMap[node];
    }
}


public class Pathfinding
{
    public static List<Node> FindPath(Node start, Node goal)
    {
        List<Node> openSet = new List<Node> { start };
        HashSet<Node> closedSet = new HashSet<Node>();

        // Initialize
        start.gCost = 0;
        start.hCost = Vector3.Distance(start.position, goal.position);
        start.parent = null;

        while (openSet.Count > 0)
        {
            // Find node with lowest fCost in openSet
            Node current = GetLowestFCostNode(openSet);

            // If we reached our goal, reconstruct path
            if (current == goal)
            {
                return ReconstructPath(goal);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            // Evaluate neighbors
            foreach (Edge edge in current.edges)
            {
                Node neighbor = edge.endNode;

                if (closedSet.Contains(neighbor))
                    continue;

                float tentativeGCost = current.gCost + edge.weight;

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGCost >= neighbor.gCost)
                {
                    // not a better path
                    continue;
                }

                // This is a better path to neighbor
                neighbor.parent = current;
                neighbor.gCost = tentativeGCost;
                neighbor.hCost = Vector3.Distance(neighbor.position, goal.position);
            }
        }

        // If we get here, there's no path
        return null;
    }

    private static Node GetLowestFCostNode(List<Node> list)
    {
        Node lowest = list[0];
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].fCost < lowest.fCost)
            {
                lowest = list[i];
            }
        }
        return lowest;
    }

    private static List<Node> ReconstructPath(Node goal)
    {
        List<Node> path = new List<Node>();
        Node current = goal;
        while (current != null)
        {
            path.Add(current);
            current = current.parent;
        }
        path.Reverse();
        return path;
    }
}

