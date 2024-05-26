// using Runevision.Common;
// using System;
// using System.Collections.Generic;
// using System.Collections.ObjectModel;
// using Godot;
// using Godot.Collections;
//
// public struct SpecData {
// 	public int pointCount;
// 	public Vector4 splat;
// 	public Vector4 bounds;
// };
//
// public struct SpecPoint {
// 	public Vector3 pos;
// 	public float innerWidth;
// 	public float outerWidth;
// 	public float splatWidth;
// 	public float centerElevation;
// };
//
// public struct SpecPointB {
// 	public Vector3 pos;
// 	public Vector4 props; // innerWidth, outerWidth, splatWidth, centerElevation
//
// 	public static explicit operator SpecPointB(SpecPoint p) {
// 		return new SpecPointB() {
// 			pos = p.pos,
// 			props = new Vector4(p.innerWidth, p.outerWidth, p.splatWidth, p.centerElevation)
// 		};
// 	}
// };
//
// public static class TerrainDeformation {
//
// 	static ListPool<SpecPoint> specPointListPool = new ListPool<SpecPoint>(4096);
// 	static ListPool<SpecData> specDataListPool = new ListPool<SpecData>(128);
//
// 	public static void ApplySpecs(
// 		Array<float> heights,
// 		Array<Vector3> dists,
// 		Array<Vector4> splats,
// 		Point gridOffset,
// 		Point gridSize,
// 		Vector2 cellSize,
// 		IReadOnlyList<DeformationSpec> specs,
// 		Func<SpecPoint, SpecPoint> postprocess = null,
// 		Func<SpecData, SpecData> postprocessSpecs = null
// 	) {
// 		if (specs.Count == 0)
// 			return;
//
// 		List<SpecData> specDatas = specDataListPool.Get();
// 		for (int i = 0; i < specs.Count; i++) {
// 			DeformationSpec spec = specs[i];
// 			int specPointCount = spec.points.Count;
// 			specDatas.Add(new SpecData() {
// 				pointCount = specPointCount,
// 				splat = spec.splat,
// 				bounds = new Vector4(
// 					spec.bounds.min.X - cellSize.X, spec.bounds.min.Y - cellSize.Y,
// 					spec.bounds.max.X + cellSize.X, spec.bounds.max.Y + cellSize.Y)
// 			});
// 		}
//
// 		List<SpecPoint> specPoints = specPointListPool.Get();
// 		for (int i = 0; i < specs.Count; i++) {
// 			DeformationSpec spec = specs[i];
// 			for (int j = 0; j < spec.points.Count; j++) {
// 				specPoints.Add(spec.points[j]);
// 			}
// 		}
//
// 		if (postprocess != null) {
// 			for (int i = specPoints.Count - 1; i >= 0; i--) {
// 				specPoints[i] = postprocess(specPoints[i]);
// 			}
// 		}
//
// 		if (postprocessSpecs != null) {
// 			for (int i = specDatas.Count - 1; i >= 0; i--) {
// 				specDatas[i] = postprocessSpecs(specDatas[i]);
// 			}
// 		}
//
// 		Array<SpecPointB> specPointsArray = new Array<SpecPointB>(specPoints.Count, Allocator.Persistent);
// 		Array<SpecData> specDatasArray = new Array<SpecData>(specDatas.Count, Allocator.Persistent);
//
// 		// UnityEngine.Profiling.Profiler.BeginSample("SetupSpecData");
// 		for (int i = 0; i < specPoints.Count; i++)
// 			specPointsArray[i] = (SpecPointB)specPoints[i];
// 		for (int i = 0; i < specDatas.Count; i++)
// 			specDatasArray[i] = specDatas[i];
// 		// UnityEngine.Profiling.Profiler.EndSample();
//
// 		// UnityEngine.Profiling.Profiler.BeginSample("Dispatch");
// 		Array<float> heightsPointerArray = heights;
// 		Array<Vector3> distsPointerArray = dists;
// 		Array<Vector4> splatsPointerArray = splats;
// 		TerrainDeformationMethod.ApplySpecs(
// 			specDatasArray,
// 			specPointsArray,
// 			(uint)specDatas.Count,
// 			gridOffset.X,
// 			gridOffset.Y,
// 			(uint)gridSize.X,
// 			(uint)gridSize.Y,
// 			cellSize,
// 			ref heightsPointerArray,
// 			ref distsPointerArray,
// 			ref splatsPointerArray);
// 		// UnityEngine.Profiling.Profiler.EndSample();
//
// 		specPointsArray.Dispose();
// 		specDatasArray.Dispose();
//
// 		specPointListPool.Return(ref specPoints);
// 		specDataListPool.Return(ref specDatas);
// 	}
// }
