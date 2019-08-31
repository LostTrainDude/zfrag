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

    [SerializeField] const int _size = 800;

    public float Hours;
    public float Minutes;
    public float Seconds;
    public TextMeshProUGUI ElapsedTimeText;

    public int SectorsToDefrag = 0;
    public int Percentage = 0;

    public TextMeshProUGUI CompletionText;

    public float Completion = 0f;
    public float CompletionRate = 0f;

    [SerializeField] GameObject _sectorPrefab;

    [SerializeField] GameObject _sectorsPanel;

    public List<Sector> AllSectors = new List<Sector>();

    [SerializeField] List<Image> _allSectorsImages;
    public List<Image> AllSectorsImages { get => _allSectorsImages; set => _allSectorsImages = value; }

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

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < _size; i++)
        {
            GameObject sectorObject = Instantiate(_sectorPrefab, _sectorsPanel.transform);
            sectorObject.name = i.ToString();

            Sector sector = sectorObject.GetComponent<Sector>();
            AllSectors.Add(sectorObject.GetComponent<Sector>());

            Image _sectorImage = sectorObject.GetComponent<Image>();
            int index = Random.Range(0, 2);
            _sectorImage.sprite = Legend[index];

            sector.SpriteID = index;
            AllSectorsImages.Add(_sectorImage);
        }

        foreach (Sector sector in AllSectors)
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


    // Update is called once per frame
    void Update()
    {
        float t = Time.time;

        Seconds = (int)(t % 60);
        Minutes = (int)((t / 60) % 60);
        Hours = (int)((t / 3600) % 24);

        ElapsedTimeText.text = string.Format("Elapsed Time: {0}:{1}:{2}", Hours.ToString("00"), Minutes.ToString("00"), Seconds.ToString("00"));

        if (Input.GetKeyDown(KeyCode.T))
        {
            CheckGrid();
        }

    }
}
