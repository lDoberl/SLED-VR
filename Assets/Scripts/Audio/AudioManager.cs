using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource soundSource;
        [SerializeField] private List<Sound> music, sounds;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void PlayMusic(string soundName)
        {
            Sound s = music.Find(x => x.ClipName == soundName);

            if (s == null)
            {
                Debug.LogWarning("Music: " + soundName + " not found!");
                return;
            }
            
            musicSource.clip = s.Clip;
            musicSource.Play();
        }

        public void PlaySound(string soundName)
        {
            Sound s = sounds.Find(x => x.ClipName == soundName);

            if (s == null)
            {
                Debug.LogWarning("Sound " + soundName + " not found!");
                return;
            }
            
            soundSource.PlayOneShot(s.Clip);
        }

        public void PlayRandomSoundByName(string soundName, AudioSource source)
        {
            List<Sound> matchingSounds = sounds.FindAll(sound => sound.ClipName == soundName);

            if (matchingSounds.Count > 0)
            {
                int randomIndex = Random.Range(0, matchingSounds.Count);
                Sound randomSound = matchingSounds[randomIndex];

                source.PlayOneShot(randomSound.Clip);
            }
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }
    }
}
