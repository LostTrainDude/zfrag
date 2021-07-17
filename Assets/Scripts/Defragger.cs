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
    public static int SECTOR_BAD = 2;
    public static int SECTOR_DEFRAGMENTED = 3;

    public static string CHAR_UNUSED = "\u2592";
    public static string CHAR_USED = "\u25d8";
    public static string CHAR_BAD = "B";

    public static Color32 ColorUnused = new Color32(170, 170, 170, 255);
    public static Color32 ColorFragmented = new Color32(255, 255, 255, 255);
    public static Color32 ColorDefragmented = new Color32(255, 255, 85, 255);
}

public enum DefraggerState
{
    START,                  // The main menu
    //PAUSE,                  
    DEFAULT,                // The normal drag/drop gameplay
    AUTODEFRAG,
    FREEPAINTING,
    COMPLETE
}

public class Defragger : MonoBehaviour
{
    private static Defragger _instance;
    public static Defragger instance { get => _instance; }

    ////////// General settings

    /// <summary>
    /// How many sectors will there be in the HDD
    /// </summary>
    [SerializeField] private int _size = 800;

    /// <summary>
    /// The Prefab used to Instantiate new Sectors
    /// </summary>
    [SerializeField] private GameObject _sectorPrefab;

    /// <summary>
    /// The Panel that will be parent to all instantiated Sectors
    /// </summary>
    [SerializeField] private GameObject _sectorsPanel;


    ////////// UI variables

    /// <summary>
    /// The bottom-left text in the Game Window
    /// </summary>
    public TextMeshProUGUI FooterText;
    public List<string> RandomFooterText = new List<string>();

    /// <summary>
    /// List of all the UI menus (Start, Options, Quit, Credits)
    /// </summary>
    [SerializeField] private List<GameObject> _allMenus = new List<GameObject>();

    /// <summary>
    /// The Start Menu
    /// </summary>
    [SerializeField] private GameObject _startMenu;

    /// <summary>
    /// The Defrag Complete Menu
    /// </summary>
    [SerializeField] private GameObject _defragCompleteMenu;

    /// <summary>
    /// The Quit Menu
    /// </summary>
    [SerializeField] private GameObject _quitMenu;

    /// <summary>
    /// The Text that appears in the Quit Menu
    /// </summary>
    [SerializeField] private TextMeshProUGUI _quitMenuText;

    /// <summary>
    /// The Label for the AutoDefrag button
    /// </summary>
    [SerializeField] private TextMeshProUGUI _autoDefraggingLabelText;


    ////////// Time Management variables and settings

    private float _startTime;
    private float _hours;
    private float _minutes;
    private float _seconds;
    
    /// <summary>
    /// The Label that shows the Elapsed Time in the Game Window
    /// </summary>
    public TextMeshProUGUI ElapsedTimeText;

    /// <summary>
    /// The amount of time left before a Sector gets defragged in AutoDefrag Mode
    /// </summary>
    private float _defragCountdown = 0f;


    ////////// AutoDefrag Speed settings

    /// <summary>
    /// The modifier for the AutoDefrag speed
    /// </summary>
    [SerializeField] private int _autoDefragRate = 7;

    /// <summary>
    /// The "human-readable" representation of the AutoDefrag Speed
    /// </summary>
    [SerializeField] private int _defragSpeed = 4;
    public int DefragSpeed { get => _defragSpeed; set => _defragSpeed = value; }


    ////////// Defragger State variables

    /// <summary>
    /// The current State of the Defragger
    /// </summary>
    [SerializeField] private DefraggerState _state;
    public DefraggerState State { get => _state; set => _state = value; }

    /// <summary>
    /// The previous State of the Defragger
    /// </summary>
    [SerializeField] private DefraggerState _previousState;
    public DefraggerState PreviousState { get => _previousState; set => _previousState = value; }


    ////////// Defrag status variables

    public bool IsPaused = false;
    public bool IsAutoDefragEnabled = false;
    public bool IsAutoDefragEndless = false;
    public bool IsFreePaintingEnabled = false;

