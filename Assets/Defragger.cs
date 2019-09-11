using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class Constants
{
    public static int SECTOR_UNUSED = 0;
    public static int SECTOR_FRAGMENTED = 1;
    public static int SECTOR_DEFRAGMENTED = 2;

    public static string CHAR_UNUSED = "\u2592";
    public static string CHAR_USED = "\u25d8";

    public static Color32 ColorUnused = new Color32(170, 170, 170, 255);
    public static Color32 ColorFragmented = new Color32(255, 255, 255, 255);
    public static Color32 ColorDefragmented = new Color32(255, 255, 85, 255);
}

public class Defragger : MonoBehaviour
{
    private static Defragger _instance;
    public static Defragger Instance { get { return _instance; } }

    // General settings

    public int Size = 800;

    [SerializeField] GameObject _sectorPrefab;
    public GameObject SectorPrefab { get => _sectorPrefab; set => _sectorPrefab = value; }

    [SerializeField] GameObject _sectorsPanel;
    public GameObject SectorsPanel { get => _sectorsPanel; set => _sectorsPanel = value; }

    public TextMeshProUGUI FooterText;
    public List<string> RandomFooterText = new List<string>();

    // UI variables

    [SerializeField] List<GameObject> _allMenus = new List<GameObject>();

    [SerializeField] GameObject _startMenu;
    [SerializeField] GameObject _quitMenu;
    [SerializeField] TextMeshProUGUI _quitMenuText;

    [SerializeField] TextMeshProUGUI _autoDefraggingLabelText;
    public TextMeshProUGUI AutoDefraggingLabelText { get => _autoDefraggingLabelText; set => _autoDefraggingLabelText = value; }

    public bool IsDefragComplete = false;

    // Time Management variables and Settings

    private float _startTime;
    public float StartTime { get => _startTime; set => _startTime = value; }

    private float _hours;
    public float Hours { get => _hours; set => _hours = value; }

    private float _minutes;
    public float Minutes { get => _minutes; set => _minutes = value; }

    private float _seconds;
    public float Seconds { get => _seconds; set => _seconds = value; }

    public TextMeshProUGUI ElapsedTimeText;

    private float _timeLeft = 0f;
    public float TimeLeft { get => _timeLeft; set => _timeLeft = value; }

    public int AutoDefragRate = 7;

    // Defrag status variables

    //public bool IsAutoDefragging = false;
    public bool IsAutoDefragEndless = false;
    //public bool IsFreePaintingEnabled = false;

    private IState _state;
    public IState State { get => _state; set => _state = value; }

    private List<Sector> _allSectors = new List<Sector>();
    public List<Sector> AllSectors { get => _allSectors; set => _allSectors = value; }

    public int TotalSectorsToDefrag = 0;
    public int SectorsDefragged = 0;

    public int StartCheckingFromIndex;

    // ProgressBar status variables

    public double ProgressBarBlocksToFill = 0f;
    public double CompletionRate = 0f;
    public double Percentage = 0f;

    public TextMeshProUGUI CompletionText;

    // Singleton
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

    void Start()
    {
        _state = new PauseState(this, _state);

        // Stop time
        Time.timeScale = 0f;

        // Instantiate all Sectors from their Prefab
        for (int i = 0; i < Size; i++)
        {
            GameObject sectorObject = Instantiate(_sectorPrefab, _sectorsPanel.transform);
            sectorObject.name = i.ToString();
        }

        NewHDD();
    }

