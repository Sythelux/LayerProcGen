using Godot;
using Runevision.Common;
using Runevision.LayerProcGen;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Terrain3DBindings;

/// <summary>
/// Unlike Unity The Terrain3D Plugin in Godot has some differences:
/// - built-in LOD, which also acts differently than the rest of Godot
///     - we can remove the LOD from here and let Terrain3D handle it internally
///     - TODO: the lod is also used to decide if CultivationLayer and LocationLayer is being active or not
/// - the Region Size is currently limited to 1024, more might come in the future
///     - this is fine we can just remove the Terrain Variation A-D and only leave one that has 1024 size
/// - one Terrain3D holds multiple regions as Array
///     - we use those such, that a Chunk is a region.
/// </summary>
/// <typeparam name="L"></typeparam>
/// <typeparam name="C"></typeparam>
public struct QueuedTerrainCallback<L, C> : IQueuedAction
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public float[,] heightmap;
    public float[,,] splatmap;
    public int[,] detailMap;
    public MeshInstance3D[] treeInstances; //there is no TreeInstance in Godot, but we can use Meshinstance, should be as powerful
    public Vector3 position;
    public TransformWrapper chunkParent;
    public L layer;
    public Point index;

    public QueuedTerrainCallback(
        float[,] heightmap,
        float[,,] splatmap,
        int[,] detailMap,
        MeshInstance3D[] treeInstances,
        TransformWrapper chunkParent,
        Vector3 position,
        L layer,
        Point index
    )
    {
        this.heightmap = heightmap;
        this.splatmap = splatmap;
        this.detailMap = detailMap;
        this.treeInstances = treeInstances;
        this.chunkParent = chunkParent;
        this.position = position;
        this.layer = layer;
        this.index = index;
    }

    static Terrain3D? GetOrCreateTerrain(L layer)
    {
        // int unusedCount = layer.unusedTerrains.Count;
        // if (unusedCount > 0)
        // {
        //     Node3D existingTerrain = layer.unusedTerrains[unusedCount - 1];
        //     layer.unusedTerrains.RemoveAt(unusedCount - 1);
        //     Logg.Log("Reusing terrain", false);
        //     return existingTerrain;
        // }

        if (layer.layerParent == null)
        {

            Logg.Log("Creating new terrain", false);

            // Set heights.
            //TerrainData data = new TerrainData(); // Doesn't work anymore for grass detail.
            Terrain3D terrain = new Terrain3D();
            Terrain3DStorage data = TerrainResources.instance.TerrainData;
            data.RegionSize = RegionSize.SIZE_1024; //there is only one size currently
            data.HeightRange = new Vector2(layer.terrainBaseHeight, layer.terrainHeight);
            // data.heightmapResolution = layer.gridResolution + 1;
            // data.alphamapResolution = layer.gridResolution + 1;
            // data.SetDetailResolution(layer.gridResolution, 32);
            // data.size = new Vector3(
            // 	layer.chunkW * 128 / 124,
            // 	layer.terrainHeight,
            // 	layer.chunkH * 128 / 124
            // );

            terrain.Storage = data;
            terrain.TextureList = TerrainResources.instance.TextureList;

            // Set detail maps.
            if (layer.lodLevel == 0)
            {
                // DetailPrototype grassDetail = new DetailPrototype(); //<- proton scatter instead
                // grassDetail.prototypeTexture = TerrainResources.instance.grassDetail;
                // grassDetail.healthyColor = new Color(0.9f, 1.0f, 1.1f);
                // grassDetail.dryColor = new Color(1.1f, 1.0f, 0.9f);
                // grassDetail.minHeight = 0.3f;
                // grassDetail.maxHeight = 0.6f;
                // grassDetail.minWidth = 0.4f;
                // grassDetail.maxWidth = 0.7f;
                // data.detailPrototypes = new DetailPrototype[] { grassDetail };
                // #if UNITY_2022_3_OR_NEWER
                // data.SetDetailScatterMode(DetailScatterMode.InstanceCountMode);
                // #endif
                // data.wavingGrassAmount = 0.03f;
                // data.wavingGrassSpeed = 30;
                // data.wavingGrassStrength = 4;
                // data.wavingGrassTint = Colors.White * 0.7f;
            }

            // Create GameObject and Terrain3D.
            if (layer.lodLevel == 0)
            {
                //TODO: maybe generate nav mesh here?
            }

            Node3D terrainNode = terrain.Instance as Node3D;

            terrainNode.Name = "Terrain3D" + layer.lodLevel;
            // terrainNode.Visible = false;

            // Setup Terrain3D component.
            // terrain.allowAutoConnect = false;
            // terrain.drawInstanced = true;
            //TODO: this is "inverted" somehow terrain.mesh_lods = layer.lodLevel;
            // terrain.heightmapPixelError = 8;
            terrain.Material = TerrainResources.instance.Material;
            // terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            // terrain.detailObjectDistance = 50;
            // terrain.treeBillboardDistance = 70;
            // terrain.treeDistance = 2000;
            layer.layerParent = terrain;

        }
        return layer.layerParent;
    }

    public void Process()
    {
        LayerManagerBehavior.instance.StartCoroutine(ProcessRoutine());
    }

    public IEnumerator? ProcessRoutine()
    {
        Terrain3D? terrain = GetOrCreateTerrain(layer);

        // Profiler.BeginSample("Get terrainData");
        // var terrainNode = terrain.FindChildren("*", nameof(Terrain3D), owned: false).FirstOrDefault();
        if (terrain == null)
            yield break;

        // Profiler.EndSample();

        int sliceCount = 16;
        int res = heightmap.GetLength(0);
        int sliceSize = (res - 1) / sliceCount;
        // float[,] slice = new float[sliceSize + 1, res];
        for (int i = 0; i < sliceCount; i++)
        {
            // Profiler.BeginSample("CopyIntoSlice");
            int offset = i * sliceSize;
            for (int z = 0; z < sliceSize + 1; z++)
            {
                for (int x = 0; x < res; x++)
                {
                    // slice[z, x] = heightmap[z + offset, x];
                    terrain.Storage.SetHeight(new Vector3(x + (index.x * (res - 1)), 0, (z + offset) + (index.y * (res - 1))), heightmap[z + offset, x]);
                    //TODO: setting single pixels is very slow and we should instead make the heightmap array an image
                }
            }
            Console.Write("|");
            Console.Write($"{0 + index.x * (res - 1)};{(0 + offset) + (index.y * (res - 1))};");
            Console.Write($"{(res - 1) + index.x * (res - 1)};{((sliceSize) + offset) + (index.y * (res - 1))}");
            Console.WriteLine();

            // Profiler.EndSample();
            // Profiler.BeginSample("SetHeightsDelayLOD");
            // data.SetHeightsDelayLOD(0, offset, slice);
            // Profiler.EndSample();
            terrain.Storage.ForceUpdateMaps(MapType.TYPE_HEIGHT);
            yield return null;
        }
        // Profiler.BeginSample("SyncHeightmap");
        // Profiler.EndSample();

        // Profiler.BeginSample("SetAlphamaps");
        // data.SetAlphamaps(0, 0, splatmap);
        // Profiler.EndSample();

        if (layer.lodLevel == 0)
        {
            // Profiler.BeginSample("SetDetailLayer");
            // data.SetDetailLayer(0, 0, 0, detailMap);
            // Profiler.EndSample();
        }

        // Profiler.BeginSample("Set treeInstances");
        // if (treeInstances != null)
        // data.treeInstances = treeInstances;
        // Profiler.EndSample();

        // chunkParent.AddChild(terrain.Instance as Node3D);

        // terrain.Position = position;

        // Profiler.BeginSample("Flush");
        // terrain.Flush();
        // Profiler.EndSample();

        // Profiler.BeginSample("Register");
        TerrainLODManager.instance.RegisterChunk(layer.lodLevel, index, terrain.Storage);
        // Profiler.EndSample();
    }
}

