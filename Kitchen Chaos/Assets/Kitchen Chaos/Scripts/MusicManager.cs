using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace KC
{
    public class MusicManager : MonoBehaviour
    {
        private const string PLAYER_PREFS_MUSIC_VOLUME = "MusicVolume";
        public static MusicManager Instance { get; private set; } = null;

        [SerializeField] private AudioSource audioSource;

        private float volume = .2f;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);

            if (audioSource==null)
                audioSource = GetComponent<AudioSource>();
            audioSource.volume = volume;

            volume = PlayerPrefs.GetFloat(PLAYER_PREFS_MUSIC_VOLUME, volume);
        }

        public void ChangeVolume(float step)
        {
            volume += step;
            if (volume > 1f)
                volume = 0f;

            audioSource.volume = volume;

            PlayerPrefs.SetFloat(PLAYER_PREFS_MUSIC_VOLUME, volume);
            PlayerPrefs.Save(); // to prevent save changed data from when it crashes
        }
        public float GetVolume() => volume;
    }
}