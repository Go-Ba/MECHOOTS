using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    List<AudioSource> sources = new List<AudioSource>();

    private void Awake()
    {
        if (instance != this)
        {
            if (instance != null)
                Destroy(instance);
            instance = this;
        }
    }
    private void Start()
    {
        for (int i = 0; i < 6; i++)       
            SpawnAudioSource();
        
    }
    static void ResetSource(AudioSource _source)
    {
        _source.volume = 1;
        _source.pitch = 1;
        _source.transform.position = instance.transform.position;
        _source.loop = false;
        _source.name = "Audio Source";
        _source.spatialBlend = 0;
        _source.maxDistance = 500;
        _source.rolloffMode = AudioRolloffMode.Logarithmic;
    }
    public static void Play(AudioClip _clip, float _pitchRandom, float _volume, Vector3 _position) => Play(_clip, _pitchRandom, _volume, _position, 100, AudioRolloffMode.Logarithmic);
    public static void Play(AudioClip _clip, float _pitchRandom, float _volume, Vector3 _position, float _maxDistance, AudioRolloffMode _rolloff)
    {
        if (_clip == null) return;

        var source = instance.RequestSource();
        ResetSource(source);
        source.transform.position = _position;
        source.clip = _clip;
        source.volume = _volume;
        source.pitch = 1 + Random.Range(-_pitchRandom, _pitchRandom);
        source.name = _clip.name + "Audio Source";
        source.spatialBlend = 1;
        source.maxDistance = _maxDistance;
        source.rolloffMode = _rolloff;
        source.Play();
    }
    public static void Play(AudioClip _clip, float _pitchRandom, float _volume)
    {
        if (_clip == null) return;

        var source = instance.RequestSource();
        ResetSource(source);
        source.clip = _clip;
        source.volume = _volume;
        source.pitch = 1 + Random.Range(-_pitchRandom, _pitchRandom);
        source.name = _clip.name + "Audio Source";
        source.Play();
    }
    public static void Play(AudioClip[] _clips, float _pitchRandom, float _volume)
    {
        if (_clips == null) return;

        var source = instance.RequestSource();
        ResetSource(source);
        AudioClip clip = _clips[Random.Range(0, _clips.Length)];
        source.clip = clip;
        source.volume = _volume;
        source.pitch = 1 + Random.Range(-_pitchRandom, _pitchRandom);
        source.name = clip.name + "Audio Source";
        source.Play();
    }
    public static void StartLoop(AudioClip _clip, float _volume, string _name)
    {
        if (_clip == null) return;

        var source = instance.RequestSource();
        source.name = _name;
        source.clip = _clip;
        source.volume = _volume;
        source.pitch = 1;
        source.loop = true;
        source.Play();
    }
    public static void StopLoop(string _name)
    {
        var source = instance.FindSourceName(_name);
        if (source != null)
        {
            source.Stop();
            source.name = "Audio Source";
            source.loop = false;
        }
    }
    AudioSource RequestSource()
    {
        foreach (var source in sources)        
            if (!source.isPlaying) return source;
        return SpawnAudioSource();
    }
    AudioSource FindSourceName(string _name)
    {
        foreach (var source in sources)
            if (source.name == _name) return source;
        return null;
    }
    AudioSource SpawnAudioSource()
    {
        var obj = new GameObject("Audio Source");
        obj.transform.parent = transform;
        var source = obj.AddComponent<AudioSource>();
        sources.Add(source);

        return source;
    }
}
