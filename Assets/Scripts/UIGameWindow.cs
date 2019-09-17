using UnityEngine;
using TMPro;

public class UIGameWindow : MonoBehaviour
{
    /// <summary>
    /// The Text component for the "AUTODEFRAG ENABLED/DISABLED" label
    /// </summary>
    [SerializeField] private TextMeshProUGUI _autoDefragLabelText;

    /// <summary>
    /// Updates the label on the "Free Painting" button in the Options Menu
    /// </summary>
    public void UpdateToggleAutoDefragButtonLabel()
    {
        if (Defragger.instance.State == DefraggerState.AUTODEFRAG && !Defragger.instance.IsFreePaintingEnabled)
        {
            _autoDefragLabelText.text = "AUTODEFRAG ENABLED";
        }
        else
        {
            _autoDefragLabelText.text = "AUTODEFRAG DISABLED";
        }
    }
}
