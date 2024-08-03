using System;
using System.Collections;
using System.Collections.Generic;
using GDExtension.Wrappers;
using Godot;
using Godot.Collections;
using Godot.Util;
using Runevision.Common;
using Runevision.LayerProcGen;
using Terrain3DDemo.Scripts.Generation.Layers;
using Terrain3DDemo.Scripts.Utilities;
using Terrain3DExtensions;

public struct MapQueuedTerrainCallback<L, C> : IQueuedAction
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public Image heightmap;
    public Image detailMap;
    public MeshInstance3D[] treeInstances; //there is no TreeInstance in Godot, but we can use Meshinstance, should be as powerful
    private readonly List<Transform3D> grassTransforms;
    public L layer;
    public Point index;
    readonly DPoint terrainOrigin;
    private readonly int regionSize;

    public MapQueuedTerrainCallback(Image heightmap,
        Image detailMap,
        MeshInstance3D[] treeInstances,
        List<Transform3D> grassTransforms,
        L layer,
        Point index,
        DPoint terrainOrigin
    )
    {
        this.heightmap = heightmap;
        this.detailMap = detailMap;
        this.treeInstances = treeInstances;
        this.grassTransforms = grassTransforms;
        this.layer = layer;
        this.index = index;
        this.terrainOrigin = terrainOrigin;
        regionSize = (int)Terrain3DStorage.RegionSizeEnum.Size1024;
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
        var storage = TerrainLODManager.instance.Terrain3D.Storage;
        var instancer = TerrainLODManager.instance.Terrain3D.Instancer;
        var startPos = index * layer.chunkW;
        Terrain3DRegion? terrain = GetOrCreateTerrain(new Vector3(startPos.x, 0, startPos.y), layer);
        if (terrain?.HeightMap == null)
            yield break;
        startPos.x = (startPos.x + regionSize) % regionSize;
        startPos.y = (startPos.y + regionSize) % regionSize;
        // Terrain3DRegion? terrain = GetOrCreateTerrain(new Vector3(startPos.x, 0, startPos.y), layer);
        // if (terrain == null)
        //     yield break;

        terrain.HeightMap ??= Image.CreateEmpty(regionSize, regionSize, false, Image.Format.Rf);
        terrain.ControlMap ??= Image.CreateEmpty(regionSize, regionSize, false, Image.Format.Rf);
        DPoint cellMultiplier = (DPoint)layer.chunkSize / layer.gridResolution; // 128/256
        // float minHeight = layer.terrainBaseHeight;
        // float totalHeight = layer.terrainHeight - layer.terrainBaseHeight;
        // TerrainLODManager.instance.terrain3D.Storage.HeightRange = new Vector2(minHeight, layer.terrainHeight);
        if (grassTransforms != null)
            instancer?.AddTransforms(TerrainLODManager.instance.Terrain3D.Assets.MeshList[0].Id, new Array<Transform3D>(grassTransforms), new Array<Color>());

        // GD.Print($"HandleUnderSizedRegions: {cellMultiplier}, {layer.chunkSize}, {layer.gridResolution}, {startPos}");
        // GD.Print($"\t: {layer.lodLevel} {position} in region:{terrain.RegionOffset}, {startPos}; on index:{index}, {index * layer.chunkW}");

        terrain.HeightMap.BlitRect(heightmap, new Rect2I(0, 0, heightmap.GetWidth(), heightmap.GetHeight()), new Vector2I(startPos.x, startPos.y));

        // for (var x = 0; x < layer.chunkSize.x; x++)
        // {
        //     for (var z = 0; z < layer.chunkSize.y; z++)
        //     {
        //         var globalPosition = new Vector3(startPos.x + x, 0, startPos.y + z);
        //         // storage.SetHeight(globalPosition, heightmap.GetPixel(z, x).R);
        //         storage.SetControl(globalPosition, (int)controlmap[(int)(z / cellMultiplier.y), (int)(x / cellMultiplier.x)]);
        //     }
        //
        //     yield return null;
        //     // TerrainLODManager.instance.Terrain3D.Storage.ForceUpdateMaps((int)Terrain3DStorage.MapType.TypeMax);
        // }

        storage.ForceUpdateMaps((int)Terrain3DStorage.MapType.TypeMax);
        yield return null;
    }
}

