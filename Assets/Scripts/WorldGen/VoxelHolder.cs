using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class VoxelHolder : MonoBehaviour
{
    public static Voxel emptyVoxel = new Voxel() { voxID = 0 };
    public Vector3 voxelPosition;
    private MeshRenderer meshRender;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshData meshData = new MeshData();
    private Dictionary<Vector3, Voxel> dictionaryData;
    public void voxelInitialize(Material mat, Vector3 pos)
    {
        // Assigning a material and a position for the voxel position 
        ComponentConfig();
        // Using a dictionary means less iterating over empty data
        dictionaryData = new Dictionary<Vector3, Voxel>();
        meshRender.sharedMaterial = mat;
        voxelPosition = pos;
    }
    public Voxel this[Vector3 index]
    {
        get
        {
            if (dictionaryData.ContainsKey(index)) return dictionaryData[index];
            else return emptyVoxel;
        }
        set
        {
            if (dictionaryData.ContainsKey(index)) dictionaryData[index] = value;
            else dictionaryData.Add(index, value);
        }
    }
    public void ClearDictionary()
    {
        dictionaryData.Clear();
    }

    private void ComponentConfig()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRender = GetComponent<MeshRenderer>();
    }

    #region Mesh Data
    public struct MeshData 
    {
        public Mesh mesh;
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector2> uvs;

        public bool initialized;
        public void ClearData()
        {
            if (!initialized)
            {
                vertices = new List<Vector3>();
                triangles = new List<int>();
                uvs = new List<Vector2>();
                initialized = true;
                mesh = new Mesh();
                Debug.Log("Initialization Done");
            }
            else
            {
                vertices.Clear();
                triangles.Clear();
                uvs.Clear();
                mesh.Clear();
            }
        }
        public void UpdateMesh(bool sharedVertices = false)
        {
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, false);
            mesh.SetUVs(0, uvs);
            mesh.Optimize();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
        }
    }
    #endregion

    #region Voxel statics
    static readonly Vector3[] voxelVertices = new Vector3[8]
    {
        // A voxel consists of 8 vertices
        new Vector3(0, 0, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, 1, 0),
        new Vector3(1, 1, 0),

        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(0, 1, 1),
        new Vector3(1, 1, 1),
    };
    static readonly Vector3[] voxelFaceChecks = new Vector3[6]
    {
        new Vector3(0, 0, -1),  // Back
        new Vector3(0, 0, 1),   // Front
        new Vector3(-1, 0, 0),  // Left
        new Vector3(1, 0, 0),   // Right
        new Vector3(0, -1, 0),  // Bottom
        new Vector3(0, 1, 0),   // Top
    };
    static readonly int[,] voxelVertexIndexes = new int[6, 4]
    {
        // A voxel has 6 faces with 4 vertices each
        {0, 1, 2, 3},
        {4, 5, 6, 7},
        {4, 0, 6, 2},
        {5, 1, 7, 3},
        {0, 1, 4, 5},
        {2, 3, 6, 7},
    };

    static readonly Vector2[] voxelUVs = new Vector2[4]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 1),
    };

    static readonly int[,] voxelTris = new int[6, 6]
    {
        {0, 2, 3, 0, 3, 1},
        {0, 1, 2, 1, 3, 2},
        {0, 2, 3, 0, 3, 1},
        {0, 1, 2, 1, 3, 2},
        {0, 1, 2, 1, 3, 2},
        {0, 2, 3, 0, 3, 1},
    };
    #endregion
    public void GenerateMesh()
    {
        // Clearing all the data to make sure that we start with a blank state
        meshData.ClearData();
        Vector3 blockPos;
        Voxel block;


        int counter = 0;
        Vector3[] faceVertices = new Vector3[4];
        Vector2[] faceUVs = new Vector2[4];

        foreach(KeyValuePair<Vector3, Voxel> kvp in dictionaryData)
        {
            if (kvp.Value.voxID == 0) continue;

            blockPos = kvp.Key;
            block = kvp.Value;
            // Iterating over each face direction
            for (int i = 0; i < 6; i++)
            {
                if (this[blockPos + voxelFaceChecks[i]].isSolid) continue;  // if this face is facing a solid cube, don't draw
                // Drawing this face

                // Collecting the appropriate vertices from the default vertices and add the block position
                for (int j = 0; j < 4; j++)
                {
                    faceVertices[j] = voxelVertices[voxelVertexIndexes[i, j]] + blockPos;
                    faceUVs[j] = voxelUVs[j];
                }
                for (int j = 0; j < 6; j++)
                {
                    meshData.vertices.Add(faceVertices[voxelTris[i, j]]);
                    meshData.uvs.Add(faceUVs[voxelTris[i, j]]);

                    meshData.triangles.Add(counter++);
                }
            }
            //Debug.Log("Mesh Generated");
        }
        
    }
    public void UploadMesh()
    {
        meshData.UpdateMesh();
        if (meshRender == null) ComponentConfig();

        meshFilter.mesh = meshData.mesh;
        if (meshData.vertices.Count > 3)
        {
            meshCollider.sharedMesh = meshData.mesh;
        }
    }
}
