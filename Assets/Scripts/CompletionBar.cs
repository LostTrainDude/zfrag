using System.Text;
using System.Text.RegularExpressions;
using TMPro;

public class CompletionBar : GenericSingletonClass<CompletionBar>
{
    /// <summary>
    /// The actual ProgressBar: a string made of 30 characters
    /// </summary>
    private TextMeshProUGUI _progressBarText;

    /// <summary>
    /// The initial state of the ProgressBar: a string that contains 30 "unused" blocks
    /// </summary>
    private string _emptyBar;

    private void OnEnable()
    {
        Defragger.OnGridScanned += Defragger_OnGridScanned;
    }

    private void OnDisable()
    {
        Defragger.OnGridScanned -= Defragger_OnGridScanned;
    }

    private void Defragger_OnGridScanned(double progressBarBlocksToFill)
    {
        FillProgressBar(progressBarBlocksToFill);
    }

    void Start()
    {
        ResetBar();
    }

    /// <summary>
    /// Fills the ProgressBar up to the given amount of bars (Max: 30)
    /// </summary>
    /// <param name="cells"></param>
    public void FillProgressBar(double cells)
    {
        // The character that represents the "empty" cell to find and replace with the "full" one
        var _regex = new Regex(Regex.Escape(Constants.CHAR_UNUSED));

        // To avoid rounding errors, if the defrag is formally complete
        // then fill the ProgressBar completely
        if (Defragger.instance.SectorsDefragged == Defragger.instance.TotalSectorsToDefrag)
        {
            FillBarCompletely();
            return;
        }

        // Also make sure it stays within the bounds
        if (cells < 1) return;
        if (cells > 30) cells = 30;

        // Truncate to remove digits after the decimal point
        double truncatedCells = System.Math.Truncate(cells);

        // Find and replace the given amount of "empty" cells with the "full" ones
        _progressBarText.text = _regex.Replace(_emptyBar, "<color=#ffffff>\u2588</color>", (int)truncatedCells);
    }

    /// <summary>
    /// Completely fills the ProgressBar with "full" cells
    /// </summary>
    public void FillBarCompletely()
    {
        var _regex = new Regex(Regex.Escape(Constants.CHAR_UNUSED));
        _progressBarText.text = _regex.Replace(_emptyBar, "<color=#ffffff>\u2588</color>", 30);
    }

    /// <summary>
    /// Resets the ProgressBar to its completely empty initial state
    /// </summary>
    public void ResetBar()
    {
        if (_progressBarText == null) _progressBarText = GetComponentInChildren<TextMeshProUGUI>();
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < 30; i++)
        {
            sb.Append("\u2592");
        }

        _emptyBar = sb.ToString();
        _progressBarText.text = sb.ToString();
    }
}
