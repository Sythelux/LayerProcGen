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

public abstract class LandscapeChunk<L, C> : LayerChunk<L, C>
	where L : LandscapeLayer<L, C>, new()
	where C : LandscapeChunk<L, C>, new()
{
	static ListPool<LocationSpec> locationSpecListPool = new ListPool<LocationSpec>(128);
	static ListPool<PathSpec> pathSpecListPool = new ListPool<PathSpec>(128);

	protected float[,] heights;
	public uint[,] controls;
	public Vector3[,] dists;

	const int GridOffset = 4;

	protected LandscapeChunk()
	{
		heights = new float[layer.gridResolution, layer.gridResolution];
		controls = new uint[layer.gridResolution, layer.gridResolution];
		dists = new Vector3[layer.gridResolution, layer.gridResolution];
	}

	public override void Create(int level, bool destroy)
	{
		if (destroy)
		{
			heights.Clear();
		}
		else
		{
			Build();
		}

		// GD.Print($"{GetType().Name} ({bounds}) {MethodBase.GetCurrentMethod()}: {level}, {destroy}");
		base.Create(level, destroy);
	}

	private void Build()
	{
		SimpleProfiler.ProfilerHandle ph;

		DPoint cellSize = (DPoint)layer.chunkSize / layer.chunkResolution;
		DPoint terrainOrigin = index * layer.chunkSize - cellSize * GridOffset;

		ph = SimpleProfiler.Begin(phc, "Height Noise");
		float height = layer.terrainBaseHeight;
		HeightNoise(terrainOrigin, cellSize, layer.gridResolution, ref heights, ref dists, layer.terrainHeight, height);
		SimpleProfiler.End(ph);

		ControlBase(ref controls);

		if (layer.lodLevel < 3)
		{
			// Apply deformation from locations.
			ph = SimpleProfiler.Begin(phc, "Deform-Locations");
			List<LocationSpec> locationSpecs = locationSpecListPool.Get();
			LocationLayer.instance.GetLocationSpecsOverlappingBounds(this, locationSpecs, bounds);
			TerrainDeformation.ApplySpecs(
				ref heights, ref dists, ref controls,
				index * layer.chunkResolution - Point.one * GridOffset,
				Point.one * (layer.gridResolution),
				((Vector2)layer.chunkSize) / layer.chunkResolution,
				locationSpecs,
				(SpecPointB p) =>
				{
					p.centerElevation = 0;
					return p;
				});
			locationSpecListPool.Return(ref locationSpecs);
			SimpleProfiler.End(ph);

			if (layer.lodLevel < 2)
			{
				// Apply deformation from paths.
				ph = SimpleProfiler.Begin(phc, "Deform-Paths");
				List<PathSpec> pathSpecs = pathSpecListPool.Get();
				CultivationLayer.instance.GetPathsOverlappingBounds(this, pathSpecs, bounds);
				TerrainDeformation.ApplySpecs(
					ref heights, ref dists, ref controls,
					index * layer.chunkResolution - Point.one * GridOffset,
					Point.one * (layer.gridResolution),
					((Vector2)layer.chunkSize) / layer.chunkResolution,
					pathSpecs);
				pathSpecListPool.Return(ref pathSpecs);
				SimpleProfiler.End(ph);
			}
		}

		RandomHash rand = new RandomHash(123);

		ph = SimpleProfiler.Begin(phc, "Splat Noise (GetNormal)");
		HandleControls(terrainOrigin, cellSize, layer.gridResolution, ref heights, ref controls);
		SimpleProfiler.End(ph);

		if (layer.lodLevel < 1)
		{
			ph = SimpleProfiler.Begin(phc, "Generate Details");
			// var detailMapPointerArray = detailMap.AsSpan();
			GenerateDetails(layer.gridResolution, rand, ref controls);
			SimpleProfiler.End(ph);
		}

		IQueuedAction action;
		if (layer.chunkW < (int)RegionSize.SIZE_1024)
		{
			action = new MapQueuedTerrainCallback<L, C>(
				heights, controls, null, null,
				layer, index
			);
			MainThreadActionQueue.Enqueue(action);
		}
		else
		{
			var regionSize = (int)RegionSize.SIZE_1024;

			var subRegionSize = new Point(layer.chunkW / regionSize, layer.chunkH / regionSize);

			for (int i = 0; i < subRegionSize.x; i++)
			for (int j = 0; j < subRegionSize.y; j++)
			{
				var subIndex = new Point(i, j);
				var baseStartPos = index * layer.chunkW;
				var subStartPos = subIndex * regionSize;
				var startPos = baseStartPos + subStartPos;
				// GD.Print($"HandleOverSizedRegions: {cellSize}, {subIndex} {layer.chunkSize}, {startPos}");


				var heightImg = Image.CreateEmpty(regionSize, regionSize, false, Image.Format.Rf);
				var detailImg = Image.CreateEmpty(regionSize, regionSize, false, Image.Format.Rgb8);

				CopyHeights(subIndex, subRegionSize, regionSize, layer, ref heights, ref heightImg);

				// CopyControls(layer.gridResolution, ref controls, ref controlImg);

				action = new ImgQueuedTerrainCallback<L, C>(
					heightImg, detailImg, null,
					layer, startPos, index
				);
				MainThreadActionQueue.Enqueue(action);
			}
		}
	}

	private void ControlBase(ref uint[,] controlMap)
	{
		for (var z = 0; z < controlMap.GetLength(0); z++)
		for (var x = 0; x < controlMap.GetLength(1); x++)
		{
			controlMap[z, x].SetAutoshaded(true);
		}
	}

	private static void CopyHeights(Point subIndex, Point subRegionSize, int regionSize, L layer, ref float[,] heights, ref Image img)
	{
		var offW = subIndex.x * heights.GetLength(0) / subRegionSize.x;
		var offD = subIndex.y * heights.GetLength(1) / subRegionSize.y;
		var difW = layer.chunkW / heights.GetLength(0);
		var difD = layer.chunkH / heights.GetLength(1);
		for (var x = 0; x < regionSize; x++)
		{
			for (var z = 0; z < regionSize; z++)
			{
				img.SetPixel(x, z, Colors.Red * (heights[z / difW + offD, x / difD + offW]));
			}
		}
	}

	static void CopyControls(
		int resolution,
		ref uint[,] controls,
		ref Image controlsImg
	)
	{
		for (int x = 0; x < controls.GetLength(0); x++)
		{
			for (int y = 0; y < controls.GetLength(1); y++)
			{
				controlsImg.SetPixel(x, y, new Color(controls[x, y])); //TODO: test this will probably omit green blue and alpha ad fail to convert it to Format.Rf
				// controlsImg.SetPixel(x, y, new Color(controls[x, y].X, controls[x, y].Y, controls[x, y].Z, controls[x, y].W));
			}
		}
	}

	private static IEnumerable<byte> CopyControls2(Point subIndex, Point subRegionSize, int regionSize, L layer, uint[,] controls)
	{
		var offW = subIndex.x * controls.GetLength(0) / subRegionSize.x;
		var offD = subIndex.y * controls.GetLength(1) / subRegionSize.y;
		var difW = layer.chunkW / controls.GetLength(0);
		var difD = layer.chunkH / controls.GetLength(1);
		for (var x = 0; x < regionSize; x++)
		for (var z = 0; z < regionSize; z++)
			foreach (var b in BitConverter.GetBytes(controls[z / difW + offD, x / difD + offW]))
				yield return b;
	}

	static IEnumerable<byte> CopyControls2(uint[,] controls)
	{
		for (int z = 0; z < controls.GetLength(0); z++)
		for (int x = 0; x < controls.GetLength(1); x++)
			foreach (var b in BitConverter.GetBytes(controls[z, x]))
				yield return b;
	}

	static void GenerateDetails(
		int resolution, in RandomHash rand,
		ref uint[,] controls
	)
	{
		for (int x = GridOffset; x < resolution - GridOffset; x++)
		{
			for (int z = GridOffset; z < resolution - GridOffset; z++)
			{
				//TODO: splatmapping
				// uint controlsAvg = 0.25f * (controls[z, x] + controls[z + 1, x] + controls[z, x + 1] + controls[z + 1, x + 1]);
				// float grassControlAvg = controlsAvg.X;
				// if (grassControlAvg > 0.4f)
				// {
				//     float grassControlMax = Mathf.Max(
				//         Mathf.Max(controls[z, x].X, controls[z + 1, x].X),
				//         Mathf.Max(controls[z, x + 1].X, controls[z + 1, x + 1].X)
				//     );
				//     float grassDetailVal = grassControlMax * 10f + rand.Range(-0.5f, 0.5f, x, z, 9);
				//     detailMap[z, x] = Mathf.RoundToInt(grassDetailVal);
				// }
				// else
				{
					// detailMap[z, x] = 0;
				}
			}
		}
	}

	static void HeightNoise(in DPoint terrainOrigin, in DPoint cellSize, int gridResolution,
		ref float[,] heights, ref Vector3[,] dists, float terrainHeight, float terrainBaseHeight)
	{
		var inverseTerrainHeight = 1f / terrainHeight;

		for (var zRes = 0; zRes < gridResolution; zRes++)
		{
			for (var xRes = 0; xRes < gridResolution; xRes++)
			{
				var p = (Vector2)(terrainOrigin + new Point(xRes, zRes) * cellSize);
				heights[zRes, xRes] = TerrainNoise.GetHeight(p);
				// dists[zRes, xRes] = new Vector3(0f, 0f, 1000f);
			}
		}
	}

	static void HandleControls(
		in DPoint terrainOrigin, in DPoint cellSize, int gridResolution,
		ref float[,] heights, ref uint[,] controls
	)
	{
		// Skip edges in iteration - we need those for calculating normal only.
		float doubleCellSize = 2f * (float)cellSize.x;
		for (var zRes = 1; zRes < gridResolution - 1; zRes++)
		{
			for (var xRes = 1; xRes < gridResolution - 1; xRes++)
			{
				uint current = controls[zRes, xRes];
				// current.SetBaseTextureId(1);
				// current.SetOverlayTextureId(0);
				GetNormal(xRes, zRes, doubleCellSize, heights, out Vector3 normal);

				// Handle grass vs cliff based on steepness.
				float cliff = normal.Y < 0.65f ? 1f : 0f;
				Vector4 terrainControl = new Vector4(1f - cliff, cliff, 0f, 0f);

				// Reduce path control where there's cliff control.
				// current.Z = Mathf.Min(current.Z, 1f - cliff);
				//
				// // Apply terrain controls (grass/cliff) with remaining unused weight.
				// float usedWeight = current.X + current.Y + current.Z + current.W;
				// current += terrainControl * (1f - usedWeight);

				controls[zRes, xRes] = current;
			}
		}
	}

	static void GetNormal(int x, int z, float doubleCellSize, in float[,] heights, out Vector3 normal)
	{
		normal = new Vector3(
			heights[z, x + 1] - heights[z, x - 1],
			doubleCellSize,
			heights[z + 1, x] - heights[z - 1, x]
		).Normalized();
	}

	static void HandleEdges(int fromEdge, float lowerDist, ref float[,] heights)
	{
		for (int i = fromEdge; i < heights.GetLength(0) - fromEdge; i++)
		{
			heights[fromEdge, i] -= lowerDist;
			heights[i, fromEdge] -= lowerDist;
			heights[heights.GetLength(0) - fromEdge - 1, i] -= lowerDist;
			heights[i, heights.GetLength(0) - fromEdge - 1] -= lowerDist;
		}
	}
}
