using Godot;
using Runevision.Common;

public static class Maths {

	public static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) {
		// Faster to do it element-wise since we then avoid many Vector3 constructor calls
		// caused by the various Vector3 math operations.
		return new Vector3(
			Bezier(a.X, b.X, c.X, d.X, t),
			Bezier(a.Y, b.Y, c.Y, d.Y, t),
			Bezier(a.Z, b.Z, c.Z, d.Z, t)
		);
	}

	public static float Bezier(float a, float b, float c, float d, float t) {
		float u = 1f - t;
		return
			u * u * u * a +
			u * u * t * b * 3 +
			u * t * t * c * 3 +
			t * t * t * d;
	}

	public static DFloat Bezier(DFloat a, DFloat b, DFloat c, DFloat d, DFloat t) {
		DFloat ab = DFloat.Lerp(a, b, t);
		DFloat bc = DFloat.Lerp(b, c, t);
		DFloat cd = DFloat.Lerp(c, d, t);
		DFloat abc = DFloat.Lerp(ab, bc, t);
		DFloat bcd = DFloat.Lerp(bc, cd, t);
		return DFloat.Lerp(abc, bcd, t);
	}

	public static float LineSegmentPointDist(Vector2 v, Vector2 w, Vector2 p) {
		return Mathf.Sqrt(LineSegmentPointSqrDist(v, w, p));
	}

	public static float LineSegmentPointDist(DPoint v, DPoint w, DPoint p) {
		return Mathf.Sqrt(LineSegmentPointSqrDist(v, w, p));
	}

	public static float LineSegmentPointSqrDist(Vector2 v, Vector2 w, Vector2 p) {
		// Return minimum distance between line segment vw and point p
		float l2 = (v - w).LengthSquared();        // i.e. |w-v|^2 -  avoid a sqrt
		if (l2 == 0.0)
			return (p - v).LengthSquared();        // v == w case
												// Consider the line extending the segment, parameterized as v + t (w - v).
												// We find projection of point p onto the line. 
												// It falls where t = [(p-v) . (w-v)] / |w-v|^2
		float t = (p - v).Dot( w - v) / l2;
		if (t < 0.0f)
			return (p - v).LengthSquared();        // Beyond the 'v' end of the segment
		else if (t > 1.0f)
			return (p - w).LengthSquared();        // Beyond the 'w' end of the segment
		Vector2 projection = v + t * (w - v);   // Projection falls on the segment
		return (p - projection).LengthSquared();
	}

	public static float LineSegmentPointSqrDist(DPoint v, DPoint w, DPoint p) {
		// Return minimum distance between line segment vw and point p
		float l2 = ((Vector2)(v - w)).LengthSquared(); // i.e. |w-v|^2 -  avoid a sqrt
		if (l2 == 0.0)
			return ((Vector2)(p - v)).LengthSquared(); // v == w case
												// Consider the line extending the segment, parameterized as v + t (w - v).
												// We find projection of point p onto the line. 
												// It falls where t = [(p-v) . (w-v)] / |w-v|^2
		float t = ((Vector2)(p - v)).Dot((Vector2)(w - v)) / l2;
		if (t < 0.0f)
			return ((Vector2)(p - v)).LengthSquared(); // Beyond the 'v' end of the segment
		else if (t > 1.0f)
			return ((Vector2)(p - w)).LengthSquared(); // Beyond the 'w' end of the segment
		DPoint projection = v + (Vector2)(w - v) * t; // Projection falls on the segment
		return ((Vector2)(p - projection)).LengthSquared();
	}

	public static float InsideLineSegmentProjection(Vector2 v, Vector2 w, Vector2 p) {
		float l2 = (v - w).LengthSquared();
		return (p - v).Dot(w - v) / l2;
	}

	// Returns true if the lines intersect, otherwise false.
	public static bool LineSegmentIntersection(Vector2 pA1, Vector2 pA2, Vector2 pB1, Vector2 pB2,
		out Vector2 intersection, out float fracA, out float fracB, float tolerance = 0
	) {
		Vector2 vA = pA2 - pA1;
		Vector2 vB = pB2 - pB1;

		fracB = (vA.X * (pA1.Y - pB1.Y) - vA.Y * (pA1.X - pB1.X)) / (-vB.X * vA.Y + vA.X * vB.Y);
		fracA = (vB.X * (pA1.Y - pB1.Y) - vB.Y * (pA1.X - pB1.X)) / (-vB.X * vA.Y + vA.X * vB.Y);
		
		if (fracB >= -tolerance && fracB <= 1f + tolerance &&
			fracA >= -tolerance && fracA <= 1f + tolerance
		) {
			// Collision detected
			intersection = pA1 + (fracA * vA);
			return true;
		}

		intersection = default;
		return false;
	}

	public static void Swap<T>(ref T lhs, ref T rhs) {
		T temp;
		temp = lhs;
		lhs = rhs;
		rhs = temp;
	}
}
