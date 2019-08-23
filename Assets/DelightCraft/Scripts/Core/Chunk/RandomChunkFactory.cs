using System.Collections.Generic;
using DelightCraft.Infrastructure.Map;
using DelightCraft.Infrastructure.Property;
using UnityEngine;
using Entity = DelightCraft.Infrastructure.Entity;

namespace DelightCraft.Core.Chunk
{
    /// <summary>
    /// ランダムな地形チャンクを生成するファクトリ
    /// </summary>
    public class RandomChunkFactory
    {
        /// <summary>
        /// パーリンノイズを生成するためのプロパティ
        /// </summary>
        private PerlinNoiseProperty noiseProperty   = null;

        /// <summary>
        /// タイルマップの設定ファイル
        /// </summary>
        private TileMapDefinition tileMapDefinition = null;

        /// <summary>
        /// パーリンノイズの制御クラス
        /// </summary>
        private PerlinNoise perlinNoise = null;

        /// <summary>
        /// チャンクのエンティティ
        /// </summary>
        private Entity.Chunk chunk = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="noiseProperty"></param>
        /// <param name="tileMapDefinition"></param>
        public RandomChunkFactory(PerlinNoiseProperty noiseProperty, TileMapDefinition tileMapDefinition)
        {
            this.noiseProperty = noiseProperty;
            this.perlinNoise   = new PerlinNoise(noiseProperty.Seed);
            perlinNoise.Frequency = noiseProperty.Frequency;
        }

        /// <summary>
        /// 地形の生成
        /// </summary>
        public Entity.Chunk Create(Vector3Int startPosition)
        {
            var blockMap = CreateBlockMap(startPosition);
            var x = (int) noiseProperty.Size.x;
            var y = (int) noiseProperty.Size.y;
            var length = x * y;
            return new Entity.Chunk(length, x, y, blockMap);
        }

        private Dictionary<Vector3Int, Color> CreateBlockMap(Vector3Int startPosition)
        {
            Dictionary<Vector3Int, Color> chunkMap = new Dictionary<Vector3Int, Color>();
            float frequencyX = 1.0f / noiseProperty.Size.x;
            float frequencyZ = 1.0f / noiseProperty.Size.y;

            int length = ((int) noiseProperty.Size.x * (int) noiseProperty.Size.y);
            for (int i = 0; i < length; i++)
            {
                int x = i % (int)noiseProperty.Size.x + startPosition.x;
                int z = i / (int)noiseProperty.Size.x + startPosition.z;
                float n = perlinNoise.OctaveNoise(x * frequencyX, z * frequencyZ, noiseProperty.Octaves, noiseProperty.Persistence);
                int nonOffsetY = (int) (n * 10);
                int y = nonOffsetY + noiseProperty.Thickness + startPosition.y;

                if (nonOffsetY < -2)
                {
                    chunkMap.Add(new Vector3Int(x, y, z), Color.blue);
                    for (int h = nonOffsetY; h < -2; h++)
                    {
                        AddBlockMap(chunkMap, new Vector3Int(x, h + noiseProperty.Thickness + startPosition.y, z), Color.blue);
                    }
                }

                else
                {
                    AddBlockMap(chunkMap, new Vector3Int(x, y, z), Color.green);
                }

                for (int v = 0; v < y; v++)
                {
                    AddBlockMap(chunkMap, new Vector3Int(x, y, z), new Color(140, 50, 20));
                }
            }

            return chunkMap;
        }

        private void AddBlockMap(Dictionary<Vector3Int, Color> chunkMap, Vector3Int position, Color color)
        {
            if (chunkMap.ContainsKey(position))
            {
                return;
            }

            chunkMap.Add(position, color);
        }
    }
}