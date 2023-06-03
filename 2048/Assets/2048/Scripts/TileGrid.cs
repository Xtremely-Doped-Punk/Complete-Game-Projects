using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2048
{
    public class TileGrid : MonoBehaviour
    {
        [SerializeField] private TileRow[] tileRows;
        [SerializeField] private TileCell[] tileCells;

        public bool IsInitialized { get; private set; } = false;

        public void InitializeTileGrid()
        {
            tileRows = GetComponentsInChildren<TileRow>();
            tileCells = GetComponentsInChildren<TileCell>();

            for (int x = 0; x < tileRows.Length; x++)
            {
                TileCell[] tileRowCells = tileRows[x].GetTileCells();
                //Debug.Log("row:" + x + " row length=" + tileRowCells.Length);

                for (int y = 0; y < tileRowCells.Length; y++)
                {
                    //Debug.Log("col:" + y);
                    tileRowCells[y].SetCoordinates(new Vector2Int(x, y));
                }
            }
            IsInitialized = true;
        }

        public TileCell GetTileCell(int x, int y)
        {
            if ((x >= 0 && x < GameManager.Instance.GridWidth)
                && (y >= 0 && y < GameManager.Instance.GridHeight))
            {
                //Debug.Log("Grid get tile cell:" + tileRows[x].GetTileCells()[y].GetCoordinates() + " found for dimensions (x=" + x + ",y=" + y + ")");
                return tileRows[x].GetTileCells()[y];
            }
            else
            {
                //Debug.LogWarning("Grid get tile cell dimention out of bounds (x=" + x + ",y=" + y + ")");
                return null;
            }
        }

        public TileCell GetTileCell(Vector2Int coordinates) => GetTileCell(coordinates.x, coordinates.y);

        public TileCell GetAdjacentTileCell(TileCell tileCell, Vector2Int gridDir)
        {
            Vector2Int coordinates = tileCell.GetCoordinates();
            coordinates.x += gridDir.x;
            coordinates.y += gridDir.y;
            return GetTileCell(coordinates);
        }

        public TileCell GetRandomEmptyTileCell()
        {
            TileCell[] emptyTileCells = GetEmptyTileCells();
            if (emptyTileCells.Length == 0)
                return null;

            int index = Random.Range(0, emptyTileCells.Length);
            return emptyTileCells[index];
        }

        private TileCell[] GetNonEmptyTileCells()
        {
            return tileCells.ToList().Where(x => !x.IsEmpty()).ToArray();
        }
        private TileCell[] GetEmptyTileCells()
        {
            return tileCells.ToList().Where(x => x.IsEmpty()).ToArray();
        }

        public void ClearAllTileCells()
        {
            foreach (TileCell cell in tileCells)
                cell.ClearTile();
        }
    }
}