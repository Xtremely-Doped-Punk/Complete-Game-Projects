using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _2048
{
    public class TileBoard : MonoBehaviour
    {
        public event EventHandler OnGameOver;
        public event EventHandler OnMaxTileValueChanged;

        [SerializeField] private Tile tilePrefab;
        [SerializeField] private TileState[] tileStates;

        [SerializeField] private TileGrid tileGrid;
        private readonly List<Tile> activeTiles = new();

        private int maxTileValueIndex = 0;

        private void Awake()
        {
            if (tileGrid == null)
                tileGrid = GetComponentInChildren<TileGrid>();
        }

        public IEnumerator InitializeBoard(int initialTileSpawnCount = 2)
        {
            maxTileValueIndex = 0;
            yield return new WaitUntil(() => tileGrid.IsInitialized);
            for (int i = 0; i < initialTileSpawnCount; i++)
                CreateRandomTile();
        }

        private void Update()
        {
            if (activeTiles.Count == 0) return;

            float horizontal = InputManager.Instance.TickHorizontalSwipe();
            float vertical = InputManager.Instance.TickVerticalSwipe();

            if (horizontal != 0 && vertical != 0) // skip ehen mutiple axis swipe detected
                return;
             else if (horizontal != 0 || vertical != 0)
                MoveTiles(horizontal, vertical);
        }

        public void CreateRandomTile()
        {
            int powerIndex = Random.Range(0, maxTileValueIndex);
            Tile tile = Instantiate(tilePrefab, transform);
            TileCell randomEmptyCell = tileGrid.GetRandomEmptyTileCell();
            tile.SpawnOnTileCell(randomEmptyCell);
            tile.SetupTile(TwoPowX(powerIndex + 1), tileStates[powerIndex]);
            activeTiles.Add(tile);
        }

        private void MoveTiles(float horizontal, float vertical)
        {
            Vector2Int gridDir = new Vector2Int(-(int)vertical, (int)horizontal);
            /*
            vertical corresponds to change in rows, i.e. x-indicies of grid in reverse;
            y axis input direction is inverted to indicies order of grid 
            (as top to bottom is increasing indicies and vice versa)
            
            similarly horizontal corresponds to change in columns, i.e.  y-indices of grid
            */

            if (horizontal != 0)
            {
                if (horizontal > 0)
                {
                    // move rightwards, (skip right-most col as it is already at right, idx=gridWidth-1, thus startCol=gridWidth-2, colIncr=-1)
                    //Debug.Log("RightWards");
                    MoveTiles(gridDir, 0, 1, GameManager.Instance.GridWidth - 2, -1);
                }
                else
                {
                    //Debug.Log("LefttWards");
                    // move leftwards, (skip left-most row as it is already at left, idx=0, thus startCol=1, colIncr=1)
                    MoveTiles(gridDir, 0, 1, 1, 1);
                }
            }
            else
            {
                if (vertical > 0)
                {
                    //Debug.Log("UpWards");
                    // move upwards, (skip top-most row as it is already at top, idx=0, thus startRow=1, rowIncr=1)
                    MoveTiles(gridDir, 1, 1, 0, 1);
                }
                else
                {
                    //Debug.Log("DownWards");
                    // move downwards, (skip bottm-most row as it is already at bottom, idx=gridHeight-1, thus startRow=gridHeight-2, rowIncr=-1)
                    MoveTiles(gridDir, GameManager.Instance.GridHeight - 2, -1, 0, 1);
                }
            }
        }

        private void MoveTiles(Vector2Int direction , int startRow, int rowIncrement, int startCol, int colIncrement)
        {
            bool changed = false;
            for (int x = startRow; x < GameManager.Instance.GridHeight && x >= 0; x += rowIncrement)
            {
                for (int y = startCol; y < GameManager.Instance.GridWidth && y >= 0; y += colIncrement)
                {
                    TileCell tileCell = tileGrid.GetTileCell(x, y);
                    if (tileCell != null && !tileCell.IsEmpty())
                        changed |= TryMoveSingleTile(tileCell.GetTile(), direction);
                }
            }

            if (changed)
                UpdateGameStatus();
        }

        private void UpdateGameStatus()
        {
            InputManager.Instance.SwipesSleepTimer(GameManager.Instance.GetDelay()); // to disable input when animating

            if (activeTiles.Count != GameManager.Instance.GridSize)
            {
                CreateRandomTile();
            }

            if (CheckGameOver())
            {
                OnGameOver?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool CheckGameOver()
        {
            if (activeTiles.Count != GameManager.Instance.GridSize)
                return false;
            
            bool isMergesAvailabe = false;
            // check any merges are is possible
            foreach (Tile tile in activeTiles)
            {
                Tile up = tileGrid.GetAdjacentTileCell(tile.GetTileCell(), Vector2Int.up)?.GetTile();
                Tile down = tileGrid.GetAdjacentTileCell(tile.GetTileCell(), Vector2Int.down)?.GetTile();
                Tile left = tileGrid.GetAdjacentTileCell(tile.GetTileCell(), Vector2Int.left)?.GetTile();
                Tile right = tileGrid.GetAdjacentTileCell(tile.GetTileCell(), Vector2Int.right)?.GetTile();

                if (Tile.CanMerge(tile, up) || Tile.CanMerge(tile, down) || Tile.CanMerge(tile,left) || Tile.CanMerge(tile,right))
                {
                    isMergesAvailabe = true;
                    break;
                }
            }
            return !isMergesAvailabe;
        }

        private bool TryMoveSingleTile(Tile tile, Vector2Int direction)
        {
            TileCell changeCell = null;
            TileCell adjacentTileCell = tileGrid.GetAdjacentTileCell(tile.GetTileCell(), direction);
            int max = GameManager.Instance.GridSize;
            while (adjacentTileCell != null && max>0)
            {
                max--;
                if (!adjacentTileCell.IsEmpty())
                {
                    if (Tile.CanMerge(tile, adjacentTileCell.GetTile()))
                    {
                        // merge cells
                        MergeTiles(tile, adjacentTileCell.GetTile());
                        return true;
                    }
                    break;
                }
                changeCell = adjacentTileCell;
                //Debug.Log(max + "-->" + changeCell);
                adjacentTileCell = tileGrid.GetAdjacentTileCell(changeCell, direction);
            }

            if (changeCell != null)
            {
                tile.MoveToCell(changeCell);
                return true;
            }
            else
                return false;
        }

        private void MergeTiles(Tile tile, Tile adjtile)
        {
            //Debug.Log("Merging tile:" + tile.GetTileCell().GetCoordinates() + " and adj-tile:" + adjtile.GetTileCell().GetCoordinates());
            activeTiles.Remove(tile);
            tile.MergeWithCell(adjtile.GetTileCell());

            //int powerIndex = GetTileStateIndex(adjtile.GetTileState());
            int powerIndex = TwoInversePowX(adjtile.GetNumber()) - 1;
            //Debug.Log(powerIndex + "==" + GetTileStateIndex(adjtile.GetTileState()));
            
            powerIndex = Mathf.Clamp(powerIndex + 1, 0, tileStates.Length - 1);

            if (powerIndex > maxTileValueIndex)
            {
                maxTileValueIndex = powerIndex;
                OnMaxTileValueChanged?.Invoke(this, EventArgs.Empty);
            }

            int number = TwoPowX(powerIndex + 1);
            GameManager.Instance.IncrementScore(number);
            adjtile.SetupTile(number, tileStates[powerIndex]);
        }

        private int GetTileStateIndex(TileState tileState)
        {
            for(int i=0; i<tileStates.Length; i++)
            {
                if (tileStates[i] == tileState) 
                    return i;
            }
            return -1;
        }

        public void ClearBoard()
        {
            tileGrid.ClearAllTileCells();

            foreach(var tile in activeTiles)
            {
                Destroy(tile.gameObject);
            }
            activeTiles.Clear();
        }

        public static int TwoPowX(int power)
        {
            return (1 << power);
        }
        public static int TwoInversePowX(int value)
        {
            int power = 0;
            while (value > 1)
            {
                value >>= 1;
                power++;
            }
            return power;
        }

        public int GetMaxTileValue() => TwoPowX(maxTileValueIndex+1);
    }
}