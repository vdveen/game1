using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject vehiclePrefab; 
    [SerializeField] private int numberOfVehicles = 40;
    [SerializeField] public Transform[] waypoints;    
    [SerializeField] private RoadGenerator roadGenerator;
    [SerializeField] private RoadNetwork network; //added
    public Transform vehicleContainer;

    private void Start()
    {
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        // Wait one frame to ensure RoadGenerator's Start has run
        yield return null;

        if (roadGenerator != null)
        {
            // Try to get waypoints directly from RoadGenerator's list
            waypoints = roadGenerator.waypoints.Select(w => w.transform).ToArray();

            // If no waypoints found, try finding them in the container
            if (waypoints == null || waypoints.Length == 0)
            {
                waypoints = roadGenerator.waypointContainer.GetComponentsInChildren<Transform>()
                    .Where(t => t.GetComponent<Waypoint>() != null)
                    .ToArray();
            }

            // Double check we have waypoints
            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogError("No waypoints found in RoadGenerator!");
                yield break;
            }

            // Then generate the network
            network.BuildNetworkFromWaypoints(roadGenerator.waypoints); 
            
            SpawnVehicles();
        }
        else
        {
            Debug.LogError("RoadGenerator reference not set in GameManager!");
        }

    }

    private void SpawnVehicles()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError("No waypoints available for spawning vehicles!");
            return;
        }

        vehicleContainer = new GameObject("Vehicles").transform;
        vehicleContainer.SetParent(transform);

        for (int i = 0; i < numberOfVehicles; i++)
        {
            // Pick a random waypoint for spawning
            int randomWaypointIndex = Random.Range(0, waypoints.Length);
            Transform spawnWaypoint = waypoints[randomWaypointIndex];
            Vector3 spawnPosition = spawnWaypoint.position;
            
            // Add small random offset to prevent vehicles spawning exactly on top of each other
            spawnPosition += new Vector3(Random.Range(-0.5f, 0.5f), 0f, Random.Range(-0.5f, 0.5f));

            // Spawn the vehicle
            GameObject vehicle = Instantiate(vehiclePrefab, spawnPosition, Quaternion.identity, vehicleContainer);
            vehicle.tag = "Vehicle";
            
            // Set the initial waypoint
            VehicleController controller = vehicle.GetComponent<VehicleController>();
            if (controller != null)
            {
                controller.SetInitialWaypoint(spawnWaypoint.GetComponent<Waypoint>());
            }
        }
    }
}