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

public abstract class LandscapeLayer<L, C> : ChunkBasedDataLayer<L, C>
	where L : LandscapeLayer<L, C>, new()
	where C : LandscapeChunk<L, C>, new()
{
	public abstract int lodLevel { get; }

	public const int GridResolution = 256;
	public int gridResolution = GridResolution;
	public int chunkResolution = GridResolution - 8;
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
}

//@formatter:off
public class LandscapeLayerA : LandscapeLayer<LandscapeLayerA, LandscapeChunkA> {
	public override int lodLevel => 0;
	public override int chunkW => (int)RegionSize.SIZE_1024/8;
	public override int chunkH => (int)RegionSize.SIZE_1024/8;
}

public class LandscapeLayerB : LandscapeLayer<LandscapeLayerB, LandscapeChunkB> {
	public override int lodLevel => 1;
	public override int chunkW => (int)RegionSize.SIZE_1024/4;
	public override int chunkH => (int)RegionSize.SIZE_1024/4;
}

public class LandscapeLayerC : LandscapeLayer<LandscapeLayerC, LandscapeChunkC> {
	public override int lodLevel => 2;
	public override int chunkW => (int)RegionSize.SIZE_1024/4;
	public override int chunkH => (int)RegionSize.SIZE_1024/4;
}

public class LandscapeLayerD : LandscapeLayer<LandscapeLayerD, LandscapeChunkD> {
	public override int lodLevel => 3;
	public override int chunkW => (int)RegionSize.SIZE_1024;
	public override int chunkH => (int)RegionSize.SIZE_1024;
}

public class LandscapeChunkA : LandscapeChunk<LandscapeLayerA, LandscapeChunkA> { }
public class LandscapeChunkB : LandscapeChunk<LandscapeLayerB, LandscapeChunkB> { }
public class LandscapeChunkC : LandscapeChunk<LandscapeLayerC, LandscapeChunkC> { }
public class LandscapeChunkD : LandscapeChunk<LandscapeLayerD, LandscapeChunkD> { }

//@formatter:on
