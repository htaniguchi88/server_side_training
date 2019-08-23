using System;
using UnityEngine;

namespace DelightCraft.Infrastructure.Property
{
    /// <summary>
    /// パーリンノイズのプロパティClass
    /// </summary>
    [Serializable]
    public class PerlinNoiseProperty
    {
        [Header("PerlinNoiseProperty")]
        [SerializeField, Range(1, 16)]       private int        octaves;
        [SerializeField, Range(0.1f, 64.0f)] private float      frequency;
        [SerializeField]                     private float      persistence;
        [SerializeField]                     private uint       seed;
        [SerializeField]                     private int        thickness;
        [SerializeField]                     private Vector2Int size;

        public int     Octaves     => octaves;
        public float   Frequency   => frequency;
        public float   Persistence => persistence;
        public uint    Seed        => seed;
        public int     Thickness   => thickness;
        public Vector2 Size        => size;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="octaves"></param>
        /// <param name="frequency"></param>
        /// <param name="persistence"></param>
        /// <param name="seed"></param>
        /// <param name="size"></param>
        public PerlinNoiseProperty(int octaves, float frequency, float persistence, uint seed, Vector2Int size)
        {
            this.octaves = octaves;
            this.frequency = frequency;
            this.persistence = persistence;
            this.size = size;
            this.seed = seed;
        }
    }
}