    /// <summary>
    /// A List of all the Sectors instantiated
    /// </summary>
    private List<Sector> _allSectors = new List<Sector>();

    /// <summary>
    /// The amount of sectors that have to be defragged
    /// </summary>
    public int TotalSectorsToDefrag = 0;

    /// <summary>
    /// The amount of defragged sectors
    /// </summary>
    public int SectorsDefragged = 0;

    [Range(0, 100)]
    public int BadSectorPercentage = 15;

    /// <summary>
    /// Tracks the first unchecked Sector in the grid when scanning
    /// </summary>
    private int _startCheckingFromIndex;


    ////////// ProgressBar status variables

    /// <summary>
    /// Tracks how many blocks have to be filled in the ProgressBar
    /// </summary>
    public double ProgressBarBlocksToFill = 0f;

    /// <summary>
    /// Used to calculate ProgressBarBlocksToFill
    /// </summary>
    public double CompletionRate = 0f;

    /// <summary>
    /// The actual percentage of Defrag completed
    /// </summary>
    public double Percentage = 0f;

    /// <summary>
    /// The label used to show the Completion percentage
    /// </summary>
    public TextMeshProUGUI CompletionText;


    ////////// Events

    // Invoked when the Defraggler changes its state
    public delegate void Delegate_OnStateChanged(DefraggerState newState);
    public static event Delegate_OnStateChanged OnStateChanged;

    // Invoked when the Defraggler has finished scanning the grid
    public delegate void Delegate_OnGridScanned(double progressBarBlocksToFill);
    public static event Delegate_OnGridScanned OnGridScanned;

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

    private void OnEnable()
    {
        MouseDrag.OnSectorDraggingStarted += MouseDrag_OnSectorDraggingStarted;
        MouseDrag.OnSectorDropped += MouseDrag_OnSectorDropped;
    }

    private void OnDisable()
    {
        MouseDrag.OnSectorDraggingStarted -= MouseDrag_OnSectorDraggingStarted;
        MouseDrag.OnSectorDropped -= MouseDrag_OnSectorDropped;
    }

    private void MouseDrag_OnSectorDraggingStarted()
    {
        // Change ther Random text in the Game Window's Footer
        FooterText.text = ChangeRandomFooterText();
    }

    private void MouseDrag_OnSectorDropped()
    {
        // Randomly update the text in the footer
        FooterText.text = ChangeRandomFooterText();

        if (State != DefraggerState.FREEPAINTING)
        {
            ScanGrid();
        }
    }

    /// <summary>
    /// Called by the the button in the Start Menu, it starts the game
    /// </summary>
    public void StartGame()
    {
        // Let time advance
        Time.timeScale = 1;

        // Switch off the GridLayoutGroup component to avoid shifting when drag/drop is enabled
        _sectorsPanel.GetComponent<GridLayoutGroup>().enabled = false;

        // Switch to DEFAULT State
        SetState(DefraggerState.DEFAULT);
    }

    /// <summary>
    /// Resets the Sectors Grid with a new HDD
    /// </summary>
    public void ResetDefrag()
    {
        NewHDD();

        if (_state == DefraggerState.COMPLETE)
        {
            if (_previousState == DefraggerState.AUTODEFRAG)// || _previousState == DefraggerState.FREEPAINTING)
            {
                if (IsAutoDefragEnabled) SetState(_previousState);
                else SetState(DefraggerState.DEFAULT);
            }
            else if (_previousState == DefraggerState.FREEPAINTING)
            {
                if (IsFreePaintingEnabled) SetState(_previousState);
                else SetState(DefraggerState.DEFAULT);
            }
            else
            {
                if (IsAutoDefragEnabled) SetState(DefraggerState.AUTODEFRAG);
                else SetState(DefraggerState.DEFAULT);
            }
        }

        ScanGrid(); // TODO: Redundant?
    }

