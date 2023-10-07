using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private List<Audio> audioList = new();
    public Dictionary<string, Audio> audios = new();
    private void Start()
    {
        audios = new();
        foreach (var audio in audioList)
        {
            audios.Add(audio.key.ToLower().Trim(), audio);
        }
    }
    public void Load()
    {
        foreach (var kvp in audios)
        {
            kvp.Value.Load();
        }
    }
    public void Play(string key)
    {
        if (audios.ContainsKey(key))
        {
            audios[key].Play();
        }
    }
    public void Stop(string key)
    {
        if (audios.ContainsKey(key))
        {
            audios[key].Stop();
        }
    }

    [System.Serializable]
    public class Audio
    {
        public string key;
        public AudioSource source;
        public bool sfx;
        public bool loop;
        public bool forcePlay = true;
        public bool mute;
        public void Load()
        {

        }
        public void Play()
        {
            if (!mute)
            {
                if (!forcePlay && source.isPlaying)
                {
                    return;
                }
                if (source.isPlaying && forcePlay)
                {
                    source.Stop();
                }
                source.Play();
            }
            else
            {
                Stop();
            }
        }
        public void Stop()
        {
            if (source.isPlaying)
            {
                source.Stop();
            }
        }
    }
}
