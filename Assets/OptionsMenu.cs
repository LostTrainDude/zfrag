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
    public TextMeshProUGUI FreePaintingModeButtonText { get => _freePaintingModeButtonText; set => _freePaintingModeButtonText = value; }

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

    public void SwitchAutoDefrag()
    {
        if (Defragger.Instance.State.GetType() == typeof(AutoDefragState))
        {   
            Defragger.Instance.State.Exit();
        }
        else
        {
            Defragger.Instance.State = new AutoDefragState(Defragger.Instance, Defragger.Instance.State);
        }
    }

    /// <summary>
    /// Switch between DefaultPlayState and FreePaintingState
    /// </summary>
    public void SwitchFreePaintingMode()
    {
        if (Defragger.Instance.State.GetType() == typeof(FreePaintingState))
        {
            _freePaintingModeButtonText.text = "DISABLED";
            Defragger.Instance.State = new DefaultPlayState(Defragger.Instance, Defragger.Instance.State);
            Defragger.Instance.ScanGrid();
        }
        else
        {
            _freePaintingModeButtonText.text = "ENABLED";
            Defragger.Instance.State = new FreePaintingState(Defragger.Instance);
        }
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
        _defragSpeedText.text = string.Format("Speed: {0}x", _defragSpeed);
    }

    public void OnEnable()
    {
        if (Defragger.Instance.State.GetType() == typeof(FreePaintingState))
        {
            _freePaintingModeButtonText.text = "ENABLED";
        }
        else
        {
            _freePaintingModeButtonText.text = "DISABLED";
        }
    }
}
