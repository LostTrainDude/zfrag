using UnityEngine;
using TMPro;

public class UIGameWindow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _autoDefraggingLabelText;

    public void UpdateToggleAutoDefragButtonLabel()
    {
        if (Defragger.instance.State == DefraggerState.AUTODEFRAG && !Defragger.instance.IsFreePaintingEnabled)
        {
            _autoDefraggingLabelText.text = "AUTODEFRAG ENABLED";
        }
        else
        {
            _autoDefraggingLabelText.text = "AUTODEFRAG DISABLED";
        }
    }
}
