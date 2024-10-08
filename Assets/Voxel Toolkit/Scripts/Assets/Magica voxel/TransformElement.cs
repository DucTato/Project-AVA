using System;
using System.Collections.Generic;
using System.Globalization;
using Unity.Mathematics;
using UnityEngine;

namespace VoxelToolkit.MagicaVoxel
{
	public class TransformElement : HierarchyElement
	{
		public int ChildId;
		public int LayerId;
		public Dictionary<string, string>[] Frames;
		public HierarchyElement Child;

		public TransformElement(int id, int childId, int layerId, Dictionary<string, string>[] frames, Dictionary<string, string> attributes) : base(id, attributes)
		{
			ChildId = childId;
			LayerId = layerId;
			Frames = frames;
		}

		public override HierarchyNode AddToAsset(VoxelAsset asset, List<Model> models)
		{
			var transformation = ScriptableObject.CreateInstance<Transformation>();
			transformation.name = $"Transformation {Id}";
			transformation.Layer = asset.FindLayerById(LayerId);
            
			transformation.Child = Child?.AddToAsset(asset, models);

			transformation.Frames = new TransformationFrame[Frames.Length];
			for (var index = 0; index < Frames.Length; index++)
			{
				var serializedFrame = Frames[index];
				if (!serializedFrame.TryGetValue("_t", out string serializedTranslation))
					serializedTranslation = "0 0 0";
                
				var splitedTranslation = Array.ConvertAll(serializedTranslation.Split(' '),
					x => int.Parse(x, CultureInfo.InvariantCulture));

				var rotation = new Matrix3x3Int();
                
				if (serializedFrame.TryGetValue("_r", out string serializedRotation))
				{
					var integer = int.Parse(serializedRotation);
					var firstTwo = integer & 3;
					var secondTwo = (integer & (3 << 2)) >> 2;
					var forth = integer & 16;
					var fifth = integer & 32;
					var sixth = integer & 64;
					var third = 0;

					switch (firstTwo)
					{
						case 0:
							rotation.E00 = forth == 0 ? 1 : -1;
							break;
						case 1:
							rotation.E20 = forth == 0 ? 1 : -1;
							break;
						case 2:
							rotation.E10 = forth == 0 ? 1 : -1;
							break;
						default:
							throw new Exception("Unexpected rotation element");
					}
                    
					switch (secondTwo)
					{
						case 0:
							rotation.E02 = fifth == 0 ? 1 : -1;
							third = firstTwo == 1 ? 2 : 1;
							break;
						case 1:
							rotation.E22 = fifth == 0 ? 1 : -1;
							third = firstTwo == 0 ? 2 : 0;
							break;
						case 2:
							rotation.E12 = fifth == 0 ? 1 : -1;
							third = firstTwo == 0 ? 1 : 0;
							break;
						default:
							throw new Exception("Unexpected rotation element");
					}
                    
					switch (third)
					{
						case 0:
							rotation.E01 = sixth == 0 ? 1 : -1;
							break;
						case 1:
							rotation.E21 = sixth == 0 ? 1 : -1;
							break;
						case 2:
							rotation.E11 = sixth == 0 ? 1 : -1;
							break;
						default:
							throw new Exception("Unexpected rotation element");
					}
				}
				else
					rotation = Matrix3x3Int.Identity;

				transformation.Frames[index] = new TransformationFrame()
				{
					Translation = new int3(splitedTranslation[0], splitedTranslation[2], splitedTranslation[1]),
					Transformation = rotation
				};
			}

			return transformation;
		}
	}
}