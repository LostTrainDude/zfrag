using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : GenericSingletonClass<AudioController>
{
    /// <summary>
    /// Stores all the AudioSources available in the game
    /// </summary>
    [SerializeField] private AudioSource[] _audioSources;

    /// <summary>
    /// The AudioClip for the drag/drop clack sound
    /// </summary>
    [SerializeField] private AudioClip _clackSound;

    /// <summary>
    /// The AudioClip for the completed defrag chime
    /// </summary>
    [SerializeField] private AudioClip _endChimeSound;

    /// <summary>
    /// The List of Unplayed seek sounds that will be chosen randomly
    /// </summary>
    [SerializeField] private List<AudioClip> _unplayedSeekSounds = new List<AudioClip>();

    /// <summary>
    /// The List that will be filled with the seek sounds after being played
    /// </summary>
    [SerializeField] private List<AudioClip> _playedSeekSounds = new List<AudioClip>();

    /// <summary>
    /// Checks whether the Seek sounds are looping or not
    /// </summary>
    [SerializeField] private bool _isLooping = false;
    
    /// <summary>
    /// Checks whether HDD sounds are enabled or not
    /// </summary>
    [SerializeField] private bool _hddSoundsEnabled = true;
    public bool HDDSoundsEnabled { get => _hddSoundsEnabled; set => _hddSoundsEnabled = value; }

    /// <summary>
    /// Checks whether drag/drop clack sounds are enabled or not
    /// </summary>
    [SerializeField] private bool _clackSoundsEnabled = true;
    public bool ClackSoundsEnabled { get => _clackSoundsEnabled; set => _clackSoundsEnabled = value; }

    private void OnEnable()
    {
        Defragger.OnStateChanged += Defragger_OnStateChanged;
        MouseDrag.OnSectorDropped += MouseDrag_OnDraggableDropped;
    }

    private void OnDisable()
    {
        Defragger.OnStateChanged -= Defragger_OnStateChanged;
        MouseDrag.OnSectorDropped -= MouseDrag_OnDraggableDropped;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Gets the AudioSources available in the scene.
        // 0 = HDD Sounds; 1 = Seek sounds
        _audioSources = GetComponentsInChildren<AudioSource>();
    }

    private void Defragger_OnStateChanged(DefraggerState newState)
    {
        switch(newState)
        {
            case DefraggerState.AUTODEFRAG:
                StartLooping();
                break;

            case DefraggerState.COMPLETE:
                EndLooping();
                if (Defragger.instance.PreviousState != DefraggerState.COMPLETE) PlayEndChime();
                break;

            default:
                EndLooping();
                break;
        }
    }

    private void MouseDrag_OnDraggableDropped()
    {
        PlayClack();
        PlaySeekSound();
    }

    /// <summary>
    /// Pseudorandomly plays one of the Seek sounds from a list
    /// </summary>
    public void PlaySeekSound()
    {
        // Avoid overlapping
        if (_audioSources[1].isPlaying) return;

        _audioSources[1].volume = 1f;

        // Pick a random Seek sound from the Unplayed List
        AudioClip a = _unplayedSeekSounds[UnityEngine.Random.Range(0, _unplayedSeekSounds.Count)];
        
        // Remove it from the Unplayed List
        _unplayedSeekSounds.Remove(a);

        // Play it
        _audioSources[1].PlayOneShot(a);

        // Add it to the Played List
        _playedSeekSounds.Add(a);
        
        // If there are no more Unplayed Seek sounds, reload all the Played
        // AudioClips and start again
        if (_unplayedSeekSounds.Count <= 0)
        {
            _unplayedSeekSounds.AddRange(_playedSeekSounds);
            _playedSeekSounds.Clear();
        }
    }

    /// <summary>
    /// Switches Seek ounds looping on and off
    /// </summary>
    public void ToggleLooping()
    {
        if (_isLooping)
        {
            EndLooping();
        }
        else
        {
            if (!Defragger.instance.IsFreePaintingEnabled && Defragger.instance.State == DefraggerState.AUTODEFRAG)
            {
                StartLooping();
            }
        }
    }

    /// <summary>
    /// Starts the Seek Sounds looping
    /// </summary>
    public void StartLooping()
    {
        // Exit if is already looping
        if (_isLooping)
        {
            return;
        }

        _isLooping = true;

        _audioSources[1].Play();
        _audioSources[0].Stop();
    }

    /// <summary>
    /// Ends the Seek Sounds looping
    /// </summary>
    public void EndLooping()
    {
        // Exit if isn't looping
        if (!_isLooping)
        {
            return;
        }

        _isLooping = false;

        _audioSources[0].Play();
        _audioSources[1].Stop();
    }

    /// <summary>
    /// Decreases the volume for all HDD sounds
    /// </summary>
    public void DecreaseHDDVolume()
    {
        _audioSources[1].outputAudioMixerGroup.audioMixer.GetFloat("hddVolume", out float volume);
        _audioSources[1].outputAudioMixerGroup.audioMixer.SetFloat("hddVolume", volume-2f);
    }

    /// <summary>
    /// Increases the volume for all HDD sounds
    /// </summary>
    public void IncreaseHDDVolume()
    {
        _audioSources[1].outputAudioMixerGroup.audioMixer.GetFloat("hddVolume", out float volume);

        if (volume >= 0f)
        {
            return;
        }

        _audioSources[1].outputAudioMixerGroup.audioMixer.SetFloat("hddVolume", volume + 2f);
    }

    /// <summary>
    /// Decreases the volume for the drag/drop clack sound
    /// </summary>
    public void DecreaseClackVolume()
    {
        float volume;
        _audioSources[2].outputAudioMixerGroup.audioMixer.GetFloat("clackVolume", out volume);
        _audioSources[2].outputAudioMixerGroup.audioMixer.SetFloat("clackVolume", volume - 2f);
    }

    /// <summary>
    /// Increases the volume for the drag/drop clack sound
    /// </summary>
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

    /// <summary>
    /// Switches HDD sounds on and off
    /// </summary>
    public void ToggleHDDSounds()
    {
        _hddSoundsEnabled = !_hddSoundsEnabled;

        _audioSources[0].enabled = !_audioSources[0].enabled;
        _audioSources[1].enabled = !_audioSources[1].enabled;

        if (_isLooping) _audioSources[1].Play();
    }

    /// <summary>
    /// Switches drag/drop clack sounds on and off
    /// </summary>
    public void ToggleClackSounds()
    {
        _clackSoundsEnabled = !_clackSoundsEnabled;

        _audioSources[2].enabled = !_audioSources[2].enabled;
    }

    /// <summary>
    /// Plays the drag/drop clack sound
    /// </summary>
    public void PlayClack()
    {
        if (!_audioSources[2].enabled) return;

        _audioSources[2].PlayOneShot(_clackSound);
    }

    /// <summary>
    /// Plays the finished Defrag chime
    /// </summary>
    public void PlayEndChime()
    {
        if (!_audioSources[2].enabled) return;

        _audioSources[2].PlayOneShot(_endChimeSound);
    }
}
