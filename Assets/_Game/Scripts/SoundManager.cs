using System.Runtime.CompilerServices;
using UnityEngine;

namespace Game
{
    //todo: use service locator
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private AudioBook _audioBook;
        [SerializeField] private AudioSource _audioSource;
        
        private static SoundManager _instance;

        public static SoundManager Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindObjectOfType<SoundManager>();
                return _instance;
            }
        }

        public void PlaySound(Audio audioClip)
        {
            var clip = _audioBook[audioClip];
            if (clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarningFormat("No clip found for sound {0}", audioClip);
            }
        }
    }
}