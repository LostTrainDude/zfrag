using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoveTextOnClick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public TextMeshProUGUI Text;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Ue");
        Text.margin = new Vector4(Text.margin.x + 16, Text.margin.y, Text.margin.z, Text.margin.w);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Text.margin = new Vector4(Text.margin.x - 16, Text.margin.y, Text.margin.z, Text.margin.w);
    }
}
