using UnityEngine;
using TMPro;

public class UIOptionsMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _endlessDefragButtonText;
    [SerializeField] private TextMeshProUGUI _hddSoundsButtonsText;
    [SerializeField] private TextMeshProUGUI _clackSoundsText;
    [SerializeField] private TextMeshProUGUI _freePaintingModeButtonText;
    [SerializeField] private TextMeshProUGUI _defragSpeedText;

    public void OnEnable()
    {
        UpdateToggleFreePaintingButtonLabel();
    }

    public void Start()
    {
        UpdateDefragSpeedText();
    }

    /// <summary>
    /// Updates the label on the "Free Painting" button in the Options Menu
    /// </summary>
    public void UpdateToggleFreePaintingButtonLabel()
    {
        if (Defragger.instance.IsFreePaintingEnabled)
        {
            _freePaintingModeButtonText.text = "ENABLED";
            //_autoDefraggingLabelText.text = "AUTODEFRAG DISABLED";
        }
        else
        {
            _freePaintingModeButtonText.text = "DISABLED";
        }
    }

    /// <summary>
    /// Updates the Defrag Speed text component in the Options Menu
    /// </summary>
    public void UpdateDefragSpeedText()
    {
        _defragSpeedText.text = string.Format("Speed: {0}x", Defragger.instance.DefragSpeed);
    }

    /// <summary>
    /// Updates the label on the "Hard Disk Sounds" button in the Options Menu
    /// </summary>
    public void UpdateToggleHDDSoundsButtonLabel()
    {
        if (AudioController.instance.HDDSoundsEnabled)
        {
            _hddSoundsButtonsText.text = "ENABLED";
        }
        else
        {
            _hddSoundsButtonsText.text = "DISABLED";
        }
    }

    /// <summary>
    /// Updates the label on the "Other Sounds" button in the Options Menu
    /// </summary>
    public void UpdateToggleClackSoundsButtonLabel()
    {
        if (AudioController.instance.ClackSoundsEnabled)
        {
            _clackSoundsText.text = "ENABLED";
        }
        else
        {
            _clackSoundsText.text = "DISABLED";
        }
    }

    /// <summary>
    /// Updates the label on the "Endless Defrag" button in the Options Menu
    /// </summary>
    public void UpdateToggleEndlessDefragButtonLabel()
    {
        if (Defragger.instance.IsAutoDefragEndless)
        {
            _endlessDefragButtonText.text = "ENABLED";
        }
        else
        {
            _endlessDefragButtonText.text = "DISABLED";
        }
    }
}
