#nullable enable
using Runevision.Common;
using Runevision.LayerProcGen;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Terrain3DBindings;

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

    static Node3D GetOrCreateTerrain(L layer)
    {
        int unusedCount = layer.unusedTerrains.Count;
        if (unusedCount > 0)
        {
            Node3D existingTerrain = layer.unusedTerrains[unusedCount - 1];
            layer.unusedTerrains.RemoveAt(unusedCount - 1);
            Logg.Log("Reusing terrain", false);
            return existingTerrain;
        }

        Logg.Log("Creating new terrain", false);

        // Set heights.
        //TerrainData data = new TerrainData(); // Doesn't work anymore for grass detail.
        Terrain3D terrain = new Terrain3D();
        Terrain3DStorage data = TerrainResources.instance.TerrainData;
        // data.heightmapResolution = layer.gridResolution + 1;
        // data.alphamapResolution = layer.gridResolution + 1;
        // data.SetDetailResolution(layer.gridResolution, 32);
        // data.size = new Vector3(
        // 	layer.chunkW * 128 / 124,
        // 	layer.terrainHeight,
        // 	layer.chunkH * 128 / 124
        // );

        // Set splat maps.
        var grassSplat = new Terrain3DTexture();
        var cliffSplat = new Terrain3DTexture();
        var pathSplat = new Terrain3DTexture();
        grassSplat.AlbedoTexture = TerrainResources.instance.grassTex;
        cliffSplat.AlbedoTexture = TerrainResources.instance.cliffTex;
        pathSplat.AlbedoTexture = TerrainResources.instance.pathTex;
        // grassSplat.tileSize = Vector2.One * 3.0f;
        // cliffSplat.tileSize = Vector2.One * 6.0f;
        // pathSplat.tileSize  = Vector2.One * 2.0f;
        terrain.Storage = data;
        terrain.TextureList = new Terrain3DTextureList();
        terrain.TextureList.SetTexture(0, grassSplat);
        terrain.TextureList.SetTexture(1, cliffSplat);
        terrain.TextureList.SetTexture(2, pathSplat);

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
        Node3D newTerrainGameObject = new Node3D();
        if (layer.lodLevel == 0)
        {
            //TODO: maybe generate nav mesh here?
        }

        newTerrainGameObject.AddChild(terrain.Instance as Node3D);
        newTerrainGameObject.Name = "Terrain3D" + layer.lodLevel;
        newTerrainGameObject.Visible = false;

        // Setup Terrain3D component.
        // terrain.allowAutoConnect = false;
        // terrain.drawInstanced = true;
        // terrain.groupingID = layer.lodLevel;
        // terrain.heightmapPixelError = 8;
        terrain.Material = TerrainResources.instance.Material;
        // terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        // terrain.detailObjectDistance = 50;
        // terrain.treeBillboardDistance = 70;
        // terrain.treeDistance = 2000;

        return newTerrainGameObject;
    }

    public void Process()
    {
        LayerManagerBehavior.instance.StartCoroutine(ProcessRoutine());
    }

    public IEnumerator ProcessRoutine()
    {
        Node3D terrain = GetOrCreateTerrain(layer);

        // Profiler.BeginSample("Get terrainData");
        var data = new Terrain3D(terrain.FindChildren("*", nameof(Terrain3D)).FirstOrDefault());
        // Profiler.EndSample();

        int sliceCount = 16;
        int res = heightmap.GetLength(0);
        int sliceSize = (res - 1) / sliceCount;
        float[,] slice = new float[sliceSize + 1, res];
        for (int i = 0; i < sliceCount; i++)
        {
            // Profiler.BeginSample("CopyIntoSlice");
            int offset = i * sliceSize;
            for (int z = 0; z < sliceSize + 1; z++)
            {
                for (int x = 0; x < res; x++)
                {
                    slice[z, x] = heightmap[z + offset, x];
                }
            }

            // Profiler.EndSample();
            // Profiler.BeginSample("SetHeightsDelayLOD");
            // data.SetHeightsDelayLOD(0, offset, slice);
            // Profiler.EndSample();
            yield return null;
        }
        // Profiler.BeginSample("SyncHeightmap");
        // data.SyncHeightmap();
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

        chunkParent.AddChild(data.Instance as Node3D);
        terrain.Position = position;

        // Profiler.BeginSample("Flush");
        // terrain.Flush();
        // Profiler.EndSample();

        // Profiler.BeginSample("Register");
        TerrainLODManager.instance.RegisterChunk(layer.lodLevel, index, terrain);
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

        Terrain3D terrain = new Terrain3D(chunkParent.transform.FindChildren("*", nameof(Terrain3D)).FirstOrDefault());
        if (terrain.Instance is Node3D terrainInstance)
        {
            terrainInstance.Reparent(default);
            terrainInstance.Visible = false;
            layer.unusedTerrains.Add(terrainInstance.GetParentNode3D());
            chunkParent.transform.QueueFree();
            TerrainLODManager.instance.UnregisterChunk(layer.lodLevel, index);
        }
    }
}

