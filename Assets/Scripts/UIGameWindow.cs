using UnityEngine;
using TMPro;

public class UIGameWindow : MonoBehaviour
{
    /// <summary>
    /// The Text component for the "AUTODEFRAG ENABLED/DISABLED" label
    /// </summary>
    [SerializeField] private TextMeshProUGUI _autoDefragLabelText;

    private void OnEnable()
    {
        Defragger.OnStateChanged += Defragger_OnStateChanged;
    }

    private void OnDisable()
    {
        Defragger.OnStateChanged -= Defragger_OnStateChanged;
    }

    private void Defragger_OnStateChanged(DefraggerState newState)
    {
        UpdateToggleAutoDefragButtonLabel();
    }

    /// <summary>
    /// Updates the label on the "Free Painting" button in the Options Menu
    /// </summary>
    public void UpdateToggleAutoDefragButtonLabel()
    {
        if (Defragger.instance.IsAutoDefragEnabled)
        {
            _autoDefragLabelText.text = "AUTODEFRAG ENABLED";
        }
        else
        {
            _autoDefragLabelText.text = "AUTODEFRAG DISABLED";
        }
    }
}
