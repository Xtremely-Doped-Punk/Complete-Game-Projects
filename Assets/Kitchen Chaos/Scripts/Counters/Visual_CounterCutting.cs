using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KC
{
    public class Visual_CounterCutting : MonoBehaviour
    {
        private const string CUT = "Cut";
        [SerializeField] private Animator animator;
        [SerializeField] private CounterCutting counterCutting;
        [SerializeField] private ProgressBarUI progressBar;

        private void Awake()
        {
            if (counterCutting == null)
                Debug.Log("Counter Cutting reference not set to Visual Script:" + this);

            progressBar.SetHasProgressBarReference(counterCutting);

            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        private void Start()
        {
            counterCutting.OnPlayerSwitchInteraction += HandleCuttingVisualsOnInteraction;
            counterCutting.OnProgessChanged += HandleCuttingVisualsOnInteraction;
        }

        private void HandleCuttingVisualsOnInteraction(object sender, EventArgs e)
        {
            progressBar.gameObject.SetActive(counterCutting.HasKitchenObject());
        }

        private void HandleCuttingVisualsOnInteraction(object sender, IHasProgressBar.ProgessChangedEventArg e)
        {
            // if given 0 value, means it was reset, then no need to play visuals of progressing
            if (e.progressNormalized > 0)
                animator.SetTrigger(CUT);
        }
    }
}