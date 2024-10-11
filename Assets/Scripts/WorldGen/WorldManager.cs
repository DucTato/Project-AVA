using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public VoxelColor[] worldColors;
    public Material worldMaterial;
    public static WorldManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<WorldManager>();
            return _instance;
        }
    }
    private static WorldManager _instance;
    private VoxelHolder container;
    // Start is called before the first frame update
    void Start()
    {
        // Check if there're multiple instances of the world manager singleton
        // Only 1 instance of a world manager class is allowed
        if (_instance != null)
        {
            if (_instance != this) Destroy(this);
        }
        else
        {
            _instance = this;
        }
        GameObject temp = new GameObject("Voxel Root");
        temp.transform.parent = transform;
        container = temp.AddComponent<VoxelHolder>();
        container.voxelInitialize(worldMaterial, Vector3.zero);
        for (int x = 0; x < 20; x++)
        {
            for (int z = 0; z < 20; z++)
            {
                int randomYHeight = Random.Range(1, 10);
                for (int y = 0; y < randomYHeight; y++)
                {
                    container[new Vector3(x, y, z)] = new Voxel() { voxID = 1 };
                }
            }
        }
        container.GenerateMesh();
        container.UploadMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
