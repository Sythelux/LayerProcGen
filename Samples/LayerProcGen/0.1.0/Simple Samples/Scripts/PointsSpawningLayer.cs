using Runevision.Common;
using Runevision.LayerProcGen;
using System.Collections.Generic;
using Godot;

public class PointsSpawningChunk : LayerChunk<PointsSpawningLayer, PointsSpawningChunk> {
	
	// A pool of List<Point> that all have the specified capacity.
	static ListPool<Point> pointsListPool = new ListPool<Point>(12);

	// Data for this chunk goes here.
	TransformWrapper chunkParent;

	public override void Create(int level, bool destroy) {
		if (destroy) {
			QueuedGameObjectDestruction.Enqueue(chunkParent, false);
		}
		else {
			// Get a List from the list pool.
			List<Point> points = pointsListPool.Get();
			
			// Fill it with the points from the PointsLayer that are within the chunk bounds.
			PointsLayer.instance.GetPointsInBounds(this, points, bounds);
			
			// Instantiate a sphere for each point.
			// We need to capture the current value of relevant outer variables by copying them.
			// Otherwise the action executing delayed on the main thread may use incorrect
			// newer values stored in the same outer variables, which may even reference a
			// newer incarnation of a chunk that has been recycled and reused in the mean time.
			chunkParent = new TransformWrapper(PointsSpawningLayer.instance.layerParent, index);
			TransformWrapper currentChunkParent = chunkParent; // Capture current chunk parent.
			foreach (Point point in points) {
				Point currentPoint = point; // Capture current point in foreach loop.
				MainThreadActionQueue.Enqueue(() =>
				{
					var meshInstance3D = new MeshInstance3D();
					meshInstance3D.Mesh = new SphereMesh();
					Transform3D tr = meshInstance3D.Transform;
					tr.Origin = new Vector3(currentPoint.x, 0f, currentPoint.y);
					tr.ScaledLocal( Vector3.One * 50f);
					currentChunkParent.AddChild(meshInstance3D);
				});
			}

			// Return the List to the list pool.
			pointsListPool.Return(ref points);
		}
	}
}

public class PointsSpawningLayer : ChunkBasedDataLayer<PointsSpawningLayer, PointsSpawningChunk> {
	// Specify the world space dimensions of the chunks.
	public override int chunkW { get { return 100; } }
	public override int chunkH { get { return 100; } }

	// A Transform parent for all objects spawned by this layer.
	public Node3D layerParent;

	public PointsSpawningLayer() {
		// Create the layer parent Transform. We're on the main thread, so it's ok.

		layerParent = new Node3D { Name = "PointsSpawnLayer" };
		
		// Dependencies on other layers are set up here with appropriate padding.
		AddLayerDependency(new LayerDependency(PointsLayer.instance, new Point(0, 0)));
	}
}
