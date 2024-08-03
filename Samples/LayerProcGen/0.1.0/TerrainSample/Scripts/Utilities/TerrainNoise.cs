namespace Godot.Util
{
    public static class TerrainNoise
    {
        private static FastNoiseLite Noise;
        private static float TotalHeight = 1;
        private static float MinHeight = 0;

        private static Vector2 TerrainHeight { get; set; }

        public static void SetFullTerrainHeight(Vector2 terrainHeight)
        {
            TerrainHeight = terrainHeight;
            TotalHeight = Mathf.Abs(terrainHeight.X) + terrainHeight.Y;
            MinHeight = Mathf.Abs(terrainHeight.X);
        }

        public static float GetHeight(Vector2 coords)
        {
            return Mathf.Clamp(( Noise.GetNoise2Dv(coords)+ 1f) / 2f, 0, 1) * (TotalHeight - MinHeight) + MinHeight;
        }

        static TerrainNoise()
        {
            Noise = new FastNoiseLite();
            Noise.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Simplex);

            Noise.SetFrequency(0.002f);
            Noise.SetFractalLacunarity(2f);
            Noise.SetFractalGain(0.5f);
            Noise.SetFractalOctaves(4);

            Noise.SetFractalType(FastNoiseLite.FractalTypeEnum.Fbm);
        }
    }
}