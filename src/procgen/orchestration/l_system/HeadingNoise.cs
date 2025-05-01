using Godot;
using Godot.Util;                      // for FastNoiseLite
using System.Collections.Generic;

public static class HeadingNoise
{
    // One FastNoiseLite per world‑seed so result is deterministic.
    private static readonly Dictionary<int, FastNoiseLite> _cache = new();

    public static Vector3 PerturbDirection(
        Vector3 dir, Vector3 pos, int seed,
        float maxDeg = 8f, float freq = 0.002f
    ){
        if (!_cache.TryGetValue(seed, out var noise))
        {
            noise = new FastNoiseLite();
            noise.Seed            = seed;
            noise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);
            noise.SetFrequency(freq);
            noise.SetFractalType(FastNoiseLite.FractalTypeEnum.Fbm);
            noise.SetFractalOctaves(3);
            noise.SetFractalLacunarity(2f);
            noise.SetFractalGain(0.5f);
            _cache[seed] = noise;
        }

        // Sample 2D noise in [-1…1], convert to radians
        float sample = noise.GetNoise2D(pos.X, pos.Z);
        float offset = maxDeg * Mathf.Pi / 180f * sample;

        return dir.Rotated(Vector3.Up, offset).Normalized();
    }
}