    /// <summary>
    /// Scans the Grid for defragged Sectors
    /// </summary>
    public void ScanGrid()
    {
        Sector[] sectorChildren = _sectorsPanel.GetComponentsInChildren<Sector>();

        for (int i = _startCheckingFromIndex; i < sectorChildren.Length; i++)
        {
            // Defrag is complete, so break out of the loop
            if (SectorsDefragged == TotalSectorsToDefrag)
            {
                //SwitchToComplete();
                return;
            }
            // Scan a Sector
            Sector sector = sectorChildren[i];

            // If Sector is UNUSED, stop searching
            if (sector.State == Constants.SECTOR_UNUSED) return;

            // If Sector is BURNT, skip it
            if (sector.State == Constants.SECTOR_BAD) continue;

            // If a Sector is Fragmented, Defragment it
            if (sector.State == Constants.SECTOR_FRAGMENTED) sector.State = Constants.SECTOR_DEFRAGMENTED;

            // Change the Glyph color to the Defragmented white
            sector.Glyph.color = Constants.ColorDefragmented;

            // Make it undraggable by the mouse
            sector.gameObject.tag = "Untagged";

            // Increase the number of Sectors defragged
            SectorsDefragged++;

            // Update the index from which the next Scan will start
            _startCheckingFromIndex = i + 1;

            // Update the amount of blocks to fill in the ProgressBar
            ProgressBarBlocksToFill = CompletionRate * SectorsDefragged;

            // Update the percentage of Defrag completion
            Percentage = ((double)SectorsDefragged / (double)TotalSectorsToDefrag) * 100f;

            // Update the label accordingly
            if (Percentage >= 100) CompletionText.text = $"Completion                {Math.Truncate(Percentage)}%";
            else if (Percentage >= 10) CompletionText.text = $"Completion                 {Math.Truncate(Percentage)}%";
            else CompletionText.text = $"Completion                  {Math.Truncate(Percentage)}%";

            OnGridScanned?.Invoke(ProgressBarBlocksToFill);

            if (IsDefragComplete())
            {
                if (IsAutoDefragEndless)
                {
                    ResetDefrag();
                    return;
                }
                SwitchToComplete();
                return;
            }
        }
    }

    /// <summary>
    /// Sets up the game to enter Free Painting Mode
    /// </summary>
    public void SetupFreePainting()
    {
        // Get all the Sectors on the screen
        Sector[] sectorChildren = _sectorsPanel.GetComponentsInChildren<Sector>();

        foreach (Sector sector in sectorChildren)
        {
            // Make them all FRAGMENTED
            if (sector.State == Constants.SECTOR_DEFRAGMENTED)
            {
                sector.State = Constants.SECTOR_FRAGMENTED;
                sector.Glyph.color = Constants.ColorFragmented;
            }

            // Make sure they can be dragged/dropped
            sector.gameObject.tag = "UIDraggable";
        }

        _startCheckingFromIndex = 0;
        SectorsDefragged = 0;
        ProgressBarBlocksToFill = 0;
        Percentage = 0;

        ResetProgressBar();
        CompletionText.text = $"Completion                  0%";

        SetState(DefraggerState.FREEPAINTING);
    }


    /// <summary>
    /// Switches between AUTODEFRAG and other States
    /// </summary>
    public void ToggleAutoDefrag()
    {
        IsAutoDefragEnabled = !IsAutoDefragEnabled;

        if (_state == DefraggerState.COMPLETE && IsDefragComplete())
        {
            return;
        }

        if (_state == DefraggerState.AUTODEFRAG)
        {
            if (IsDefragComplete())
            {
                if (IsAutoDefragEndless)
                {
                    ResetDefrag();
                    return;
                }

                SwitchToComplete();
            }
            else SetState(DefraggerState.DEFAULT);
        }
        else if (_state == DefraggerState.FREEPAINTING)
        {
            ToggleFreePainting();
            SetState(DefraggerState.AUTODEFRAG);
        }
        else
        {
            SetState(DefraggerState.AUTODEFRAG);
        }
    }


    /// <summary>
    /// Switches to Complete State
    /// </summary>
    public void SwitchToComplete()
    {
        _allSectors.ForEach(x => x.gameObject.tag = "Untagged");

        FooterText.text = "Finished condensing";
        
        SetState(DefraggerState.COMPLETE);
        
        _defragCompleteMenu.SetActive(true);
    }

