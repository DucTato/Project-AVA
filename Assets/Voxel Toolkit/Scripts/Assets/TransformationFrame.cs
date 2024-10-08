using Unity.Mathematics;
using UnityEngine;

namespace VoxelToolkit
{
    /// <summary>
    /// Represents a frame of an animation
    /// </summary>
    [System.Serializable]
    public struct TransformationFrame
    {
        /// <summary>
        /// The translation of the frame
        /// </summary>
        public int3 Translation;
        /// <summary>
        /// The transformation of the frame
        /// </summary>
        public Matrix3x3Int Transformation;
    }
}