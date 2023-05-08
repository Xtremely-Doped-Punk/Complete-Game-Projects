using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _2048
{
    public class TileRow : MonoBehaviour
    {
        [SerializeField] private TileCell[] tileCells;

        public void InitializeTileRow()
        {
            tileCells = GetComponentsInChildren<TileCell>();
        }

        public TileCell[] GetTileCells() => tileCells;
    }
}