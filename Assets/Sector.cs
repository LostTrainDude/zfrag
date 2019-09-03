using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Sector : MonoBehaviour
{
    public int Index = 0;
    public int State = 0;
    public TextMeshProUGUI Glyph;

    private void Awake()
    {
        Glyph = GetComponent<TextMeshProUGUI>();
    }
}
