using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace VoxelToolkit
{
    /// <summary>
    /// Builds a game objects hierarchy for a specific asset
    /// </summary>
	public class GameObjectBuilder
    {
        /// <summary>
        /// The hue shift to be applied to the materials
        /// </summary>
        public float HueShift { get; set; } = 0.0f;
        /// <summary>
        /// Saturation to be applied to the materials
        /// </summary>
        public float Saturation { get; set; } = 1.3f;
        /// <summary>
        /// Brightness to be applied to the materials
        /// </summary>
        public float Brightness { get; set; } = 0.0f;
        /// <summary>
        /// The opaque material to be used for the object built
        /// </summary>
        public UnityEngine.Material OpaqueMaterial { get; set; }
        /// <summary>
        /// The transparent material to be used for the object built
        /// </summary>
        public UnityEngine.Material TransparentMaterial { get; set; }
        /// <summary>
        /// How much should transparent edges of the mesh should be shifted (Helps with the incorrect shadows)
        /// </summary>
        public float TransparentEdgeShift { get; set; }
        /// <summary>
        /// How much should opaque edges of the mesh should be shifted (Helps with the incorrect shadows)
        /// </summary>
        public float OpaqueEdgeShift { get; set; }
        /// <summary>
        /// The scale of the mesh built
        /// </summary>
        public float Scale { get; set; } = 0.1f;
        /// <summary>
        /// The chunk size to be used to generate the mesh
        /// </summary>
        public int ChunkSize { get; set; } = 16;
        /// <summary>
        /// Generation approach to be taken to generate meshes
        /// </summary>
        public MeshGenerationApproach MeshGenerationApproach { get; set; } = MeshGenerationApproach.Textureless;
        /// <summary>
        /// The origin mode of the objects to be generated
        /// </summary>
        public OriginMode OriginMode { get; set; } = OriginMode.Center;
        /// <summary>
        /// Index format to be used. If null UInt32 will be used if supported  
        /// </summary>
        public IndexFormat? IndexFormat { get; set; }
#if UNITY_EDITOR
        /// <summary>
        /// If set the mesh is going to have UV2 lightmap coords (Editor Only)
        /// </summary>
        public bool GenerateLightmapUV { get; set; }
#endif
        /// <summary>
        /// If set the resulting objects going to have mesh colliders with respected meshes
        /// </summary>
        public bool GenerateColliders { get; set; } = true;
        /// <summary>
        /// Should the hierarchy be reduced to remove empty objects
        /// </summary>
        public bool ReduceHierarchy { get; set; } = true;

        /// <summary>
        /// Creates a game object for the given asset
        /// </summary>
        /// <param name="asset">The asset to generate game object hierarchy from</param>
        /// <returns>The game object built from the given asset</returns>
        public GameObject CreateGameObject(VoxelAsset asset)
        {
            var go = new GameObject("Root");

            AddHierarchyObject(asset, go, asset.HierarchyRoot, 0);
            
            return go;
        }

        private void AddHierarchyObject(VoxelAsset asset, GameObject parent, HierarchyNode node, int idChain)
        {
            if (node is Group group)
                AddGroupGameObject(asset, parent, group, ++idChain);
            else if (node is Transformation transformation)
                AddTransformationGameObject(asset, parent, transformation, ++idChain);
            else if (node is Shape shape)
                AddShapeGameObject(asset, parent, shape, ++idChain);
            else
                throw new Exception("Unexpected hierarchy object type");
        }

        private void AddShapeGameObject(VoxelAsset asset, GameObject parent, Shape shape, int idChain)
        {
            var go = new GameObject(shape.name);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            var opaqueName = MeshGenerationApproach == MeshGenerationApproach.Textured
                ? "/VoxelToolkitDefaultOpaqueTextured"
                : "/VoxelToolkitDefaultOpaqueTextureless";
                
            var transparentName = MeshGenerationApproach == MeshGenerationApproach.Textured
                ? "/VoxelToolkitDefaultTransparentTextured"
                : "/VoxelToolkitDefaultTransparentTextureless";

            var opaqueMaterial = OpaqueMaterial == null
                ? Resources.Load<UnityEngine.Material>($"{PathUtility.GetMaterialPath()}{opaqueName}")
                : OpaqueMaterial;
            
            var transparentMaterial = TransparentMaterial == null
                ? Resources.Load<UnityEngine.Material>($"{PathUtility.GetMaterialPath()}{transparentName}")
                : TransparentMaterial;
            
            var format = IndexFormat ?? (SystemInfo.supports32bitsIndexBuffer
                ? UnityEngine.Rendering.IndexFormat.UInt32
                : UnityEngine.Rendering.IndexFormat.UInt16);
            
            for (var index = 0; index < shape.ModelsCount; index++)
            {
                var model = shape[index];
                var maxSize = Mathf.Max(model.Size.x, model.Size.y, model.Size.z);
                var voxelObject = VoxelObject.CreateFromModel(model, asset.Palette, Mathf.Min(maxSize, ChunkSize));
                voxelObject.Scale = Scale;
                voxelObject.OpaqueEdgeShift = OpaqueEdgeShift;
                voxelObject.TransparentEdgeShift = TransparentEdgeShift;
                voxelObject.MeshGenerationApproach = MeshGenerationApproach;

                voxelObject.HueShift = HueShift;
                voxelObject.Saturation = Saturation;
                voxelObject.Brightness = Brightness;
                
                var meshShift = OriginMode == OriginMode.Corner ?
                                Vector3.zero : 
                                -((Vector3)model.Size / 2.0f);
                
                voxelObject.UpdateMeshes(format, meshShift);

#if UNITY_EDITOR
                if (GenerateLightmapUV)
                    voxelObject.GenerateLightmapUV();
#endif
                
                var shift = (Vector3)(shape[index].Size / 2) * Scale;
                for (var modelIndex = 0; modelIndex < voxelObject.MeshesCount; modelIndex++)
                {
                    var descriptor = voxelObject.GetMesh(modelIndex);
                    var id = $"{idChain}{modelIndex}{shape.ID}{index}";

                    if (index == 0)
                    {
                        var child = new GameObject(id);
                        var meshFilter = child.AddComponent<MeshFilter>();

                        child.transform.SetParent(go.transform);
                        child.transform.localPosition = -shift - meshShift * Scale;
                        child.transform.localRotation = Quaternion.identity;

                        if (GenerateColliders)
                        {
                            var meshCollider = child.AddComponent<MeshCollider>();
                            meshCollider.sharedMesh = descriptor.Mesh;
                        }

                        var renderer = child.AddComponent<MeshRenderer>();
                        
                        renderer.sharedMaterial = descriptor.Kind == MeshKind.Opaque ? opaqueMaterial : transparentMaterial;
                        if (MeshGenerationApproach == MeshGenerationApproach.Textured)
                        {
                            renderer.sharedMaterial = new UnityEngine.Material(renderer.sharedMaterial)
                                 {
                                     mainTexture = descriptor.ColorTexture
                                 };
                        }

                        meshFilter.sharedMesh = descriptor.Mesh;
                    }

                    descriptor.Mesh.name = id;
                }
                
                voxelObject.DisposeWithoutMeshes();
            }

            if (!ReduceHierarchy || go.transform.childCount != 1)
                return;
            
            for (var index = 0; index < go.transform.childCount; index++)
                go.transform.GetChild(index).SetParent(go.transform.parent);
            
            Object.DestroyImmediate(go);
        }

        private void AddGroupGameObject(VoxelAsset asset, GameObject parent, Group group, int idChain)
        {
            var go = new GameObject(group.name);
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            for (var index = 0; index < group.ChildrenCount; index++)
                AddHierarchyObject(asset, go, group[index], ++idChain);
        }

        private void AddTransformationGameObject(VoxelAsset asset, GameObject parent, Transformation transformation, int idChain)
        {
            var go = new GameObject(transformation.name);
            go.transform.SetParent(parent.transform);

            var frameIndex = 0;
            var frame = transformation.Frames[frameIndex];
            
            go.transform.localPosition = (float3)frame.Translation * Scale;
            go.transform.localRotation = ((Matrix4x4)frame.Transformation).transpose.rotation;
            
            AddHierarchyObject(asset, go, transformation.Child, ++idChain);
            
            if (!ReduceHierarchy || go.transform.childCount > 1)
                return;
            
            for (var index = 0; index < go.transform.childCount; index++)
                go.transform.GetChild(index).SetParent(go.transform.parent);
            
            Object.DestroyImmediate(go);
        }
	}
}