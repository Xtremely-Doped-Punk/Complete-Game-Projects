using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public class ResetStaticDataManager : MonoBehaviour
    {
        private void Awake()
        {
            // add all the static event containing classes whenever updated
            BaseCounter.ResetStaticData();
            CounterCutting.ResetStaticData();
            CounterTrash.ResetStaticData();
            PlayerController.ResetStaticData();
        }
    }
}