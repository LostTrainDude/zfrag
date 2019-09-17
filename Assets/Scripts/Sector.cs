using UnityEngine;
using TMPro;

public class Sector : MonoBehaviour
{
    /// <summary>
    /// The State of the Sector. 0 = UNUSED, 1 = FRAGMENTED, 2 = DEFRAGMENTED
    /// </summary>
    public int State = 0;

    /// <summary>
    /// The Glyph that will represent the Sector in the grid
    /// </summary>
    public TextMeshProUGUI Glyph;

    private void Awake()
    {
        Glyph = GetComponent<TextMeshProUGUI>();
    }
}
