namespace DelightCraft.Scripts.Core.Calcuration
{
    public class XorShift
    {
        private readonly uint[] vector = null;

        public XorShift(uint seed = 100)
        {
            vector = new uint[4];

            for (uint i = 1; i <= vector.Length; i++)
            {
                seed = 1812433253 * (seed ^ (seed >> 30)) + i;
                vector[i - 1] = seed;
            }
        }

        public float Random()
        {
            uint t = vector[0];
            uint w = vector[3];

            vector[0] = vector[1];
            vector[1] = vector[2];
            vector[2] = w;

            t ^= t << 11;
            t ^= t >> 8;
            w ^= w >> 19;
            w ^= t;

            vector[3] = w;

            return w * 2.3283064365386963e-10f;
        }
    }
}