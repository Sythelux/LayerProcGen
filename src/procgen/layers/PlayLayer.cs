using System.Reflection;
using Godot;
using Runevision.LayerProcGen;
using Runevision.Common;

public class PlayLayer : ChunkBasedDataLayer<PlayLayer, PlayChunk>
{
    public override int chunkW => 8;
    public override int chunkH => 8;

    public PlayLayer()
    {
        AddLayerDependency(new LayerDependency(LandscapeLayerD.instance, 2048, 2048));
        AddLayerDependency(new LayerDependency(LandscapeLayerC.instance, 1024, 1024));
        AddLayerDependency(new LayerDependency(LandscapeLayerB.instance,  512,  512));
        AddLayerDependency(new LayerDependency(LandscapeLayerA.instance,  256,  256));

        AddLayerDependency(new LayerDependency(
            LSystemVillageLayer.instance,
            256,
            256
        ));
        GD.Print("PlayLayer Create");
    }
}