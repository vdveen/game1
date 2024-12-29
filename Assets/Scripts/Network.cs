using UnityEngine;

// Represents a single junction or intersection in your road network
public class Node
{
    public Vector3 position;
    /*
    public List<Edge> edges = new List<Edge>();

    // For A*
    public float gCost;       // Distance from the start node
    public float hCost;       // Heuristic estimate to the end node
    public Node parent;       // Used for path reconstruction

    public float fCost => gCost + hCost; // Convenience property
    */
}

// Represents a directed or undirected road connection between two nodes
public class Edge
{
    public Node startNode;
    public Node endNode;
    /*
    public float weight;  // Cost of traveling along this edge (distance, time, etc.)
    public bool isOneWay;
    */
}

