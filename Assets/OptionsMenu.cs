using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _endlessDefragButtonText;
    [SerializeField] TextMeshProUGUI _volumeText;
    [SerializeField] TextMeshProUGUI _hddSoundsButtonsText;
    [SerializeField] TextMeshProUGUI _blipButtonsText;
    [SerializeField] TextMeshProUGUI _freePaintingModeButtonText;
    [SerializeField] TextMeshProUGUI _defragSpeedText;
    int _defragSpeed = 4;

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

    public void SwitchEndlessDefrag()
    {
        Defragger.Instance.IsAutoDefragEndless = !Defragger.Instance.IsAutoDefragEndless;
        if (Defragger.Instance.IsAutoDefragEndless)_endlessDefragButtonText.text = "ENABLED";
        else _endlessDefragButtonText.text = "DISABLED";
    }

    public void SwitchFreePaintingMode()
    {
        // ON -> OFF
        if (Defragger.Instance.IsFreePaintingEnabled)
        {
            Defragger.Instance.ExitPaintingState();
            _freePaintingModeButtonText.text = "DISABLED";
        }
        else // OFF -> ON
        {
            Defragger.Instance.EnterPaintingState();          
            _freePaintingModeButtonText.text = "ENABLED";
        }

        Defragger.Instance.ScanGrid();
    }

    public void DecreaseAutoDefragRate()
    {
        if (Defragger.Instance.AutoDefragRate == 1) return;

        Defragger.Instance.AutoDefragRate -= 1;
        _defragSpeed++;
        _defragSpeedText.text = string.Format("Speed: {0}x", _defragSpeed);
    }

    public void IncreaseAutoDefragRate()
    {
        if (Defragger.Instance.AutoDefragRate == 10) return;

        Defragger.Instance.AutoDefragRate += 1;
        _defragSpeed--;

        _defragSpeedText.text = string.Format("Speed: {0}x", _defragSpeed);
    }

    public void Start()
    {
        //_volumeText.text = "Volume: " + (AudioController.instance.AudioSources[0].volume * 10f).ToString("0");
        _defragSpeedText.text = string.Format("Speed: {0}x", _defragSpeed);
    }

    public void OnEnable()
    {
        if (Defragger.Instance.IsFreePaintingEnabled) _freePaintingModeButtonText.text = "ENABLED";
        else _freePaintingModeButtonText.text = "DISABLED";
    }
}
