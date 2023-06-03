using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace KC
{
    public class PlayerAnimator : NetworkBehaviour
    {
        private const string IS_WALKING = "IsWalking";
        [SerializeField] private PlayerController controller;
        [SerializeField] private Animator animator;
        [SerializeField] private float footStepsSFXTimerMax = .1f;

        private float footStepsSFXTimer;
        private bool isClientAuth;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            
            animator.SetBool(IS_WALKING, false);

            isClientAuth = animator.transform.GetComponent<NetworkAnimator>() is OwnerNetworkAnimator;
        }

        private void Update()
        {
            // multiplayer check condition
            if (!IsOwner) return;

            // rest game mechanics
            NetworkHandlePlayerAnimatorAuth();

            if (!controller.IsWalkAnimTriggered && footStepsSFXTimer < 0) return;
            footStepsSFXTimer -= Time.deltaTime;

            if (footStepsSFXTimer < 0 & controller.IsWalkAnimTriggered)
            {
                footStepsSFXTimer = footStepsSFXTimerMax;
                SoundManager.Instance.PlayPlayerFootSteps(transform.position);
            }
        }

        private void NetworkHandlePlayerAnimatorAuth()
        {
            if (isClientAuth)
                UpdateAnimator();
            else
                NetworkHandlePlayerAnimatorServerRpc();
        }

        [ServerRpc(RequireOwnership = false)] // server rpc to reponds to client's request
        private void NetworkHandlePlayerAnimatorServerRpc()
        {
            UpdateAnimator();
        }

        private void UpdateAnimator()
        {
            animator.SetBool(IS_WALKING, controller.IsWalkAnimTriggered);
        }
    }
}