    /// <summary>
    /// Starts a new game
    /// </summary>
    public void StartGame()
    {
        // Turn off the GridLayoutGroup to enable drag/drop
        TurnOffGrid();

        if (_state != null) _state.Exit();
        _state = new DefaultPlayState(this, _state);
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    /// <summary>
    /// Switches to AutoDefrag Mode
    /// </summary>
    public void AutoDefragMode()
    {
        _state.Exit();
        _state = new AutoDefragState(this, _state);
    }

    /// <summary>
    /// Returns a List of all the Sectors marked as FRAGMENTED
    /// </summary>
    public List<Sector> GetFragmentedSectors()
    {
        List<Sector> list = new List<Sector>();

        foreach (Sector s in _allSectors)
        {
            if (s.State == Constants.SECTOR_FRAGMENTED)
            {
                list.Add(s);
            }
        }

        return list;
    }

    /// <summary>
    /// Updates the ProgressBar
    /// </summary>
    public void UpdateProgressBar()
    {
        CompletionBar.Instance.FillProgressBar(ProgressBarBlocksToFill);
    }

    // Switch off the GridLayoutGroup component to avoid shifting
    // when dragging and dropping blocks
    void TurnOffGrid()
    {
        _sectorsPanel.GetComponent<GridLayoutGroup>().enabled = false;
    }

    /// <summary>
    /// Returns the first Sector marked as UNUSED
    /// </summary>
    /// <returns></returns>
    Sector FirstUnusedSector()
    {
        foreach (Sector s in _sectorsPanel.GetComponentsInChildren<Sector>())
        {
            if (s.State == Constants.SECTOR_UNUSED)
            {
                return s;
            }
        }

        return null;
    }

    /// <summary>
    /// Defrags a random fragmented sector, then updates the Grid and ProgressBar
    /// </summary>
    public void DefragOne()
    {
        List<Sector> fragmentedSectors = GetFragmentedSectors();

        if (fragmentedSectors.Count > 0)
        {
            // Pick the first UNUSED sector
            Sector unusedSector = FirstUnusedSector();
            int originalUnusedSectorSiblingIndex = unusedSector.transform.GetSiblingIndex();
            Vector3 originalUnusedSectorPosition = unusedSector.transform.position;

            // Pick a random FRAGMENTED sector
            int randomIndex = UnityEngine.Random.Range(0, fragmentedSectors.Count);

            Sector sectorToDefragment = _allSectors.Find(s => s == fragmentedSectors[randomIndex]);
            int originalSectorToDefragmentSiblingIndex = sectorToDefragment.transform.GetSiblingIndex();
            Vector3 originalSectorToDefragmentPosition = sectorToDefragment.transform.position;

            // Switch them
            unusedSector.transform.position = originalSectorToDefragmentPosition;
            sectorToDefragment.transform.position = originalUnusedSectorPosition;

            unusedSector.transform.SetSiblingIndex(originalSectorToDefragmentSiblingIndex);
            sectorToDefragment.transform.SetSiblingIndex(originalUnusedSectorSiblingIndex);

            // Update the RandomFooterText
            FooterText.text = ChangeRandomFooterText();
        }

        ScanGrid();
        UpdateProgressBar();
    }

    /// <summary>
    /// Scans the Grid for 
    /// </summary>
    public void ScanGrid()
    {
        Sector[] sectorChildren = _sectorsPanel.GetComponentsInChildren<Sector>();

        for (int i = StartCheckingFromIndex; i < sectorChildren.Length; i++)
        {
            // Defrag is complete, so break out of the loop
            if (SectorsDefragged == TotalSectorsToDefrag) return;

            // Scan a Sector
            Sector sector = sectorChildren[i];

            // If Sector is UNUSED, stop searching
            if (sector.State == Constants.SECTOR_UNUSED) return;

            // If a Sector is Fragmented, Defragment it
            if (sector.State == Constants.SECTOR_FRAGMENTED)
            {
                sector.State = Constants.SECTOR_DEFRAGMENTED;
            }

            //sector.Glyph.text = CHAR_USED;
            sector.Glyph.color = new Color32(255, 255, 85, 255); // Works only if hardcoded(?)

            sector.gameObject.tag = "Untagged";

            SectorsDefragged++;

            StartCheckingFromIndex = i+1;

            ProgressBarBlocksToFill = CompletionRate * SectorsDefragged;

            Percentage = ((double)SectorsDefragged / (double)TotalSectorsToDefrag) * 100f;

            if (Percentage >= 100) CompletionText.text = string.Format("Completion                {0}%", Math.Truncate(Percentage));
            else if (Percentage >= 10) CompletionText.text = string.Format("Completion                 {0}%", Math.Truncate(Percentage));
            else CompletionText.text = string.Format("Completion                  {0}%", Math.Truncate(Percentage));
        }
    }

    public void EnterPaintingState()
    {

    }

    public void ExitPaintingState()
    {

    }

    public void NewHDD()
    {
        SectorsDefragged = 0;
        TotalSectorsToDefrag = 0;
        Percentage = 0;
        ProgressBarBlocksToFill = 0;
        CompletionRate = 0;

        _timeLeft = ((float)AutoDefragRate / 10f);

        CompletionText.text = string.Format("Completion                  0%");
        
        StartCheckingFromIndex = 0;

        _hours = 0f;
        _minutes = 0f;
        _seconds = 0f;
        _startTime = Time.time;

        CompletionBar.Instance.ResetBar();

        _allSectors.Clear();

        foreach (Sector sector in _sectorsPanel.GetComponentsInChildren<Sector>())
        {
            sector.gameObject.tag = "UIDraggable";

            int index = UnityEngine.Random.Range(0, 2);

            if (index == Constants.SECTOR_UNUSED)
            {
                sector.Glyph.text = Constants.CHAR_UNUSED;
                sector.Glyph.color = Constants.ColorUnused;
            }
            else if (index == Constants.SECTOR_FRAGMENTED)
            {
                sector.Glyph.text = Constants.CHAR_USED;
                sector.Glyph.color = Constants.ColorFragmented;
            }
            
            sector.State = index;

            _allSectors.Add(sector);
        }

        TotalSectorsToDefrag = GetFragmentedSectors().Count;

        CompletionRate = 30f / (double)TotalSectorsToDefrag;

        ScanGrid();
        UpdateProgressBar();

        FooterText.text = ChangeRandomFooterText();
    }

    public void SwitchPause()
    {
        if (_state.GetType() != typeof(PauseState))
        {
            _state = new PauseState(this, _state);
        }
        else
        {
            _state.Exit();
        }
    }

    public string ChangeRandomFooterText()
    {
        return RandomFooterText[UnityEngine.Random.Range(0, RandomFooterText.Count)];
    }

    public void AdvanceTime()
    {
        float t = Time.time - _startTime;

        _seconds = (int)(t % 60);
        _minutes = (int)((t / 60) % 60);
        _hours = (int)((t / 3600) % 24);

        ElapsedTimeText.text = string.Format("Elapsed Time: {0}:{1}:{2}", _hours.ToString("00"), _minutes.ToString("00"), _seconds.ToString("00"));
    }
  
    // Update is called once per frame
    void Update()
    {
        if (_state.GetType() != null) Debug.Log(_state.GetType().ToString());

        _state.Execute();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_startMenu.activeSelf)
            {
                return;
            }

            foreach (GameObject menu in _allMenus)
            {
                if (menu.activeSelf)
                {
                    menu.SetActive(false);
                    SwitchPause();
                    return;
                }
            }

            if (!_quitMenu.activeSelf)
            {
                if (IsDefragComplete)
                {
                    _quitMenuText.text = "Are you sure you want to quit?\n\n\nYou can always come back later on, if your mind is not at peace";
                }
                else
                {
                    _quitMenuText.text = "Are you sure you want to quit?\n\n\nYour mind has not been defragmented completely and it may not work as expected";
                }

                _quitMenu.SetActive(true);
                SwitchPause();
            }
            else
            {
                _quitMenu.SetActive(false);
                SwitchPause();
            }
        }
    }
}
