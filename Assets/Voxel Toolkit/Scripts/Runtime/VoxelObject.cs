using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.Windows;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace VoxelToolkit
{
    /// <summary>
    /// The kind of the generated mesh
    /// </summary>
    public enum MeshKind
    {
        /// <summary>
        /// The mesh is invalid
        /// </summary>
        Invalid,
        /// <summary>
        /// The mesh is for opaque voxels
        /// </summary>
        Opaque,
        /// <summary>
        /// The mesh is for transparent voxels
        /// </summary>
        Transparent
    }

    /// <summary>
    /// The approach used to generate meshes
    /// </summary>
    public enum MeshGenerationApproach
    {
        /// <summary>
        /// No textures used. All data is in the vertices
        /// </summary>
        Textureless,
        /// <summary>
        /// Color data is encoded to texture
        /// </summary>
        Textured
    }
    
    /// <summary>
    /// Describes the generated mesh
    /// </summary>
    public struct MeshDescriptor
    { 
        internal MeshKind kind;
        
        /// <summary>
        /// The resulting mesh
        /// </summary>
        public readonly Mesh Mesh;
        public readonly Texture2D ColorTexture;
        
        /// <summary>
        /// The kind of the mesh
        /// </summary>
        public MeshKind Kind => kind;

        public MeshDescriptor(Mesh mesh)
        {
            Mesh = mesh;
            kind = MeshKind.Invalid;
            ColorTexture = null;
        }
        
        public MeshDescriptor(MeshKind kind, Mesh mesh, Texture2D texture)
        {
            this.kind = kind;
            Mesh = mesh;
            ColorTexture = texture;
        }
    }
    
    /// <summary>
    /// Describes the way origin point will be chosen
    /// </summary>
    public enum OriginMode
    {
        /// <summary>
        /// The center of the model will be an origin
        /// </summary>
        Center,
        /// <summary>
        /// The origin will math the coordinates start
        /// </summary>
        Corner
    }

    /// <summary>
    /// The class responsible for a voxel meshes generation
    /// </summary>
    public class VoxelObject : IDisposable
    {
        private readonly int chunkSize = 16;
        private Chunk emptyChunk;

        private bool isDisposed = false;

        private List<MeshDescriptor> meshes = new List<MeshDescriptor>();

        private Chunk EmptyChunk
        {
            get
            {
                if (!emptyChunk.IsValid)
                    emptyChunk = new Chunk(new NativeArray<Voxel>(chunkSize * chunkSize * chunkSize, Allocator.Persistent));

                return emptyChunk;
            }
        }

        /// <summary>
        /// The count of the generated meshes
        /// </summary>
        public int MeshesCount => meshes.Count;

        private float hueShift = 0.0f;
        private float saturation = 1.3f;
        private float brightness = 0.0f;

        /// <summary>
        /// The hue shift to be applied to the materials
        /// </summary>
        public float HueShift
        {
            get => hueShift;
            set
            {
                if (Mathf.Approximately(hueShift, value))
                    return;

                hueShift = value;
                materialsAreDirty = true;
                MarkDirty();
            }
        }
        /// <summary>
        /// Saturation to be applied to the materials
        /// </summary>
        public float Saturation 
        {
            get => saturation;
            set
            {
                if (Mathf.Approximately(saturation, value))
                    return;

                saturation = value;
                materialsAreDirty = true;
                MarkDirty();
            }
        }
        /// <summary>
        /// Brightness to be applied to the materials
        /// </summary>
        public float Brightness
        {
            get => brightness;
            set
            {
                if (Mathf.Approximately(brightness, value))
                    return;

                brightness = value;
                materialsAreDirty = true;
                MarkDirty();
            }
        }
        
        private struct VertexSet
        {
            public readonly NativeArray<Vertex> Vertices;
            public int VerticesCount;

            public VertexSet(NativeArray<Vertex> vertices, int verticesCount)
            {
                Vertices = vertices;
                VerticesCount = verticesCount;
            }
        }

        private struct Chunk : IDisposable
        {
            public NativeArray<Voxel> Voxels;

            public VertexSet OpaqueVertices;
            public VertexSet TransparentVertices;

            public bool IsDirty;
            
            public int VoxelsCount;

            private bool isValid;

            public bool IsValid => isValid;
            
            public Chunk(NativeArray<Voxel> voxels)
            {
                Voxels = voxels;
                VoxelsCount = 0;

                isValid = true;
                IsDirty = true;
                var verticesCount = voxels.Length * 6 * 4;
                OpaqueVertices = new VertexSet(new NativeArray<Vertex>(verticesCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory), 0);
                TransparentVertices = new VertexSet(new NativeArray<Vertex>(verticesCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory), 0);
            }

            public void Dispose()
            {
                if (!isValid)
                    return;

                Voxels.Dispose();
                OpaqueVertices.Vertices.Dispose();
                TransparentVertices.Vertices.Dispose();
                
                isValid = false;
            }
        }
        
        private struct MeshGenerationEntry
        {
            public readonly NativeArray<Vertex> Vertices;
            public readonly MeshKind MeshKind;
            public readonly int StartIndex;
            public readonly int Count;

            public MeshGenerationEntry(NativeArray<Vertex> vertices, int startIndex, int count, MeshKind meshKind)
            {
                Vertices = vertices;
                StartIndex = startIndex;
                Count = count;
                MeshKind = meshKind;
            }
        }
        
        private struct QuadGenerationJobEntry
        {
            public readonly Vector3Int Location;
            public readonly QuadGenerationJob Job;
            public VertexGenerationJob OpaqueVertexGenerationJob;
            public VertexGenerationJob TransparentVertexGenerationJob;

            public QuadGenerationJobEntry(QuadGenerationJob job, VertexGenerationJob opaqueVertexGenerationJob, VertexGenerationJob transparentVertexGenerationJob, Vector3Int location)
            {
                Job = job;
                OpaqueVertexGenerationJob = opaqueVertexGenerationJob;
                TransparentVertexGenerationJob = transparentVertexGenerationJob;
                Location = location;
            }
        }

        private float scale = 1.0f;
        private float transparentEdgeShift = 0.0f;
        private float opaqueEdgeShift = 0.0f;
        private int3 size;
        private Chunk[][][] chunks;
        private Material[] originalPalette;
        private bool materialsAreDirty = false;
        private NativeArray<TransformedMaterial> palette;
        private MeshGenerationApproach meshGenerationApproach;

        public MeshGenerationApproach MeshGenerationApproach
        {
            get => meshGenerationApproach;
            set
            {
                if (meshGenerationApproach == value)
                    return;
                
                meshGenerationApproach = value;
                
                MarkDirty();
            }
        }
        
        /// <summary>
        /// The scale of the voxel object
        /// </summary>
        public float Scale
        {
            get => scale;
            set
            {
                if (Mathf.Approximately(scale, value))
                    return;

                scale = value;
                MarkDirty();
            }
        }

        /// <summary>
        /// The shift of the edge of the voxel object applied for opaque objects
        /// </summary>
        public float OpaqueEdgeShift
        {
            get => opaqueEdgeShift;
            set
            {
                if (Mathf.Approximately(opaqueEdgeShift, value))
                    return;

                opaqueEdgeShift = value;
                MarkDirty();
            }
        }
        
        /// <summary>
        /// The shift of the edge of the voxel object applied for transparent objects
        /// </summary>
        public float TransparentEdgeShift
        {
            get => transparentEdgeShift;
            set
            {
                if (Mathf.Approximately(transparentEdgeShift, value))
                    return;
                
                transparentEdgeShift = value; 
                MarkDirty();
            }
        }

        /// <summary>
        /// Provides the size of the voxel object
        /// </summary>
        public int3 Size => size;

        /// <summary>
        /// The count of the chunks for the current voxel object
        /// </summary>
        public Vector3Int ChunksCount
        {
            get
            {
                var xChunksCount = Mathf.CeilToInt((float)size.x / chunkSize);
                var yChunksCount = Mathf.CeilToInt((float)size.y / chunkSize);
                var zChunksCount = Mathf.CeilToInt((float)size.z / chunkSize);

                return new Vector3Int(xChunksCount, yChunksCount, zChunksCount);
            }
        }
        
        /// <summary>
        /// Creates the voxel object of the given size and chunks size
        /// </summary>
        /// <param name="size">The size of the voxel object</param>
        /// <param name="palette">The palette to be used for the mesh generation</param>
        /// <param name="chunkSize">The chunk size of the voxel object</param>
        public VoxelObject(Vector3Int size, ReadonlyArray<Material> palette, int chunkSize)
        {
            if (palette.Length > 256)
                throw new ArgumentException($"Max palette size is 256 but provided the one with size of {palette.Length}");

            if (chunkSize <= 0 || chunkSize > 256)
                throw new ArgumentException($"Chunk size should in in range [0, 256] but got {chunkSize}");

            if (size.x <= 0 || size.y <= 0 || size.z <= 0)
                throw new ArgumentException($"All components of the size vector should be greater than zero but got {size}");

            this.originalPalette = palette.ToArray();
            this.chunkSize = chunkSize;
            this.palette = new NativeArray<TransformedMaterial>(palette.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            materialsAreDirty = true;
            
            this.size = new int3(size.x, size.y, size.z);
            var chunkCount = ChunksCount;

            chunks = new Chunk[chunkCount.x][][];
            for (var xIndex = 0; xIndex < chunkCount.x; xIndex++)
            {
                chunks[xIndex] = new Chunk[chunkCount.y][];
                for (var yIndex = 0; yIndex < chunkCount.y; yIndex++)
                    chunks[xIndex][yIndex] = new Chunk[chunkCount.z];
            }
        }
        
        /// <summary>
        /// Creates the voxel object of the given size and chunks size
        /// </summary>
        /// <param name="size">The size of the voxel object</param>
        /// <param name="chunkSize">The chunk size of the voxel object</param>
        public VoxelObject(Vector3Int size, int chunkSize)
        {
            this.chunkSize = chunkSize;
            originalPalette = new Material[256];
            for (var index = 0; index < originalPalette.Length; index++)
                originalPalette[index] = Material.Base;
            
            palette = new NativeArray<TransformedMaterial>(256, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            materialsAreDirty = true;
            
            this.size = new int3(size.x, size.y, size.z);
            var chunkCount = ChunksCount;

            chunks = new Chunk[chunkCount.x][][];
            for (var xIndex = 0; xIndex < chunkCount.x; xIndex++)
            {
                chunks[xIndex] = new Chunk[chunkCount.y][];
                for (var yIndex = 0; yIndex < chunkCount.y; yIndex++)
                    chunks[xIndex][yIndex] = new Chunk[chunkCount.z];
            }
        }

        ~VoxelObject()
        {
            Dispose();
        }

        /// <summary>
        /// Sets the material for the voxel object
        /// </summary>
        /// <param name="index">The index of the material to be set</param>
        /// <param name="material">The material to be set</param>
        public void SetMaterial(byte index, Material material)
        {
            originalPalette[index] = material;
            palette[index] = new TransformedMaterial(material, HueShift, Saturation, Brightness);
            for (var x = 0; x < chunks.Length; x++)
            {
                var xChunks = chunks[x];
                for (var y = 0; y < xChunks.Length; y++)
                {
                    var yChunks = xChunks[y];
                    for (var z = 0; z < yChunks.Length; z++)
                    {
                        var chunk = yChunks[z];
                        if (chunk.VoxelsCount == 0)
                            continue;

                        chunk.IsDirty = true;
                        chunks[x][y][z] = chunk;
                    }
                }
            }
        }

        private void CheckDisposed()
        {
            if (isDisposed)
                throw new ObjectDisposedException("Voxel object is disposed but you are trying to use it");
        }

        /// <summary>
        /// Get access to the generated mesh
        /// </summary>
        /// <param name="index">The index of the mesh to obtain</param>
        /// <returns>Mesh descriptor of the requested mesh</returns>
        public MeshDescriptor GetMesh(int index)
        {
            CheckDisposed();
            return meshes[index];
        }

        /// <summary>
        /// Copies the voxel object from one to another
        /// </summary>
        /// <param name="other">The object to copy to</param>
        public void CopyTo(VoxelObject other)
        {
            CheckDisposed();
            NativeArray<TransformedMaterial>.Copy(palette, other.palette);
            var sizeToIterate = math.min(size, other.size);
            for (var x = 0; x < sizeToIterate.x; x++)
                for (var y = 0; y < sizeToIterate.y; y++)
                    for (var z = 0; z < sizeToIterate.z; z++)
                        other[new Vector3Int(x, y, z)] = this[new Vector3Int(x, y, z)];
        }
        
        /// <summary>
        /// Creates voxel object directly from voxel model
        /// </summary>
        /// <param name="model">The voxel model voxel object to be created from</param>
        /// <param name="palette">The palette used for mesh generation</param>
        /// <param name="chunkSize">The chunk size of the object to be generated</param>
        /// <returns></returns>
        public static VoxelObject CreateFromModel(Model model, ReadonlyArray<Material> palette, int chunkSize)
        {
            var voxelObject = new VoxelObject(model.Size, palette, chunkSize);
            for (var index = 0; index < model.VoxelsCount; index++)
                voxelObject[model[index].Position] = new Voxel(VoxelKind.Solid, (byte)model[index].Material);
            
            return voxelObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (int3 Chunk, int Index, int3 InsideChunkPosition) GetChunkLocation(int3 position)
        {
            var chunkIndex = position / chunkSize;
            var chunkPosition = position - chunkIndex * chunkSize;
            return (chunkIndex, chunkSize * chunkSize * chunkPosition.y + chunkSize * chunkPosition.x + chunkPosition.z, chunkPosition);
        }

        /// <summary>
        /// Checks if the voxel position belongs to the voxel object volume
        /// </summary>
        /// <param name="value">The position to check</param>
        /// <returns>True if the position is inside the voxel object volume. False if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInVolume(Vector3Int value) => IsInVolume(new int3(value.x, value.y, value.z));

        /// <summary>
        /// Checks if the voxel position belongs to the voxel object volume
        /// </summary>
        /// <param name="value">The position to check</param>
        /// <returns>True if the position is inside the voxel object volume. False if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInVolume(int3 value)
        {
            return math.all(value >= 0) && math.all(value < size);
        }

        /// <summary>
        /// Provides access to a specific voxel of the voxel object
        /// </summary>
        /// <param name="position">The position of the voxel</param>
        public Voxel this[Vector3Int position]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this[new int3(position.x, position.y, position.z)];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this[new int3(position.x, position.y, position.z)] = value;
        }
        
        /// <summary>
        /// Provides access to a specific voxel of the voxel object
        /// </summary>
        /// <param name="position">The position of the voxel</param>
        public Voxel this[int3 position]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                CheckDisposed();
                var (chunk, index, insideChunkPosition) = GetChunkLocation(position);
                var chunkData = chunks[chunk.x][chunk.y][chunk.z];
                return chunkData.Voxels.IsCreated ? chunkData.Voxels[index] : new Voxel();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                CheckDisposed();
                var (chunk, index, insideChunkPosition) = GetChunkLocation(position);
                var chunksCount = ChunksCount;
                if (chunksCount.x <= chunk.x ||
                    chunksCount.y <= chunk.y ||
                    chunksCount.z <= chunk.z ||
                    chunk.x < 0 ||
                    chunk.y < 0 ||
                    chunk.z < 0)
                    throw new ArgumentException($"Trying to set the voxel outside of the object boundaries {chunk}");
                
                var chunkData = chunks[chunk.x][chunk.y][chunk.z];
                if (!chunkData.IsValid && value.VoxelKind == VoxelKind.Empty)
                    return;
                
                if (!chunkData.IsValid)
                {
                    if (value.VoxelKind == VoxelKind.Empty)
                        return;

                    chunkData = new Chunk(new NativeArray<Voxel>(chunkSize * chunkSize * chunkSize,
                        Allocator.Persistent));
                    
                    chunks[chunk.x][chunk.y][chunk.z] = chunkData;
                    
                    MarkDirtyAround(chunk);
                }

                var current = chunkData.Voxels[index];
                if (current.Equals(value))
                    return;

                var wasEmpty = current.VoxelKind == VoxelKind.Empty;
                chunkData.IsDirty = true;
                if (value.VoxelKind == VoxelKind.Empty)
                {
                    if (wasEmpty)
                        return;
                    
                    chunkData.VoxelsCount--;
                    if (chunkData.VoxelsCount == 0)
                    {
                        chunkData.Dispose();
                        chunkData = new Chunk();
                    }
                    else
                        chunkData.Voxels[index] = value;
                }
                else
                {
                    chunkData.Voxels[index] = value;
                    if (wasEmpty)
                        chunkData.VoxelsCount++;
                }

                chunks[chunk.x][chunk.y][chunk.z] = chunkData;
                
                if (insideChunkPosition.x == 0)
                    MarkDirty(chunk + new int3(-1, 0, 0));
                
                if (insideChunkPosition.x == chunkSize - 1)
                    MarkDirty(chunk + new int3(1, 0, 0));
                
                if (insideChunkPosition.y == 0)
                    MarkDirty(chunk + new int3(0, -1, 0));
                
                if (insideChunkPosition.y == chunkSize - 1)
                    MarkDirty(chunk + new int3(0, 1, 0));
                
                if (insideChunkPosition.z == 0)
                    MarkDirty(chunk + new int3(0, 0, -1));
                
                if (insideChunkPosition.z == chunkSize - 1)
                    MarkDirty(chunk + new int3(0, 0, 1));
            }
        }

        private void MarkDirtyAround(int3 position)
        {
            MarkDirty(position + new int3(1, 0, 0));
            MarkDirty(position + new int3(-1, 0, 0));
            MarkDirty(position + new int3(0, 1, 0));
            MarkDirty(position + new int3(0, -1, 0));
            MarkDirty(position + new int3(0, 0, 1));
            MarkDirty(position + new int3(0, 0, -1));
        }

        private void MarkDirty()
        {
            for (var xIndex = 0; xIndex < ChunksCount.x; xIndex++)
                for (var yIndex = 0; yIndex < ChunksCount.y; yIndex++)
                    for (var zIndex = 0; zIndex < ChunksCount.z; zIndex++)
                        MarkDirty(new int3(xIndex, yIndex, zIndex));
        }
        
        private void MarkDirty(int3 position)
        {
            var count = ChunksCount;
            if (position.x < 0 || position.x >= count.x)
                return;
            
            if (position.y < 0 || position.y >= count.y)
                return;
            
            if (position.z < 0 || position.z >= count.z)
                return;

            var chunk = chunks[position.x][position.y][position.z];
            chunk.IsDirty = true;
            chunks[position.x][position.y][position.z] = chunk;
        }

        private Chunk GetChunk(Vector3Int position)
        {
            var count = ChunksCount;
            if (position.x < 0 || position.x >= count.x)
                return EmptyChunk;
            
            if (position.y < 0 || position.y >= count.y)
                return EmptyChunk;
            
            if (position.z < 0 || position.z >= count.z)
                return EmptyChunk;

            var result = chunks[position.x][position.y][position.z];
            return result.Voxels.IsCreated ? result : EmptyChunk;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Generates lightmap UV set for the object
        /// </summary>
        public void GenerateLightmapUV()
        {
            CheckDisposed();
            var settings = new UnityEditor.UnwrapParam();
            UnityEditor.UnwrapParam.SetDefaults(out settings);
            settings.packMargin = 0.04f;
            foreach (var meshDescriptor in meshes)
                UnityEditor.Unwrapping.GenerateSecondaryUVSet(meshDescriptor.Mesh, settings);
        }
#endif
        
        private void WaitForAllHandles(List<JobHandle> handles)
        {
            var handlesToWait = new NativeArray<JobHandle>(handles.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            for (var index = 0; index < handles.Count; index++)
                handlesToWait[index] = handles[index];
            
            handles.Clear();

            JobHandle.CompleteAll(handlesToWait);

            handlesToWait.Dispose();
        }
        
        /// <summary>
        /// Regenerates meshes
        /// </summary>
        /// <param name="indexFormat">The index format to be used for the meshes</param>
        /// <param name="generationApproach">The generation approach to be taken to generate meshes</param>
        /// <param name="shift">The shift applied to the vertices</param>
        public void UpdateMeshes(IndexFormat indexFormat, Vector3 shift = new Vector3())
        {
            if (materialsAreDirty)
            {
                for (var index = 0; index < palette.Length; index++)
                    palette[index] = new TransformedMaterial(originalPalette[index], HueShift, Saturation, Brightness);

                materialsAreDirty = false;
            }
            
            CheckDisposed();
            var jobHandles = ListPool<JobHandle>.Get();
            var facesGenerationJobs = ListPool<FacesGenerationJob>.Get();
            var quadGenerationJobs = ListPool<QuadGenerationJobEntry>.Get();
            var meshGenerationJobs = ListPool<MeshGenerationEntry>.Get();
            var textureFillJobs = ListPool<TextureFillJob>.Get();
            
            var chunkVolume = chunkSize * chunkSize * chunkSize;
            var facesCount = chunkVolume;
            var dirtyChunksCount = 0;
            for (var x = 0; x < chunks.Length; x++)
            {
                var xChunks = chunks[x];
                for (var y = 0; y < xChunks.Length; y++)
                {
                    var yChunks = xChunks[y];
                    for (var z = 0; z < yChunks.Length; z++)
                    {
                        var chunk = yChunks[z];
                        if (chunk.VoxelsCount == 0)
                            continue;
                        
                        if (!chunk.IsDirty)
                            continue;

                        dirtyChunksCount++;

                        chunk.IsDirty = false;
                        chunks[x][y][z] = chunk;
                        
                        var faceGenerationJob = new FacesGenerationJob();
                        faceGenerationJob.Voxels = chunk.Voxels;
                        faceGenerationJob.UpperChunk = GetChunk(new Vector3Int(x, y + 1, z)).Voxels;
                        faceGenerationJob.LowerChunk = GetChunk(new Vector3Int(x, y - 1, z)).Voxels;
                        faceGenerationJob.LeftChunk = GetChunk(new Vector3Int(x - 1, y, z)).Voxels;
                        faceGenerationJob.RightChunk = GetChunk(new Vector3Int(x + 1, y, z)).Voxels;
                        faceGenerationJob.CloserChunk = GetChunk(new Vector3Int(x, y, z - 1)).Voxels;
                        faceGenerationJob.FurtherChunk = GetChunk(new Vector3Int(x, y, z + 1)).Voxels;
                        faceGenerationJob.Palette = palette;
                        faceGenerationJob.Faces = new NativeArray<Face>(facesCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                        faceGenerationJob.ChunkSize = chunkSize;
                        faceGenerationJob.ChunkSizeSquared = chunkSize * chunkSize;

                        var quadGenerationJob = new QuadGenerationJob();
                        quadGenerationJob.IgnoreMaterials = meshGenerationApproach == MeshGenerationApproach.Textured;
                        quadGenerationJob.Faces = faceGenerationJob.Faces;
                        quadGenerationJob.TransparentQuads = new NativeList<Quad>(512, Allocator.TempJob);
                        quadGenerationJob.Palette = palette;
                        quadGenerationJob.OpaqueQuads = new NativeList<Quad>(512, Allocator.TempJob);
                        quadGenerationJob.ChunkSize = chunkSize;

                        var opaqueVertexGenerationJob = new VertexGenerationJob();
                        opaqueVertexGenerationJob.EdgeShift = opaqueEdgeShift;

                        opaqueVertexGenerationJob.Vertices = chunk.OpaqueVertices.Vertices;
                        
                        opaqueVertexGenerationJob.Palette = palette;
                        opaqueVertexGenerationJob.Quads = quadGenerationJob.OpaqueQuads;

                        opaqueVertexGenerationJob.Shift = new Vector4(x, y, z) * chunkSize;
                        opaqueVertexGenerationJob.Shift += new float4(shift, 0.0f);
                        
                        opaqueVertexGenerationJob.Scale = scale;

                        var transparentVertexGenerationJob = opaqueVertexGenerationJob;
                        transparentVertexGenerationJob.EdgeShift = transparentEdgeShift;
                        transparentVertexGenerationJob.Quads = quadGenerationJob.TransparentQuads;
                        transparentVertexGenerationJob.Vertices = chunk.TransparentVertices.Vertices;
                        
                        facesGenerationJobs.Add(faceGenerationJob);
                        quadGenerationJobs.Add(new QuadGenerationJobEntry(quadGenerationJob,
                            opaqueVertexGenerationJob,
                            transparentVertexGenerationJob,
                            new Vector3Int(x, y, z)));

                        var faceJob = faceGenerationJob.Schedule(chunkVolume, 256);
                        jobHandles.Add(faceJob);
                        var quadsJob = quadGenerationJob.Schedule(faceJob);
                        jobHandles.Add(quadsJob);

                        if (meshGenerationApproach != MeshGenerationApproach.Textured)
                            continue;
                        
                        for (var countIndex = 0; countIndex < 2; countIndex++)
                        {
                            var textureFillJob = new TextureFillJob();

                            textureFillJob.Chunk = chunk.Voxels;
                            textureFillJob.Palette = palette;
                            textureFillJob.TextureDescriptors = new NativeList<TextureDescriptor>(1024, Allocator.TempJob);
                            textureFillJob.Quads = countIndex == 0
                                ? quadGenerationJob.OpaqueQuads
                                : quadGenerationJob.TransparentQuads;

                            textureFillJob.ColorTarget = new NativeList<Color32>(128 * 128, Allocator.TempJob);

                            textureFillJob.ChunkSize = chunkSize;
                            textureFillJob.ChunkSizeSquared = chunkSize * chunkSize;
                    
                            jobHandles.Add(textureFillJob.Schedule(quadsJob));
                                
                            textureFillJobs.Add(textureFillJob);
                        }
                    }
                }
            }
            
            if (dirtyChunksCount == 0)
            {
                ListPool<FacesGenerationJob>.Release(facesGenerationJobs);
                ListPool<QuadGenerationJobEntry>.Release(quadGenerationJobs);
                ListPool<MeshGenerationEntry>.Release(meshGenerationJobs);
                ListPool<TextureFillJob>.Release(textureFillJobs);

                return;
            }

            foreach (var mesh in meshes)
                mesh.Mesh.Clear(true);
            
            WaitForAllHandles(jobHandles);

            Texture2D atlas = null;
            NativeArray<Rect> uvs = default;
            if (meshGenerationApproach == MeshGenerationApproach.Textured)
            {
                atlas = new Texture2D(1, 1, TextureFormat.RGBA32, false, false);
                atlas.filterMode = FilterMode.Point;
                atlas.wrapMode = TextureWrapMode.Clamp;

                var textures = ListPool<Texture2D>.Get();

                foreach (var textureFillJob in textureFillJobs)
                {
                    var count = textureFillJob.Quads.Length;

                    var array = textureFillJob.ColorTarget.AsArray().Reinterpret<UnityEngine.Color32>();

                    for (var index = 0; index < count; index++)
                    {
                        var textureInfo = textureFillJob.TextureDescriptors[index];

                        var texture = new Texture2D(textureInfo.Size.x, textureInfo.Size.y, TextureFormat.RGBA32, false,
                            false);
                        var subArray = array.GetSubArray(textureInfo.StartPosition,
                            textureInfo.Size.x * textureInfo.Size.y);

                        texture.SetPixelData(subArray, 0, 0);
                        texture.Apply(false);

                        textures.Add(texture);
                    }

                    textureFillJob.ColorTarget.Dispose();
                    textureFillJob.TextureDescriptors.Dispose();
                }

                uvs = new NativeArray<Rect>(atlas.PackTextures(textures.ToArray(), 0), Allocator.TempJob);
                atlas.Apply(false);

                foreach (var texture in textures)
                    GameObject.DestroyImmediate(texture);

                ListPool<Texture2D>.Release(textures);
            }
            else
                uvs = new NativeArray<Rect>(0, Allocator.TempJob);
            
            ListPool<TextureFillJob>.Release(textureFillJobs);

            var startingQuadLocation = 0;
            for (var index = 0; index < quadGenerationJobs.Count; index++)
            {
                var quadGenerationJobEntry = quadGenerationJobs[index];
                var opaqueJob = quadGenerationJobEntry.OpaqueVertexGenerationJob;
                opaqueJob.VerticesCount = quadGenerationJobEntry.Job.OpaqueQuads.Length * 4;
                opaqueJob.UVsStart = startingQuadLocation;
                quadGenerationJobEntry.OpaqueVertexGenerationJob = opaqueJob;
                
                startingQuadLocation += quadGenerationJobEntry.Job.OpaqueQuads.Length;

                var transparentJob = quadGenerationJobEntry.TransparentVertexGenerationJob;
                transparentJob.VerticesCount = quadGenerationJobEntry.Job.TransparentQuads.Length * 4;
                transparentJob.UVsStart = startingQuadLocation;
                quadGenerationJobEntry.TransparentVertexGenerationJob = transparentJob;
                
                startingQuadLocation += quadGenerationJobEntry.Job.TransparentQuads.Length;
                
                quadGenerationJobEntry.OpaqueVertexGenerationJob.UVs = uvs;
                quadGenerationJobEntry.TransparentVertexGenerationJob.UVs = uvs;
                
                quadGenerationJobs[index] = quadGenerationJobEntry;

                jobHandles.Add(quadGenerationJobEntry.OpaqueVertexGenerationJob.Schedule(quadGenerationJobEntry.Job.OpaqueQuads.Length, 256));
                jobHandles.Add(quadGenerationJobEntry.TransparentVertexGenerationJob.Schedule(quadGenerationJobEntry.Job.TransparentQuads.Length, 256));
            }
            
            WaitForAllHandles(jobHandles);

            foreach (var job in quadGenerationJobs)
            {
                var chunk = GetChunk(job.Location);
                
                {
                    var set = chunk.OpaqueVertices;
                    set.VerticesCount = job.OpaqueVertexGenerationJob.VerticesCount;
                    chunk.OpaqueVertices = set;
                }

                {
                    var set = chunk.TransparentVertices;
                    set.VerticesCount = job.TransparentVertexGenerationJob.VerticesCount;
                    chunk.TransparentVertices = set;
                }

                chunks[job.Location.x][job.Location.y][job.Location.z] = chunk;
            }

            var opaqueVertices = GenerateMeshes(MeshKind.Opaque, indexFormat, meshGenerationJobs, jobHandles);
            var transparentVertices = GenerateMeshes(MeshKind.Transparent, indexFormat, meshGenerationJobs, jobHandles);
            
            WaitForAllHandles(jobHandles);
            
            foreach (var facesGenerationJob in facesGenerationJobs)
                facesGenerationJob.Faces.Dispose();
            
            foreach (var quadGenerationJob in quadGenerationJobs)
            {
                quadGenerationJob.Job.TransparentQuads.Dispose();
                quadGenerationJob.Job.OpaqueQuads.Dispose();
            }

            uvs.Dispose();
            
            ListPool<QuadGenerationJobEntry>.Release(quadGenerationJobs);
            
            while (meshes.Count > meshGenerationJobs.Count)
            {
                Object.Destroy(meshes[meshes.Count - 1].Mesh);
                meshes.RemoveAt(meshes.Count - 1);
            }

            while (meshes.Count < meshGenerationJobs.Count)
            {
                var mesh = new Mesh();
                mesh.MarkDynamic();
                
                meshes.Add(new MeshDescriptor(MeshKind.Opaque, mesh, atlas));
            }
            
            ListPool<FacesGenerationJob>.Release(facesGenerationJobs);

            WaitForAllHandles(jobHandles);
            
            for (var index = 0; index < meshGenerationJobs.Count; index++)
            {
                var job = meshGenerationJobs[index];

                var mesh = meshes[index];
                mesh.kind = job.MeshKind;
                meshes[index] = mesh;

                mesh.Mesh.SetVertexBufferParams(job.Count,
                    new VertexAttributeDescriptor(VertexAttribute.Position,
                        VertexAttributeFormat.Float32,
                        3),
                    new VertexAttributeDescriptor(VertexAttribute.Normal,
                        VertexAttributeFormat.SNorm8,
                        4),
                    new VertexAttributeDescriptor(VertexAttribute.Color,
                        VertexAttributeFormat.UNorm8,
                        4),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0,
                        VertexAttributeFormat.Float32,
                        2),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord3,
                                                  VertexAttributeFormat.UNorm8,
                                                  4));

                var flags = MeshUpdateFlags.DontNotifyMeshUsers | 
                            MeshUpdateFlags.DontValidateIndices |
                            MeshUpdateFlags.DontResetBoneBounds |
                            MeshUpdateFlags.DontRecalculateBounds;

                mesh.Mesh.SetVertexBufferData(job.Vertices, job.StartIndex, 0, job.Count, flags: flags);
                
                var trianglesCount = job.Count / 2;
                var indicesCount = trianglesCount * 3;
                
                mesh.Mesh.SetIndexBufferParams(indicesCount, indexFormat);
                
                if (indexFormat == IndexFormat.UInt32)
                    mesh.Mesh.SetIndexBufferData(IndexGenerator.Instance.GetWithSizeOfU32(indicesCount), 0, 0, indicesCount, flags: flags);
                else
                    mesh.Mesh.SetIndexBufferData(IndexGenerator.Instance.GetWithSizeOfU16(indicesCount), 0, 0, indicesCount, flags: flags);
                
                mesh.Mesh.subMeshCount = 1;
                mesh.Mesh.SetSubMesh(0, new SubMeshDescriptor(0, indicesCount), flags: flags);
                var extents = (float3)Size * Scale;
                mesh.Mesh.bounds = new Bounds(extents * 0.5f + (float3)shift * Scale, extents);
                mesh.Mesh.MarkModified();
            }

            if (transparentVertices.IsCreated)
                transparentVertices.Dispose();
            
            if (opaqueVertices.IsCreated)
                opaqueVertices.Dispose();
            
            ListPool<MeshGenerationEntry>.Release(meshGenerationJobs);
        }

        private NativeArray<Vertex> GenerateMeshes(MeshKind kind, IndexFormat indexFormat,
            List<MeshGenerationEntry> meshGenerationJobs, List<JobHandle> jobHandles)
        {
            uint maxVertices = indexFormat == IndexFormat.UInt16 ? 65528 : UInt32.MaxValue;
            var totalCount = 0;

            var chunksInvolved = 0;
            for (var x = 0; x < chunks.Length; x++)
            {
                var xChunks = chunks[x];
                for (var y = 0; y < xChunks.Length; y++)
                {
                    var yChunks = xChunks[y];
                    for (var z = 0; z < yChunks.Length; z++)
                    {
                        var chunk = yChunks[z];
                        if (chunk.VoxelsCount == 0)
                            continue;

                        var set = kind == MeshKind.Opaque ? chunk.OpaqueVertices : chunk.TransparentVertices;
                        if (set.VerticesCount == 0)
                            continue;

                        totalCount += set.VerticesCount;
                        chunksInvolved++;
                    }
                }
            }
            
            if (totalCount == 0)
                return new NativeArray<Vertex>();

            var targetBuffer = new NativeArray<Vertex>(totalCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            var chunkIndex = 0;
            var currentIndex = 0;
            for (var x = 0; x < chunks.Length; x++)
            {
                var xChunks = chunks[x];
                for (var y = 0; y < xChunks.Length; y++)
                {
                    var yChunks = xChunks[y];
                    for (var z = 0; z < yChunks.Length; z++)
                    {
                        var chunk = yChunks[z];
                        if (chunk.VoxelsCount == 0)
                            continue;

                        var set = kind == MeshKind.Opaque ? chunk.OpaqueVertices : chunk.TransparentVertices;
                        if (set.VerticesCount == 0)
                            continue;
                        
                        var copyJob = new CopyVerticesJob();

                        copyJob.Source = set.Vertices;
                        copyJob.Count = set.VerticesCount;
                        copyJob.Result = targetBuffer;
                        copyJob.StartPosition = currentIndex;

                        jobHandles.Add(copyJob.Schedule());
                
                        currentIndex += set.VerticesCount;

                        chunkIndex++;
                    }
                }
            }

            var leftover = totalCount;
            var startIndex = 0;
            while (leftover > 0)
            {
                var count = (int)Mathf.Min(maxVertices, leftover);
                
                var trianglesCount = count / 2;
                var indicesCount = trianglesCount * 3;
                IndexGenerator.Instance.Resize(indicesCount);
                
                meshGenerationJobs.Add(new MeshGenerationEntry(targetBuffer, startIndex, count, kind));
                
                leftover -= count;
                startIndex += count;
            }
            
            return targetBuffer;
        }

        /// <summary>
        /// Deletes all generated meshes
        /// </summary>
        public void DeleteMeshes()
        {
            CheckDisposed();
            foreach (var meshDescriptor in meshes)
            {
                GameObject.DestroyImmediate(meshDescriptor.Mesh);
                GameObject.DestroyImmediate(meshDescriptor.ColorTexture);
            }

            meshes.Clear();
        }
        
        /// <summary>
        /// Disposes the voxel object
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
                return;
            
            palette.Dispose();
            emptyChunk.Dispose();

            foreach (var mesh in meshes)
                GameObject.DestroyImmediate(mesh.Mesh);

            foreach (var xChunks in chunks)
                foreach (var yChunks in xChunks)
                    foreach (var chunk in yChunks)
                        chunk.Dispose();

            isDisposed = true;
        }
        
        /// <summary>
        /// Disposes the voxel object but keeps meshes alive
        /// </summary>
        public void DisposeWithoutMeshes()
        {
            if (isDisposed)
                return;
            
            palette.Dispose();
            emptyChunk.Dispose();

            foreach (var xChunks in chunks)
            foreach (var yChunks in xChunks)
            foreach (var chunk in yChunks)
                chunk.Dispose();

            isDisposed = true;
        }

        /// <summary>
        /// Transforms voxel object to model
        /// </summary>
        /// <returns>Model representing the voxel object</returns>
        public Model ToModel()
        {
            var model = ScriptableObject.CreateInstance<Model>();
            model.Size = size.ToVector3Int();

            var voxels = new List<VoxelData>();
            for (var x = 0; x < size.x; x++)
            {
                for (var y = 0; y < size.y; y++)
                {
                    for (var z = 0; z < size.z; z++)
                    {
                        var voxel = this[new int3(x, y, z)];
                        if (voxel.VoxelKind == VoxelKind.Empty)
                            continue;
                        
                        voxels.Add(new VoxelData(new Vector3Int(x, y, z), voxel.Material));
                    }
                }
            }
            
            model.SetVoxels(voxels);
            
            return model;
        }
    }
}