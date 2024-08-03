using System.Reflection;
using Godot;
using Runevision.LayerProcGen;

public class PlayChunk : LayerChunk<PlayLayer, PlayChunk>
{
    public PlayChunk()
    {
    }
}

public class PlayLayer : ChunkBasedDataLayer<PlayLayer, PlayChunk>
{
    public override int chunkW => 8;
    public override int chunkH => 8;

    public PlayLayer()
    {
        AddLayerDependency(new LayerDependency(CultivationLayer.instance, CultivationLayer.requiredPadding, 0));

        AddLayerDependency(new LayerDependency(LandscapeLayerD.instance, 2048, 2048));
        AddLayerDependency(new LayerDependency(LandscapeLayerC.instance, 512, 512));
        AddLayerDependency(new LayerDependency(LandscapeLayerB.instance, 256, 256));
        AddLayerDependency(new LayerDependency(LandscapeLayerA.instance, 64, 64));
    }
}
