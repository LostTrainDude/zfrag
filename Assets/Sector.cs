using UnityEngine;
using TMPro;

public class Sector : MonoBehaviour
{
    public int State = 0;
    public TextMeshProUGUI Glyph;

    private void Awake()
    {
        Glyph = GetComponent<TextMeshProUGUI>();
    }
}