    /// <summary>
    /// Switches the Endless Defrag mode on and off
    /// </summary>
    public void ToggleEndlessDefrag() => IsAutoDefragEndless = !IsAutoDefragEndless;

    /// <summary>
    /// Switches the Free Painting Mode on and off
    /// </summary>
    public void ToggleFreePainting()
    {
        IsFreePaintingEnabled = !IsFreePaintingEnabled;

        if (IsFreePaintingEnabled)
        {
            IsAutoDefragEnabled = false;
            SetupFreePainting();
            return;
        }
        else
        {
            SetState(_previousState);
        }

        ScanGrid();
    }

    public void SetState(DefraggerState _newState)
    {
        if (_newState == _state) return;

        _previousState = _state;
        _state = _newState;

        Call_OnStateChange(_state);
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
        if (_autoDefragRate == 1) return;

        _autoDefragRate -= 1;
        _defragSpeed++;
    }

    /// <summary>
    /// The higher it is, the slower it goes (max 10)
    /// </summary>
    public void IncreaseAutoDefragRate()
    {
        if (_autoDefragRate == 10) return;

        _autoDefragRate += 1;
        _defragSpeed--;
    }

    /// <summary>
    /// The higher it is, the more bad sectors will appear
    /// </summary>
    public void IncreaseBadSectorPercentage()
    {
        if (BadSectorPercentage == 100) return;

        BadSectorPercentage += 1;
    }

    /// <summary>
    /// The lower it is, the less bad sectors will appear
    /// </summary>
    public void DecreaseBadSectorPercentage()
    {
        if (BadSectorPercentage == 0) return;

        BadSectorPercentage -= 1;
    }

    public void ResetProgressBar()
    {
        CompletionBar.instance.ResetBar();
    }

    /// <summary>
    /// Returns the first Sector marked as UNUSED
    /// </summary>
    Sector FirstUnusedSector()
    {
        foreach (Sector s in _sectorsPanel.GetComponentsInChildren<Sector>())
        {
            if (s.State == Constants.SECTOR_UNUSED) return s;
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

            // Switch them in the grid
            unusedSector.transform.position = originalSectorToDefragmentPosition;
            sectorToDefragment.transform.position = originalUnusedSectorPosition;

            // Switch them in the Editor's hierarchy
            unusedSector.transform.SetSiblingIndex(originalSectorToDefragmentSiblingIndex);
            sectorToDefragment.transform.SetSiblingIndex(originalUnusedSectorSiblingIndex);

            // Update the RandomFooterText
            FooterText.text = ChangeRandomFooterText();
        }

        ScanGrid();
    }


    /// <summary>
    /// Create a "brand new HDD" to defragment. Resets every game variable to 0
    /// </summary>
    public void NewHDD()
    {
        SectorsDefragged = 0;
        TotalSectorsToDefrag = 0;
        Percentage = 0;
        ProgressBarBlocksToFill = 0;
        CompletionRate = 0;

        _defragCountdown = (float)_autoDefragRate / 10f;

        CompletionText.text = $"Completion                  0%";

        _startCheckingFromIndex = 0;

        _hours = 0f;
        _minutes = 0f;
        _seconds = 0f;
        _startTime = Time.time;

        ResetProgressBar();

        // Clears the Sectors List
        _allSectors.Clear();

        // Then fills it back again with "brand new" sectors
        foreach (Sector sector in _sectorsPanel.GetComponentsInChildren<Sector>())
        {
            sector.gameObject.tag = "UIDraggable";

            int index = 0;

            if ((UnityEngine.Random.Range(0, 100) <= BadSectorPercentage)) index = Constants.SECTOR_BAD;
            else index = UnityEngine.Random.Range(0, 2);

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
            else if (index == Constants.SECTOR_BAD)
            {
                sector.Glyph.text = Constants.CHAR_BAD;
                sector.Glyph.color = Constants.ColorFragmented;

                // Make it undraggable by the mouse
                sector.gameObject.tag = "Untagged";
            }

            sector.State = index;

            _allSectors.Add(sector);
        }

        TotalSectorsToDefrag = GetFragmentedSectors().Count;

        CompletionRate = 30f / (double)TotalSectorsToDefrag;

        FooterText.text = ChangeRandomFooterText();
    }

