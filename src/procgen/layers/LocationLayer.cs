using Runevision.Common;
using Runevision.LayerProcGen;
using System.Collections.Generic;
using Godot;
using Godot.Util;

public class LocationLayer : ChunkBasedDataLayer<LocationLayer, LocationChunk>, ILayerVisualization {
	public override int chunkW { get { return 360; } }
	public override int chunkH { get { return 360; } }

	public static readonly Point requiredPadding = new Point(180, 180);

	public LocationLayer() {

	}

	public override int GetLevelCount() {
		return 3;
	}

	// Includes chunk itself.
	public IEnumerable<LocationChunk> GetNeighborChunks(LocationChunk chunk) {
		for (int i = -1; i <= 1; i++) {
			for (int j = -1; j <= 1; j++) {
				yield return chunks[chunk.index.x + i, chunk.index.y + j];
			}
		}
	}

	public LocationChunk GetNeighborChunk(LocationChunk chunk, Point relativeVector) {
		return chunks[chunk.index.x + relativeVector.x, chunk.index.y + relativeVector.y];
	}

	public void GetConnectionsOwnedInBounds(ILC q, List<Location> outConnectionPairs, GridBounds bounds) {
		// Expanded bounds required since internal connections of a chunk are not necessarily
		// inside the bounds of the chunk they're generated and stored in.
		HandleChunksInBounds(q, bounds.GetExpanded(requiredPadding), 2, chunk => {
			chunk.GetConnectionsOwnedInBounds(outConnectionPairs, bounds);
		});
	}

	public void GetLocationsOwnedInBounds(ILC q, List<Location> outLocations, GridBounds bounds) {
		// Expanded bounds required since locations are not necessarily inside the bounds
		// of the chunk they're generated and stored in.
		HandleChunksInBounds(q, bounds.GetExpanded(requiredPadding), 1, chunk => {
			chunk.GetLocationsOwnedInBounds(outLocations, bounds);
		});
	}

	public void GetLocationSpecsOverlappingBounds(ILC q, List<LocationSpec> outSpecs, GridBounds bounds) {
		// Expanded bounds required since locations are not necessarily inside the bounds
		// of the chunk they're generated and stored in.
		HandleChunksInBounds(q, bounds.GetExpanded(requiredPadding), 1, chunk => {
			chunk.GetLocationSpecsOverlappingBounds(outSpecs, bounds);
		});
	}

	public static DebugToggle debugLayer = DebugToggle.Create(">Layers/LocationLayer");
	public static DebugToggle debugInitial = DebugToggle.Create(">Layers/LocationLayer/Initial");
	public static DebugToggle debugAdjusted = DebugToggle.Create(">Layers/LocationLayer/Adjusted");
	public static DebugToggle debugRadiuses = DebugToggle.Create(">Layers/LocationLayer/Radiuses");
	public static DebugToggle debugConnections = DebugToggle.Create(">Layers/LocationLayer/Connections");

	public void VisualizationUpdate() {
		if (!debugLayer.visible)
			return;
		for (int i = 0; i < GetLevelCount(); i++) {
			VisualizationManager.BeginDebugDraw(this, i);
			HandleAllChunks(i, c => c.DebugDraw(i));
			VisualizationManager.EndDebugDraw();
		}
	}
}