public abstract class LandscapeChunk<L, C> : LayerChunk<L, C>
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    static ListPool<LocationSpec> locationSpecListPool = new ListPool<LocationSpec>(128);
    static ListPool<PathSpec> pathSpecListPool = new ListPool<PathSpec>(128);

    protected float[,] heights;
    public uint[,] controls;
    public Vector3[,] dists;
    private List<Transform3D> grassTransforms;

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
        try
        {
            SimpleProfiler.ProfilerHandle ph;

            //e.g. chunkSize: 512, gridRes/img: 256, regionSize: 1024

            DPoint cellMultiplier = (DPoint)layer.chunkSize / layer.gridResolution;
            var regionSize = (int)Terrain3DStorage.RegionSizeEnum.Size1024;
            DPoint regionFactor = (DPoint)layer.chunkSize / regionSize;
            DPoint terrainOrigin = index * layer.chunkSize /*- cellMultiplier * GridOffset*/;

            // GD.Print("index: ", index, ",\tchunkSize: ", layer.chunkSize, ",\tgridRes/img: ", layer.gridResolution, ", regionSize: ", regionSize, ", cm: ", cellMultiplier, ",\trf: ", regionFactor, ",\two: ", worldOffset, ",\t\tb: ", bounds);

            ph = SimpleProfiler.Begin(phc, "Base Noise");
            // float height = layer.terrainBaseHeight;
            BaseNoise(terrainOrigin, cellMultiplier, layer.gridResolution, ref heights, ref dists, ref controls);
            SimpleProfiler.End(ph);

            if (layer.lodLevel < 3)
            {
                // Apply deformation from locations.
                ph = SimpleProfiler.Begin(phc, "Deform-Locations");
                List<LocationSpec> locationSpecs = locationSpecListPool.Get();
                LocationLayer.instance.GetLocationSpecsOverlappingBounds(this, locationSpecs, bounds);
                TerrainDeformation.ApplySpecs(
                    ref heights, ref dists, ref controls,
                    index * layer.gridResolution - Point.one * GridOffset,
                    Point.one * (layer.gridResolution),
                    ((Vector2)layer.chunkSize) / layer.gridResolution,
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
                        index * layer.gridResolution - Point.one * GridOffset,
                        Point.one * (layer.gridResolution),
                        ((Vector2)layer.chunkSize) / layer.gridResolution,
                        pathSpecs);
                    pathSpecListPool.Return(ref pathSpecs);
                    SimpleProfiler.End(ph);
                }
            }

            RandomHash rand = new RandomHash(123);

            ph = SimpleProfiler.Begin(phc, "Splat Noise (GetNormal)");
            HandleControls(terrainOrigin, cellMultiplier, layer.gridResolution, ref heights, ref controls);
            SimpleProfiler.End(ph);

            if (layer.lodLevel < 1)
            {
                ph = SimpleProfiler.Begin(phc, "Generate Details");
                // var detailMapPointerArray = detailMap.AsSpan();
                var grassDetail = new DetailPrototype
                {
                    Color = new Color(0.9f, 1.0f, 1.1f),
                    MinHeight = 0.3f,
                    MaxHeight = 0.6f,
                    MinWidth = 0.4f,
                    MaxWidth = 0.7f
                };
                List<Transform3D> grass = new List<Transform3D>();
                GenerateDetails(terrainOrigin, grassDetail, layer.chunkSize, rand, ref heights, ref controls, ref grass);
                grassTransforms = grass;
                SimpleProfiler.End(ph);
            }


            IQueuedAction action;

            var subRegionSize = new Point(layer.chunkW / regionSize, layer.chunkH / regionSize);

            if (regionFactor.x < 1)
            {
                var heightImg = Image.CreateEmpty((int)(layer.chunkSize.x / cellMultiplier.x), (int)(layer.chunkSize.y / cellMultiplier.y), false, Image.Format.Rf);
                var detailImg = Image.CreateEmpty(regionSize, regionSize, false, Image.Format.Rgb8);

                // CopyHeights(cellMultiplier, baseStartPos, new Point(1, 1), regionSize, layer, ref heights, ref heightImg);
                // CopyHeights(cellMultiplier, ref heights, ref heightImg);
                CopyHeights(new Point(1, 1), subRegionSize, ref heights, ref heightImg);

                // GD.Print($"HandleUnderSizedRegions: {heightImg.GetSize()}, {layer.chunkSize}");
                heightImg.Resize(layer.chunkSize.x, layer.chunkSize.y, Image.Interpolation.Cubic);
                detailImg.Resize(layer.chunkSize.x, layer.chunkSize.y, Image.Interpolation.Cubic);

                action = new MapQueuedTerrainCallback<L, C>(
                    heightImg, detailImg, null, grassTransforms,
                    layer, index, terrainOrigin
                );
                MainThreadActionQueue.Enqueue(action);
            }
            else
            {
                for (int i = 0; i < subRegionSize.x; i++)
                for (int j = 0; j < subRegionSize.y; j++)
                {
                    var subIndex = new Point(i, j);
                    // var baseStartPos = index * layer.chunkW;
                    // var subStartPos = subIndex * regionSize;
                    // var startPos = baseStartPos + subStartPos;

                    var heightImg = Image.CreateEmpty((int)(layer.chunkSize.x / cellMultiplier.x), (int)(layer.chunkSize.y / cellMultiplier.y), false, Image.Format.Rf);
                    var detailImg = Image.CreateEmpty(regionSize, regionSize, false, Image.Format.Rgb8);

                    CopyHeights(subIndex, subRegionSize, ref heights, ref heightImg);

                    // GD.Print($"HandleOverSizedRegions: {subIndex}, {heightImg.GetSize()}, ({regionSize},{regionSize})");
                    heightImg.Resize(regionSize, regionSize, Image.Interpolation.Cubic);

                    // CopyControls(layer.gridResolution, ref controls, ref controlImg);

                    action = new MapQueuedTerrainCallback<L, C>(
                        heightImg, detailImg, null, grassTransforms,
                        layer, index, terrainOrigin
                    );

                    // action = new ImgQueuedTerrainCallback<L, C>(
                    //     heightImg, detailImg, null, grassTransforms,
                    //     layer, startPos, index
                    // );
                    MainThreadActionQueue.Enqueue(action);
                }
            }
        }
        catch (Exception e)
        {
            GD.PushError(e);
        }
    }

    private static void CopyHeights(Point subIndex, Point subRegionSize, ref float[,] heights, ref Image img)
    {
        int offW = subIndex.x * heights.GetLength(0) * subRegionSize.x;
        int offD = subIndex.y * heights.GetLength(1) * subRegionSize.y;
        for (int x = 0; x < img.GetWidth(); x++)
        for (int z = 0; z < img.GetHeight(); z++)
            img.SetPixel(x, z, Colors.Red * heights[z + offD, x + offW]);
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

    /// <summary>
    /// Terrain3D in Godot has Automapping, so this wouldn't be nescessary, but to place the gras instances, we still need it.
    /// </summary>
    /// <param name="terrainOrigin"></param>
    /// <param name="grassDetail"></param>
    /// <param name="resolution"></param>
    /// <param name="rand"></param>
    /// <param name="heights"></param>
    /// <param name="controls"></param>
    /// <param name="grass"></param>
    private static void GenerateDetails(DPoint terrainOrigin, DetailPrototype grassDetail, Point resolution, RandomHash rand, ref float[,] heights, ref uint[,] controls, ref List<Transform3D> grass)
    {
        var heightMapMultiplier = new Vector2I(heights.GetLength(0) / resolution.y, heights.GetLength(1) / resolution.x);
        const byte threshold = (byte)(255 * 0.65);
        for (int x = GridOffset; x < resolution.x - GridOffset; x++)
        {
            for (int z = GridOffset; z < resolution.y - GridOffset; z++)
            {
                // if (controls[x, z].GetTextureBlend() < threshold)
                // {
                for (int i = 0; i < 100; i++)
                {
                    var t = Transform3D.Identity;
                    t = t.Rotated(Vector3.Up, rand.Range(0, Mathf.Pi * 2f, x + z + i));
                    var width = rand.Range(grassDetail.MinWidth, grassDetail.MaxWidth, x + z + i);
                    t = t.Scaled(new Vector3(width, rand.Range(grassDetail.MinHeight, grassDetail.MaxHeight, x + z + i), width));
                    t.Origin = new Vector3((float)(terrainOrigin.x + x + (rand.Value(x + i) * 2 - 1)), 0, (float)(terrainOrigin.y + z + (rand.Value(z + i) * 2 - 1)));
                    t.Origin.Y = heights[z * heightMapMultiplier.Y, x * heightMapMultiplier.X];
                    // GD.Print(t.Origin);
                    grass.Add(t);
                    // }
                }

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

    static void BaseNoise(
        in DPoint terrainOrigin,
        in DPoint cellSize,
        int gridResolution,
        ref float[,] heights,
        ref Vector3[,] dists,
        ref uint[,] controls
    )
    {
        for (var zRes = 0; zRes < gridResolution; zRes++)
        {
            for (var xRes = 0; xRes < gridResolution; xRes++)
            {
                var p = (Vector2)(terrainOrigin + new Point(xRes, zRes) * cellSize);
                heights[zRes, xRes] = TerrainNoise.GetHeight(p);
                dists[zRes, xRes] = new Vector3(0f, 0f, 1000f);
                controls[zRes, xRes].SetAutoshaded(false);
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
                current.SetBaseTextureId(0);
                current.SetOverlayTextureId(1);
                // GD.Print(normal.Y * 255);
                current.SetTextureBlend(Convert.ToByte(normal.Y * 255));

                // float cliff = normal.Y < 0.65f ? 1f : 0f;
                // Vector4 terrainControl = new Vector4(1f - cliff, cliff, 0f, 0f);

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

public abstract class LandscapeLayer<L, C> : ChunkBasedDataLayer<L, C>
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public abstract int lodLevel { get; }

    public const int GridResolution = 256;
    public int gridResolution = GridResolution;
    // public int chunkResolution = GridResolution - 8;
    public float terrainBaseHeight = -100;
    public float terrainHeight = 200;

    protected LandscapeLayer(int rollingGridWidth = 32, int rollingGridHeight = 0, int rollingGridMaxOverlap = 3) : base(rollingGridWidth, rollingGridHeight, rollingGridMaxOverlap)
    {
        TerrainNoise.SetFullTerrainHeight(new Vector2(terrainBaseHeight, terrainHeight));
        if (lodLevel < 2)
            AddLayerDependency(new LayerDependency(CultivationLayer.instance, CultivationLayer.requiredPadding, 0));
        if (lodLevel < 3)
            AddLayerDependency(new LayerDependency(LocationLayer.instance, LocationLayer.requiredPadding, 1));
    }

    // public Image GetTerrainHeight(Vector3 worldPos)
    // {
    //     return layerParent != null &&
    //            layerParent.Storage.HasRegion(worldPos)
    //         ? layerParent.Storage.GetMapRegion(MapType.Height, layerParent.Storage.GetRegionIndex(worldPos))
    //         : new Image();
    // }
    //
    // public Image GetTerrainControl(Vector3 worldPos)
    // {
    //     return layerParent != null &&
    //            layerParent.Storage.HasRegion(worldPos)
    //         ? layerParent.Storage.GetMapRegion(MapType.TYPE_CONTROL, layerParent.Storage.GetRegionIndex(worldPos))
    //         : new Image();
    // }
    //
    // public Image GetTerrainColor(Vector3 worldPos)
    // {
    //     return layerParent != null &&
    //            layerParent.Storage.HasRegion(worldPos)
    //         ? layerParent.Storage.GetMapRegion(MapType.TYPE_COLOR, layerParent.Storage.GetRegionIndex(worldPos))
    //         : new Image();
    // }
}

//@formatter:off
public class LandscapeLayerA : LandscapeLayer<LandscapeLayerA, LandscapeChunkA> {
    public override int lodLevel => 0;
    public override int chunkW => (int)Terrain3DStorage.RegionSizeEnum.Size1024/8;
    public override int chunkH => (int)Terrain3DStorage.RegionSizeEnum.Size1024/8;
}

public class LandscapeLayerB : LandscapeLayer<LandscapeLayerB, LandscapeChunkB> {
    public override int lodLevel => 1;
    public override int chunkW => (int)Terrain3DStorage.RegionSizeEnum.Size1024/4;
    public override int chunkH => (int)Terrain3DStorage.RegionSizeEnum.Size1024/4;
}

public class LandscapeLayerC : LandscapeLayer<LandscapeLayerC, LandscapeChunkC> {
    public override int lodLevel => 2;
    public override int chunkW => (int)Terrain3DStorage.RegionSizeEnum.Size1024/2;
    public override int chunkH => (int)Terrain3DStorage.RegionSizeEnum.Size1024/2;
}

public class LandscapeLayerD : LandscapeLayer<LandscapeLayerD, LandscapeChunkD> {
    public override int lodLevel => 3;
    public override int chunkW => (int)Terrain3DStorage.RegionSizeEnum.Size1024;
    public override int chunkH => (int)Terrain3DStorage.RegionSizeEnum.Size1024;
}

public class LandscapeChunkA : LandscapeChunk<LandscapeLayerA, LandscapeChunkA> { }
public class LandscapeChunkB : LandscapeChunk<LandscapeLayerB, LandscapeChunkB> { }
public class LandscapeChunkC : LandscapeChunk<LandscapeLayerC, LandscapeChunkC> { }
public class LandscapeChunkD : LandscapeChunk<LandscapeLayerD, LandscapeChunkD> { }

//@formatter:on
