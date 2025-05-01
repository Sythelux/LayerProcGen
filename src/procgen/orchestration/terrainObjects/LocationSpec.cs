using Godot;
using Runevision.Common;

public class LocationSpec : DeformationSpec, IPoolable {



	public void Reset() {
		control = default;
		bounds = default;
		points.Clear();
	}

	public LocationSpec Init(Vector3 point, int type, float width, float slopeWidth, float controlWidth, float centerElevation) {
		control = (
			type == 0 ?
			new Vector4(0, 0, 1, 0) :
			new Vector4(0, 0.7f, 0.3f, 0)
		);

		float halfInnerWidth = width * 0.5f;
		float halfOuterWidth = width * 0.5f + slopeWidth;
		float halfControlWidth = controlWidth * 0.5f;
		points.Add(new SpecPointB() {
			pos = point - Vector3.Right * 0.01f,
			innerWidth = halfInnerWidth,
			outerWidth = halfOuterWidth,
			controlWidth = halfControlWidth,
			centerElevation = centerElevation
		});
		points.Add(new SpecPointB() {
			pos = point + Vector3.Right * 0.01f,
			innerWidth = halfInnerWidth,
			outerWidth = halfOuterWidth,
			controlWidth = halfControlWidth,
			centerElevation = centerElevation
		});
		return this;
	}
}
