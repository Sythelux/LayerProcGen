using Godot;
using Godot.Collections;
using Runevision.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public struct SpecData
{
    public int pointCount;
    public Vector4 control;
    public Vector4 bounds;
};

public struct SpecPointB
{
    public Vector3 pos;
    public Vector4 props; // innerWidth, outerWidth, controlWidth, centerElevation

    public float innerWidth
    {
        get => props.X;
        set => props.X = value;
    }
    public float outerWidth
    {
        get => props.Y;
        set => props.Y = value;
    }
    public float controlWidth
    {
        get => props.Z;
        set => props.Z = value;
    }
    public float centerElevation
    {
        get => props.W;
        set => props.W = value;
    }
}

public static class TerrainDeformation
{

    static ListPool<SpecPointB> specPointListPool = new ListPool<SpecPointB>(4096);
    static ListPool<SpecData> specDataListPool = new ListPool<SpecData>(128);

    public static void ApplySpecs(
        ref float[,] heights,
        ref Vector3[,] dists,
        ref uint[,] controls,
        Point gridOffset,
        Point gridSize,
        Vector2 cellSize,
        IReadOnlyList<DeformationSpec> specs,
        Func<SpecPointB, SpecPointB> postprocess = null,
        Func<SpecData, SpecData> postprocessSpecs = null
    )
    {
        if (specs.Count == 0)
            return;

        List<SpecData> specDatas = specDataListPool.Get();
        for (int i = 0; i < specs.Count; i++)
        {
            DeformationSpec spec = specs[i];
            int specPointCount = spec.points.Count;
            specDatas.Add(new SpecData()
            {
                pointCount = specPointCount,
                control = spec.control,
                bounds = new Vector4(
                    spec.bounds.min.x - cellSize.X, spec.bounds.min.y - cellSize.Y,
                    spec.bounds.max.x + cellSize.X, spec.bounds.max.y + cellSize.Y)
            });
        }

        List<SpecPointB> specPoints = specPointListPool.Get();
        specPoints.AddRange(specs.SelectMany(spec => spec.points));
        // for (int i = 0; i < specs.Count; i++)
        // {
        //     DeformationSpec spec = specs[i];
        //     for (int j = 0; j < spec.points.Count; j++)
        //     {
        //         specPoints.Add(spec.points[j]);
        //     }
        // }

        if (postprocess != null)
        {
            for (int i = specPoints.Count - 1; i >= 0; i--)
            {
                specPoints[i] = postprocess(specPoints[i]);
            }
        }

        if (postprocessSpecs != null)
        {
            for (int i = specDatas.Count - 1; i >= 0; i--)
            {
                specDatas[i] = postprocessSpecs(specDatas[i]);
            }
        }

        // SpecPointB[] specPointsArray = specPoints.Cast<SpecPointB>().ToArray();
        // SpecPointB[] specPointsArray = new SpecPointB[specPoints.Count];
        // SpecData[] specDatasArray = new SpecData[specDatas.Count];

        // UnityEngine.Profiling.Profiler.BeginSample("SetupSpecData");
        // for (int i = 0; i < specPoints.Count; i++)
            // specPointsArray[i] = (SpecPointB)specPoints[i];
        // for (int i = 0; i < specDatas.Count; i++)
        //     specDatasArray[i] = specDatas[i];
        // UnityEngine.Profiling.Profiler.EndSample();

        // UnityEngine.Profiling.Profiler.BeginSample("Dispatch");
        Span<float> heightsPointerArray = heights.AsSpan();
        Span<Vector3> distsPointerArray = dists.AsSpan();
        Span<uint> controlsPointerArray = controls.AsSpan();
        TerrainDeformationMethod.ApplySpecs(
            CollectionsMarshal.AsSpan(specDatas),
            CollectionsMarshal.AsSpan(specPoints),
            (uint)specDatas.Count,
            gridOffset.x,
            gridOffset.y,
            (uint)gridSize.x,
            (uint)gridSize.y,
            cellSize,
            ref heightsPointerArray,
            ref distsPointerArray,
            ref controlsPointerArray);
        // UnityEngine.Profiling.Profiler.EndSample();

        // specPointsArray.Dispose();
        // specDatasArray.Dispose();

        specPointListPool.Return(ref specPoints);
        specDataListPool.Return(ref specDatas);
    }
}
