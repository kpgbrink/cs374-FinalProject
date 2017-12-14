using System;

namespace HPCFinalProject
{
    static class Extensions
    {
        public static float NextFloat(this Random random) => (float)random.NextDouble();
        public static float NextFloat(this Random random, float min, float max) => random.NextFloat() * (max - min) + min;
        //public static float Between(this Math Math) => Math.Min()
        public static float Inbetween(this float value, float min, float max) => Math.Min(max, Math.Max(min, value));
        public static int Inbetween(this int value, int min, int max) => Math.Min(max, Math.Max(min, value));
    }
}
