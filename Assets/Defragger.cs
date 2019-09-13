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

public enum DefraggerState
{
    START,
    PAUSE,
    DEFAULT,
    AUTODEFRAG,
    FREEPAINTING,
    COMPLETE
}

public class Defragger : MonoBehaviour
{
    private static Defragger _instance;
    public static Defragger instance { get => _instance; }

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

    
    // Time Management variables and settings

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


    // AutoDefrag Speed settings

    [SerializeField] private int _autoDefragRate = 7;
    public int AutoDefragRate { get => _autoDefragRate; set => _autoDefragRate = value; }

    [SerializeField] private int _defragSpeed = 4;
    public int DefragSpeed { get => _defragSpeed; set => _defragSpeed = value; }

    
    // Defragger State variables

    [SerializeField] private DefraggerState _state;
    public DefraggerState State { get => _state; set => _state = value; }

    [SerializeField] private DefraggerState _previousState;
    public DefraggerState PreviousState { get => _previousState; set => _previousState = value; }

    
    // Defrag status variables

    public bool IsAutoDefragEndless = false;
    public bool IsFreePaintingEnabled = false;

    private List<Sector> _allSectors = new List<Sector>();
    public List<Sector> AllSectors { get => _allSectors; set => _allSectors = value; }

    public int TotalSectorsToDefrag = 0;
    public int SectorsDefragged = 0;

    public int StartCheckingFromIndex;

    
    // ProgressBar status variables

    //
    public double ProgressBarBlocksToFill = 0f;

    public double CompletionRate = 0f;

    public double Percentage = 0f;

    public TextMeshProUGUI CompletionText;
        
    
    // Singleton. Awake is called before Start()
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
        // Set initial State to START
        _state = DefraggerState.START;

        // Stop time
        Time.timeScale = 0f;

        // Instantiate all Sectors from a Prefab
        for (int i = 0; i < Size; i++)
        {
            GameObject sectorObject = Instantiate(_sectorPrefab, _sectorsPanel.transform);
            sectorObject.name = i.ToString();
        }

