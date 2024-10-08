using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelToolkit
{
    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, DisableSafetyChecks = true)]
    public struct VertexGenerationJob : IJobParallelFor
    {
        [ReadOnly] public int VerticesCount;
        [ReadOnly, NativeDisableParallelForRestriction] public NativeList<Quad> Quads;
        [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<TransformedMaterial> Palette;
        [ReadOnly] public int UVsStart;
        [ReadOnly, NativeDisableParallelForRestriction] public NativeArray<Rect> UVs;
        [ReadOnly] public float4 Shift;
        [ReadOnly] public float Scale;
        [ReadOnly] public float EdgeShift;

        [WriteOnly, NativeDisableParallelForRestriction] public NativeArray<Vertex> Vertices;
        
        public void Execute(int index)
        {
            var one = Scale;

            var quad = Quads[index];
            var material = Palette[quad.Material];

            var start = new float4(quad.StartX, quad.StartY, quad.StartZ, 0);
            var end = new float4(quad.EndX, quad.EndY, quad.EndZ, 0);
            var position = (start + Shift) * Scale;
            var size = end - start;
            var scaledSize = (float4)size * Scale;

            var normal = new int4();
            var tangentA = new float4();
            var tangentB = new float4();
            var vertexShift = new float4();
            switch (quad.Orientation)
            {
                case FaceOrientation.Left:
                    normal = new int4(sbyte.MinValue, 0, 0, 0);
                    tangentA = new float4(0.0f, 0.0f, scaledSize.z, 0.0f);
                    tangentB = new float4(0.0f, scaledSize.y, 0.0f, 0.0f);
                    break;
                case FaceOrientation.Right:
                    normal = new int4(sbyte.MaxValue, 0, 0, 0);
                    tangentA = new float4(0.0f, scaledSize.y, 0.0f, 0.0f);
                    tangentB = new float4(0.0f, 0.0f, scaledSize.z, 0.0f);
                    vertexShift = new float4(one, 0.0f, 0.0f, 0.0f);
                    break;
                case FaceOrientation.Top:
                    normal = new int4(0, sbyte.MaxValue, 0, 0);
                    tangentA = new float4(0.0f, 0.0f, scaledSize.z, 0.0f);
                    tangentB = new float4(scaledSize.x, 0.0f, 0.0f, 0.0f);
                    vertexShift = new float4(0.0f, one, 0.0f, 0.0f);
                    break;
                case FaceOrientation.Bottom:
                    normal = new int4(0, sbyte.MinValue, 0, 0);
                    tangentA = new float4(scaledSize.x, 0.0f, 0.0f, 0.0f);
                    tangentB = new float4(0.0f, 0.0f, scaledSize.z, 0.0f);
                    break;
                case FaceOrientation.Closer:
                    normal = new int4(0, 0, sbyte.MinValue, 0);
                    tangentB = new float4(scaledSize.x, 0.0f, 0.0f, 0.0f);
                    tangentA = new float4(0.0f, scaledSize.y, 0.0f, 0.0f);
                    break;
                case FaceOrientation.Further:
                    normal = new int4(0, 0, sbyte.MaxValue, 0);
                    tangentA = new float4(scaledSize.x, 0.0f, 0.0f, 0.0f);
                    tangentB = new float4(0.0f, scaledSize.y, 0.0f, 0.0f);
                    vertexShift = new float4(0.0f, 0.0f, one, 0.0f);
                    break;
            }
            
            position += vertexShift;

            var tanAEdgeShift = tangentA * EdgeShift;
            var tanBEdgeShift = tangentB * EdgeShift;

            var uv = UVs.Length == 0 ? new Rect() : UVs[UVsStart + index];

            var vertexIndex = index * 4;
            Vertices[vertexIndex++] = new Vertex((position + tangentA + tanAEdgeShift - tanBEdgeShift).xyz,
                normal.xyz, new float2(uv.x, uv.y), material);
            Vertices[vertexIndex++] = new Vertex((position + tangentA + tangentB + tanAEdgeShift + tanBEdgeShift).xyz,
                normal.xyz, new float2(uv.x + uv.width, uv.y), material);
            Vertices[vertexIndex++] = new Vertex((position + tangentB + tanBEdgeShift - tanAEdgeShift).xyz,
                normal.xyz, new float2(uv.x + uv.width, uv.y + uv.height), material);
            Vertices[vertexIndex] = new Vertex((position - tanAEdgeShift - tanBEdgeShift).xyz,
                normal.xyz, new float2(uv.x, uv.y + uv.height), material);
        }
    }
}