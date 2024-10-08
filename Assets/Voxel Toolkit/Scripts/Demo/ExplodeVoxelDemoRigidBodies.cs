using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VoxelToolkit.Demo
{
    [RequireComponent(typeof(DynamicVoxelObject))]
    public class ExplodeVoxelDemoRigidBodies : MonoBehaviour
    {
        [SerializeField] private VoxelAsset asset;
        
        private void Update()
        {
            var voxelObject = GetComponent<DynamicVoxelObject>();
            if (!Input.GetMouseButtonDown(0))
                return;

            var camera = Camera.main;
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            
            if (!voxelObject.Raycast(ray, out RaycastHit hit))
                return;
            
            var voxelPosition = hit.Location;

            var radius = 5;
            var query = new List<VoxelQueryResult>();
            voxelObject.QueryVoxelsInSphere(new Vector3Int(voxelPosition.x, voxelPosition.y, voxelPosition.z), radius, query);

            var palette = asset.Palette;
            foreach (var result in query)
            {
                var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var rigidBody = part.AddComponent<Rigidbody>();
                var renderer = part.GetComponent<Renderer>();
                renderer.material.color = palette[result.Voxel.Material].color.linear;
                part.transform.position = voxelObject.TransformVoxelToWorld(result.Position);
                rigidBody.velocity = Random.insideUnitSphere * Random.Range(0.5f, 1.0f);
                rigidBody.angularVelocity = Random.insideUnitSphere * Random.Range(0.0f, 180.0f);
                rigidBody.transform.localScale = Vector3.one * voxelObject.Scale;
            }
            
            voxelObject.SetSphere(new Vector3Int(voxelPosition.x, voxelPosition.y, voxelPosition.z), radius, new Voxel());
        }
    }
}