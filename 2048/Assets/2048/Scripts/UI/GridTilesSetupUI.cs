using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _2048
{
    public class GridTilesSetupUI : MonoBehaviour
    {
        [SerializeField] HorizontalOrVerticalLayoutGroup colTemplateLayout;
        [SerializeField] HorizontalOrVerticalLayoutGroup rowTemplateLayout;
        [SerializeField] Image tileCellImage;
        [SerializeField] bool isChildRow = true;

        private void Start()
        {
            LayoutGroup child, parent;
            LayoutGroup[] layoutGroups;
            int n;

            if (isChildRow)
            {
                child = rowTemplateLayout;
                parent = colTemplateLayout;
                n = GameManager.Instance.GridWidth;
            }
            else
            {
                parent = rowTemplateLayout;
                child = colTemplateLayout;
                n = GameManager.Instance.GridHeight;
            }

            layoutGroups = new LayoutGroup[n + 1];
            layoutGroups[0] = parent;
            layoutGroups[1] = child;

            for (int i = 1; i < n; i++)
            {
                Instantiate(tileCellImage, child.transform);
            }

            for (int i = 1; i < n; i++)
            {
                layoutGroups[i+1] = Instantiate(child, parent.transform);
            }

            StartCoroutine(RemoveLayoutGroups(layoutGroups));
        }

        private IEnumerator RemoveLayoutGroups(LayoutGroup[] groups)
        {
            yield return new WaitForEndOfFrame();
            foreach (LayoutGroup group in groups)
            {
                group.enabled = false;
            }

            // initialize rows first
            TileRow[] tileRows = GetComponentsInChildren<TileRow>();
            foreach(TileRow tileRow in tileRows)
                tileRow.InitializeTileRow();

            // then initialize columns
            TileGrid[] tileGrids = GetComponentsInChildren<TileGrid>();
            foreach (TileGrid tileGrid in tileGrids)
                tileGrid.InitializeTileGrid();
        }
    }
}