public struct QueuedTerrainRecycleCallback<L, C> : IQueuedAction
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public TransformWrapper chunkParent;
    public L layer;
    public Point index;

    public void Process()
    {
        if (chunkParent.transform == null)
        {
            // If terrain was not finished instantiating, we have to wait destroying it.
            MainThreadActionQueue.EnqueueNextFrame(this);
            return;
        }

        Terrain3D terrain = new Terrain3D(chunkParent.transform); //.transform.FindChildren("*", nameof(Terrain3D)).FirstOrDefault()
        if (terrain.Instance is Node3D terrainInstance)
        {
            terrainInstance.Reparent(default);
            terrainInstance.Visible = false;
            // layer.unusedTerrains.Add(terrainInstance.GetParentNode3D());
            chunkParent.transform.QueueFree();
            TerrainLODManager.instance.UnregisterChunk(layer.lodLevel, index);
        }
    }
}

public abstract class LandscapeChunk<L, C> : LayerChunk<L, C>, IGodotInstance
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public TransformWrapper chunkParent;


    // Array<float>? heightsNA;
    // Array<Vector3>? distsNA;
    // Array<Vector4>? splatsNA;
    public float[,] heights; //TODO: Godot Arrays are slower than Native arrays, but at least they seem like memory safe pointer array alternative?
    public Vector3[,] dists;
    public Vector4[,] splats;
    float[,] heightsArray;
    float[,,] splatsArray;
    int[,] detailMap;

    public LandscapeChunk()
    {
        heights = new float[layer.gridResolution + 1, layer.gridResolution + 1];
        dists = new Vector3[layer.gridResolution + 1, layer.gridResolution + 1];
        splats = new Vector4[layer.gridResolution + 1, layer.gridResolution + 1];
        heightsArray = new float[layer.gridResolution + 1, layer.gridResolution + 1];
        splatsArray = new float[layer.gridResolution + 1, layer.gridResolution + 1, 3];
        detailMap = new int[layer.gridResolution, layer.gridResolution];
        LayerManager.instance.abort += Dispose;
    }

    public void Dispose()
    {
        // heightsNA?.Clear();
        // distsNA?.Clear();
        // splatsNA?.Clear();
    }

    public override void Create(int level, bool destroy)
    {
        if (destroy)
        {
            QueuedTerrainRecycleCallback<L, C> action =
                new QueuedTerrainRecycleCallback<L, C>()
                {
                    chunkParent = chunkParent, layer = layer, index = index
                };
            MainThreadActionQueue.Enqueue(action);

            // if (heightsNA != null)
            // {
            // heights.Clear();
            // dists.Clear();
            heightsArray.Clear();
            // splats.Clear();
            splatsArray.Clear();
            detailMap.Clear();
            // }
        }
        else
        {
            Build();
            // chunkParent = new TransformWrapper(layer.layerParent.Instance as Node3D, index);
        }
    }

    const int GridOffset = 4;

    // static ListPool<LocationSpec> locationSpecListPool = new ListPool<LocationSpec>(128);
    // static ListPool<PathSpec> pathSpecListPool = new ListPool<PathSpec>(128);

    void Build()
    {
        SimpleProfiler.ProfilerHandle ph;

        DPoint cellSize = (DPoint)layer.chunkSize / layer.chunkResolution;
        DPoint terrainOrigin = index * layer.chunkSize - cellSize * GridOffset;

        // Apply noise heights.
        ph = SimpleProfiler.Begin(phc, "Height Noise");
        Vector2 terrainHeight = new Vector2(layer.terrainBaseHeight, layer.terrainHeight);
        HeightNoise(terrainOrigin, cellSize, layer.gridResolution, ref terrainHeight, ref heights, ref dists);
        // heights.Print();
        SimpleProfiler.End(ph);

        if (layer.lodLevel < 3)
        {
            // Apply deformation from locations.
            ph = SimpleProfiler.Begin(phc, "Deform-Locations");
            // List<LocationSpec> locationSpecs = locationSpecListPool.Get();
            // LocationLayer.instance.GetLocationSpecsOverlappingBounds(this, locationSpecs, bounds);
            // TerrainDeformation.ApplySpecs(
            // 	heightsNA, distsNA, splatsNA,
            // 	index * layer.chunkResolution - Point.one * GridOffset,
            // 	Point.one * (layer.gridResolution + 1),
            // 	((Vector2)layer.chunkSize) / layer.chunkResolution,
            // 	locationSpecs,
            // 	(SpecPoint p) => {
            // 		p.centerElevation = 0;
            // 		return p;
            // 	});
            // locationSpecListPool.Return(ref locationSpecs);
            SimpleProfiler.End(ph);

            if (layer.lodLevel < 2)
            {
                // Apply deformation from paths.
                ph = SimpleProfiler.Begin(phc, "Deform-Paths");
                // List<PathSpec> pathSpecs = pathSpecListPool.Get();
                // CultivationLayer.instance.GetPathsOverlappingBounds(this, pathSpecs, bounds);
                // TerrainDeformation.ApplySpecs(
                // 	heightsNA, distsNA, splatsNA,
                // 	index * layer.chunkResolution - Point.one * GridOffset,
                // 	Point.one * (layer.gridResolution + 1),
                // 	((Vector2)layer.chunkSize) / layer.chunkResolution,
                // 	pathSpecs);
                // pathSpecListPool.Return(ref pathSpecs);
                SimpleProfiler.End(ph);
            }
        }

        RandomHash rand = new RandomHash(123);
        RandomHash rand2 = new RandomHash(234);

        ph = SimpleProfiler.Begin(phc, "Splat Noise (GetNormal)");
        HandleSplats(terrainOrigin, cellSize, layer.gridResolution, ref heights, ref splats);
        SimpleProfiler.End(ph);


        ph = SimpleProfiler.Begin(phc, "Handle Edges");
        float lowering = 1 << layer.lodLevel;
        HandleEdges(0, lowering * 1.00f, ref heights);
        HandleEdges(1, lowering * 0.20f, ref heights);
        HandleEdges(2, lowering * 0.04f, ref heights);
        HandleEdges(3, lowering * 0.02f, ref heights);
        SimpleProfiler.End(ph);

        ph = SimpleProfiler.Begin(phc, "Copy Heights");

        var heightsPointerArray = heightsArray.AsSpan();
        CopyHeights(layer.gridResolution, layer.terrainBaseHeight, layer.terrainHeight, heights.AsReadOnlySpan(), ref heightsPointerArray);

        SimpleProfiler.End(ph);

        ph = SimpleProfiler.Begin(phc, "Copy Splats");
        // var splatsPointerArray = new Array3D<float>(splatsArray);
        // CopySplats(layer.gridResolution, splats, ref splatsPointerArray);
        SimpleProfiler.End(ph);

        if (layer.lodLevel < 1)
        {
            ph = SimpleProfiler.Begin(phc, "Generate Details");
            // var detailMapPointerArray = detailMap.AsSpan();
            GenerateDetails(layer.gridResolution, rand, splats, ref detailMap);
            SimpleProfiler.End(ph);
        }

        float height = layer.terrainBaseHeight;
        float posOffset = -GridOffset * layer.chunkW / layer.chunkResolution;
        QueuedTerrainCallback<L, C> action = new QueuedTerrainCallback<L, C>(
            heightsArray, splatsArray, detailMap, null, chunkParent,
            new Vector3(index.x * layer.chunkW + posOffset, height, index.y * layer.chunkH + posOffset),
            layer, index
        );

        MainThreadActionQueue.Enqueue(action);
    }

    // [BurstCompile]
    static void HeightNoise(
        in DPoint terrainOrigin, in DPoint cellSize, int gridResolution, ref Vector2 terrainHeight,
        ref float[,] heights, ref Vector3[,] dists
    )
    {
        float totalHeight = Mathf.Abs(terrainHeight.X) + terrainHeight.Y;
        float min = Math.Abs(terrainHeight.X);
        for (var zRes = 0; zRes < gridResolution; zRes++)
        {
            for (var xRes = 0; xRes < gridResolution; xRes++)
            {
                var p = (Vector2)(terrainOrigin + new Point(xRes, zRes) * cellSize);
                heights[zRes, xRes] = LandscapeLayer<L, C>.TerrainNoise.GetNoise2Dv(p) * (totalHeight - min) + min;
                dists[zRes, xRes] = new Vector3(0f, 0f, 1000f);
            }
        }
    }

    // [BurstCompile]
    static void HandleSplats(
        in DPoint terrainOrigin, in DPoint cellSize, int gridResolution,
        ref float[,] heights, ref Vector4[,] splats
    )
    {
        // Skip edges in iteration - we need those for calculating normal only.
        float doubleCellSize = 2f * (float)cellSize.x;
        for (var zRes = 1; zRes < gridResolution; zRes++)
        {
            for (var xRes = 1; xRes < gridResolution; xRes++)
            {
                Vector4 current = splats[zRes, xRes];
                GetNormal(xRes, zRes, doubleCellSize, heights, out Vector3 normal);

                // Handle grass vs cliff based on steepness.
                float cliff = normal.Y < 0.65f ? 1f : 0f;
                Vector4 terrainSplat = new Vector4(1f - cliff, cliff, 0f, 0f);

                // Reduce path splat where there's cliff splat.
                current.Z = Mathf.Min(current.Z, 1f - cliff);

                // Apply terrain splats (grass/cliff) with remaining unused weight.
                float usedWeight = current.X + current.Y + current.Z + current.W;
                current += terrainSplat * (1f - usedWeight);

                splats[zRes, xRes] = current;
            }
        }
    }

    // [BurstCompile]
    static void GetNormal(int x, int z, float doubleCellSize, in float[,] heights, out Vector3 normal)
    {
        normal = new Vector3(
            heights[z, x + 1] - heights[z, x - 1],
            doubleCellSize,
            heights[z + 1, x] - heights[z - 1, x]
        ).Normalized();
    }

    // [BurstCompile]
    static void HandleEdges(int fromEdge, float lowerDist, ref float[,] heights)
    {
        for (int i = fromEdge; i < heights.GetLength(0) - fromEdge; i++) //GetLength(0) was width
        {
            heights[fromEdge, i] -= lowerDist;
            heights[i, fromEdge] -= lowerDist;
            heights[heights.GetLength(0) - fromEdge - 1, i] -= lowerDist;
            heights[i, heights.GetLength(0) - fromEdge - 1] -= lowerDist;
        }
    }

    // [BurstCompile] <- we don't have burst, but we have native SIMD
    static void CopyHeights(
        int resolution, float terrainBaseHeight, float terrainHeight,
        in ReadOnlySpan<float> input,
        ref Span<float> results
    )
    {
        if (Sse.IsSupported)
        {
            var inverseTerrainHeight = Vector128.CreateScalar(1f / terrainHeight);
            var resultVectors = MemoryMarshal.Cast<float, Vector128<float>>(results);

            var inputVectors = MemoryMarshal.Cast<float, Vector128<float>>(input);
            var terrainBaseHeightVec = Vector128.CreateScalar(terrainBaseHeight);

            for (int i = 0; i < inputVectors.Length; i++)
            {
                resultVectors[i] = inputVectors[i];
                // resultVectors[i] = Sse.MultiplyScalar(Sse.SubtractScalar(inputVectors[i], terrainBaseHeightVec), inverseTerrainHeight);
            }
        }
        else
        {
            CopyHeightsSlow(resolution, terrainBaseHeight, terrainHeight, input, ref results);
        }
    }
    static void CopyHeightsSlow(int resolution, float terrainBaseHeight, float terrainHeight,
        in ReadOnlySpan<float> input,
        ref Span<float> results
    )
    {
    }

    // [BurstCompile]
    static void CopySplats(
        int resolution,
        in Vector4[,] splats,
        ref float[,,] splatsArray
    )
    {
        for (var zRes = 0; zRes < resolution + 1; zRes++)
        {
            for (var xRes = 0; xRes < resolution + 1; xRes++)
            {
                splatsArray[zRes, xRes, 0] = splats[zRes, xRes].X;
                splatsArray[zRes, xRes, 1] = splats[zRes, xRes].Y;
                splatsArray[zRes, xRes, 2] = splats[zRes, xRes].Z;
            }
        }
    }

    // [BurstCompile]
    static void GenerateDetails(
        int resolution, in RandomHash rand,
        in Vector4[,] splats,
        ref int[,] detailMap
    )
    {
        for (int x = GridOffset; x < resolution - GridOffset; x++)
        {
            for (int z = GridOffset; z < resolution - GridOffset; z++)
            {
                Vector4 splatsAvg = 0.25f * (splats[z, x] + splats[z + 1, x] + splats[z, x + 1] + splats[z + 1, x + 1]);
                float grassSplatAvg = splatsAvg.X;
                if (grassSplatAvg > 0.4f)
                {
                    float grassSplatMax = Mathf.Max(
                        Mathf.Max(splats[z, x].X, splats[z + 1, x].X),
                        Mathf.Max(splats[z, x + 1].X, splats[z + 1, x + 1].X)
                    );
                    float grassDetailVal = grassSplatMax * 10f + rand.Range(-0.5f, 0.5f, x, z, 9);
                    detailMap[z, x] = Mathf.RoundToInt(grassDetailVal);
                }
                else
                {
                    detailMap[z, x] = 0;
                }
            }
        }
    }

    public Node LayerRoot() => chunkParent.transform ?? new Node { Name = "No Chunkparent" };
}

