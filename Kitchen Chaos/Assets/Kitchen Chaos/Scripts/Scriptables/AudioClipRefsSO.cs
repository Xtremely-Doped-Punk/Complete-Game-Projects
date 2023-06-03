using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KC
{
    [CreateAssetMenu(menuName = "KC/AudioClipRefs")]
    public class AudioClipRefsSO : ScriptableObject
    {
        [field: SerializeField] public AudioClip[] Chop { get; private set; } = null;
        [field: SerializeField] public AudioClip[] DeliveryFailure { get; private set; } = null;
        [field: SerializeField] public AudioClip[] DeliverySuccess { get; private set; } = null;
        [field: SerializeField] public AudioClip[] FootStep { get; private set; } = null;
        [field: SerializeField] public AudioClip[] ObjectDrop { get; private set; } = null;
        [field: SerializeField] public AudioClip[] ObjectPickup { get; private set; } = null;
        [field: SerializeField] public AudioClip StoveSizzle { get; private set; } = null;
        [field: SerializeField] public AudioClip[] Trash { get; private set; } = null;
        [field: SerializeField] public AudioClip[] Warning { get; private set; } = null;
    }
}