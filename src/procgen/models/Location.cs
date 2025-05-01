using Runevision.Common;
using Runevision.LayerProcGen;
using System.Collections.Generic;
using Godot;
using Godot.Util;

public class Location : IPoolable {
	public Point position;
	public float height;
	public int id;
	public float radius;
	public Vector2 frontDir;

	public DPoint3 posWithHeight {
		get {
			return new DPoint3(position.x, height, position.y);
		}
	}

	public void Reset() {
		position = Point.zero;
		height = 0;
		id = 0;
		radius = 0;
		frontDir = Vector2.Zero;
	}
}