    /// <summary>
    /// Switches between PAUSE and other states
    /// </summary>
    public void TogglePause()
    {
        IsPaused = !IsPaused;

        if (IsPaused) Time.timeScale = 0;
        else Time.timeScale = 1;

        if (!IsPaused)
        {
            if (_state != DefraggerState.COMPLETE && IsDefragComplete())
            {
                SwitchToComplete();
            }
        }
    }

    public void Call_OnStateChange(DefraggerState _newState)
    {
        switch (_newState)
        {
            case DefraggerState.DEFAULT:
                ScanGrid();
                break;

            default:
                break;
        }

        OnStateChanged?.Invoke(_newState);
    }

    public string ChangeRandomFooterText() => RandomFooterText[UnityEngine.Random.Range(0, RandomFooterText.Count)];

    /// <summary>
    /// Makes time advance and updates the Elapsed Time label
    /// </summary>
    public void AdvanceTime()
    {
        if (IsPaused) return;

        float t = Time.time - _startTime;

        _seconds = (int)(t % 60);
        _minutes = (int)((t / 60) % 60);
        _hours = (int)((t / 3600) % 24);

        ElapsedTimeText.text = $"Elapsed Time: {_hours:00}:{_minutes:00}:{_seconds:00}";
    }

    /// <summary>
    /// Defrags a sector according to the decided AutoDefragRate
    /// </summary>
    public void AutoDefrag()
    {
        _defragCountdown -= Time.deltaTime;

        if (_defragCountdown <= 0)
        {
            DefragOne();
            _defragCountdown = (float)_autoDefragRate / 10f;
        }
    }

    /// <summary>
    /// Returns TRUE if there are no sectors left to defrag
    /// </summary>
    public bool IsDefragComplete() => TotalSectorsToDefrag == SectorsDefragged;

    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitGame() => Application.Quit();

    void Start()
    {
        // Set initial State to START
        _state = DefraggerState.START;

        // Stop time
        Time.timeScale = 0f;

        // Instantiate all Sectors from the Prefab
        for (int i = 0; i < _size; i++)
        {
            GameObject sectorObject = Instantiate(_sectorPrefab, _sectorsPanel.transform);
            sectorObject.name = i.ToString();
        }

        // Generate a new HDD to defrag
        NewHDD();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_state)
        {
            case DefraggerState.DEFAULT:
                AdvanceTime();
                break;

            case DefraggerState.AUTODEFRAG:
                AdvanceTime();
                AutoDefrag();
                break;

            case DefraggerState.FREEPAINTING:
                AdvanceTime();
                break;

            default:
                break;
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If the Start Menu is visible, do nothing
            if (_startMenu.activeSelf) return;

            // If the Defrag Complete Menu is visible, close it
            if (_defragCompleteMenu.activeSelf)
            {
                _defragCompleteMenu.SetActive(false);
                return;
            }

            // If any other menu is active, close it and toggle pause
            foreach (GameObject menu in _allMenus)
            {
                if (menu.activeSelf)
                {
                    menu.SetActive(false);
                    TogglePause();

                    // Prevent it from reaching the following code
                    return;
                }
            }

            // Open up the Quit Menu if no other menu is viisible
            // Close it, instead, if it's already open
            if (!_quitMenu.activeSelf)
            {
                TogglePause();

                if (_state == DefraggerState.COMPLETE)
                {
                    _quitMenuText.text = "Are you sure you want to quit?\n\n\nYou can always come back later on, if your mind is not at peace";
                }
                else
                {
                    _quitMenuText.text = "Are you sure you want to quit?\n\n\nYour mind has not been defragmented completely and it may not work as expected";
                }

                _quitMenu.SetActive(true);
            }
            else
            {
                _quitMenu.SetActive(false);
                TogglePause();
            }
        }
    }
}
