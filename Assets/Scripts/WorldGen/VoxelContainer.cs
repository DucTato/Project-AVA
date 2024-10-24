using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class VoxelContainer : MonoBehaviour
{
    // public static Voxel emptyVoxel = new Voxel() { voxID = 0 };
    public Vector3 rootPosition;

    public MeshData meshData;
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    

    //public NoiseBuffer dictionaryData;
    public void voxelInitialize(Material mat, Vector3 pos)
    {
        // Assigning a material and a position for the voxel position 
        ComponentConfig();
        // Using a dictionary means less iterating over empty data
        meshData = new MeshData();
        meshData.Initialize();
        meshRenderer.sharedMaterial = mat;
        rootPosition = pos;
    }
    #region Voxel accessor - UNUSED
    //public Voxel this[Vector3 index]
    //{
    //    get
    //    {
    //        return dictionaryData[index];
    //    }
    //    set
    //    {
    //        dictionaryData[index] = value;
    //    }
    //}
    #endregion
    private void ComponentConfig()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();
    }
    //public bool checkVoxelIsSolid(Vector3 point)
    //{
    //    if (point.y < 0 || (point.x > WorldManager.WorldSettings.containerSize + 2) || (point.z > WorldManager.WorldSettings.containerSize + 2)) return true;
    //    else return this[point].isSolid;

    //}

    #region Mesh Data
    [System.Serializable]
    public class MeshData 
    {
        public int[] indices;
        public Vector3[] vertices;
        public Color[] colors;
        public Mesh mesh;
        public int arraySize;
        public void Initialize()
        {
            int maxTris = (WorldManager.WorldSettings.containerSize * WorldManager.WorldSettings.containerSize * WorldManager.WorldSettings.maxHeight) / 4;
            arraySize = maxTris * 3;
            mesh = new Mesh();

            vertices = new Vector3[arraySize];
            colors = new Color[arraySize];
            indices= new int[arraySize];
        }
        public void ClearData()
        {
            mesh.Clear();
            Destroy(mesh);
            mesh = null;
        }
    }
    #endregion
   
    
    public void UploadMesh(MeshBuffer meshBuffer)
    {
        
        if (meshRenderer == null) ComponentConfig();
        // Get the count of vertices and tris from the shader
        int[] faceCount = new int[2] { 0, 0 };
        meshBuffer.countBuffer.GetData(faceCount);
        // Get all of the meshData from the buffers to local arrays
        meshBuffer.vertexBuffer.GetData(meshData.vertices, 0, 0, faceCount[0]);
        meshBuffer.indexBuffer.GetData(meshData.indices, 0, 0, faceCount[0]);
        meshBuffer.colorBuffer.GetData(meshData.colors, 0, 0, faceCount[0]);
       
        // Assign the mesh
        meshData.mesh = new Mesh();
        meshData.mesh.SetVertices(meshData.vertices, 0, faceCount[0]);
        meshData.mesh.SetIndices(meshData.indices, 0, faceCount[0], MeshTopology.Triangles, 0);
        meshData.mesh.SetColors(meshData.colors, 0, faceCount[0]);
        meshData.mesh.RecalculateBounds();
        meshData.mesh.RecalculateNormals();
        meshData.mesh.Optimize();
        meshData.mesh.UploadMeshData(true);
        meshFilter.sharedMesh = meshData.mesh;
        meshCollider.sharedMesh = meshData.mesh;
        if (!gameObject.activeInHierarchy)  gameObject.SetActive(true);
    }
    public void Dispose()
    {
        meshData.ClearData();
        meshData.indices = null;
        meshData.colors = null;
        meshData.vertices = null;
    }
    public void ClearData()
    {
        meshData.ClearData();
        meshFilter.sharedMesh = null;
        meshCollider.sharedMesh = null;
    }
}
