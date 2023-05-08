using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _2048
{
    public class TileCell : MonoBehaviour
    {
        [SerializeField] private Vector2Int coordinates;
        [SerializeField] private Tile tile;


        public bool IsEmpty() => tile == null;
        public Tile GetTile() => tile;
        public void SetTile(Tile tile) => this.tile = tile;
        public void ClearTile() => tile = null;
        public Vector2Int GetCoordinates() => coordinates;
        public void SetCoordinates(Vector2Int val) => coordinates = val;
    }
}