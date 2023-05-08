using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _2048
{
    public class Tile : MonoBehaviour
    {
        [SerializeField] private TileState state;
        [SerializeField] private TileCell cell;
        [SerializeField] private int number;
        [SerializeField] private Image background;
        [SerializeField] private TextMeshProUGUI numberText;
        [SerializeField] private Vector2 OutlineOffset = Vector2.zero;
        
        private bool locked;

        private void Awake()
        {
            if (background == null) 
                background = GetComponent<Image>();
            if (numberText == null)
                numberText = GetComponentInChildren<TextMeshProUGUI>();
        }

        public TileState GetTileState() => state;
        public TileCell GetTileCell() => cell;
        public int GetNumber() => number;
        public void SetLockTimer(float time)
        {
            this.locked = true;
            Invoke(nameof(ResetLock),time);
        }
        private void ResetLock() => locked = false;

        public bool IsLocked() => locked;

        public void SetupTile(int number, TileState state)
        {
            this.number = number;
            this.state = state;

            background.color = state.backgroundColor; 
            numberText.color = state.textColor;
            numberText.text = number.ToString();
        }

        public void SpawnOnTileCell(TileCell tileCell)
        {
            if (cell != null)
                cell.ClearTile();

            cell = tileCell;
            cell.SetTile(this);

            transform.SetParent(cell.transform);
            transform.localPosition = Vector3.zero;
            //transform.position = cell.transform.position;

            ((RectTransform)transform).sizeDelta =
                ((RectTransform)cell.transform).sizeDelta - OutlineOffset;
        }

        public void MoveToCell(TileCell tileCell)
        {
            cell.ClearTile();
            cell = tileCell;
            cell.SetTile(this);

            transform.SetParent(cell.transform);
            //transform.localPosition = Vector3.zero;
            StartCoroutine(AnimateMovement(Vector3.zero, isLocal: true, duration: GameManager.Instance.GetDelay()));
        }

        public void MergeWithCell(TileCell tileCell)
        {
            cell.ClearTile();
            cell = null;
            tileCell.GetTile().SetLockTimer(GameManager.Instance.GetDelay()); 
            // to ensure multiple merge doesnt happen to single tile at a single movement

            StartCoroutine(AnimateMovement(tileCell.transform.position, isLocal: false, 
                duration: GameManager.Instance.GetDelay(), isMerging:true));
        }

        private IEnumerator AnimateMovement(Vector3 to, bool isLocal = false, float duration = 0.1f, bool isMerging = false)
        {
            float elapsed  = 0f;
            Vector3 from;

            while (elapsed < duration)
            {
                if (isLocal)
                {
                    from = transform.localPosition;
                    transform.localPosition = Vector3.Lerp(from, to, elapsed / duration);
                }
                else
                {
                    from = transform.position;
                    transform.position = Vector3.Lerp(from, to, elapsed / duration);
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (isLocal)
                transform.localPosition = to;
            else
                transform.position = to;

            if (isMerging)
                Destroy(gameObject); // in case of merging destroy the gameobj after animating
        }

        public static bool CanMerge(Tile a, Tile b)
        {
            if (a == null || b == null) 
                return false;
            else 
                return a.number == b.number && !b.IsLocked();
            // if locked, means that tile had already gone through a merge previously only
        }
    }
}