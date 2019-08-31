using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompletionBar : MonoBehaviour
{
    private static CompletionBar _instance;
    public static CompletionBar Instance { get { return _instance; } }

    public Image[] ProgressBarChunks;
    public Sprite CompletedSprite;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        ProgressBarChunks = GetComponentsInChildren<Image>();
    }

    public void FillBar(int last)
    {
        if (last >= ProgressBarChunks.Length) last = ProgressBarChunks.Length;

        for (int i = 0; i < last; i++)
        {
            ProgressBarChunks[i].sprite = CompletedSprite;
        }
    }
}
