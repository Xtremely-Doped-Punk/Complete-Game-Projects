using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class PlayerAnimator : MonoBehaviour
    {
        private const string IS_WALKING = "IsWalking";
        [SerializeField] private PlayerController controller;
        [SerializeField] private Animator animator;
        [SerializeField] private float footStepsSFXTimerMax = .1f;

        private float footStepsSFXTimer;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            animator.SetBool(IS_WALKING, false);
        }

        private void Update()
        {
            animator.SetBool(IS_WALKING, controller.IsWalking);

            if (!controller.IsWalking && footStepsSFXTimer < 0) return;
            footStepsSFXTimer -= Time.deltaTime;

            if (footStepsSFXTimer < 0 & controller.IsWalking)
            {
                footStepsSFXTimer = footStepsSFXTimerMax;
                SoundManager.Instance.PlayPlayerFootSteps(transform.position);
            }
        }
    }
}