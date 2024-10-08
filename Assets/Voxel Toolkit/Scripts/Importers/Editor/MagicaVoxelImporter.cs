using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor.AssetImporters;
using UnityEngine;
using VoxelToolkit.MagicaVoxel;
using Debug = UnityEngine.Debug;

namespace VoxelToolkit.Editor
{
    [ScriptedImporter(2, new [] { "vox" }, importQueueOffset: 4000, AllowCaching = false)]
    public class MagicaVoxelImporter : VoxelImporter
    {
        [SerializeField] private float emissionScaleFactor = 1.8f;
        
        protected override VoxelAsset ImportAsset(AssetImportContext ctx)
        {
            using (var stream = new FileStream(ConvertProjectPathToSystemPath(ctx.assetPath), FileMode.Open))
            {
                using (var binaryReader = new BinaryReader(stream, Encoding.Default))
                {
                    var assetImporter = new VoxelToolkit.MagicaVoxelImporter();
                    assetImporter.EmissionScaleFactor = emissionScaleFactor;
                    
                    var asset = assetImporter.ImportAsset(binaryReader);
                    asset.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
            
                    return asset;
                }
            }
        }

    }
}