// TODO: we have to revisit [BurstCompile]. Godot has a mechanic called Servers, which you can use instead, but needs deep rewrite
public abstract class LandscapeChunk<L, C> : LayerChunk<L, C>, IGodotInstance
    where L : LandscapeLayer<L, C>, new()
    where C : LandscapeChunk<L, C>, new()
{
    public TransformWrapper chunkParent;

    public static FastNoiseLite TerrainNoise;

    Array<float>? heightsNA;
    Array<Vector3>? distsNA;
    Array<Vector4>? splatsNA;
    public Array2D<float> heights;
    public Array2D<Vector3> dists;
    public Array2D<Vector4> splats;
    float[,] heightsArray;
    float[,,] splatsArray;
    int[,] detailMap;

    public LandscapeChunk()
    {
        heights = new Array2D<float>(layer.gridResolution + 1, layer.gridResolution + 1, out heightsNA);
        dists = new Array2D<Vector3>(layer.gridResolution + 1, layer.gridResolution + 1, out distsNA);
        splats = new Array2D<Vector4>(layer.gridResolution + 1, layer.gridResolution + 1, out splatsNA);
        heightsArray = new float[layer.gridResolution + 1, layer.gridResolution + 1];
        splatsArray = new float[layer.gridResolution + 1, layer.gridResolution + 1, 3];
        detailMap = new int[layer.gridResolution, layer.gridResolution];
        LayerManager.instance.abort += Dispose;
    }

    static LandscapeChunk()
    {
        TerrainNoise = new FastNoiseLite();
        TerrainNoise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);

        TerrainNoise.SetFrequency(0.01f);
        TerrainNoise.SetFractalLacunarity(2f);
        TerrainNoise.SetFractalGain(0.5f);

        TerrainNoise.SetFractalType(FastNoiseLite.FractalTypeEnum.Fbm);
    }

    public void Dispose()
    {
        heightsNA?.Clear();
        distsNA?.Clear();
        splatsNA?.Clear();
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

            if (heightsNA != null)
            {
                heights.Clear();
                dists.Clear();
                heightsArray.Clear();
                splats.Clear();
                splatsArray.Clear();
                detailMap.Clear();
            }
        }
        else
        {
            chunkParent = new TransformWrapper(layer.layerParent, index);
            Build();
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
        HeightNoise(terrainOrigin, cellSize, layer.gridResolution, ref heights, ref dists);
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
        unsafe
        {
            var heightsPointerArray = new Array2D<float>(heightsArray);
            CopyHeights(layer.gridResolution, layer.terrainBaseHeight, layer.terrainHeight, heights, ref heightsPointerArray);
        }

        SimpleProfiler.End(ph);

        ph = SimpleProfiler.Begin(phc, "Copy Splats");
        var splatsPointerArray = new Array3D<float>(splatsArray);
        CopySplats(layer.gridResolution, splats, ref splatsPointerArray);
        SimpleProfiler.End(ph);

        if (layer.lodLevel < 1)
        {
            ph = SimpleProfiler.Begin(phc, "Generate Details");
            var detailMapPointerArray = new Array2D<int>(detailMap);
            GenerateDetails(layer.gridResolution, rand, splats, ref detailMapPointerArray);
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
        in DPoint terrainOrigin, in DPoint cellSize, int gridResolution,
        ref Array2D<float> heights, ref Array2D<Vector3> dists
    )
    {
        for (var zRes = 0; zRes <= gridResolution; zRes++)
        {
            for (var xRes = 0; xRes <= gridResolution; xRes++)
            {
                DPoint p = terrainOrigin + new Point(xRes, zRes) * cellSize;
                Vector2 v = (Vector2)p;
                heights[zRes, xRes] = TerrainNoise.GetNoise2D(v.X, v.Y);
                dists[zRes, xRes] = new Vector3(0f, 0f, 1000f);
            }
        }
    }

    // [BurstCompile]
    static void HandleSplats(
        in DPoint terrainOrigin, in DPoint cellSize, int gridResolution,
        ref Array2D<float> heights, ref Array2D<Vector4> splats
    )
    {
        // Skip edges in iteration - we need those for calculating normal only.
        float doubleCellSize = 2f * (float)cellSize.x;
        for (var zRes = 1; zRes <= gridResolution; zRes++)
        {
            for (var xRes = 1; xRes <= gridResolution; xRes++)
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
    static void GetNormal(int x, int z, float doubleCellSize, in Array2D<float> heights, out Vector3 normal)
    {
        normal = new Vector3(
            heights[z, x + 1] - heights[z, x - 1],
            doubleCellSize,
            heights[z + 1, x] - heights[z - 1, x]
        ).Normalized();
    }

    // [BurstCompile]
    static void HandleEdges(int fromEdge, float lowerDist, ref Array2D<float> heights)
    {
        for (int i = fromEdge; i < heights.Width - fromEdge; i++)
        {
            heights[fromEdge, i] -= lowerDist;
            heights[i, fromEdge] -= lowerDist;
            heights[heights.Width - fromEdge - 1, i] -= lowerDist;
            heights[i, heights.Width - fromEdge - 1] -= lowerDist;
        }
    }

    // [BurstCompile]
    static void CopyHeights(
        int resolution, float terrainBaseHeight, float terrainHeight,
        in Array2D<float> heights,
        ref Array2D<float> heightsArray
    )
    {
        float invTerrainHeight = 1f / terrainHeight;
        for (var zRes = 0; zRes < resolution + 1; zRes++)
        {
            for (var xRes = 0; xRes < resolution + 1; xRes++)
            {
                heightsArray[zRes, xRes] = (heights[zRes, xRes] - terrainBaseHeight) * invTerrainHeight;
            }
        }
    }

    // [BurstCompile]
    static void CopySplats(
        int resolution,
        in Array2D<Vector4> splats,
        ref Array3D<float> splatsArray
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
        in Array2D<Vector4> splats,
        ref Array2D<int> detailMap
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
    public abstract int lodLevel { get; }

    public const int GridResolution = 256;
    public int gridResolution = GridResolution;
    public int chunkResolution = GridResolution - 8;
    public float terrainBaseHeight = -100;
    public float terrainHeight = 200;

    public Node3D layerParent;

    public List<Node3D> unusedTerrains = new List<Node3D>();

    public LandscapeLayer()
    {
        layerParent = new Node3D { Name = GetType().Name };
        // if (lodLevel < 2)
        // 	AddLayerDependency(new LayerDependency(CultivationLayer.instance, CultivationLayer.requiredPadding, 0));
        // if (lodLevel < 3)
        // 	AddLayerDependency(new LayerDependency(LocationLayer.instance, LocationLayer.requiredPadding, 1));
    }

    public Terrain3D GetTerrainAtWorldPos(Vector3 worldPos)
    {
        if (GetChunkOfGridPoint(null,
                Mathf.FloorToInt(worldPos.X), Mathf.FloorToInt(worldPos.Z),
                chunkW, chunkH, out C chunk, out Point point)
           )
        {
            return new Terrain3D(chunk.chunkParent.transform?.FindChildren("*", nameof(Terrain3D)).FirstOrDefault());
        }

        return null;
    }

    public Node LayerRoot() => layerParent;
}

public class LandscapeLayerA : LandscapeLayer<LandscapeLayerA, LandscapeChunkA>
{
    public override int lodLevel
    {
        get { return 0; }
    }

    public override int chunkW
    {
        get { return 124; }
    } // 128 - 4

    public override int chunkH
    {
        get { return 124; }
    }
}

public class LandscapeLayerB : LandscapeLayer<LandscapeLayerB, LandscapeChunkB>
{
    public override int lodLevel
    {
        get { return 1; }
    }

    public override int chunkW
    {
        get { return 248; }
    } // 256 - 8

    public override int chunkH
    {
        get { return 248; }
    }
}

public class LandscapeLayerC : LandscapeLayer<LandscapeLayerC, LandscapeChunkC>
{
    public override int lodLevel
    {
        get { return 2; }
    }

    public override int chunkW
    {
        get { return 496; }
    } // 512 - 16

    public override int chunkH
    {
        get { return 496; }
    }
}

public class LandscapeLayerD : LandscapeLayer<LandscapeLayerD, LandscapeChunkD>
{
    public override int lodLevel
    {
        get { return 3; }
    }

    public override int chunkW
    {
        get { return 992; }
    } // 1024 - 32

    public override int chunkH
    {
        get { return 992; }
    }
}

public class LandscapeChunkA : LandscapeChunk<LandscapeLayerA, LandscapeChunkA>
{
}

public class LandscapeChunkB : LandscapeChunk<LandscapeLayerB, LandscapeChunkB>
{
}

public class LandscapeChunkC : LandscapeChunk<LandscapeLayerC, LandscapeChunkC>
{
}

public class LandscapeChunkD : LandscapeChunk<LandscapeLayerD, LandscapeChunkD>
{
}