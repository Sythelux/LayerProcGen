#nullable enable
using Runevision.Common;
using Runevision.LayerProcGen;
using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Godot.Util;

public class GeoGridChunk : LayerChunk<GeoGridLayer, GeoGridChunk>, IDisposable
{
    public float[,] heights;
    public Vector3[,] dists;
    public uint[,] controls;

    public GeoGridChunk()
    {
        heights = new float[layer.gridChunkRes.y, layer.gridChunkRes.x];
        dists = new Vector3[layer.gridChunkRes.y, layer.gridChunkRes.x];
        controls = new uint[layer.gridChunkRes.y, layer.gridChunkRes.x];
        // LayerManager.instance.abort += Dispose;
    }

    public void Dispose()
    {

    }

    public override void Create(int level, bool destroy)
    {
        if (destroy)
        {
            heights.Clear();
            dists.Clear();
            controls.Clear();
        }
        else
        {
            Build();
        }
    }

    static ListPool<LocationSpec> locationSpecListPool = new ListPool<LocationSpec>(64);

    void Build()
    {
        Point gridChunkRes = layer.gridChunkRes;
        Point gridOrigin = index * gridChunkRes;
        SimpleProfiler.ProfilerHandle ph;

        // Apply terrain noise.
        ph = SimpleProfiler.Begin(phc, "Height Noise");
        for (var zRes = 0; zRes < gridChunkRes.y; zRes++)
        {
            for (var xRes = 0; xRes < gridChunkRes.x; xRes++)
            {
                DPoint p = new DPoint(
                    (gridOrigin.x + xRes) * TerrainPathFinder.halfCellSize,
                    (gridOrigin.y + zRes) * TerrainPathFinder.halfCellSize
                );
                heights[zRes, xRes] = TerrainNoise.GetHeight((Vector2)p);
                dists[zRes, xRes] = new Vector3(0f, 0f, 1000f);
            }
        }
        SimpleProfiler.End(ph);

        // Apply deformation from locations.
        ph = SimpleProfiler.Begin(phc, "Deform-LocationDeformation");
        List<LocationSpec> locationSpecs = locationSpecListPool.Get();
        LocationLayer.instance.GetLocationSpecsOverlappingBounds(this, locationSpecs, bounds);
        TerrainDeformation.ApplySpecs(
        	ref heights, ref dists, ref controls,
        	gridOrigin,
        	gridChunkRes,
        	Point.one * TerrainPathFinder.halfCellSize,
        	locationSpecs,
        	(SpecPointB p) => {
        		p.innerWidth += 2;
        		p.centerElevation = 0;
        		return p;
        	},
        	(SpecData d) => {
        		d.bounds = new Vector4(
        			d.bounds.X - 2, d.bounds.Y - 2, d.bounds.Z + 2, d.bounds.W + 2);
        		return d;
        	});
        locationSpecListPool.Return(ref locationSpecs);
        SimpleProfiler.End(ph);
    }
}
