using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Audio
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider soundSlider;

        private void Start()
        {
            if (PlayerPrefs.HasKey("Music") && PlayerPrefs.HasKey("Sound"))
            {
                LoadVolume();
            }
            else
            {
                SetMusicVolume();
                SetSoundVolume();
            }
        }

        public void SetMusicVolume()
        {
            float volume = musicSlider.value;
            audioMixer.SetFloat("Music", MathF.Log10(volume) * 20);
            PlayerPrefs.SetFloat("Music", volume);
        }

        public void SetSoundVolume()
        {
            float volume = soundSlider.value;
            audioMixer.SetFloat("Sound", MathF.Log10(volume) * 20);
            PlayerPrefs.SetFloat("Sound", volume);
        }

        private void LoadVolume()
        {
            float musicVolume = PlayerPrefs.GetFloat("Music");
            musicSlider.value = musicVolume;

            float soundVolume = PlayerPrefs.GetFloat("Sound");
            soundSlider.value = soundVolume;
        }
    }
}
