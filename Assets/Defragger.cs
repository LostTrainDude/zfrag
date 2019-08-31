using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Defragger : MonoBehaviour
{
    private static Defragger _instance;
    public static Defragger Instance { get { return _instance; } }

    [Header("Sectors Window")]
    [SerializeField] const int _size = 800;
    [SerializeField] int _maxToDefrag = 10;
    [SerializeField] GameObject _sectorPrefab;
    [SerializeField] GameObject _sectorsPanel;

    public bool isPaused = true;
    public bool isAutoDefragging = false;
    [SerializeField] TextMeshProUGUI _autoDefraggingLabelText;
    public bool isDefragComplete = false;

    [Header("Clock Variables")]
    float _startTime;
    public float Hours;
    public float Minutes;
    public float Seconds;
    public TextMeshProUGUI ElapsedTimeText;

    public int SectorsToDefrag = 0;
    public int SectorsDefragged = 0;

    public double CompletionChunksToFill = 0f;
    public double CompletionRate = 0f;
    public double Percentage = 0f;
    public TextMeshProUGUI CompletionText;

    private List<Sector> _allSectors = new List<Sector>();
    private List<Image> _allSectorsImages = new List<Image>();

    public List<Sprite> Legend;

    public int StartCheckingFromIndex;

    public TextMeshProUGUI FooterText;
    public List<string> RandomFooterText = new List<string>();

    [SerializeField] List<GameObject> _allMenus = new List<GameObject>();

    [SerializeField] GameObject _startMenu;
    [SerializeField] GameObject _quitMenu;
    [SerializeField] TextMeshProUGUI _quitMenuText;

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
        if (isAutoDefragging)
        {
            _autoDefraggingLabelText.text = "AUTODEFRAG DISABLED";
            CancelInvoke("SolveOne");
        }
        else
        {
            _autoDefraggingLabelText.text = "AUTODEFRAG ENABLED";
            InvokeRepeating("SolveOne", 0f, 0.2f);
        }

        isAutoDefragging = !isAutoDefragging;
    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0f;

        for (int i = 0; i < _size; i++)
        {
            GameObject sectorObject = Instantiate(_sectorPrefab, _sectorsPanel.transform);
            sectorObject.name = i.ToString();

            Sector sector = sectorObject.GetComponent<Sector>();
            _allSectors.Add(sectorObject.GetComponent<Sector>());

            Image _sectorImage = sectorObject.GetComponent<Image>();

            int index = UnityEngine.Random.Range(0, 2);
            _sectorImage.sprite = Legend[index];

            sector.SpriteID = index;
            _allSectorsImages.Add(_sectorImage);
        }

        foreach (Sector sector in _allSectors)
        {
            if (sector.SpriteID == 1)
            {
                SectorsToDefrag++;
            }
        }

        CompletionRate = 30f / (double)SectorsToDefrag;

        Invoke("TurnOffGrid", 0.1f);
        Invoke("CheckGrid", 0.2f);
        Invoke("RefreshFillBar", 0.3f);
    }

    public void RefreshFillBar()
    {
        CompletionBar.Instance.FillBar(CompletionChunksToFill);
    }

    void TurnOffGrid()
    {
        _sectorsPanel.GetComponent<GridLayoutGroup>().enabled = false;
    }

    public void CheckGrid()
    {
        Sector[] sectorChildren = _sectorsPanel.GetComponentsInChildren<Sector>();

        for (int i = StartCheckingFromIndex; i < sectorChildren.Length; i++)
        {
            if (SectorsDefragged == SectorsToDefrag) break;
            Sector sector = sectorChildren[i];

            if (sector.SpriteID != 1)
            {
                return;
            }

            sector.GetComponent<Image>().sprite = Legend[2];
            sector.gameObject.tag = "Untagged";
            SectorsDefragged++;

            StartCheckingFromIndex = i+1;

            CompletionChunksToFill = CompletionRate * SectorsDefragged;
            Percentage = ((double)SectorsDefragged / (double)SectorsToDefrag) * 100f;

            if (Percentage >= 100)
            {
                CompletionText.text = string.Format("Completion                {0}%", System.Math.Truncate(Percentage));
            }
            else if (Percentage >= 10)
            {
                CompletionText.text = string.Format("Completion                 {0}%", System.Math.Truncate(Percentage));
            }
            else
            {
                CompletionText.text = string.Format("Completion                  {0}%", System.Math.Truncate(Percentage));
            }
        }

        isDefragComplete = true;

        if (IsInvoking("SolveOne"))
        {
            CancelInvoke("SolveOne");
        }
        foreach (Sector sector in _allSectors)
        {
            sector.gameObject.tag = "Untagged";
        }

        AudioController.instance.EndLooping();
        FooterText.text = "Finished condensing";
    }

    public void Restart()
    {
        if (IsInvoking("SolveOne"))
        {
            CancelInvoke("SolveOne");
        }

        bool isRandomized = false;
        int leftToAdd = _maxToDefrag;

        if (leftToAdd == 0)
        {
            isRandomized = true;
        }
        
        isDefragComplete = false;

        SectorsDefragged = 0;
        SectorsToDefrag = 0;

        Percentage = 0;
        CompletionChunksToFill = 0;
        CompletionRate = 0;
        CompletionText.text = string.Format("Completion                  0%");
        StartCheckingFromIndex = 0;

        Hours = 0f;
        Minutes = 0f;
        Seconds = 0f;
        _startTime = Time.time;

        foreach(Image chunk in CompletionBar.Instance.ProgressBarChunks)
        {
            chunk.sprite = Legend[0];
        }

        _allSectors.Clear();
        _allSectorsImages.Clear();

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

            sector.gameObject.GetComponent<Image>().sprite = Legend[index];
            sector.SpriteID = index;

            _allSectors.Add(sector);
            _allSectorsImages.Add(sector.gameObject.GetComponent<Image>());
        }

        foreach (Sector sector in _allSectors)
        {
            if (sector.SpriteID == 1)
            {
                SectorsToDefrag++;
            }
        }

        CompletionRate = 30f / (double)SectorsToDefrag;
        CheckGrid();
        RefreshFillBar();
        FooterText.text = ChangeRandomFooterText();
        isPaused = false;

        AudioController.instance.StartLooping();

        if (isAutoDefragging)
        {
            InvokeRepeating("SolveOne", 0f, 0.2f);
        }
    }

    public void SolveOne()
    {
        if (isPaused) return;

        Sector[] sectorChildren = _sectorsPanel.GetComponentsInChildren<Sector>();

        for (int i = StartCheckingFromIndex; i < sectorChildren.Length; i++)
        {
            if (sectorChildren[i].SpriteID == 1)
            {
                sectorChildren[StartCheckingFromIndex].SpriteID = 1;
                sectorChildren[StartCheckingFromIndex].GetComponent<Image>().sprite = Legend[1];
                sectorChildren[i].SpriteID = 0;
                sectorChildren[i].GetComponent<Image>().sprite = Legend[0];
                FooterText.text = ChangeRandomFooterText();
                break;
            }
        }

        CheckGrid();
        RefreshFillBar();
    }

    public void SwitchPause()
    {
        if (Time.timeScale == 0) Time.timeScale = 1;
        else Time.timeScale = 0;

        isPaused = !isPaused;
    }

    public string ChangeRandomFooterText()
    {
        return RandomFooterText[UnityEngine.Random.Range(0, RandomFooterText.Count)];
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPaused && !isDefragComplete)
        {
            float t = Time.time - _startTime;

            Seconds = (int)(t % 60);
            Minutes = (int)((t / 60) % 60);
            Hours = (int)((t / 3600) % 24);

            ElapsedTimeText.text = string.Format("Elapsed Time: {0}:{1}:{2}", Hours.ToString("00"), Minutes.ToString("00"), Seconds.ToString("00"));
        }

        /*
        if (Input.GetKey(KeyCode.T))
        {
            SolveOne();
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            RefreshFillBar();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            SwitchAutoDefragging();
        }
        */

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
                if (isDefragComplete)
                {
                    _quitMenuText.text = "Are you sure you want to quit?\n\n\nYou can always come back later on, if your mind is not at peace";
                }
                else
                {
                    _quitMenuText.text = "Are you sure you want to quit?\n\n\nYour mind has been defragmented completely and it may not work as expected";
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
