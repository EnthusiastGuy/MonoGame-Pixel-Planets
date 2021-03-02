namespace ShadersTest.General
{
    using System;

    // For now, we'll just use random
    static class Procedural
    {
        private static Random random = new Random();

        // Cheap float random
        public static float GetFloatBetween(float min, float max)
        {
            float precision = 10000;
            int intMin = (int)(min * precision);
            int intMax = (int)(max * precision);

            return GetIntBetween(intMin, intMax) / precision;
        }

        public static int GetIntBetween(int min, int max)
        {
            return random.Next(min, max);
        }
    }
}
