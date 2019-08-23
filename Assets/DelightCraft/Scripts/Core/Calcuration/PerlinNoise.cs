using DelightCraft.Scripts.Core.Calcuration;
using UnityEngine;

namespace DelightCraft.Core
{
    /// <summary>
    /// パーリンノイズ
    /// </summary>
    public class PerlinNoise
    {
        private readonly int[] _p = null;

        /// <summary>
        /// 周波数
        /// </summary>
        public float Frequency { get; set; } = 32.0f;

        /// <summary>
        /// Constructor
        /// </summary>
        public PerlinNoise(uint seed)
        {
            var xorshit = new XorShift(seed);

            int[] p = new int[256];
            for (int i = 0; i < p.Length; i++)
            {
                // 0 - 255の間のランダムな値を生成する
                p[i] = (int) Mathf.Floor(xorshit.Random() * 256);
            }

            // pの倍の数の配列を生成する
            int[] p2 = new int[p.Length * 2];
            for (int i = 0; i < p2.Length; i++)
            {
                p2[i] = p[i & 255];
            }

            _p = p2;
        }

        private float Fade(float t)
        {
            // 6t^5 - 5t^5 + 10t^3
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }

        /// <summary>
        /// Linear interpoloation
        /// </summary>
        private float Lerp(float t, float a, float b)
        {
            return a + t * (b - a);
        }

        /// <summary>
        /// Calculate gradient vector.
        /// </summary>
        private float Grad(int hash, float x, float y, float z)
        {
            int h = hash & 15;
            float u = (h < 8) ? x : y;
            float v = (h < 4) ? y : (h == 12 || h == 14) ? x : z;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        private float Noise(float x, float y = 0, float z = 0)
        {
            // Repeat while 0 - 255
            int X = (int) Mathf.Floor(x) & 255;
            int Y = (int) Mathf.Floor(y) & 255;
            int Z = (int) Mathf.Floor(z) & 255;

            // trim integer
            x -= Mathf.Floor(x);
            y -= Mathf.Floor(y);
            z -= Mathf.Floor(z);

            float u = Fade(x);
            float v = Fade(y);
            float w = Fade(z);

            int[] p = _p;

            #region ### calulate hashes from array of p ###

            int A, B, AA, AB, BA, BB, AAA, ABA, AAB, ABB, BAA, BBA, BAB, BBB;

            A = p[X + 0] + Y;
            AA = p[A] + Z;
            AB = p[A + 1] + Z;
            B = p[X + 1] + Y;
            BA = p[B] + Z;
            BB = p[B + 1] + Z;

            AAA = p[AA + 0];
            ABA = p[BA + 0];
            AAB = p[AB + 0];
            ABB = p[BB + 0];
            BAA = p[AA + 1];
            BBA = p[BA + 1];
            BAB = p[AB + 1];
            BBB = p[BB + 1];

            #endregion ### calulate hashes from array of p ###

            float a = Grad(AAA, x + 0, y + 0, z + 0);
            float b = Grad(ABA, x - 1, y + 0, z + 0);
            float c = Grad(AAB, x + 0, y - 1, z + 0);
            float d = Grad(ABB, x - 1, y - 1, z + 0);
            float e = Grad(BAA, x + 0, y + 0, z - 1);
            float f = Grad(BBA, x - 1, y + 0, z - 1);
            float g = Grad(BAB, x + 0, y - 1, z - 1);
            float h = Grad(BBB, x - 1, y - 1, z - 1);

            return Lerp(w, Lerp(v, Lerp(u, a, b),
                    Lerp(u, c, d)),
                Lerp(v, Lerp(u, e, f),
                    Lerp(u, g, h)));
        }

        public float OctaveNoise(float x, int octaves, float persistence = 0.5f)
        {
            float result = 0;
            float amp = 1.0f;
            float f = Frequency;
            float maxValue = 0;

            for (int i = 0; i < octaves; i++)
            {
                result += Noise(x * f) * amp;
                f *= 2.0f;
                maxValue += amp;
                amp *= persistence;
            }

            return result / maxValue;
        }

        public float OctaveNoise(float x, float y, int octaves, float persistence = 0.5f)
        {
            float result = 0;
            float amp = 1.0f;
            float f = Frequency;
            float maxValue = 0;

            for (int i = 0; i < octaves; i++)
            {
                result += Noise(x * f, y * f) * amp;
                f *= 2.0f;
                maxValue += amp;
                amp *= persistence;
            }

            return result / maxValue;
        }

        public float OctaveNoise(float x, float y, float z, int octaves, float persistence = 0.5f)
        {
            float result = 0;
            float amp = 1.0f;
            float f = Frequency;
            float maxValue = 0;

            for (int i = 0; i < octaves; i++)
            {
                result += Noise(x * f, y * f, z * f) * amp;
                f *= 2.0f;
                maxValue += amp;
                amp *= persistence;
            }

            return result / maxValue;
        }
    }
}