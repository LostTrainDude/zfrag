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

    [SerializeField] bool HDDSoundsEnabled = true;
    [SerializeField] bool BlipSoundsEnabled = true;

    public void SwitchHDDSounds()
    {
        AudioController.instance.SwitchHDDSounds();
        HDDSoundsEnabled = !HDDSoundsEnabled;
        if (HDDSoundsEnabled) _hddSoundsButtonsText.text = "ENABLED";
        else _hddSoundsButtonsText.text = "DISABLED";
    }

    public void SwitchBlipSounds()
    {
        AudioController.instance.SwitchBlipSounds();
        BlipSoundsEnabled= !BlipSoundsEnabled;
        if (BlipSoundsEnabled) _blipButtonsText.text = "ENABLED";
        else _blipButtonsText.text = "DISABLED";
    }

    public void RaiseVolume()
    {
        AudioController.instance.RaiseVolume();
        _volumeText.text = "Volume: " + (AudioController.instance.AudioSources[0].volume * 10f).ToString("0");
    }

    public void LowerVolume()
    {
        AudioController.instance.LowerVolume();
        _volumeText.text = "Volume: " + (AudioController.instance.AudioSources[0].volume * 10f).ToString("0");
    }

    public void OnEnable()
    {
        //_volumeText.text = "Volume: " + (AudioController.instance.AudioSources[0].volume * 10f).ToString("0");
    }
}
