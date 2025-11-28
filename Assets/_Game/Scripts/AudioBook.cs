using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    //better use serialized dictionary from odin or import it
    [CreateAssetMenu(fileName = "AudioBook", menuName = "Game/AudioBook", order = 0)]
    public class AudioBook : ScriptableObject
    {
        private Dictionary<Audio, AudioClip[] > _audios;

        [SerializeField] private AudioData[] Audios;
        
        public AudioClip this[Audio key]
        {
            get
            {
                if (_audios == null)
                {
                    _audios = new Dictionary<Audio, AudioClip[]>();
                    foreach (var audio in Audios)
                    {
                        _audios.Add(audio.Key, audio.Clips);
                    }
                }

                if (_audios.TryGetValue(key, out var clips))
                {
                    return clips[Random.Range(0, clips.Length)];
                }

                return null;
            }
        }
    }

    [Serializable]
    public class AudioData
    {
        public Audio Key;
        public AudioClip[] Clips;
    }

    public enum Audio
    {
        Flip,
        Match,
        Mismatch,
        GameOver
    }
}