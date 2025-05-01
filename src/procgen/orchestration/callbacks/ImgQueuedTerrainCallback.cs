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

public struct ImgQueuedTerrainCallback<L, C> : IQueuedAction
	where L : LandscapeLayer<L, C>, new()
	where C : LandscapeChunk<L, C>, new()
{
	public Image heightmap;
	public Image detailMap;
	public MeshInstance3D[] treeInstances; //there is no TreeInstance in Godot, but we can use Meshinstance, should be as powerful
	public L layer;
	public Point index;
	private Point startPos;

	public ImgQueuedTerrainCallback(
		Image heightmap,
		Image detailMap,
		MeshInstance3D[] treeInstances,
		L layer,
		Point startPos,
		Point index
	)
	{
		this.heightmap = heightmap;
		this.detailMap = detailMap;
		this.treeInstances = treeInstances;
		this.layer = layer;
		this.startPos = startPos;
		this.index = index;
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
		Terrain3DRegion? terrain = GetOrCreateTerrain(new Vector3(startPos.x, 0, startPos.y), layer);
		if (terrain == null)
			yield break;
		terrain.HeightMap = heightmap;
		// terrain.ControlMap = controlmap; // leave it to autoshading on that resolution

		if (layer.lodLevel == 0)
		{
			// terrain.ColorMap = ColorMap;
		}

		// if (treeInstances != null)
		//     terrain.treeInstances = treeInstances;

		TerrainLODManager.instance.terrain3DWrapper.Storage.ForceUpdateMaps();
		yield return null;
	}
}
