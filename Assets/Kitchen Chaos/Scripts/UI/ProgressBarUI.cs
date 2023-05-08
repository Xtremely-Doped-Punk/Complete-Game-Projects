using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class ProgressBarUI : MonoBehaviour
    {
        [SerializeField] private Transform hasProgressTransform;
        [SerializeField] private Transform progressBarParent;
        [SerializeField] private Image progressBarImage;
        [field: SerializeField] public bool HideProgressOnExtremes { get; private set; } = true;

        // note interfaces cant be exposed/serialized as there is no guarantee that i would have used by a class
        // thus only work arround is exposed-gameObj's transform.getComp<Interface>() at start()
        private IHasProgressBar hasProgress; 
        
        private void Start()
        {
            if (hasProgress == null && !hasProgressTransform.TryGetComponent<IHasProgressBar>(out hasProgress))
            {
                hasProgress = hasProgressTransform.GetComponentInParent<IHasProgressBar>();
                if (hasProgress == null)
                    Debug.LogError("given reference to 'hasProgressTransform':" + hasProgressTransform +
                        " does'nt have any class implementing IProgressBar interface!");
            }

            hasProgress.OnProgessChanged += HandleProgressVisualsOnChanged;
            progressBarImage.fillAmount = 0;
            ParentSetActive(false);
        }

        private void HandleProgressVisualsOnChanged(object sender, IHasProgressBar.ProgessChangedEventArg e)
        {
            ParentSetActive(HideProgressOnExtremes && !(e.progressNormalized <= 0f || e.progressNormalized >= 1f));
            progressBarImage.fillAmount = e.progressNormalized;
        }


        private void ParentSetActive(bool active)
        {
            progressBarParent.gameObject.SetActive(active);
        }

        public void SetHasProgressBarReference(IHasProgressBar hasProgress)
        { this.hasProgress = hasProgress; }
    }
}