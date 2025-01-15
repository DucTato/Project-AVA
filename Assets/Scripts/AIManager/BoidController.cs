using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidController : MonoBehaviour
{
    [SerializeField]
    private Boid boidPrefab, boidPrefab2, boidPrefab3;
    [SerializeField]
    private int spawnBoids = 30;
    [SerializeField]
    private float boidSpeed = 10f;
    [SerializeField]
    private float boidSteeringSpeed = 100f;
    [SerializeField]
    private float boidNoClumpingArea = 10f;
    [SerializeField]
    private float boidLocalArea = 10f;
    [SerializeField]
    private float boidSimulationArea;
    [SerializeField]
    private int boidReloadCount = 5;
    private List<Boid> _boids;
    
    private void Start()
    {
        _boids = new List<Boid>();

        for (int i = 0; i < spawnBoids; i++)
        {
            SpawnBoid(boidPrefab.gameObject, 0);
        }
        for (int i = 0; i < spawnBoids; i++)
        {
            SpawnBoid(boidPrefab2.gameObject, 1);
        }
        for (int i = 0; i < spawnBoids; i++)
        {
            SpawnBoid(boidPrefab3.gameObject, 2);
        }
    }

    private void Update()
    {

        foreach (Boid boid in _boids)
        {
            boid.SimulateMovement(_boids, Time.deltaTime, transform.position);
            //var boidPos = boid.transform.position;

            //if (Vector3.Distance(boidPos, transform.position) > boidSimulationArea)
            //{
                
            //    boidPos = Utilities.SpawnSphereOnEdgeRandomly3D(gameObject, boidSimulationArea);
            //    boidPos.y = Random.Range(transform.position.y - boidSimulationArea, transform.position.y + boidSimulationArea);
            //}

            //boid.transform.position = boidPos;
        }
    }
    public void LaunchBoids(Target target)
    {
        if (_boids.Count == 0 && boidReloadCount > 0) 
        {
            for (int i = 0; i < spawnBoids; i++)
            {
                SpawnBoid(boidPrefab.gameObject, 0);
            }
            for (int i = 0; i < spawnBoids; i++)
            {
                SpawnBoid(boidPrefab2.gameObject, 1);
            }
            for (int i = 0; i < spawnBoids; i++)
            {
                SpawnBoid(boidPrefab3.gameObject, 2);
            }
            // Uses up reserve
            boidReloadCount--;
            return; 
        }
        Debug.Log("Boid launched");
        var index = Random.Range(0, _boids.Count);
        _boids[index].GetComponent<Boid>().Launch(target);
        _boids.RemoveAt(index);
    }
    private void SpawnBoid(GameObject prefab, int swarmIndex)
    {
        var boidInstance = Instantiate(prefab);

        //boidInstance.transform.localPosition += new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10));
        boidInstance.transform.localPosition = Utilities.SpawnSphereOnEdgeRandomly3D(gameObject, boidSimulationArea);
        boidInstance.transform.localRotation = Random.rotation;

        var boid = boidInstance.GetComponent<Boid>();
        boid.SwarmIndex = swarmIndex;
        boid.Speed = boidSpeed;
        boid.SteeringSpeed = boidSteeringSpeed;
        boid.LocalAreaRadius = boidLocalArea;
        boid.NoClumpingRadius = boidNoClumpingArea;

        _boids.Add(boid);
    }
}
