using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public Material worldMaterial;
    private VoxelHolder container;
    // Start is called before the first frame update
    void Start()
    {
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
