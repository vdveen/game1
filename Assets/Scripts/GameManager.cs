using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject vehiclePrefab; // Drag your sphere prefab here
    [SerializeField] private int numberOfVehicles = 40;
    [SerializeField] private float spacingDistance = 1.2f;
    [SerializeField] public int numberOfNodes = 25;
    [SerializeField] public Transform[] waypoints;

    private void Start()
    {
        SpawnVehicles();
        SpawnNodes();
    }

    private void SpawnVehicles()
    {
        // We'll align the spheres along the Z-axis "behind" each other
        Vector3 spawnPosition = Vector3.zero;

        for (int i = 0; i < numberOfVehicles; i++)
        {
            GameObject vehicle = Instantiate(vehiclePrefab, spawnPosition, Quaternion.identity);
            // Set a tag so the raycast can detect this
            vehicle.tag = "Vehicle"; //kan ook in de prefab
            
            // Offset the spawn position so that each new sphere is behind the previous one
            spawnPosition += Vector3.back * spacingDistance;
        }
    }
    
    // Create a random set of nodes
    private void SpawnNodes()
    {
        // Initialize the array with the specified size
        waypoints = new Transform[numberOfNodes];

        // Create GameObjects and add them to the waypoints array
        for (int i = 0; i < numberOfNodes; i++)
        {
            // Create a new GameObject
            GameObject waypointObject = new GameObject($"Waypoint_{i}");
            
            // Set random position
            waypointObject.transform.position = new Vector3(Random.Range(-50f, 50f), 0f, Random.Range(-50f, 50f));

            // Add the Transform to the waypoints array
            waypoints[i] = waypointObject.transform;
        }
    }
}