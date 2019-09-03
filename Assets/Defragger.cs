using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Defragger : MonoBehaviour
{
    // Constants
    const int SECTOR_UNUSED = 0;
    const int SECTOR_FRAGMENTED = 1;
    const int SECTOR_DEFRAGMENTED = 2;

    const string CHAR_UNUSED = "\u2592";
    const string CHAR_USED = "\u25d8";

    // Colors
    public Color32 ColorUnused = new Color32(170, 170, 170, 255);
    public Color32 ColorFragmented = new Color32(255, 255, 255, 255);
    public Color32 ColorComplete = new Color32(255, 255, 85, 255);

    private static Defragger _instance;
    public static Defragger Instance { get { return _instance; } }

    [Header("Sectors Window")]
    public const int Size = 800;
    public int MaxToDefrag = 0;
    [SerializeField] GameObject _sectorPrefab;
    [SerializeField] GameObject _sectorsPanel;

    public bool IsPaused = true;
    public bool IsAutoDefragging = false;
    public bool IsAutoDefragEndless = false;
    public bool IsFreePaintingEnabled = false;

    public int AutoDefragRate = 1;

    [SerializeField] TextMeshProUGUI _autoDefraggingLabelText;
    public bool IsDefragComplete = false;

    [Header("Clock Variables")]
    private float _startTime;
    private float _hours;
    private float _minutes;
    private float _seconds;
    public TextMeshProUGUI ElapsedTimeText;

    public int TotalSectorsToDefrag = 0;
    public int SectorsDefragged = 0;
    public double CompletionChunksToFill = 0f;
    public double CompletionRate = 0f;
    public double Percentage = 0f;

    public TextMeshProUGUI CompletionText;

    private List<Sector> _allSectors = new List<Sector>();

    public int StartCheckingFromIndex;

    public TextMeshProUGUI FooterText;
    public List<string> RandomFooterText = new List<string>();

    [SerializeField] List<GameObject> _allMenus = new List<GameObject>();

    [SerializeField] GameObject _startMenu;
    [SerializeField] GameObject _quitMenu;
    [SerializeField] TextMeshProUGUI _quitMenuText;

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

    public void StartGame()
    {
        SwitchPause();
        _startTime = Time.time;

        FooterText.text = ChangeRandomFooterText();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void SwitchAutoDefragging()
    {
        // ON -> OFF
        if (IsAutoDefragging)
        {
            // Stop the HDD seeking sounds
            AudioController.instance.EndLooping();

            _autoDefraggingLabelText.text = "AUTODEFRAG DISABLED";

            IsAutoDefragging = false;
        }
        else // OFF -> ON
        {
            // Don't FreePaint when AutoDefragging
            if (IsFreePaintingEnabled) ExitPaintingState();

            // Start the HDD seeking sounds
            AudioController.instance.StartLooping();

            _autoDefraggingLabelText.text = "AUTODEFRAG ENABLED";

            IsAutoDefragging = true;
        }
    }

    float timeLeft = 0f;

    void Start()
    {
        timeLeft = ((float)AutoDefragRate / 10f);
        // Stop time
        Time.timeScale = 0f;

        for (int i = 0; i < Size; i++)
        {
            // Create a new Sector GameObject from Prefab
            GameObject sectorObject = Instantiate(_sectorPrefab, _sectorsPanel.transform);
            sectorObject.name = i.ToString();

            // Add the Sector component to the List
            Sector sector = sectorObject.GetComponent<Sector>();

            sector.Index = i;

            // Randomly choose its State (0-1)
            int state = UnityEngine.Random.Range(0, 2);
            sector.State = state;

            if (state == SECTOR_UNUSED)
            {
                sector.Glyph.text = CHAR_UNUSED;
                sector.Glyph.color = ColorUnused;
            }
            else if (state == SECTOR_FRAGMENTED)
            {
                sector.Glyph.text = CHAR_USED;
                sector.Glyph.color = ColorFragmented;
            }

            _allSectors.Add(sectorObject.GetComponent<Sector>());
        }

        TotalSectorsToDefrag = GetSectorsToDefrag().Count();

        CompletionRate = 30f / (double)TotalSectorsToDefrag;

        Invoke("TurnOffGrid", 0.1f);
        Invoke("CheckGrid", 0.2f);
        Invoke("RefreshFillBar", 0.3f);
    }

    public List<Sector> GetSectorsToDefrag()
    {
        List<Sector> list = new List<Sector>();

        foreach (Sector s in _allSectors)
        {
            if (s.State == SECTOR_FRAGMENTED)
            {
                list.Add(s);
            }
        }

        return list;
    }

    public List<Sector> GetFragmentedSectors()
    {
        List<Sector> list = new List<Sector>();

        foreach (Sector sector in _allSectors)
        {
            if (sector.State == SECTOR_FRAGMENTED)
            {
                list.Add(sector);
            }
        }

        return list;
    }

    public void RefreshFillBar()
    {
        CompletionBar.Instance.FillBar(CompletionChunksToFill);
    }

    // Switch off the GridLayoutGroup component to avoid shifting
    // when dragging and dropping blocks
    void TurnOffGrid()
    {
        _sectorsPanel.GetComponent<GridLayoutGroup>().enabled = false;
    }

    Sector FirstUnusedSector()
    {
        foreach (Sector s in _sectorsPanel.GetComponentsInChildren<Sector>())
        {
            if (s.State == SECTOR_UNUSED)
            {
                return s;
            }
        }

        return null;
    }

    public void DefragOne()
    {
        if (IsPaused) return;
        if (IsDefragComplete) return;
        if (IsFreePaintingEnabled) return;

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

            FooterText.text = ChangeRandomFooterText();
        }

        ScanGrid();
        RefreshFillBar();
    }

    // Check how many blocks in a row are defragmented
    // starting from the top
    public void ScanGrid()
    {
        if (IsFreePaintingEnabled) return;

        Sector[] sectorChildren = _sectorsPanel.GetComponentsInChildren<Sector>();

        for (int i = StartCheckingFromIndex; i < sectorChildren.Length; i++)
        {
            // Defrag is complete, so break out of the loop
            if (SectorsDefragged == TotalSectorsToDefrag) break;

            // Scan a Sector
            Sector sector = sectorChildren[i];

            // If Sector is UNUSED, stop searching
            if (sector.State == SECTOR_UNUSED) return;

            // If a Sector is Fragmented, Defragment it
            if (sector.State == SECTOR_FRAGMENTED)
            {
                sector.State = SECTOR_DEFRAGMENTED;
            }

            //sector.Glyph.text = CHAR_USED;
            sector.Glyph.color = new Color32(255, 255, 85, 255); // Works only if hardcoded(?)

            sector.gameObject.tag = "Untagged";

            SectorsDefragged++;

            StartCheckingFromIndex = i+1;

            CompletionChunksToFill = CompletionRate * SectorsDefragged;

            Percentage = ((double)SectorsDefragged / (double)TotalSectorsToDefrag) * 100f;

            if (Percentage >= 100) CompletionText.text = string.Format("Completion                {0}%", System.Math.Truncate(Percentage));
            else if (Percentage >= 10) CompletionText.text = string.Format("Completion                 {0}%", System.Math.Truncate(Percentage));
            else CompletionText.text = string.Format("Completion                  {0}%", System.Math.Truncate(Percentage));
        }

        IsDefragComplete = true;

        foreach (Sector sector in _allSectors)
        {
            sector.gameObject.tag = "Untagged";
        }

        if (!IsAutoDefragging) AudioController.instance.EndLooping();
        else
        {
            if (IsAutoDefragEndless) Restart();
            else AudioController.instance.EndLooping();
        }

        FooterText.text = "Finished condensing";
    }

    public void EnterPaintingState()
    {
        IsFreePaintingEnabled = true;

        // Don't AUTODEFRAG when FreePainting
        if (IsAutoDefragging) SwitchAutoDefragging();

        Sector[] sectorChildren = _sectorsPanel.GetComponentsInChildren<Sector>();

        foreach (Sector sector in sectorChildren)
        {
            if (sector.State == SECTOR_DEFRAGMENTED)
            {
                sector.State = SECTOR_FRAGMENTED;
                sector.Glyph.color = ColorFragmented;
            }

            sector.gameObject.tag = "UIDraggable";
        }

        if (IsDefragComplete) IsDefragComplete = false;

        StartCheckingFromIndex = 0;
        SectorsDefragged = 0;
        CompletionChunksToFill = 0;
        Percentage = 0;
        CompletionBar.Instance.ResetBar();
        CompletionText.text = string.Format("Completion                  0%");
    }

    public void ExitPaintingState()
    {
        IsFreePaintingEnabled = false;
        TotalSectorsToDefrag = GetSectorsToDefrag().Count();

        ScanGrid();
        CompletionBar.Instance.FillBar(CompletionChunksToFill);
    }

    public void Restart()
    {
        if (IsAutoDefragging) AudioController.instance.StartLooping();

        bool isRandomized = false;
        int leftToAdd = MaxToDefrag;

        if (leftToAdd == 0)
        {
            isRandomized = true;
        }
        
        IsDefragComplete = false;

        SectorsDefragged = 0;
        TotalSectorsToDefrag = 0;
        Percentage = 0;
        CompletionChunksToFill = 0;
        CompletionRate = 0;

        timeLeft = ((float)AutoDefragRate / 10f);

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

            int index = 0;
            if (!isRandomized)
            {
                if (leftToAdd > 0)
                {
                    index = UnityEngine.Random.Range(0, 2);
                    leftToAdd--;
                }
                else
                {
                    index = 0;
                }
            }
            else
            {
                index = UnityEngine.Random.Range(0, 2);
            }

            if (index == SECTOR_UNUSED)
            {
                sector.Glyph.text = CHAR_UNUSED;
                sector.Glyph.color = ColorUnused;
            }
            else if (index == SECTOR_FRAGMENTED)
            {
                sector.Glyph.text = CHAR_USED;
                sector.Glyph.color = ColorFragmented;
            }
            
            sector.State = index;

            _allSectors.Add(sector);
        }

        foreach (Sector sector in _allSectors)
        {
            if (sector.State == SECTOR_FRAGMENTED)
            {
                TotalSectorsToDefrag++;
            }
        }

        CompletionRate = 30f / (double)TotalSectorsToDefrag;

        ScanGrid();
        RefreshFillBar();

        FooterText.text = ChangeRandomFooterText();

        IsPaused = false;
    }

    public static void Swap<T>(IList<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }

    public void SwitchPause()
    {
        if (Time.timeScale == 0) Time.timeScale = 1;
        else Time.timeScale = 0;

        IsPaused = !IsPaused;
    }

    public string ChangeRandomFooterText()
    {
        return RandomFooterText[UnityEngine.Random.Range(0, RandomFooterText.Count)];
    }
  
    // Update is called once per frame
    void Update()
    {
        if (!IsPaused && !IsDefragComplete)
        {
            float t = Time.time - _startTime;

            _seconds = (int)(t % 60);
            _minutes = (int)((t / 60) % 60);
            _hours = (int)((t / 3600) % 24);

            ElapsedTimeText.text = string.Format("Elapsed Time: {0}:{1}:{2}", _hours.ToString("00"), _minutes.ToString("00"), _seconds.ToString("00"));
        }

        if (IsAutoDefragging && !IsPaused)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                DefragOne();
                timeLeft = ((float)AutoDefragRate / 10f);
            }
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
