using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = Unity.Mathematics.Random;

namespace VoxelToolkit
{
	public class SnowVoxelObjectModifier : VoxelObjectModifier
	{
		[SerializeField] private int snowMaterialIndex;
		[SerializeField] private int icicleMaterialIndex;
		[SerializeField] private int snowCastDistance = 10;
		[SerializeField] private float coverageChance = 0.99f;
		[SerializeField] private float icicleChance = 0.05f;
		[SerializeField] private int maxIcicleLength = 4;
		[SerializeField] private uint seed = 23412;
		[SerializeField] private List<byte> materialsToIgnore = new List<byte>();
		
		public override void Apply(VoxelAsset target)
		{
			var path = new List<HierarchyNode>();
			var created = new HashSet<HierarchyNode>();
			AddSnow(target, target.HierarchyRoot, path, created, Matrix3x3Int.Identity);
		}

		private void AddSnow(VoxelAsset asset, HierarchyNode node, List<HierarchyNode> path, HashSet<HierarchyNode> map, Matrix3x3Int accumulatedOrientation)
		{
			if (map.Contains(node))
				return;

			var direction = accumulatedOrientation.Inversed * new int3(0, 1, 0);

			path.Add(node);
			if (node is Group group)
			{
				for (var index = 0; index < group.ChildrenCount; index++)
					AddSnow(asset, group[index], path, map, accumulatedOrientation);
			}
			else if (node is Shape shape)
			{
				var random = new Random(seed);
				foreach (var model in shape.Models)
				{
					var voxelObject = VoxelObject.CreateFromModel(model, asset.Palette, 32);
					var snow = new VoxelObject(voxelObject.Size.ToVector3Int(), 32);

					var size = voxelObject.Size;
					for (var x = 0; x < voxelObject.Size.x; x++)
					{
						for (var y = 0; y < voxelObject.Size.y; y++)
						{
							for (var z = 0; z < voxelObject.Size.z; z++)
							{
								var thisVoxel = voxelObject[new int3(x, y, z)];
								
								if (thisVoxel.VoxelKind == VoxelKind.Empty)
									continue;
								
								if (materialsToIgnore.Contains(thisVoxel.Material))
									continue;

								var foundSolid = false;
								for (var yc = 1; yc <= snowCastDistance; yc++)
								{
									var position = new int3(x, y, z) + direction * yc;
									var upperVoxel = math.any(position < 0) || math.any(position >= size) ? new Voxel() : voxelObject[position];
									if (upperVoxel.VoxelKind == VoxelKind.Empty)
										continue;

									foundSolid = true;
									break;
								}

								if (foundSolid)
									continue;
								
								if (random.NextFloat() > coverageChance)
									continue;
								
								snow[new int3(x, y, z)] = new Voxel(VoxelKind.Solid, (byte)snowMaterialIndex);
							}
						}
					}
					
					for (var x = 0; x < voxelObject.Size.x; x++)
					{
						for (var y = 0; y < voxelObject.Size.y; y++)
						{
							for (var z = 0; z < voxelObject.Size.z; z++)
							{
								var thisVoxel = voxelObject[new int3(x, y, z)];
								if (thisVoxel.VoxelKind == VoxelKind.Empty)
									continue;

								var icicleSize = 1 + (random.NextInt() % (maxIcicleLength - 1)); 
								var foundSolid = false;
								for (var yc = 1; yc <= icicleSize; yc++)
								{
									var position = new int3(x, y, z) - direction * yc;
									var lowerVoxel = math.any(position < 0) ? new Voxel(VoxelKind.Solid, 0) : voxelObject[position];
									if (lowerVoxel.VoxelKind == VoxelKind.Empty)
										continue;

									foundSolid = true;
									break;
								}

								if (foundSolid)
									continue;
								
								if (random.NextFloat() > icicleChance)
									continue;

								for (var yShift = 0; yShift < icicleSize; yShift++)
								{
									var position = new int3(x, y, z) - direction * (yShift + 2);
									if (math.any(position < 0) || math.any(position >= size))
										continue;
									
									snow[position] = new Voxel(VoxelKind.Solid, (byte)icicleMaterialIndex);
								}
							}
						}
					}
					
					var shapeClone = ScriptableObject.CreateInstance<Shape>();
					shapeClone.name = $"{shape.name} clone";

					var lastGroup = path.FindLast(x => x is Group) as Group;
					var lastTransformation = path.FindLast(x => x is Transformation) as Transformation;

					var transformationClone = ScriptableObject.Instantiate(lastTransformation);
					
					transformationClone.name = $"{lastTransformation.name} clone";

					transformationClone.Frames[0].Translation += new int3(0, 1, 0);

					var updatedModel = snow.ToModel();
					updatedModel.name = $"{model.name} clone";
					updatedModel.ParentAsset = asset;
					
					shapeClone.AddModel(updatedModel);
					transformationClone.Child = shapeClone;
					lastGroup.AddChild(transformationClone);

					map.Add(shapeClone);
					
					voxelObject.Dispose();
					snow.Dispose();
				}
			}
			else if (node is Transformation transformation)
			{
				accumulatedOrientation *= transformation.Frames[0].Transformation;
				AddSnow(asset, transformation.Child, path, map, accumulatedOrientation);
			}
			
			path.RemoveAt(path.Count - 1);
		}
	}
}