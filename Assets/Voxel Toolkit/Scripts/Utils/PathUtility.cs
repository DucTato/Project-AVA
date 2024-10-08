using UnityEngine;
using UnityEngine.Rendering;

namespace VoxelToolkit
{
    public static class PathUtility
    {
        public static string GetMaterialPath()
        {
            var pipeline = GraphicsSettings.currentRenderPipeline;
            if (pipeline == null)
                pipeline = QualitySettings.renderPipeline;
            
            return pipeline == null ? 
                "Voxel Toolkit/Materials/BuiltIn" :
                "Voxel Toolkit/Materials/XRP";
        }

    }
}