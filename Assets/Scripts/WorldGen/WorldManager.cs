using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public Material worldMaterial;
    private VoxelRenderer container;
    // Start is called before the first frame update
    void Start()
    {
        GameObject temp = new GameObject("Voxel Renderer");
        temp.transform.parent = transform;
        container = temp.AddComponent<VoxelRenderer>();

        container.voxelInitialize(worldMaterial, Vector3.zero);
        container.GenerateMesh();
        container.UploadMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
