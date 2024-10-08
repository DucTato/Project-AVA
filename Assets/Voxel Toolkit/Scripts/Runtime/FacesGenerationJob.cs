using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace VoxelToolkit
{
    [Flags]
    public enum FaceOrientation : byte
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
        Closer = 16,
        Further = 32,
    }
    
    public struct Face
    {
        public FaceOrientation Faces;
        public readonly byte Material;

        public Face(FaceOrientation faces, byte material)
        {
            Faces = faces;
            Material = material;
        }
    }

    [BurstCompile(FloatPrecision.Low, FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, DisableSafetyChecks = true)]
    public struct FacesGenerationJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Voxel> Voxels;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Voxel> UpperChunk;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Voxel> LowerChunk;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Voxel> LeftChunk;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Voxel> RightChunk;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Voxel> CloserChunk;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<Voxel> FurtherChunk;
        [NativeDisableParallelForRestriction] [ReadOnly] public NativeArray<TransformedMaterial> Palette;

        [ReadOnly] public int ChunkSize;
        [ReadOnly] public int ChunkSizeSquared;

        [NativeDisableParallelForRestriction] [WriteOnly] public NativeArray<Face> Faces;

        public void Execute(int index)
        {
            var lastChunkIndex = ChunkSize - 1;
            var multiplier = new int4(ChunkSize, ChunkSizeSquared, 1, 0);

            var y = index / ChunkSizeSquared;
            var leftover = index - (y * ChunkSizeSquared);
            var x = leftover / ChunkSize;
            var z = leftover - (x * ChunkSize);
            
            var center = math.dot(multiplier, new int4(x, y, z, 0));
            var centerVoxel = Voxels[center];
            if (centerVoxel.VoxelKind == VoxelKind.Empty)
            {
                Faces[center] = new Face();
                return;
            }

            var material = Palette[centerVoxel.Material];

            var higherChunk = y == lastChunkIndex ? UpperChunk : Voxels;

            var lowerChunk = y == 0 ? LowerChunk : Voxels;

            var closerChunk = z == 0 ? CloserChunk : Voxels;

            var furtherChunk = z == lastChunkIndex ? FurtherChunk : Voxels;

            var leftChunk = x == 0 ? LeftChunk : Voxels;

            var rightChunk = x == lastChunkIndex ? RightChunk : Voxels;

            var higher =
                math.dot(multiplier, y == lastChunkIndex ? new int4(x, 0, z, 0) : new int4(x, y + 1, z, 0));

            var lower =
                math.dot(multiplier, y == 0 ? new int4(x, lastChunkIndex, z, 0) : new int4(x, y - 1, z, 0));

            var closer =
                math.dot(multiplier, z == 0 ? new int4(x, y, lastChunkIndex, 0) : new int4(x, y, z - 1, 0));

            var further =
                math.dot(multiplier, z == lastChunkIndex ? new int4(x, y, 0, 0) : new int4(x, y, z + 1, 0));

            var right =
                math.dot(multiplier, x == lastChunkIndex ? new int4(0, y, z, 0) : new int4(x + 1, y, z, 0));

            var left =
                math.dot(multiplier, x == 0 ? new int4(lastChunkIndex, y, z, 0) : new int4(x - 1, y, z, 0));

            var voxelHigher = higherChunk[higher];
            var higherMaterial = Palette[voxelHigher.Material];
            var voxelLower = lowerChunk[lower];
            var lowerMaterial = Palette[voxelLower.Material];
            var voxelCloser = closerChunk[closer];
            var closerMaterial = Palette[voxelCloser.Material];
            var voxelFurther = furtherChunk[further];
            var furtherMaterial = Palette[voxelFurther.Material];
            var voxelOnTheLeft = leftChunk[left];
            var leftMaterial = Palette[voxelOnTheLeft.Material];
            var voxelOnTheRight = rightChunk[right];
            var rightMaterial = Palette[voxelOnTheRight.Material];

            var centerMaterialType = material.MaterialType;

            var faces = FaceOrientation.None;
            if (voxelHigher.VoxelKind == VoxelKind.Empty ||
                higherMaterial.MaterialType != centerMaterialType)
                faces |= FaceOrientation.Top;

            if (voxelLower.VoxelKind == VoxelKind.Empty ||
                lowerMaterial.MaterialType != centerMaterialType)
                faces |= FaceOrientation.Bottom;

            if (voxelCloser.VoxelKind == VoxelKind.Empty ||
                closerMaterial.MaterialType != centerMaterialType)
                faces |= FaceOrientation.Closer;

            if (voxelFurther.VoxelKind == VoxelKind.Empty ||
                furtherMaterial.MaterialType != centerMaterialType)
                faces |= FaceOrientation.Further;

            if (voxelOnTheLeft.VoxelKind == VoxelKind.Empty ||
                leftMaterial.MaterialType != centerMaterialType)
                faces |= FaceOrientation.Left;

            if (voxelOnTheRight.VoxelKind == VoxelKind.Empty ||
                rightMaterial.MaterialType != centerMaterialType)
                faces |= FaceOrientation.Right;

            Faces[center] = new Face(faces, centerVoxel.Material);
        }
    }
}