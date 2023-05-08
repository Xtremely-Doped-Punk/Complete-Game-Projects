using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class Visual_CounterPlates : MonoBehaviour
    {
        [SerializeField] private CounterPlates counterPlates;
        [SerializeField] private GameObject plateVisualPrefab;
        [SerializeField] private float PlateThickness = 0.065f;

        private List<GameObject> plateVisualGameObjectList = new();

        private void Start()
        {
            counterPlates.OnPlatePicked += HandleVisualsOnPlatePicked;
            counterPlates.OnPlateSpawned += HandleVisualsOnPlateSpawned;
        }

        private void HandleVisualsOnPlatePicked(object sender, System.EventArgs e)
        {
            int lastPlateIndex = plateVisualGameObjectList.Count - 1;
            Destroy(plateVisualGameObjectList[lastPlateIndex]);
            plateVisualGameObjectList.RemoveAt(lastPlateIndex);
        }

        private void HandleVisualsOnPlateSpawned(object sender, System.EventArgs e)
        {

            GameObject plateVisualObj = Instantiate(plateVisualPrefab, counterPlates.GetHolderTransform());

            var kitchenObjLocalPos = plateVisualObj.transform.localPosition;
            plateVisualObj.transform.localPosition +=
                new Vector3(kitchenObjLocalPos.x,
                kitchenObjLocalPos.y + plateVisualGameObjectList.Count * PlateThickness,
                kitchenObjLocalPos.z);

            plateVisualGameObjectList.Add(plateVisualObj); // append to last as we dont want offset for 1st plate
        }
    }
}