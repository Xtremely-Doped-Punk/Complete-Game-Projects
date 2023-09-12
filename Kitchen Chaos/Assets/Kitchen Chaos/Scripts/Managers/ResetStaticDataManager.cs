using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class ResetStaticDataManager : MonoBehaviour // dont forget to add this component if testing directly in game-scene
    {
        static ResetStaticDataManager instance;
        private void Awake()
        {
            if (instance != null)
            {
                DestroyImmediate(instance);
                return;
            }
            else
                instance = this; 

            // add all the static event containing classes whenever updated
            BaseCounter.ResetStaticData();
            CounterCutting.ResetStaticData();
            CounterTrash.ResetStaticData();
            PlayerController.ResetStaticData();
        }
    }
}