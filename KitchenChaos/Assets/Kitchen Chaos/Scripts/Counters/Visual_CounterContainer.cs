using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class Visual_CounterContainer : MonoBehaviour
    {
        private const string OPEN_CLOSE = "OpenClose";
        [SerializeField] private Animator animator;
        [SerializeField] private CounterContainer counterContainer;
        [SerializeField] private SpriteRenderer doorSpriteRenderer;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void Start()
        {
            counterContainer.OnPlayerInteraction += HandleContainerVisualOnInteraction;

            doorSpriteRenderer.sprite = counterContainer.KitchenItemSO.Icon;
            doorSpriteRenderer.enabled = true;
        }

        private void HandleContainerVisualOnInteraction(object sender, System.EventArgs e)
        {
            animator.SetTrigger(OPEN_CLOSE);
        }
    }
}