using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KC
{
    public class SoundManager : MonoBehaviour
    {
        private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";

        public static SoundManager Instance { get; private set; } = null;

        [field: SerializeField] public AudioClipRefsSO AudioClipRefsSO { get; private set; } = null;

        private float volume = .8f;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);

            volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
        }

        private void Start()
        {
            DeliveryManager.Instance.OnDeliverySuccess += (object sender, EventArgs e) => 
            PlaySound(AudioClipRefsSO.DeliverySuccess, FindSenderPosition(sender));

            DeliveryManager.Instance.OnDeliveryFailure += (object sender, EventArgs e) => 
            PlaySound(AudioClipRefsSO.DeliveryFailure, FindSenderPosition(sender));

            CounterCutting.OnAnyCut += (object sender, EventArgs e) =>
            PlaySound(AudioClipRefsSO.Chop, FindSenderPosition(sender));
            
            PlayerController.OnPlayerPickedSomething += (object sender, EventArgs e) =>
            PlaySound(AudioClipRefsSO.ObjectPickup, FindSenderPosition(sender));

            BaseCounter.OnPlayerDroppedSomethingOnCounters += (object sender, EventArgs e) =>
            PlaySound(AudioClipRefsSO.ObjectDrop, FindSenderPosition(sender));

            CounterTrash.OnAnyObjectTrashed += (object sender, EventArgs e) =>
            PlaySound(AudioClipRefsSO.Trash, FindSenderPosition(sender));
        }

        private Vector3 FindSenderPosition(object sender)
        {
            // Debug.Log(transform) //error when static events are not reseted
            if (sender is Component)
                return (sender as Component).transform.position;
            else
                Debug.LogError("Given sender-object:" + sender + 
                    " is not component type, thus transform component cant be found!");
            return Vector3.zero;
        }

        private void PlaySound(AudioClip[] audioClips, Vector3 position, float volumeMultiplier = 1f)
        {
            // play random sound
            PlaySound(audioClips[Random.Range(0, audioClips.Length - 1)], position, volumeMultiplier);
        }

        private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
        {
            //Debug.Log("playing:" + audioClip + ", at pos:" + position);
            
            if (volumeMultiplier > 1f || volumeMultiplier < 0)
                volumeMultiplier = Mathf.Clamp(volumeMultiplier, 0f, 1f);

            AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
        }

        public void PlayPlayerFootSteps(Vector3 position)
        {
            PlaySound(AudioClipRefsSO.FootStep, position, volume);
        }

        public void PlayCountdownSound()
        {
            PlaySound(AudioClipRefsSO.Warning, Vector3.zero);
        }

        public void PlayWarningSound(Vector3 position)
        {
            PlaySound(AudioClipRefsSO.Warning, position);
        }

        public void ChangeVolume(float step)
        {
            volume += step;
            if (volume > 1f)
                volume = 0f;

            PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
            PlayerPrefs.Save(); // to prevent save changed data from when it crashes
        }
        public float GetVolume() => volume;
    }
}