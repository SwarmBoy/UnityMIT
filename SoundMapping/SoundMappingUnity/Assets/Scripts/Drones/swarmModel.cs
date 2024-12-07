using System.Collections;
using System.Collections.Generic;
using FischlWorks_FogWar;
using Unity.VisualScripting;
using UnityEngine;

public class swarmModel : MonoBehaviour
{
    public static GameObject swarmHolder;
    public GameObject dronePrefab;
    public int numDrones = 10;
    public float spawnRadius = 10f;
    public static float spawnHeight = 10f;

    public float lastObstacleAvoidance = -1f;

    public csFogWar fogWar;

    void Awake()
    {
        spawn();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            spawn();
            this.GetComponent<Timer>().Restart();
        }
    }

    void spawn()
    {
        fogWar.ResetMapAndFogRevealers();

        swarmHolder = GameObject.FindGameObjectWithTag("Swarm");
        //kill all drones
        foreach (Transform child in swarmHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        for (int i = 0; i < numDrones; i++)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-spawnRadius, spawnRadius), spawnHeight, Random.Range(-1, 1));
            GameObject drone = Instantiate(dronePrefab, spawnPosition, Quaternion.identity);
            drone.transform.parent = swarmHolder.transform;
            drone.name = "Drone"+i.ToString();

            fogWar.AddFogRevealer(drone.transform, 5, true);
        }

        this.GetComponent<HapticAudioManager>().Reset();
        this.GetComponent<DroneNetworkManager>().Reset();
    }

    void Start()
    {
        this.GetComponent<sendInfoGameObject>().setupCallback(getAverageCohesion);
        this.GetComponent<sendInfoGameObject>().setupCallback(getAverageAlignment);
        this.GetComponent<sendInfoGameObject>().setupCallback(getAverageSeparation);
        this.GetComponent<sendInfoGameObject>().setupCallback(getAverageMigration);
        this.GetComponent<sendInfoGameObject>().setupCallback(getAverageObstacleAvoidance);
        this.GetComponent<sendInfoGameObject>().setupCallback(getDeltaAverageObstacle);
    }

    public void RemoveDrone(GameObject drone)
    {
        if (drone.transform.parent == swarmHolder.transform)
        {
            drone.gameObject.SetActive(false);
            drone.transform.parent = null;
            //this.GetComponent<CameraMovement>().resetFogExplorers();
        }

        this.GetComponent<Timer>().DroneDiedCallback();

        if (swarmHolder.transform.childCount == 0)
        {
            this.GetComponent<Timer>().Restart();
            spawn();
        }
    }

    DataEntry getAverageCohesion()
    {
        Vector3 averageCohesion = Vector3.zero;
        int numDrones = 0;

        foreach (Transform drone in swarmHolder.transform)
        {
            averageCohesion += drone.GetComponent<DroneController>().cohesionForce;
            numDrones++;
        }

        if (numDrones > 0)
        {
            averageCohesion /= numDrones;
        }

        return new DataEntry("averageCohesion", averageCohesion.magnitude.ToString(), fullHistory: true);
    }

    DataEntry getDeltaAverageObstacle()
    {
        Vector3 averageObstacle = Vector3.zero;
        int numDrones = 0;

        foreach (Transform drone in swarmHolder.transform)
        {
            averageObstacle += drone.GetComponent<DroneController>().obstacleAvoidanceForce;
            numDrones++;
        }

        if (numDrones > 0)
        {
            averageObstacle /= numDrones;
        }




        if(lastObstacleAvoidance < 0)
        {
            lastObstacleAvoidance = averageObstacle.magnitude;
            return new DataEntry("deltaObstacle", "0", fullHistory: true);
        }

        float delta = (averageObstacle.magnitude - lastObstacleAvoidance)*Time.deltaTime;
        lastObstacleAvoidance = averageObstacle.magnitude;


        return new DataEntry("deltaObstacle", delta.ToString(), fullHistory: true);
    }

    DataEntry getAverageAlignment()
    {
        Vector3 averageAlignment = Vector3.zero;
        int numDrones = 0;

        foreach (Transform drone in swarmHolder.transform)
        {
            averageAlignment += drone.GetComponent<DroneController>().alignmentForce;
            numDrones++;
        }

        if (numDrones > 0)
        {
            averageAlignment /= numDrones;
        }

        return new DataEntry("averageAlignment", averageAlignment.magnitude.ToString(), fullHistory: true);
    }

    DataEntry getAverageSeparation()
    {
        Vector3 averageSeparation = Vector3.zero;
        int numDrones = 0;

        foreach (Transform drone in swarmHolder.transform)
        {
            averageSeparation += drone.GetComponent<DroneController>().separationForce;
            numDrones++;
        }

        if (numDrones > 0)
        {
            averageSeparation /= numDrones;
        }

        return new DataEntry("averageSeparation", averageSeparation.magnitude.ToString(), fullHistory: true);
    }

    DataEntry getAverageMigration()
    {
        Vector3 averageMigration = Vector3.zero;
        int numDrones = 0;

        foreach (Transform drone in swarmHolder.transform)
        {
            averageMigration += drone.GetComponent<DroneController>().migrationForce;
            numDrones++;
        }

        if (numDrones > 0)
        {
            averageMigration /= numDrones;
        }

        return new DataEntry("averageMigration", averageMigration.magnitude.ToString(), fullHistory: true);
    }

    DataEntry getAverageObstacleAvoidance()
    {
        Vector3 averageObstacleAvoidance = Vector3.zero;
        int numDrones = 0;

        foreach (Transform drone in swarmHolder.transform)
        {
            averageObstacleAvoidance += drone.GetComponent<DroneController>().obstacleAvoidanceForce;
            numDrones++;
        }

        if (numDrones > 0)
        {
            averageObstacleAvoidance /= numDrones;
        }

        return new DataEntry("averageObstacleAvoidance", averageObstacleAvoidance.magnitude.ToString(), fullHistory: true);
    }

}
