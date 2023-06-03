using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class FollowTransform : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private bool shouldUpdateRotation = true;

        public void SetTargetTransform(Transform target, bool shouldUpdateRotation = true)
        {
            targetTransform = target;
            this.shouldUpdateRotation = shouldUpdateRotation;
        }

        private void LateUpdate()
        {
            if (targetTransform == null) 
                return;

            transform.position = targetTransform.position;
            if (shouldUpdateRotation)
                transform.rotation = targetTransform.rotation;
        }
    }
}