using System;
using System.Collections.Generic;
using DelightCraft.Infrastructure.Entity;
using UnityEngine;

namespace DelightCraft.Infrastructure.Map
{
    [Serializable]
    public class TileMapDefinition : ScriptableObject
    {
        [SerializeField] private List<Tile> tileMap = new List<Tile>();

        public List<Tile> TileMap => tileMap;
    }
}