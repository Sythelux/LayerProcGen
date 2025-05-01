#nullable enable
using Runevision.Common;
using Runevision.LayerProcGen;
using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using Godot.Util;

// The GeoGridLayer is an intermediary layer between the LocationLayer and the CultivationLayer.
// Each CultivationLayer chunk needs data well outside of its own bounds.
// If they were each to calculate that themselves, there would be a lot of redundant
// calculations of the overlapping areas. The GeoGridLayer performs these calculations
// instead (essentially caching them) so multiple CultivationLayer chunks can use
// the same already calculated data.

public class GeoGridLayer : ChunkBasedDataLayer<GeoGridLayer, GeoGridChunk>
{
    public override int chunkW { get { return 360; } }
    public override int chunkH { get { return 360; } }

    public Point gridChunkRes;

    public GeoGridLayer()
    {
        gridChunkRes = chunkSize / TerrainPathFinder.halfCellSize;

        AddLayerDependency(new LayerDependency(LocationLayer.instance, LocationLayer.requiredPadding, 1));
    }

    public void GetDataInBounds(ILC q, GridBounds bounds, float[,] heights, Vector3[,] dists, uint[,] controls)
    {
        HandleGridPoints(q, bounds, chunkSize / TerrainPathFinder.halfCellSize,
            (GeoGridChunk chunk, Point localPointInChunk, Point globalPoint) =>
            {
                int x = globalPoint.x - bounds.min.x;
                int z = globalPoint.y - bounds.min.y;
                heights[z, x] = chunk.heights[localPointInChunk.y, localPointInChunk.x];
                dists[z, x] = chunk.dists[localPointInChunk.y, localPointInChunk.x];
                controls[z, x] = chunk.controls[localPointInChunk.y, localPointInChunk.x];
            }
        );
    }

        public GeoGridChunk GetChunk(Point index)
    {
        lock(chunks)
        {
            GeoGridChunk chunk = chunks[index];
            return chunk != null && chunk.level >= 0 ? chunk : null;
        }
    }

    public float SampleHeightAt(float x, float z)
    {
        float cellSize    = TerrainPathFinder.fullCellSize; // not halfCellSize
        int   globalX     = Mathf.FloorToInt(x / cellSize);
        int   globalZ     = Mathf.FloorToInt(z / cellSize);

        // which chunk am I in?
        var chunkIdx     = new Point(globalX / gridChunkRes.x, globalZ / gridChunkRes.y);
        var chunk        = GetChunk(chunkIdx);
        if (chunk == null) return 0;

        // local floating‐point cell coords inside the chunk
        float localXF    = (x / cellSize) - chunkIdx.x * gridChunkRes.x;
        float localZF    = (z / cellSize) - chunkIdx.y * gridChunkRes.y;

        int ix           = Mathf.Clamp(Mathf.FloorToInt(localXF), 0, gridChunkRes.x - 2);
        int iz           = Mathf.Clamp(Mathf.FloorToInt(localZF), 0, gridChunkRes.y - 2);
        float fx         = localXF - ix;
        float fz         = localZF - iz;

        // grab the four corners
        float h00 = chunk.heights[iz    , ix    ];
        float h10 = chunk.heights[iz    , ix + 1];
        float h01 = chunk.heights[iz + 1, ix    ];
        float h11 = chunk.heights[iz + 1, ix + 1];

        // bilinear interpolate and return
        float h0 = Mathf.Lerp(h00, h10, fx);
        float h1 = Mathf.Lerp(h01, h11, fx);
        return Mathf.Lerp(h0,  h1,  fz);
    }
}
