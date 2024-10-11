using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeManager : MonoBehaviour
{
    public ComputeShader noiseShader;
    // Object pools
    private List<NoiseBuffer> allNoiseComputeBuffer = new List<NoiseBuffer>();  
    private Queue<NoiseBuffer> availableNoiseComputeBuffer = new Queue<NoiseBuffer>();
    [SerializeField]
    private int xThreads;
    [SerializeField]
    private int yThreads;
    private static ComputeManager instance;
    public static ComputeManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<ComputeManager>();
            return instance;
        }
    }
    public void Initialize(int count = 256)
    {
        Debug.Log("WorldSettings: " + WorldManager.WorldSettings.containerSize + " " + WorldManager.WorldSettings.maxHeight);
        xThreads = WorldManager.WorldSettings.containerSize / 8 + 1;
        yThreads = WorldManager.WorldSettings.maxHeight / 8;
        noiseShader.SetInt("containerSizeX", WorldManager.WorldSettings.containerSize);
        noiseShader.SetInt("containerSizeY", WorldManager.WorldSettings.maxHeight);

        noiseShader.SetBool("generateCaves", true);
        noiseShader.SetBool("forceFloor", true);

        noiseShader.SetInt("maxHeight", WorldManager.WorldSettings.maxHeight);
        noiseShader.SetInt("oceanHeight", 42);

        noiseShader.SetFloat("noiseScale", 0.004f);
        noiseShader.SetFloat("caveScale", 0.01f);
        noiseShader.SetFloat("caveThreshold", 0.8f);
        noiseShader.SetInt("surfaceVoxelID", 1);
        noiseShader.SetInt("subSurfaceVoxelID", 2);

        for (int i = 0; i < count; i++)
        {
            CreateNewNoiseBuffer();
        }
    }
    #region Noise Buffers
    #region Pooling
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
    #endregion
    #region Compute Helpers
    // Used to dispatch the shader
    public void GenerateVoxelData(ref VoxelHolder holder)
    {
        noiseShader.SetBuffer(0, "voxelArray", holder.dictionaryData.noiseBuffer);
        noiseShader.SetBuffer(0, "count", holder.dictionaryData.countBuffer);

        //noiseShader.SetVector("chunkPosition", holder.holderPosition);
        noiseShader.SetVector("seedOffset", Vector3.zero);

        noiseShader.Dispatch(0, xThreads, yThreads, xThreads);

        AsyncGPUReadback.Request(holder.dictionaryData.noiseBuffer, (callback) =>
        {
            callback.GetData<Voxel>(0).CopyTo(WorldManager.Instance.root.dictionaryData.voxelArray.array);
            WorldManager.Instance.root.RenderMesh();
        });
    }
    private void ClearVoxelData(NoiseBuffer buffer)
    {
        buffer.countBuffer.SetData(new int[] { 0 });
        noiseShader.SetBuffer(1, "voxelArray", buffer.noiseBuffer);
        noiseShader.Dispatch(1, xThreads, yThreads, xThreads);
    }
    #endregion
    #endregion
    private void OnApplicationQuit()
    {
        DisposeAllBuffer();
    }
    private void DisposeAllBuffer()
    {
        foreach (NoiseBuffer buffer in allNoiseComputeBuffer)
        {
            buffer.Dispose();
        }
    }
}
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

        voxelArray = new IndexedArray<Voxel>();
        noiseBuffer = new ComputeBuffer(voxelArray.Count, 4);
        noiseBuffer.SetData(voxelArray.GetData);
        initialized = true;
    }
    public void Dispose()
    {
        countBuffer?.Dispose();
        noiseBuffer?.Dispose();
        initialized = false;
    }
    public Voxel this[Vector3 index]
    {
        get
        {
            return voxelArray[index];
        }
        set
        {
            voxelArray[index] = value;
        }
    }
}
