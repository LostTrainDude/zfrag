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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LowerVolume()
    {
        foreach (AudioSource a in AudioSources)
        {
            a.volume -= 0.1f;
        }
    }

    public void RaiseVolume()
    {
        foreach (AudioSource a in AudioSources)
        {
            a.volume += 0.1f;
        }
    }

    public void SwitchHDDSounds()
    {
        AudioSources[0].enabled = !AudioSources[0].enabled;
    }

    public void SwitchBlipSounds()
    {
        AudioSources[0].enabled = !AudioSources[1].enabled;
    }
}