        // Generate a new HDD to defrag
        NewHDD();
    }

    /// <summary>
    /// Called by the the button in the Start Menu, it starts the game
    /// </summary>
    public void StartGame()
    {
        _previousState = DefraggerState.START;
        Time.timeScale = 1;

        // Switch off the GridLayoutGroup component to avoid shifting when drag/drop is enabled
        _sectorsPanel.GetComponent<GridLayoutGroup>().enabled = false;

        // Switch to DEFAULT State
        SwitchToDefaultState();
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }

    public void ToggleAutoDefrag()
    {
        _previousState = _state;

        if (_state == DefraggerState.AUTODEFRAG)
        {
            SwitchToDefaultState();
        }
        else if (_state == DefraggerState.DEFAULT)
        {
            SwitchToAutoDefrag();
        }
        else if (_state == DefraggerState.FREEPAINTING)
        {
            ToggleFreePainting();
            SwitchToAutoDefrag();
        }
    }

    /// <summary>
    /// Switches to AutoDefrag Mode
    /// </summary>
    public void SwitchToAutoDefrag()
    {
        // Change State
        _state = DefraggerState.AUTODEFRAG;

        if (IsAutoDefragEndless)
        {
            ToggleEndlessDefrag();
        }
    }

    public void ToggleEndlessDefrag()
    {
        IsAutoDefragEndless = !IsAutoDefragEndless;
    }

    public void SwitchToDefaultState()
    {
        // Change State
        _state = DefraggerState.DEFAULT;

        if (IsAutoDefragEndless)
        {
            IsAutoDefragEndless = false;
        }

        ScanGrid();
        UpdateProgressBar();
    }

    public void ToggleFreePainting()
    {
        IsFreePaintingEnabled = !IsFreePaintingEnabled;

        if (IsFreePaintingEnabled)
        {
            SetupFreePainting();
        }
        else
        {
            ScanGrid();
            UpdateProgressBar();
        }
    }

    /// <summary>
    /// Switches to Free Painting Mode
    /// </summary>
    public void SetupFreePainting()
    {
        Sector[] sectorChildren = SectorsPanel.GetComponentsInChildren<Sector>();

        foreach (Sector sector in sectorChildren)
        {
            if (sector.State == Constants.SECTOR_DEFRAGMENTED)
            {
                sector.State = Constants.SECTOR_FRAGMENTED;
                sector.Glyph.color = Constants.ColorFragmented;
            }

            sector.gameObject.tag = "UIDraggable";
        }

        if (IsDefragComplete)
        {
            IsDefragComplete = false;
        }

        StartCheckingFromIndex = 0;
        SectorsDefragged = 0;
        ProgressBarBlocksToFill = 0;
        Percentage = 0;

        ResetProgressBar();
        CompletionText.text = string.Format("Completion                  0%");
    }

    public void SwitchToComplete()
    {
        foreach (Sector sector in AllSectors)
        {
            sector.gameObject.tag = "Untagged";
        }

        FooterText.text = "Finished condensing";
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
    /// The lower it is, the faster it goes (min 1)
    /// </summary>
    public void DecreaseAutoDefragRate()
    {
        if (AutoDefragRate == 1)
        {
            return;
        }

        AutoDefragRate -= 1;
        _defragSpeed++;
    }

    /// <summary>
    /// The higher it is, the slower it goes (max 10)
    /// </summary>
    public void IncreaseAutoDefragRate()
    {
        if (AutoDefragRate == 10)
        {
            return;
        }

        AutoDefragRate += 1;
        _defragSpeed--;
    }

    /// <summary>
    /// Updates the ProgressBar
    /// </summary>
    public void UpdateProgressBar()
    {
        CompletionBar.instance.FillProgressBar(ProgressBarBlocksToFill);
    }

    public void ResetProgressBar()
    {
        CompletionBar.instance.ResetBar();
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

    public void NewHDD()
    {
        // Reset everyting to 0
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

        CompletionBar.instance.ResetBar();

        // Clear all the sectors
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
    
    public void SwitchToFreePainting()
    {
        _state = DefraggerState.FREEPAINTING;
    }

    public void TogglePause()
    {
        if (_state == DefraggerState.PAUSE)
        {
            Time.timeScale = 1;

            if (IsFreePaintingEnabled && _state != DefraggerState.FREEPAINTING)
            {
                SwitchToFreePainting();
            }
            else
            {
                _state = _previousState;
            }
        }
        else
        {
            _previousState = _state;
            Time.timeScale = 0;
            _state = DefraggerState.PAUSE;
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

    /// <summary>
    /// Defrags a sector according to the decided AutoDefragRate
    /// </summary>
    public void AutoDefrag()
    {
        TimeLeft -= Time.deltaTime;

        if (TimeLeft <= 0)
        {
            DefragOne();
            TimeLeft = ((float)AutoDefragRate / 10f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Time.timeScale);

        switch (_state)
        {
            case DefraggerState.PAUSE:
                break;

            case DefraggerState.DEFAULT:
                AdvanceTime();
                break;

            case DefraggerState.AUTODEFRAG:
                AdvanceTime();
                AutoDefrag();

                if (TotalSectorsToDefrag == SectorsDefragged)
                {
                    _previousState = _state;
                    _state = DefraggerState.COMPLETE;
                }
                break;

            case DefraggerState.FREEPAINTING:
                AdvanceTime();
                break;

            case DefraggerState.COMPLETE:
                SwitchToComplete();

                if (_previousState == DefraggerState.AUTODEFRAG)
                {
                    if (IsAutoDefragEndless)
                    {
                        NewHDD();
                    }
                    else
                    {
                        AudioController.instance.EndLooping();
                    }

                    SwitchToAutoDefrag();
                }
                else
                {
                    AudioController.instance.EndLooping();
                }
                break;
        }


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
                    TogglePause();
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
                TogglePause();
            }
            else
            {
                _quitMenu.SetActive(false);
                TogglePause();
            }
        }
    }
}
