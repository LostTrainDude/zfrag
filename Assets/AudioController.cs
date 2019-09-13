using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioController _instance;
    public static AudioController instance { get => _instance; }

    [SerializeField] private AudioSource[] _audioSources;
    [SerializeField] private AudioClip _clackSound;

    [SerializeField] private bool _isLooping = false;

    [SerializeField] private bool _hddSoundsEnabled = true;
    public bool HDDSoundsEnabled { get => _hddSoundsEnabled; set => _hddSoundsEnabled = value; }

    [SerializeField] private bool _clackSoundsEnabled = true;
    public bool ClackSoundsEnabled { get => _clackSoundsEnabled; set => _clackSoundsEnabled = value; }

    [SerializeField] private List<AudioClip> _unplayedSeekSounds = new List<AudioClip>();
    [SerializeField] private List<AudioClip> _playedSeekSounds = new List<AudioClip>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _audioSources = GetComponentsInChildren<AudioSource>();
    }

    public void PlaySeekSound()
    {
        if (_audioSources[1].isPlaying) return;

        _audioSources[1].volume = 1f;

        AudioClip a = _unplayedSeekSounds[UnityEngine.Random.Range(0, _unplayedSeekSounds.Count)];
        _unplayedSeekSounds.Remove(a);

        _audioSources[1].PlayOneShot(a);

        _playedSeekSounds.Add(a);
        if (_unplayedSeekSounds.Count <= 0)
        {
            _unplayedSeekSounds.AddRange(_playedSeekSounds);
            _playedSeekSounds.Clear();
        }
    }

    public void ToggleLooping()
    {
        if (_isLooping)
        {
            EndLooping();
        }
        else
        {
            StartLooping();
        }
    }

    public void StartLooping()
    {
        if (_isLooping)
        {
            return;
        }

        _isLooping = true;

        _audioSources[1].Play();
        _audioSources[0].Stop();
    }

    public void EndLooping()
    {
        if (!_isLooping)
        {
            return;
        }

        _isLooping = false;

        _audioSources[0].Play();
        _audioSources[1].Stop();
    }

    public IEnumerator FadeOut(AudioSource audioSource, float FadeTime, bool stop=true)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            yield return null;
        }

        if (stop)
        {
            audioSource.Stop();
        }
    }

    public IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        audioSource.volume = 0f;

        while (audioSource.volume < 1)
        {
            audioSource.volume += Time.deltaTime / FadeTime;
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            PlaySeekSound();
        }
    }

    public void DecreaseHDDVolume()
    {
        float volume;
        _audioSources[1].outputAudioMixerGroup.audioMixer.GetFloat("hddVolume", out volume);
        _audioSources[1].outputAudioMixerGroup.audioMixer.SetFloat("hddVolume", volume-2f);
    }

    public void IncreaseHDDVolume()
    {
        float volume;
        _audioSources[1].outputAudioMixerGroup.audioMixer.GetFloat("hddVolume", out volume);

        if (volume >= 0f)
        {
            return;
        }

        _audioSources[1].outputAudioMixerGroup.audioMixer.SetFloat("hddVolume", volume + 2f);
    }

    public void DecreaseClackVolume()
    {
        float volume;
        _audioSources[2].outputAudioMixerGroup.audioMixer.GetFloat("clackVolume", out volume);
        _audioSources[2].outputAudioMixerGroup.audioMixer.SetFloat("clackVolume", volume - 2f);
    }

    public void IncreaseClackVolume()
    {
        float volume;
        _audioSources[2].outputAudioMixerGroup.audioMixer.GetFloat("clackVolume", out volume);

        if (volume >= 0f)
        {
            return;
        }

        _audioSources[2].outputAudioMixerGroup.audioMixer.SetFloat("clackVolume", volume + 2f);
    }

    public void ToggleHDDSounds()
    {
        _hddSoundsEnabled = !_hddSoundsEnabled;

        _audioSources[0].enabled = !_audioSources[0].enabled;
        _audioSources[1].enabled = !_audioSources[1].enabled;

        if (_isLooping)
        {
            _audioSources[1].Play();
        }
    }

    public void ToggleClackSounds()
    {
        _clackSoundsEnabled = !_clackSoundsEnabled;

        _audioSources[2].enabled = !_audioSources[2].enabled;
    }

    public void PlayClack()
    {
        if (!_audioSources[2].enabled)
        {
            return;
        }

        _audioSources[2].PlayOneShot(_clackSound);
    }
}
