using System;
using System.Collections.Generic;
using UnityEngine;

namespace DelightCraft.Infrastructure.Entity
{
    [Serializable]
    public class Chunk
    {
        [SerializeField] private int length = 0;
        [SerializeField] private int rangeX = 0;
        [SerializeField] private int rangeY = 0;
        [SerializeField] private Dictionary<Vector3Int, Color> blockMap = null;

        public int Length => length;

        public int RangeX => rangeX;

        public int RangeY => rangeY;

        public Dictionary<Vector3Int, Color> BlockMap => blockMap;

        public Chunk(int length, int rangeX, int rangeY, Dictionary<Vector3Int, Color> blockMap)
        {
            this.length = length;
            this.rangeX = rangeX;
            this.rangeY = rangeY;
            this.blockMap = blockMap;
        }

        /// <summary>
        /// 隣接していない面が発生する場所とカメラに写っている場所を取得する
        /// </summary>
        /// <returns></returns>
        public Dictionary<Vector3Int, Color> GetCoordinateNoneAdjacent(Camera mainCamera)
        {
            Dictionary<Vector3Int, Color> renderingEnabledPositions = new Dictionary<Vector3Int, Color>();

            Rect rect = new Rect(0, 0, 1, 1);
            foreach (Vector3Int blockPosition in blockMap.Keys)
            {
                if ((mainCamera.transform.position - blockPosition).sqrMagnitude < 900)
                {
                    renderingEnabledPositions.Add(blockPosition, blockMap[blockPosition]);
                    continue;
                }

                var viewportPosition = mainCamera.WorldToViewportPoint(blockPosition);
                if (rect.Contains(viewportPosition))
                {
                    renderingEnabledPositions.Add(blockPosition, blockMap[blockPosition]);
                    continue;
                }

                if (HasAdjacentNonePosition(blockPosition))
                {
                    renderingEnabledPositions.Add(blockPosition, blockMap[blockPosition]);
                }
            }

            return renderingEnabledPositions;
        }

        /// <summary>
        /// 隣接していない面が存在するかをチェックします。
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool HasAdjacentNonePosition(Vector3Int position)
        {
            if (!blockMap.ContainsKey(new Vector3Int(position.x - 1, position.y, position.z)))
            {
                return false;
            }

            if (!blockMap.ContainsKey(new Vector3Int(position.x + 1, position.y, position.z)))
            {
                return false;
            }

            if (!blockMap.ContainsKey(new Vector3Int(position.x, position.y - 1, position.z)))
            {
                return false;
            }

            if (!blockMap.ContainsKey(new Vector3Int(position.x, position.y + 1, position.z)))
            {
                return false;
            }

            if (!blockMap.ContainsKey(new Vector3Int(position.x, position.y, position.z - 1)))
            {
                return false;
            }

            if (!blockMap.ContainsKey(new Vector3Int(position.x, position.y, position.z + 1)))
            {
                return false;
            }

            return true;
        }
    }
}