using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeManager : MonoBehaviour
{
    public ComputeShader noiseShader;
    public ComputeShader voxelShader;
    // Object pools
    private List<NoiseBuffer> allNoiseComputeBuffer = new List<NoiseBuffer>();  
    private Queue<NoiseBuffer> availableNoiseComputeBuffer = new Queue<NoiseBuffer>();
    private List<MeshBuffer> allMeshComputeBuffers = new List<MeshBuffer>();
    private Queue<MeshBuffer> availableMeshComputeBuffer = new Queue<MeshBuffer>();

    ComputeBuffer noiseLayersArray;
    ComputeBuffer voxelColorsArray;

    [Header("Noise Settings")]
    public int seed;
    public NoiseLayers[] noiseLayers;

    static float ColorfTo32 (Color32 color)
    {
        if (color.r == 0) color.r = 1;
        if (color.g == 0) color.g = 1;
        if (color.b == 0) color.b = 1;
        if (color.a == 0) color.a = 1;
        return (color.r << 24) | (color.g << 16) | (color.b << 8) | (color.a);
    }
    private int xThreads;
    private int yThreads;
    public int numberOMeshBuffers = 0;

    private static ComputeManager instance;
    public static ComputeManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<ComputeManager>();
            return instance;
        }
    }
    public void Initialize(int count = 18)
    {
        //Debug.Log("WorldSettings: " + WorldManager.WorldSettings.containerSize + " " + WorldManager.WorldSettings.maxHeight);
        xThreads = WorldManager.WorldSettings.containerSize / 8 + 1;
        yThreads = WorldManager.WorldSettings.maxHeight / 8;

        noiseLayersArray = new ComputeBuffer(noiseLayers.Length, 36);
        noiseLayersArray.SetData(noiseLayers);
        noiseShader.SetInt("containerSizeX", WorldManager.WorldSettings.containerSize);
        noiseShader.SetInt("containerSizeY", WorldManager.WorldSettings.maxHeight);

        noiseShader.SetBool("generateCaves", true);
        noiseShader.SetBool("forceFloor", true);

        noiseShader.SetInt("maxHeight", WorldManager.WorldSettings.maxHeight);
        noiseShader.SetInt("oceanHeight", 42);
        noiseShader.SetInt("seed", seed);

        noiseShader.SetBuffer(0, "noiseArray", noiseLayersArray);
        noiseShader.SetInt("noiseCount", noiseLayers.Length);

        VoxelColor32[] converted = new VoxelColor32[WorldManager.Instance.worldColors.Length];
        int colorCount = 0;
        foreach (VoxelColor c in WorldManager.Instance.worldColors)
        {
            VoxelColor32 temp = new VoxelColor32();
            temp.colorValue = ColorfTo32(c.colorValue);
            temp.smoothness = c.smoothness;
            temp.metallic = c.metallic;
            converted[colorCount++] = temp;
        }
        voxelColorsArray = new ComputeBuffer(converted.Length, 12);
        voxelColorsArray.SetData(converted);

        voxelShader.SetBuffer(0, "voxelColors", voxelColorsArray);
        voxelShader.SetInt("containerSizeX", WorldManager.WorldSettings.containerSize);
        voxelShader.SetInt("containerSizeY", WorldManager.WorldSettings.maxHeight);

        for (int i = 0; i < count; i++)
        {
            CreateNewNoiseBuffer();
            CreateNewMeshBuffer();
        }
    }
    
    #region Pooling
    /// <summary>
    /// This is object pool for noise buffers
    /// </summary>
    /// <returns></returns>
    public NoiseBuffer GetNoiseBuffer()
    {
        //Debug.Log("Noise Buffer Called");
        if (availableNoiseComputeBuffer.Count > 0) return availableNoiseComputeBuffer.Dequeue();
        else return CreateNewNoiseBuffer(false);
    }
    public NoiseBuffer CreateNewNoiseBuffer(bool enqueue = true)
    {
        NoiseBuffer buffer = new NoiseBuffer();
        buffer.InitializeBuffer();
        allNoiseComputeBuffer.Add(buffer);
        if (enqueue)    availableNoiseComputeBuffer.Enqueue(buffer);
        return buffer;
    }
    public void ClearAndRequeueBuffer(NoiseBuffer buffer)
    {
        ClearVoxelData(buffer);
        availableNoiseComputeBuffer.Enqueue(buffer);
    }
    /// <summary>
    /// This is object pool for mesh buffers
    /// </summary>
    /// <returns></returns>
    public MeshBuffer CreateNewMeshBuffer(bool enqueue = true)
    {
        MeshBuffer buffer = new MeshBuffer();
        buffer.InitializeBuffer();
        allMeshComputeBuffers.Add(buffer);
        if (enqueue) availableMeshComputeBuffer.Enqueue(buffer);
        numberOMeshBuffers++;
        return buffer;
    }
    public MeshBuffer GetMeshBuffer()
    {
        if (availableMeshComputeBuffer.Count > 0) return availableMeshComputeBuffer.Dequeue();
        else
        {
            Debug.Log("New mesh buffer");
            return CreateNewMeshBuffer(false);
        }
    }
    public void ClearAndRequeueBuffer(MeshBuffer buffer)
    {
        availableMeshComputeBuffer.Enqueue(buffer);
    }
    #endregion
    #region Compute Helpers
    // Used to dispatch the shader
    public void GenerateVoxelData(VoxelContainer cont, Vector3 pos)
    {
        NoiseBuffer noiseBuffer = GetNoiseBuffer();
        noiseBuffer.countBuffer.SetCounterValue(0);
        noiseBuffer.countBuffer.SetData(new uint[] { 0 });
        noiseShader.SetBuffer(0, "voxelArray", noiseBuffer.noiseBuffer);
        noiseShader.SetBuffer(0, "count", noiseBuffer.countBuffer);

        noiseShader.SetVector("chunkPosition", cont.rootPosition);
        noiseShader.SetVector("seedOffset", Vector3.zero);

        noiseShader.Dispatch(0, xThreads, yThreads, xThreads);

        MeshBuffer meshBuffer = GetMeshBuffer();
        meshBuffer.countBuffer.SetCounterValue(0);
        meshBuffer.countBuffer.SetData(new uint[] { 0, 0 });
        voxelShader.SetVector("chunkPosition", cont.rootPosition);
        voxelShader.SetBuffer(0, "voxelArray", noiseBuffer.noiseBuffer);
        voxelShader.SetBuffer(0, "counter", meshBuffer.countBuffer);

        voxelShader.SetBuffer(0, "vertexBuffer", meshBuffer.vertexBuffer);
        voxelShader.SetBuffer(0, "indexBuffer", meshBuffer.indexBuffer);
        voxelShader.SetBuffer(0, "colorBuffer", meshBuffer.colorBuffer);

        voxelShader.Dispatch(0, xThreads, yThreads,xThreads);

        AsyncGPUReadback.Request(meshBuffer.countBuffer, (callback) =>
        {
            if (WorldManager.Instance.activeHolders.ContainsKey(pos))
            {
                WorldManager.Instance.activeHolders[pos].UploadMesh(meshBuffer);
            }
            ClearAndRequeueBuffer(noiseBuffer);
            ClearAndRequeueBuffer(meshBuffer);
        });
    }
    private void ClearVoxelData(NoiseBuffer buffer)
    {
        buffer.countBuffer.SetData(new int[] { 0 });
        noiseShader.SetBuffer(1, "voxelArray", buffer.noiseBuffer);
        noiseShader.Dispatch(1, xThreads, yThreads, xThreads);
    }
    #endregion
   
    private void OnApplicationQuit()
    {
        DisposeAllBuffer();
    }
    private void DisposeAllBuffer()
    {
        noiseLayersArray?.Dispose();
        voxelColorsArray?.Dispose();
        foreach (NoiseBuffer buffer in allNoiseComputeBuffer)
            buffer.Dispose();
        
        foreach (MeshBuffer buffer in allMeshComputeBuffers)
            buffer.Dispose();
    }
}
#region Noise Buffer
public struct NoiseBuffer
{
    public ComputeBuffer noiseBuffer;
    public ComputeBuffer countBuffer;
    public bool initialized;
    public bool cleared;
    public IndexedArray<Voxel> voxelArray;

