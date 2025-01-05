using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject vehiclePrefab; // Drag your sphere prefab here
    [SerializeField] private int numberOfVehicles = 40;
    [SerializeField] public Transform[] waypoints;    
    [SerializeField] private RoadGenerator roadGenerator;

    private void Start()
    {
        // Wait for RoadGenerator to finish generating waypoints
        if (roadGenerator != null)
        {
            waypoints = roadGenerator.GetComponentsInChildren<Transform>()
                .Where(t => t.GetComponent<Waypoint>() != null)
                .ToArray();
        }
        SpawnVehicles();
    }

    private void SpawnVehicles()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("No waypoints available for spawning vehicles!");
            return;
        }

        for (int i = 0; i < numberOfVehicles; i++)
        {
            // Pick a random waypoint for spawning
            int randomWaypointIndex = Random.Range(0, waypoints.Length);
            Vector3 spawnPosition = waypoints[randomWaypointIndex].position;
            
            // Add small random offset to prevent vehicles spawning exactly on top of each other
            spawnPosition += new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));

            // Spawn the vehicle
            GameObject vehicle = Instantiate(vehiclePrefab, spawnPosition, Quaternion.identity);
            vehicle.tag = "Vehicle";
        }
    }
}