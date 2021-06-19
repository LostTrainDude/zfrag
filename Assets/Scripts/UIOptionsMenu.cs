using UnityEngine;
using TMPro;

public class UIOptionsMenu : MonoBehaviour
{
    /// <summary>
    /// The Text component for the label on the "Endless Defrag" button
    /// </summary>
    [SerializeField] private TextMeshProUGUI _endlessDefragButtonText;

    /// <summary>
    /// The Text component for the label on the "Hard Disk Sounds" button
    /// </summary>
    [SerializeField] private TextMeshProUGUI _hddSoundsButtonsText;

    /// <summary>
    /// The Text component for the label on the "Other Sounds" button
    /// </summary>
    [SerializeField] private TextMeshProUGUI _clackSoundsText;

    /// <summary>
    /// The Text component for the label on the "Free Painting" button
    /// </summary>
    [SerializeField] private TextMeshProUGUI _freePaintingModeButtonText;

    /// <summary>
    /// The Text component for the Defrag Speed label
    /// </summary>
    [SerializeField] private TextMeshProUGUI _defragSpeedLabelText;

    /// <summary>
    /// The Text component for the Defrag Speed label
    /// </summary>
    [SerializeField] private TextMeshProUGUI _badSectorsPercentage;

    /// <summary>
    /// Updates the label on the "Free Painting" button
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
    /// Updates the Defrag Speed Text component
    /// </summary>
    public void UpdateDefragSpeedText()
    {
        _defragSpeedLabelText.text = $"Speed: {Defragger.instance.DefragSpeed}x";
    }

    public void UpdateBadSectorsPercentageText()
    {
        _badSectorsPercentage.text = $"Bad Sectors occurrence: {Defragger.instance.BadSectorPercentage}%";
    }

    /// <summary>
    /// Updates the label on the "Hard Disk Sounds" button
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
    /// Updates the label on the "Other Sounds" button
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
    /// Updates the label on the "Endless Defrag" button
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

    public void OnEnable()
    {
        UpdateToggleFreePaintingButtonLabel();
    }

    public void Start()
    {
        UpdateDefragSpeedText();
        UpdateBadSectorsPercentageText();
    }
}