public abstract class LandscapeLayer<L, C> : ChunkBasedDataLayer<L, C>, IGodotInstance
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public static FastNoiseLite TerrainNoise;

    public abstract int lodLevel { get; }

    public const int GridResolution = 256;
    public int gridResolution = GridResolution;
    public int chunkResolution = GridResolution;
    public float terrainBaseHeight = -100;
    public float terrainHeight = 200;

    public Terrain3D? layerParent;

    // public List<Node3D> unusedTerrains = new List<Node3D>();

    public LandscapeLayer()
    {
        // if (lodLevel < 2)
        // 	AddLayerDependency(new LayerDependency(CultivationLayer.instance, CultivationLayer.requiredPadding, 0));
        // if (lodLevel < 3)
        // 	AddLayerDependency(new LayerDependency(LocationLayer.instance, LocationLayer.requiredPadding, 1));
    }

    static LandscapeLayer()
    {
        TerrainNoise = new FastNoiseLite();
        TerrainNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);

        TerrainNoise.SetFrequency(0.002f);
        TerrainNoise.SetFractalLacunarity(2f);
        TerrainNoise.SetFractalGain(0.5f);
        TerrainNoise.SetFractalOctaves(6);

        TerrainNoise.SetFractalType(FastNoiseLite.FractalTypeEnum.Fbm);
    }

    public Terrain3D GetTerrainAtWorldPos(Vector3 worldPos)
    {
        return layerParent; //TODO: do some boundary check

        // if (GetChunkOfGridPoint(null,
        //     Mathf.FloorToInt(worldPos.X), Mathf.FloorToInt(worldPos.Z),
        //     chunkW, chunkH, out C chunk, out Point point)
        // )
        // {
        //     return new Terrain3D(chunk.chunkParent.transform?.FindChildren("*", nameof(Terrain3D)).FirstOrDefault());
        // }
        //
        // return null;
    }

    public Node LayerRoot() => layerParent?.Instance as Node3D;
}

/// <summary>
/// Aka Terrain3D base class holder
/// </summary>
public class LandscapeLayerTerrain3D : LandscapeLayer<LandscapeLayerTerrain3D, LandscapeChunkTerrain3D>
{
    public override int lodLevel
    {
        get { return 0; }
    }

    public override int chunkW
    {
        get { return 1024; }
    }

    public override int chunkH
    {
        get { return 1024; }
    }
}

/// <summary>
/// aka wrapper for Terrain3DStorage.Regions
/// </summary>
public class LandscapeChunkTerrain3D : LandscapeChunk<LandscapeLayerTerrain3D, LandscapeChunkTerrain3D>
{
}
