using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    public interface IHasProgressBar
    {
        public class ProgessChangedEventArg : EventArgs { public float progressNormalized; }
        public event EventHandler<ProgessChangedEventArg> OnProgessChanged;
    }
}