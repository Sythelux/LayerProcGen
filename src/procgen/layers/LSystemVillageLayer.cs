using Runevision.Common;
using Runevision.LayerProcGen;
using Godot;

public class LSystemVillageLayer : ChunkBasedDataLayer<LSystemVillageLayer, LSystemVillageChunk>, ILayerVisualization
{
	public override int chunkW { get { return 128; } }
	public override int chunkH { get { return 128; } }

    public Node3D layerParent;

    public LSystemVillageLayer()
    {
        GD.Print("LSystemVillageLayer Create");

        layerParent = new Node3D { Name = "LSystemVillageLayer" };
        // AddLayerDependency(new LayerDependency(CultivationLayer.instance, CultivationLayer.requiredPadding, 0));
    }

    public Node LayerRoot() => layerParent;

	public static DebugToggle debugLayer = DebugToggle.Create(">Layers/LSystemVillageLayer");

    public void VisualizationUpdate() { 
        if (!debugLayer.visible)
			return;
		for (int i = 0; i < GetLevelCount(); i++) {
			VisualizationManager.BeginDebugDraw(this, i);
			HandleAllChunks(i, c => c.DebugDraw());
			VisualizationManager.EndDebugDraw();
		}
     }
}
