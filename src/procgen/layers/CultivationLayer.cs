using Runevision.Common;
using Runevision.LayerProcGen;
using System.Collections.Generic;
using Godot;

public class CultivationLayer : ChunkBasedDataLayer<CultivationLayer, CultivationChunk>, ILayerVisualization ,IGodotInstance{
	public override int chunkW { get { return 360; } }
	public override int chunkH { get { return 360; } }

	public static readonly Point requiredPadding = new Point(180, 180);

	public Point gridChunkRes;
	public Point gridPadding;
	public Point gridSize;
	public Point worldSpacePadding;

	public Node3D layerParent;

	static DebugToggle debugPaths = DebugToggle.Create(">Layers/CultivationLayer/Paths", true);
	static DebugToggle debugPathsRaw = DebugToggle.Create(">Layers/CultivationLayer/Paths Raw");
	static DebugToggle debugPathBounds = DebugToggle.Create(">Layers/CultivationLayer/Paths Bounds");
	static DebugToggle debugHeights = DebugToggle.Create(">Layers/CultivationLayer/Heights");
	static DebugToggle debugDirections = DebugToggle.Create(">Layers/CultivationLayer/Directions");

	public CultivationLayer() {
		gridChunkRes = chunkSize / TerrainPathFinder.halfCellSize;
		// Make the grid for each chunk cover an area extending further our than the chunk.
		// This ensures the pathfinding can succeed even when it extends partially beyond the chunk.
		worldSpacePadding = chunkSize;
		gridPadding = gridChunkRes;
		gridSize = gridChunkRes + gridPadding * 2;

		layerParent = new Node3D { Name = "CultivationLayer" };

		AddLayerDependency(new LayerDependency(GeoGridLayer.instance, worldSpacePadding, 0));
		AddLayerDependency(new LayerDependency(LocationLayer.instance, LocationLayer.requiredPadding, 2));
	}

	public void VisualizationUpdate() {
		VisualizationManager.BeginDebugDraw(this, 0);
		if (debugPaths.visible || debugPathsRaw.visible || debugPathBounds.visible)
			HandleAllChunks(0, c => c.DebugDraw(debugPaths.animAlpha, debugPathsRaw.animAlpha, debugPathBounds.animAlpha));
		VisualizationManager.EndDebugDraw();

		GridBounds focusBounds = GridBounds.Empty();
		if (debugHeights.enabled || debugDirections.enabled) {
			foreach (var dep in LayerManager.instance.topDependencies) {
				if (dep.layer == PlayLayer.instance) {
					focusBounds = new GridBounds(dep.focus - Point.one * 50, Point.one * 100);
					break;
				}
			}
		}
		if (debugHeights.enabled) {
			HandleChunksInBounds(null, focusBounds, 0, c => c.DrawHeights(focusBounds));
		}
		if (debugDirections.enabled) {
			HandleChunksInBounds(null, focusBounds, 0, c => c.DrawDirections(focusBounds));
		}
	}

	public void GetPathsOverlappingBounds(ILC q, List<PathSpec> outPaths, GridBounds bounds) {
		// Add paths within bounds.
		HandleChunksInBounds(q, bounds.GetExpanded(requiredPadding), 0, chunk => {
			foreach (var path in chunk.paths)
				if (bounds.Overlaps(path.bounds))
					outPaths.Add(path);
		});
	}

	public Node LayerRoot() => layerParent;
}
