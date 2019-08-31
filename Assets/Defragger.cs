﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Defragger : MonoBehaviour
{
    private static Defragger _instance;
    public static Defragger Instance { get { return _instance; } }

    [SerializeField] const int _size = 800;
    [SerializeField] GameObject _sectorPrefab;
    [SerializeField] GameObject _sectorsPanel;

    public bool HasStarted = false;

    // Clock variables
    float _startTime;
    public float Hours;
    public float Minutes;
    public float Seconds;
    public TextMeshProUGUI ElapsedTimeText;

    public int SectorsToDefrag = 0;
    public float Completion = 0f;
    public float CompletionRate = 0f;
    public int Percentage = 0;
    public TextMeshProUGUI CompletionText;

    private List<Sector> _allSectors = new List<Sector>();
    private List<Image> _allSectorsImages = new List<Image>();

    public List<Sprite> Legend;

    public int LastCheckedIndex;

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
        HasStarted = true;
        _startTime = Time.time;
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < _size; i++)
        {
            GameObject sectorObject = Instantiate(_sectorPrefab, _sectorsPanel.transform);
            sectorObject.name = i.ToString();

            Sector sector = sectorObject.GetComponent<Sector>();
            _allSectors.Add(sectorObject.GetComponent<Sector>());

            Image _sectorImage = sectorObject.GetComponent<Image>();
            int index = Random.Range(0, 2);
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

        CompletionRate = Mathf.Round((30f / SectorsToDefrag) * 100f) / 100f;

        Invoke("TurnOffGrid", 0.1f);
        Invoke("CheckGrid", 0.2f);
    }

    void TurnOffGrid()
    {
        _sectorsPanel.GetComponent<GridLayoutGroup>().enabled = false;
    }

    public void CheckGrid()
    {
        Sector[] sectorChildren = _sectorsPanel.GetComponentsInChildren<Sector>();
        for (int i = LastCheckedIndex; i < sectorChildren.Length; i++)
        {
            Sector sector = sectorChildren[i];

            if (sector.SpriteID != 1)
            {
                return;
            }

            sector.GetComponent<Image>().sprite = Legend[2];
            sector.gameObject.tag = "Untagged";
            LastCheckedIndex = i;
            Completion = CompletionRate * LastCheckedIndex;
            Percentage = Mathf.FloorToInt((Completion / 30) * 100f);
            if (Percentage > 9) CompletionText.text = string.Format("Completion                 {0}%", Percentage);
            else CompletionText.text = string.Format("Completion                  {0}%", Percentage);
            continue;
        }

        Debug.Log("All defragmented!");
    }

    public void Restart()
    {
        SectorsToDefrag = 0;
        Completion = 0;
        CompletionRate = 0;
        CompletionText.text = string.Format("Completion                  {0}%", Percentage);
        Percentage = 0;
        LastCheckedIndex = 0;

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

            int index = Random.Range(0, 2);
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

        CompletionRate = Mathf.Round((30f / SectorsToDefrag) * 100f) / 100f;
        Invoke("CheckGrid", 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (HasStarted)
        {
            float t = Time.time - _startTime;

            Seconds = (int)(t % 60);
            Minutes = (int)((t / 60) % 60);
            Hours = (int)((t / 3600) % 24);

            ElapsedTimeText.text = string.Format("Elapsed Time: {0}:{1}:{2}", Hours.ToString("00"), Minutes.ToString("00"), Seconds.ToString("00"));
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            CheckGrid();
        }
    }
}