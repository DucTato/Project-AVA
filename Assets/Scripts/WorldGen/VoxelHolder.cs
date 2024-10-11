using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class VoxelHolder : MonoBehaviour
{
    // public static Voxel emptyVoxel = new Voxel() { voxID = 0 };
    public Vector3 rootPosition;
    private MeshRenderer meshRender;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshData meshData = new MeshData();

    public NoiseBuffer dictionaryData;
    public void voxelInitialize(Material mat, Vector3 pos)
    {
        // Assigning a material and a position for the voxel position 
        ComponentConfig();
        // Using a dictionary means less iterating over empty data
        dictionaryData = ComputeManager.Instance.GetNoiseBuffer();
        meshRender.sharedMaterial = mat;
        rootPosition = pos;
    }
    public Voxel this[Vector3 index]
    {
        get
        {
            return dictionaryData[index];
        }
        set
        {
            dictionaryData[index] = value;
        }
    }
    public void ClearDictionary()
    {
        ComputeManager.Instance.ClearAndRequeueBuffer(dictionaryData);
    }
    public void RenderMesh()
    {
        // Clearing all the data to make sure that we start with a blank state
        meshData.ClearData();
        GenerateMesh();
        UploadMesh();
    }
    private void ComponentConfig()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRender = GetComponent<MeshRenderer>();
    }
    public bool checkVoxelIsSolid(Vector3 point)
    {
        if (point.y < 0 || (point.x > WorldManager.WorldSettings.containerSize + 2) || (point.z > WorldManager.WorldSettings.containerSize + 2)) return true;
        else return this[point].isSolid;

    }

    #region Mesh Data
    public struct MeshData 
    {
        public Mesh mesh;
        public List<Vector3> vertices;
        public List<int> triangles;
        public List<Vector2> uvs;
        public List<Vector2> uvs2;
        public List<Color> colors;

        public bool initialized;
        public void ClearData()
        {
            if (!initialized)
            {
                vertices = new List<Vector3>();
                triangles = new List<int>();
                uvs = new List<Vector2>();
                uvs2 = new List<Vector2>();
                colors = new List<Color>();
                initialized = true;
                mesh = new Mesh();
                //Debug.Log("Initialization Done");
            }
            else
            {
                vertices.Clear();
                triangles.Clear();
                uvs.Clear();
                uvs2.Clear();
                colors.Clear();
                mesh.Clear();
            }
        }
        public void UpdateMesh(bool sharedVertices = false)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0, false);
            mesh.SetUVs(0, uvs);
            mesh.Optimize();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
            // Coloring
            mesh.SetColors(colors);
            mesh.SetUVs(2, uvs2);
            mesh.SetUVs(0, uvs);
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
        
        Vector3 blockPos;
        Voxel block;
        VoxelColor voxelColor;
        Color colorAlphaValue;
        Vector2 voxelSmoothness;

        int counter = 0;
        Vector3[] faceVertices = new Vector3[4];
        Vector2[] faceUVs = new Vector2[4];
        for (int x = 1; x < WorldManager.WorldSettings.containerSize + 1; x++)
        {
            for (int y = 0; y < WorldManager.WorldSettings.maxHeight; y++)
            {
                for (int z = 1; z < WorldManager.WorldSettings.containerSize + 1; z++)
                {
                    //Debug.Log("WorldSettings: " + WorldManager.WorldSettings.containerSize + WorldManager.WorldSettings.maxHeight);
                    blockPos = new Vector3(x, y, z);
                    block = this[blockPos];
                    // Do the check on solid blocks
                    if (!block.isSolid) continue;

                    voxelColor = WorldManager.Instance.worldColors[block.ID - 1];
                    colorAlphaValue = voxelColor.colorValue;
                    colorAlphaValue.a = 1;  // alpha value determines the transparency
                    voxelSmoothness = new Vector2(voxelColor.metallic, voxelColor.smoothness);
                    // Iterating over each face direction
                    for (int i = 0; i < 6; i++)
                    {
                        if (checkVoxelIsSolid(blockPos + voxelFaceChecks[i])) continue;  // if this face is facing a solid cube, don't draw
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
                            // Coloring
                            meshData.uvs2.Add(voxelSmoothness);
                            meshData.colors.Add(colorAlphaValue);
                        }
                    }
                }
            }
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
