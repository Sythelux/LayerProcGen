using System.Collections.Generic;
using GDExtension.Wrappers;
using Godot;
using Terrain3DDemo.Scripts.Generation.Layers;

namespace Terrain3DDemo.Scripts.Utilities;

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
        get => TerrainLODManager.instance.Terrain3D.Storage.GetMapRegion((int)Terrain3DStorage.MapType.TypeHeight, regionIndex);
        set => TerrainLODManager.instance.Terrain3D.Storage.SetMapRegion((int)Terrain3DStorage.MapType.TypeHeight, regionIndex, value);
    }

    public Image? ControlMap
    {
        get => TerrainLODManager.instance.Terrain3D.Storage.GetMapRegion((int)Terrain3DStorage.MapType.TypeControl, regionIndex);
        set => TerrainLODManager.instance.Terrain3D.Storage.SetMapRegion((int)Terrain3DStorage.MapType.TypeControl, regionIndex, value);
    }

    public Image? ColorMap
    {
        get => TerrainLODManager.instance.Terrain3D.Storage.GetMapRegion((int)Terrain3DStorage.MapType.TypeColor, regionIndex);
        set => TerrainLODManager.instance.Terrain3D.Storage.SetMapRegion((int)Terrain3DStorage.MapType.TypeColor, regionIndex, value);
    }

    public Vector2I RegionOffset
    {
        get => TerrainLODManager.instance.Terrain3D.Storage.RegionOffsets[regionIndex];
        set => TerrainLODManager.instance.Terrain3D.Storage.RegionOffsets[regionIndex] = value;
    }

    public static Terrain3DRegion Create(int regionIndex)
    {
        return new Terrain3DRegion(regionIndex);
    }
}