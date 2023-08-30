using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2048
{
    public class TileGrid : MonoBehaviour
    {
        [SerializeField] private Tile1DArray[] tileRows;
        [SerializeField] private TileCell[] tileCells;
        private bool isRowMajor;

        public void InitializeTileGrid(bool isRowMajor)
        {
            this.isRowMajor = isRowMajor;
            tileRows = GetComponentsInChildren<Tile1DArray>();
            tileCells = GetComponentsInChildren<TileCell>();

            for (int x = 0; x < tileRows.Length; x++)
            {
                TileCell[] tileRowCells = tileRows[x].GetTileCells();
                //Debug.Log("row:" + x + " row length=" + tileRowCells.Length);

                for (int y = 0; y < tileRowCells.Length; y++)
                {
                    //Debug.Log("col:" + y);
                    var coor = (isRowMajor) ? new Vector2Int(x, y) : new Vector2Int(y, x);
                    tileRowCells[y].SetCoordinates(coor);
                }
            }
        }

        public TileCell GetTileCell(int x, int y)
        {
            TileCell result = null;
            if ((x >= 0 && x < GameManager.Instance.GridHeight)
                && (y >= 0 && y < GameManager.Instance.GridWidth))
            {
                if (isRowMajor)
                    result = tileRows[x].GetTileCells()[y];
                else
                    result = tileRows[y].GetTileCells()[y];
                //Debug.Log($"TileGrid:: GetTileCell():{result.GetCoordinates()} found for dimensions (x={x}, y={y})");
            }
            else
            {
                //Debug.LogWarning("TileGrid:: GetTileCell() dimentions out of bounds (x=" + x + ",y=" + y + ")");
            }
            return result;
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