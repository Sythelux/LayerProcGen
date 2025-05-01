using Godot;
using Godot.Collections;
using Runevision.Common;
using System;
using Terrain3D.Scripts.Utilities;

class TerrainDeformationMethod {

	// PathPoint props: Vector4: innerWidth, outerWidth, controlWidth, centerElevation

	static float mod(float a, float b) {
		return a - b * Mathf.Floor(a / b);
	}

	static float inverselerp(float a, float b, float value) {
		return (value - a) / (b - a);
	}

	static float smoothstep(float t) {
		t = Mathf.Clamp(t, 0, 1);
		return (t * t * (3.0f - 2.0f * t));
	}

	static Vector4 LineSegment(in Vector2 p, in Vector3 a, in Vector3 b, in Vector4 propsA, in Vector4 propsB, ref Vector3 minDist) {
		Vector2 ap = p - a.xz();
		Vector2 ab = b.xz() - a.xz();
		float sqrSegmentLength = ab.Dot(ab);

		// Where on the segment are we? (0 = a, 1 = b)
		float alongSegment = ap.Dot(ab) / sqrSegmentLength;
		alongSegment = Mathf.Max(-4.0f, Mathf.Min(5.0f, alongSegment)); // avoid rare errors
		float alongSegmentClamped = Mathf.Clamp(alongSegment,0,1);

		// Interpolated properties.
		Vector4 props = propsA.Lerp(propsB, alongSegmentClamped);

		// Distance to segment.
		Vector2 vectorToLine = ab * alongSegmentClamped - ap;
		float dist = vectorToLine.Length();
		float distToEdge = dist - props.X;
		if (distToEdge < minDist.Z) {
			// minDist.z is the distance to the edge (innerWidth).
			minDist.Z = distToEdge;
			// minDist.xy is a vector to the center (not edge)
			minDist.X = vectorToLine.X;
			minDist.Y = vectorToLine.Y;
		}
		// Early out if not within outer dist.
		if (dist >= props.Y)
			return new Vector4();

		// The weight increases from 0 to 1 from outerWidth to innerWidth
		// and continues beyond 1 inside innerWidth.
		float weight = 1.0f - Mathf.Min(1, (dist - props.X) / (props.Y - props.X));
		// Weight has bias to high influences.
		weight = Mathf.Pow(weight, 8.0f);

		// Calculate influence.
		float influence = inverselerp(props.Y, props.X, dist);
		influence = smoothstep(influence);

		// Interpolated height.
		float height = Mathf.Lerp(a.Y, b.Y, alongSegment);
		// Calculate and apply height elevation at center.
		float distUnclamped = (ap - ab * alongSegment).Length();
		float centerInfluence = inverselerp(props.X, props.X * 0.5f, distUnclamped);
		centerInfluence = smoothstep(centerInfluence);
		height += props.W * centerInfluence;

		// Calculate control.
		float control = Mathf.Clamp(inverselerp(props.Z + 0.0f, props.Z - 0.5f, dist),0,1);

		// Output weight, influence, height, control.
		return new Vector4(weight, influence * weight, height * weight, control);
	}

	// [BurstCompile]
	public static void ApplySpecs(
		in Span<SpecData> specDatas,
		in Span<SpecPointB> specPoints,
		uint specCount,
		int GridOffsetX,
		int GridOffsetY,
		uint GridSizeX,
		uint GridSizeY,
		in Vector2 CellSize,
		// output
		ref Span<float> heights,
		ref Span<Vector3> dists,
		ref Span<uint> controls
	) {
		for (uint x = 0; x < GridSizeX; x++) {
			for (uint y = 0; y < GridSizeY; y++) {
				uint index = x + y * GridSizeX;

				// We have to cast unsigned id to floats before adding to signed grid offset,
				// otherwise it appears we lose negative numbers.
				Vector2 p = new Vector2((GridOffsetX + (float)x), (GridOffsetY + (float)y)) * CellSize;

				Vector4 col = new Vector4(0,0,0,0);
				Vector4 control = new Vector4(0,0,0,0);
				Vector3 dist = dists[(int)index];

				uint offset = 0;
				for (int i = 0; i < specCount; i++) {
					uint count = (uint)specDatas[i].pointCount;
					Vector4 bounds = specDatas[i].bounds;
					if (p.X > bounds.X && p.Y > bounds.Y && p.X < bounds.Z && p.Y < bounds.W) {
						Vector4 currentcontrol = specDatas[i].control;
						for (int j = (int)offset; j < offset + count - 1; j++) {
							SpecPointB segA = specPoints[j];
							SpecPointB segB = specPoints[j + 1];
							Vector4 output = LineSegment(p, segA.pos, segB.pos, segA.props, segB.props, ref dist);
							col += output;
							control += currentcontrol * output.X;
						}
					}
					offset += count;
				}

				dists[(int)index] = dist;

				float height = col.Z / col.X;
				float influence = col.Y / col.X;
				control /= col.X;

				if (influence > 0) {
					heights[(int)index] = Mathf.Lerp(heights[(int)index], height, influence);
					controls[(int)index].SetBaseTextureId(1);
					controls[(int)index].SetOverlayTextureId(2);
					var blend = Mathf.Clamp(col.W, 0f, 1f);
					controls[(int)index].SetTextureBlend(Convert.ToByte(blend * 255f));
					controls[(int)index].SetAutoshaded(false);
					controls[(int)index].SetNavigation(blend > .95f);
				}
			}
		}
	}
}
