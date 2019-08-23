using System;
using DelightCraft.Scripts.Infrastructure.Type;
using UnityEngine;

namespace DelightCraft.Infrastructure.Entity
{
    [Serializable]
    public class Tile
    {
        [SerializeField] private GameObject    tileObject       = null;
        [SerializeField] private ColliderType  tileColliderType = ColliderType.Fill;

        public GameObject   TileObject       => tileObject;
        public ColliderType TileColliderType => tileColliderType;
    }
}