using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HPCFinalProject
{
    static class Extensions
    {
        public static float NextFloat(this Random random) => (float)random.NextDouble();
        public static float NextFloat(this Random random, float min, float max) => random.NextFloat() * (max - min) + min;
        //public static float Between(this Math Math) => Math.Min()
    }
}
