using System;

namespace VoxelToolkit
{
    /// <summary>
    /// The kind of the voxel
    /// </summary>
    public enum VoxelKind : byte
    {
        /// <summary>
        /// The space is empty
        /// </summary>
        Empty,
        /// <summary>
        /// The space is occupied by a voxel
        /// </summary>
        Solid
    }
    
    /// <summary>
    /// The voxel data type
    /// </summary>
    public struct Voxel : IEquatable<Voxel>
    {
        /// <summary>
        /// The kind of the voxel
        /// </summary>
        public readonly VoxelKind VoxelKind;
        
        /// <summary>
        /// The material of the voxel
        /// </summary>
        public readonly byte Material;

        public Voxel(byte material)
        {
            VoxelKind = VoxelKind.Solid;
            Material = material;
        }

        public Voxel(VoxelKind voxelKind, byte material)
        {
            VoxelKind = voxelKind;
            Material = material;
        }

        /// <summary>
        /// Compares two voxels
        /// </summary>
        /// <param name="other">The second voxel to compare with</param>
        /// <returns>Returns true if voxels are identical</returns>
        public bool Equals(Voxel other)
        {
            if (VoxelKind == VoxelKind.Empty && other.VoxelKind == VoxelKind.Empty)
                return true;
            
            return VoxelKind == other.VoxelKind && Material == other.Material;
        }

        public override bool Equals(object obj)
        {
            return obj is Voxel other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)VoxelKind * 397) ^ Material.GetHashCode();
            }
        }
    }
}