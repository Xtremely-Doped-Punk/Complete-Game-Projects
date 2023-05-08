using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class LoadingCallback : MonoBehaviour
    {
        [SerializeField] float loadingDelay = 0f;
        private float loadingTimer;

        void Update()
        {
            loadingTimer += Time.deltaTime;
            if (loadingTimer > loadingDelay)
                SceneLoader.Callback();
        }
    }
}