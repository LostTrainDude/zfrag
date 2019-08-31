using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController instance;

    public AudioSource[] AudioSources;
    public AudioClip HDDStart;
    public AudioClip HDDOnGoing;
    public AudioClip HDDStop;
    public AudioClip Clack;

    public bool HDDSoundsEnabled = true;
    public bool BlipSoundsEnabled = true;


    public List<AudioClip> Loops = new List<AudioClip>();

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

    public void StartLooping()
    {
        if (AudioSources[1].isPlaying) return;
        StartCoroutine(FadeOut(AudioSources[0], 0.4f));
        StartCoroutine(FadeIn(AudioSources[1], 0.4f));
    }

    public void EndLooping()
    {
        if (AudioSources[0].isPlaying) return;
        StartCoroutine(FadeOut(AudioSources[1], 0.3f));
        StartCoroutine(FadeIn(AudioSources[0], 0.3f));
    }

    public static IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;
            yield return null;
        }
        audioSource.Stop();
    }

    public static IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        audioSource.Play();
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
