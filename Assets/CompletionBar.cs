using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompletionBar : MonoBehaviour
{
    private static CompletionBar _instance;
    public static CompletionBar Instance { get { return _instance; } }

    private TextMeshProUGUI _progressBarText;
    private string _emptyBar;

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
        _progressBarText = GetComponentInChildren<TextMeshProUGUI>();
        ResetBar();
    }

    public void FillBar(double bars)
    {
        var _regex = new Regex(Regex.Escape("\u2592"));

        if (Defragger.Instance.SectorsDefragged == Defragger.Instance.SectorsToDefrag)
        {
            FillBarCompletely();
            return;
        }

        if (bars < 1) return;
        if (bars > 30) bars = 30;

        double barsAfter = System.Math.Truncate(bars);
        Debug.LogFormat("{0} | {1}", barsAfter, (int)barsAfter);

        _progressBarText.text = _regex.Replace(_emptyBar, "<color=#ffffff>\u2588</color>", (int)barsAfter);
    }

    public void FillBarCompletely()
    {
        var _regex = new Regex(Regex.Escape("\u2592"));
        _progressBarText.text = _regex.Replace(_emptyBar, "<color=#ffffff>\u2588</color>", 30);
    }

    public void ResetBar()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < 30; i++)
        {
            sb.Append("\u2592");
        }

        _emptyBar = sb.ToString();
        _progressBarText.text = sb.ToString();
    }
}
