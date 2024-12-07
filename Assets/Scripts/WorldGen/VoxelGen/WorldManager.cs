using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Profiling;
using UnityEditor;
using System;

public class WorldManager : MonoBehaviour
{
    public VoxelColor[] worldColors;
    public Material worldMaterial;
    [SerializeField]
    private WorldSettings worldSettings;

    public Transform mainCam;
    private Vector3 lastUpdatedPos;
    private Vector3 previouslyCheckedPos;
    // Contains all modified voxels, structures, etc...
    //public ConcurrentDictionary<Vector3, Dictionary<Vector3, Voxel>> modifiedVoxel = new ConcurrentDictionary<Vector3, Dictionary<Vector3, Voxel>>();
    public ConcurrentDictionary<Vector3, VoxelContainer> activeHolders;
    public Queue<VoxelContainer> containerPool;
    ConcurrentQueue<Vector3> containersNeedCreation = new ConcurrentQueue<Vector3>();
    ConcurrentQueue<Vector3> deactiveContainers = new ConcurrentQueue<Vector3>();
    public int maxChunks2ProcessPerFrame = 6;
    public int mainThreadID;
    private Thread checkActiveChunks;
    private bool killThreads = false;
    private bool performedFirstPass = false;
    //
    public static WorldManager Instance
    {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<WorldManager>();
            return _instance;
        }
    }
    public static WorldSettings WorldSettings;
    private static WorldManager _instance;
    public static Vector3 Position2ChunkCoord(Vector3 pos)
    {
        pos /= WorldSettings.containerSize;
        pos = math.floor(pos) * WorldSettings.containerSize;
        pos.y = 0;
        return pos;
    }
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
        //GameObject temp = new GameObject("Voxel Root");
        //temp.transform.parent = transform;
        //root = temp.AddComponent<VoxelHolder>();
        //root.voxelInitialize(worldMaterial, Vector3.zero);
        //ComputeManager.Instance.GenerateVoxelData(ref root);
        InitializeWorldProcedure();
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCam?.transform.position != lastUpdatedPos)
        {
            // Update position for CheckActiveChunksLoop
            lastUpdatedPos = Position2ChunkCoord(mainCam.transform.position);
        }

        Vector3 cont2Make;

        while (deactiveContainers.Count > 0 && deactiveContainers.TryDequeue(out cont2Make))
        {
            DeactiveContainer(cont2Make);
        }
        for (int x = 0; x < maxChunks2ProcessPerFrame; x++)
        {
            if (x < maxChunks2ProcessPerFrame && containersNeedCreation.Count > 0 && containersNeedCreation.TryDequeue(out cont2Make))
            {
                VoxelContainer cont = GetContainer(cont2Make);
                cont.rootPosition = cont2Make;
                activeHolders.TryAdd(cont2Make, cont);
                ComputeManager.Instance.GenerateVoxelData(cont, cont2Make);
                x++;
            }
        }
    }
    private void InitializeWorldProcedure()
    {
        WorldSettings = worldSettings;
        int renderSizePlusExcess = WorldSettings.renderDistance + 3;
        int totalContainers = renderSizePlusExcess * renderSizePlusExcess;
        ComputeManager.Instance.Initialize(maxChunks2ProcessPerFrame * 3);
        activeHolders = new ConcurrentDictionary<Vector3, VoxelContainer>();
        containerPool = new Queue<VoxelContainer>();

        mainThreadID = Thread.CurrentThread.ManagedThreadId;
        for (int i = 0; i < totalContainers; i++)
        {
            GenerateContainer(Vector3.zero, true);
        }
        checkActiveChunks = new Thread(CheckActiveChunksLoop);
        checkActiveChunks.Priority = System.Threading.ThreadPriority.BelowNormal;
        checkActiveChunks.Start();
    }
    private void CheckActiveChunksLoop()
    {
        Profiler.BeginThreadProfiling("Chunks", "ChunkChecker");
        int halfRenderSize = WorldSettings.renderDistance / 2;
        int renderDistancePlusOne = WorldSettings.renderDistance + 1;
        Vector3 pos = Vector3.zero;

        Bounds chunkBounds = new Bounds();
        chunkBounds.size = new Vector3(renderDistancePlusOne * WorldSettings.containerSize, 1, renderDistancePlusOne * WorldSettings.containerSize);
        while (true && !killThreads)
        {
            if (previouslyCheckedPos != lastUpdatedPos || !performedFirstPass)
            {
                previouslyCheckedPos = lastUpdatedPos;
                for (int x = -halfRenderSize; x < halfRenderSize; x++)
                    for (int z = -halfRenderSize; z < halfRenderSize; z++)
                    {
                        pos.x = x * WorldSettings.containerSize + previouslyCheckedPos.x;
                        pos.z = z * WorldSettings.containerSize + previouslyCheckedPos.z;

                        if (!activeHolders.ContainsKey(pos)) containersNeedCreation.Enqueue(pos);
                    }
                chunkBounds.center = previouslyCheckedPos;
                foreach (var kvp in activeHolders)
                {
                    if (!chunkBounds.Contains(kvp.Key)) deactiveContainers.Enqueue(kvp.Key);
                }
            }
            if (!performedFirstPass) performedFirstPass = true;
            Thread.Sleep(300);
        } 
        Profiler.EndThreadProfiling();
    }
    #region Container Pooling
    private VoxelContainer GetContainer(Vector3 pos)
    {
        if (containerPool.Count > 0) return containerPool.Dequeue();
        else return GenerateContainer(pos, false);
    }
    private VoxelContainer GenerateContainer(Vector3 pos, bool enqueue = true)
    {
        if (Thread.CurrentThread.ManagedThreadId != mainThreadID)
        {
            containersNeedCreation.Enqueue(pos);
            return null;
        }
        VoxelContainer cont = new GameObject().AddComponent<VoxelContainer>();
        cont.transform.parent = transform;
        cont.rootPosition = pos;
        cont.voxelInitialize(worldMaterial, pos);
        if (enqueue)
        {
            cont.gameObject.SetActive(false);    
            containerPool.Enqueue(cont);
        }
        return cont;
    }
    private bool DeactiveContainer(Vector3 pos)
    {
        if (activeHolders.ContainsKey(pos))
        {
            if (activeHolders.TryRemove(pos, out VoxelContainer cont))
            {
                cont.ClearData();
                containerPool.Enqueue(cont);
                cont.gameObject.SetActive(false);
                return true;
            }
            else return false;
        }
        return false;
    }
    #endregion
    private void OnApplicationQuit()
    {
        killThreads = true;
        checkActiveChunks?.Abort();
        foreach (var cont in activeHolders.Keys)
        {
            if (activeHolders.TryRemove(cont, out var c))
            {
                c.Dispose();
            }
        }
        // Force cleanup editor memmory
        //#region UNITY_EDITOR
        //EditorUtility.UnloadUnusedAssetsImmediate();
        //GC.Collect();
        //#endregion
    }
}
[System.Serializable]
public class WorldSettings
{
    public int containerSize = 16;
    public int maxHeight = 128;
    public int renderDistance = 32;
    public int ChunkCount
    {
        get { return (containerSize + 3) * (maxHeight + 1) * (containerSize + 3); }
    }
}
