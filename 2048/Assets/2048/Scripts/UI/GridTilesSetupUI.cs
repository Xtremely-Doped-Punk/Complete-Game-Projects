using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _2048
{
    [Serializable]
    public struct LayoutGroupParameters
    {
        public int leftPadding;
        public int rightPadding;
        public int topPadding;
        public int bottomPadding;

        public bool dynamicSpacing;
        [Range(0,.25f)] public float spacingScale;
        public float maxSpacing;
        public TextAnchor childAlignment;
        public bool reverseArrangement;

        public bool controlChildSizeWidth;
        public bool controlChildSizeHeight;
        public bool useChildScaleWidth;
        public bool useChildScaleHeight;
        public bool childForceExpandWidth;
        public bool childForceExpandHeight;
    }

    public static class Extentions
    {
        public static T TryGetOrAddComponent<T>(this Transform self) where T : Component
        {
            if (self.TryGetComponent<T>(out T comp))
                return comp;
            else
                return self.gameObject.AddComponent<T>();
        }

        public static int TwoPowX(this int power)
        {
            return (1 << power);
        }
        public static int TwoInversePowX(this int value)
        {
            int power = 0;
            while (value > 1)
            {
                value >>= 1;
                power++;
            }
            return power;
        }
    }

    public class GridTilesSetupUI : MonoBehaviour
    {
        [SerializeField] Image bgImg;
        [SerializeField] Transform colTemplateTransform;
        [SerializeField] Transform rowTemplateTransform;
        [SerializeField] Image tileCellImage;
        [SerializeField] bool isChildRow = true;
        [SerializeField] LayoutGroupParameters groupParameters;

        public bool IsInitialized { get; private set; } = false;
        private List<GameObject> instanciatedObjs;

        private void Start()
        {
            SetupBoard();
        }
        private void OnDestroy()
        {
            ResetBoard();
        }

        public void SetupBoard()
        {
            ResetBoard();

            HorizontalOrVerticalLayoutGroup child, parent;
            LayoutGroup[] layoutGroups;
            int m, n;

            if (isChildRow)
            {
                parent = colTemplateTransform.TryGetOrAddComponent<VerticalLayoutGroup>();
                AddLayoutRef(ref parent, groupParameters); parent.gameObject.name = "Column-Vertical";
                child = rowTemplateTransform.TryGetOrAddComponent<HorizontalLayoutGroup>();
                AddLayoutRef(ref child, groupParameters); child.gameObject.name = "Row-Horizontal";

                m = GameManager.Instance.GridWidth;
                n = GameManager.Instance.GridHeight;
            }
            else
            {
                parent = colTemplateTransform.TryGetOrAddComponent<HorizontalLayoutGroup>();
                AddLayoutRef(ref parent, groupParameters); parent.gameObject.name = "Row-Horizontal";
                child = rowTemplateTransform.TryGetOrAddComponent<VerticalLayoutGroup>();
                AddLayoutRef(ref child, groupParameters); child.gameObject.name = "Column-Vertical";

                m = GameManager.Instance.GridHeight;
                n = GameManager.Instance.GridWidth;
            }

            //Debug.Log($"parent:{parent}, child:{child}, (m,n):({m},{n})");

            layoutGroups = new LayoutGroup[n + 1];
            layoutGroups[0] = parent; parent.enabled = true;
            layoutGroups[1] = child; child.enabled = true;

            for (int i = 1; i < m; i++)
            {
                var tile = Instantiate(tileCellImage, child.transform);
                tile.name = tile.name.Replace("Clone",i.ToString());
                instanciatedObjs.Add(tile.gameObject);
            }

            for (int i = 1; i < n; i++)
            {
                var lay = Instantiate(child, parent.transform);
                lay.name = lay.name.Replace("Clone", i.ToString());
                layoutGroups[i + 1] = lay;
                instanciatedObjs.Add(lay.gameObject);
            }

            StartCoroutine(RemoveLayoutGroups(layoutGroups));
        }

        private IEnumerator RemoveLayoutGroups(LayoutGroup[] groups)
        {
            Handheld.SetActivityIndicatorStyle(AndroidActivityIndicatorStyle.Large);
            Handheld.StartActivityIndicator();
            yield return new WaitWhile(IsInvoking);
            yield return new WaitForEndOfFrame();
            Handheld.StopActivityIndicator();

            foreach (LayoutGroup group in groups)
            {
                group.enabled = false;
            }

            // initialize rows first
            Tile1DArray[] tileRows = GetComponentsInChildren<Tile1DArray>();
            foreach (Tile1DArray tileRow in tileRows)
                tileRow.InitializeTileRow();

            // then initialize columns
            TileGrid[] tileGrids = GetComponentsInChildren<TileGrid>();
            foreach (TileGrid tileGrid in tileGrids)
                tileGrid.InitializeTileGrid(isChildRow);

            IsInitialized = true;
        }

        private void AddLayoutRef(ref HorizontalOrVerticalLayoutGroup group, LayoutGroupParameters p)
        {
            group.padding.left = p.leftPadding;
            group.padding.right = p.rightPadding;
            group.padding.top = p.topPadding;
            group.padding.bottom = p.bottomPadding;

            float div = p.maxSpacing;
            if (p.dynamicSpacing)
            {
                var bgSize = bgImg.GetPixelAdjustedRect().size;
                float tileSizeMedian = ((bgSize.x / GameManager.Instance.GridWidth) + (bgSize.y / GameManager.Instance.GridHeight)) / 2;
                div = tileSizeMedian * p.spacingScale;
                //Debug.Log($"GridTilesSetupUI:: Layout Ref for dynamic spacing [tileSizeMedian:{tileSizeMedian}] = {div} for bg-size:{bgSize}");
                if (div > p.maxSpacing) div = p.maxSpacing;
                if (div < 1) div = 1;
            }
            group.spacing = div;

            group.childAlignment = p.childAlignment;
            group.reverseArrangement = p.reverseArrangement;

            group.childControlWidth = p.controlChildSizeWidth;
            group.childControlHeight = p.controlChildSizeHeight;
            group.childScaleWidth = p.useChildScaleWidth;
            group.childScaleHeight = p.useChildScaleHeight;
            group.childForceExpandWidth = p.childForceExpandWidth;
            group.childForceExpandHeight = p.childForceExpandHeight;
        }

        private void ResetBoard()
        {
            IsInitialized = false;
            if (instanciatedObjs != null)
            {
                foreach (var obj in instanciatedObjs)
                {
                    if (obj != null)
                        DestroyImmediate(obj);
                }
                instanciatedObjs.Clear();
            }
            else
                instanciatedObjs = new();

            if (rowTemplateTransform.TryGetComponent<LayoutGroup>(out var rg))
                DestroyImmediate(rg);
            if (colTemplateTransform.TryGetComponent<LayoutGroup>(out var cg))
                DestroyImmediate(cg);
        }
    }
}