using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Godot;
using Godot.Collections;
using Godot.Util;
using Runevision.Common;
using Runevision.LayerProcGen;
using Terrain3DBindings;
using Terrain3D.Scripts.Generation.Layers;
using Terrain3D.Scripts.Utilities;

public struct MapQueuedTerrainCallback<L, C> : IQueuedAction
	where L : LandscapeLayer<L, C>, new()
	where C : LandscapeChunk<L, C>, new()
{
	public float[,] heightmap;
	public uint[,] controlmap;
	public int[,] detailMap;
	public MeshInstance3D[] treeInstances; //there is no TreeInstance in Godot, but we can use Meshinstance, should be as powerful
	public L layer;
	public Point index;
	private readonly int regionSize;

	public MapQueuedTerrainCallback(
		float[,] heightmap,
		uint[,] controlmap,
		int[,] detailMap,
		MeshInstance3D[] treeInstances,
		L layer,
		Point index
	)
	{
		this.heightmap = heightmap;
		this.controlmap = controlmap;
		this.detailMap = detailMap;
		this.treeInstances = treeInstances;
		this.layer = layer;
		this.index = index;
		regionSize = (int)RegionSize.SIZE_1024;
	}

	static Terrain3DRegion? GetOrCreateTerrain(Vector3 position, L layer)
	{
		if (!TerrainLODManager.instance.HasChunkAt(position))
			TerrainLODManager.instance.CreateNewChunkAt(position);
		var chunk = TerrainLODManager.instance.GetChunkAt(position);
		return chunk.LoD < layer.lodLevel ? null : chunk;
	}

	public void Process()
	{
		LayerManagerBehavior.instance.StartCoroutine(ProcessRoutine());
	}

	public IEnumerator ProcessRoutine()
	{
		var startPos = index * layer.chunkW;
		Terrain3DRegion? terrain = GetOrCreateTerrain(new Vector3(startPos.x, 0, startPos.y), layer);
		if (terrain == null)
			yield break;

		terrain.HeightMap ??= Image.CreateEmpty(regionSize, regionSize, false, Image.Format.Rf);
		terrain.ControlMap ??= Image.CreateEmpty(regionSize, regionSize, false, Image.Format.Rf);
		DPoint cellSize = (DPoint)layer.chunkSize / layer.gridResolution;
		float minHeight = layer.terrainBaseHeight;
		float totalHeight = layer.terrainHeight - layer.terrainBaseHeight;
		// TerrainLODManager.instance.terrain3D.Storage.HeightRange = new Vector2(minHeight, layer.terrainHeight);

		// GD.Print($"HandleUnderSizedRegions: {cellSize}, {layer.chunkSize}, {layer.gridResolution}");
		// GD.Print($"\t: {layer.lodLevel} {position} in region:{terrain.RegionOffset}, {startPos}; on index:{index}, {index * layer.chunkW}");
		for (var x = 0; x < layer.chunkSize.x; x++)
		{
			for (var z = 0; z < layer.chunkSize.y; z++)
			{
				Vector3 globalPosition = new Vector3(startPos.x + x, 0, startPos.y + z);
				TerrainLODManager.instance.terrain3DWrapper.Storage.SetHeight(globalPosition, heightmap[(int)(z / cellSize.y), (int)(x / cellSize.x)]);
				TerrainLODManager.instance.terrain3DWrapper.Storage.SetControl(globalPosition, controlmap[(int)(z / cellSize.y), (int)(x / cellSize.x)]);
			}
		}

		TerrainLODManager.instance.terrain3DWrapper.Storage.ForceUpdateMaps();
		yield return null;
	}
}
