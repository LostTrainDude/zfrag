using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;

    public AudioSource[] AudioSources;
    public AudioClip Clack;

    public bool HasLoopingStarted = false;

    public bool HDDSoundsEnabled = true;
    public bool BlipSoundsEnabled = true;

    public List<AudioClip> UnplayedSeekSounds = new List<AudioClip>();
    public List<AudioClip> PlayedSeekSounds = new List<AudioClip>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        AudioSources = GetComponentsInChildren<AudioSource>();
    }

    public void PlaySeekSound()
    {
        if (AudioSources[1].isPlaying) return;

        AudioSources[1].volume = 1f;

        AudioClip a = UnplayedSeekSounds[UnityEngine.Random.Range(0, UnplayedSeekSounds.Count)];
        UnplayedSeekSounds.Remove(a);

        AudioSources[1].PlayOneShot(a);

        PlayedSeekSounds.Add(a);
        if (UnplayedSeekSounds.Count <= 0)
        {
            UnplayedSeekSounds.AddRange(PlayedSeekSounds);
            PlayedSeekSounds.Clear();
        }
    }

    public IEnumerator TemporaryFade(AudioSource a)
    {
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(FadeIn(a, .25f));
    }

    public void StartLooping()
    {
        if (HasLoopingStarted) return;

        HasLoopingStarted = true;
        AudioSources[1].Play();
        AudioSources[0].Stop();
        //StartCoroutine(FadeOut(AudioSources[0], 1.5f));
        //StartCoroutine(FadeIn(AudioSources[1], .5f));
    }

    public void EndLooping()
    {
        if (!HasLoopingStarted) return;

        HasLoopingStarted = false;
        AudioSources[0].Play();
        AudioSources[1].Stop();
        //StartCoroutine(FadeOut(AudioSources[1], 1.5f));
        //StartCoroutine(FadeIn(AudioSources[0], .5f));
    }

    public IEnumerator FadeOut(AudioSource audioSource, float FadeTime, bool stop=true)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            yield return null;
        }

        if (stop) audioSource.Stop();
    }

    public IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        if (!audioSource.isPlaying) audioSource.Play();
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

    public void LowerHDDVolume()
    {
        float volume = 0f;
        AudioSources[1].outputAudioMixerGroup.audioMixer.GetFloat("hddVolume", out volume);
        AudioSources[1].outputAudioMixerGroup.audioMixer.SetFloat("hddVolume", volume-2f);
    }

    public void RaiseHDDVolume()
    {
        float volume = 0f;
        AudioSources[1].outputAudioMixerGroup.audioMixer.GetFloat("hddVolume", out volume);
        if (volume >= 0f) return;
        AudioSources[1].outputAudioMixerGroup.audioMixer.SetFloat("hddVolume", volume + 2f);
    }

    public void LowerClackVolume()
    {
        float volume = 0f;
        AudioSources[2].outputAudioMixerGroup.audioMixer.GetFloat("clackVolume", out volume);
        AudioSources[2].outputAudioMixerGroup.audioMixer.SetFloat("clackVolume", volume - 2f);
    }

    public void RaiseClackVolume()
    {
        float volume = 0f;
        AudioSources[2].outputAudioMixerGroup.audioMixer.GetFloat("clackVolume", out volume);
        if (volume >= 0f) return;
        AudioSources[2].outputAudioMixerGroup.audioMixer.SetFloat("clackVolume", volume + 2f);
    }

    public void SwitchHDDSounds()
    {
        AudioSources[1].enabled = !AudioSources[1].enabled;
        if (AudioSources[1].enabled)
        {
            AudioSources[1].Play();
        }
    }

    public void SwitchBlipSounds()
    {
        AudioSources[2].enabled = !AudioSources[2].enabled;
    }

    public void PlayClack()
    {
        if (!AudioSources[2].enabled) return;
        AudioSources[2].PlayOneShot(Clack);
    }
}
