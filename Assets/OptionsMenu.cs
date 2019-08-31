using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _volumeText;
    [SerializeField] TextMeshProUGUI _hddSoundsButtonsText;
    [SerializeField] TextMeshProUGUI _blipButtonsText;

    public void SwitchHDDSounds()
    {
        AudioController.instance.SwitchHDDSounds();
        AudioController.instance.HDDSoundsEnabled = !AudioController.instance.HDDSoundsEnabled;
        if (AudioController.instance.HDDSoundsEnabled) _hddSoundsButtonsText.text = "ENABLED";
        else _hddSoundsButtonsText.text = "DISABLED";
    }

    public void SwitchBlipSounds()
    {
        AudioController.instance.SwitchBlipSounds();
        AudioController.instance.BlipSoundsEnabled = !AudioController.instance.BlipSoundsEnabled;
        if (AudioController.instance.BlipSoundsEnabled) _blipButtonsText.text = "ENABLED";
        else _blipButtonsText.text = "DISABLED";
    }

    public void OnEnable()
    {
        //_volumeText.text = "Volume: " + (AudioController.instance.AudioSources[0].volume * 10f).ToString("0");
    }
}