    public void InitializeBuffer()
    {
        countBuffer = new ComputeBuffer(1, 4, ComputeBufferType.Counter);
        countBuffer.SetCounterValue(0);
        countBuffer.SetData(new uint[] { 0 });

        //voxelArray = new IndexedArray<Voxel>();
        noiseBuffer = new ComputeBuffer(WorldManager.WorldSettings.ChunkCount, 4);
        //noiseBuffer.SetData(voxelArray.GetData);
        initialized = true;
    }
    public void Dispose()
    {
        countBuffer?.Dispose();
        noiseBuffer?.Dispose();
        initialized = false;
    }
    //public Voxel this[Vector3 index]
    //{
    //    get
    //    {
    //        return voxelArray[index];
    //    }
    //    set
    //    {
    //        voxelArray[index] = value;
    //    }
    //}
}
#endregion
#region Mesh Buffer
public struct MeshBuffer
{
    public ComputeBuffer countBuffer;
    public ComputeBuffer indexBuffer;
    public ComputeBuffer colorBuffer;
    public ComputeBuffer vertexBuffer;

    public bool initialized;
    public bool cleared;

    public void InitializeBuffer()
    {
        if (initialized) return;

        countBuffer = new ComputeBuffer(2, 4, ComputeBufferType.Counter);
        countBuffer.SetCounterValue(0);
        countBuffer.SetData(new uint[] {0, 0});
        // Width * Height * Width * Faces * Tris
        int maxTris = WorldManager.WorldSettings.containerSize * WorldManager.WorldSettings.containerSize * WorldManager.WorldSettings.maxHeight / 4;
        vertexBuffer ??= new ComputeBuffer(maxTris * 3, 12);
        indexBuffer ??= new ComputeBuffer(maxTris * 3, 4);
        colorBuffer ??= new ComputeBuffer(maxTris * 3, 16);
        initialized = true;
    }
    public void Dispose()
    {
        vertexBuffer?.Dispose();
        indexBuffer?.Dispose();
        colorBuffer?.Dispose();
        countBuffer?.Dispose();
        initialized = false;
    }
}
#endregion
