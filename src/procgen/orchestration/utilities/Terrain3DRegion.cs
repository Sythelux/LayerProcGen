using System.Collections.Generic;
using Godot;
using Terrain3D.Scripts.Generation.Layers;
using Terrain3DBindings;

namespace Terrain3D.Scripts.Utilities;

public class Terrain3DRegion
{
    public readonly int regionIndex;
    private static readonly Dictionary<Vector2I, int> LoDs = new Dictionary<Vector2I, int>();

    public Terrain3DRegion(int regionIndex)
    {
        this.regionIndex = regionIndex;
    }

    public int LoD
    {
        get => LoDs.GetValueOrDefault(RegionOffset, int.MaxValue);
        set => LoDs.TryAdd(RegionOffset, value);
    }

    public Image? HeightMap
    {
        get => TerrainLODManager.instance.terrain3DWrapper.Storage.GetMapRegion(MapType.TYPE_HEIGHT, regionIndex);
        set => TerrainLODManager.instance.terrain3DWrapper.Storage.SetMapRegion(MapType.TYPE_HEIGHT, regionIndex, value);
    }

    public Image? ControlMap
    {
        get => TerrainLODManager.instance.terrain3DWrapper.Storage.GetMapRegion(MapType.TYPE_CONTROL, regionIndex);
        set => TerrainLODManager.instance.terrain3DWrapper.Storage.SetMapRegion(MapType.TYPE_CONTROL, regionIndex, value);
    }

    public Image? ColorMap
    {
        get => TerrainLODManager.instance.terrain3DWrapper.Storage.GetMapRegion(MapType.TYPE_COLOR, regionIndex);
        set => TerrainLODManager.instance.terrain3DWrapper.Storage.SetMapRegion(MapType.TYPE_COLOR, regionIndex, value);
    }

    public Image? MaxMap
    {
        get => TerrainLODManager.instance.terrain3DWrapper.Storage.GetMapRegion(MapType.TYPE_MAX, regionIndex);
        set => TerrainLODManager.instance.terrain3DWrapper.Storage.SetMapRegion(MapType.TYPE_MAX, regionIndex, value);
    }

    public Vector2I RegionOffset
    {
        get => TerrainLODManager.instance.terrain3DWrapper.Storage.RegionOffsets[regionIndex];
        set => TerrainLODManager.instance.terrain3DWrapper.Storage.RegionOffsets[regionIndex] = value;
    }

    public static Terrain3DRegion Create(int regionIndex)
    {
        return new Terrain3DRegion(regionIndex);
    }
}