using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;


public class RandomSystem : MonoBehaviour
{
    [SerializeField]
    private InputField m_Folder;

    [SerializeField]
    private InputField m_LevelBeginIndex;

    [SerializeField]
    private InputField m_LevelEndIndex;

    [SerializeField]
    public InputField m_ColoredTubes;

    [SerializeField]
    public InputField m_NoColoredTubes;

    [SerializeField]
    public Text m_ColoredNumbs;

    [SerializeField]
    public GameObject m_ColorCopyGameObject;

    [SerializeField]
    public GameObject m_SampleControllerGameObject;

    public RandomSystemData RandomSystemData = new RandomSystemData();

    List<GameObject> m_ColorCopyList = new List<GameObject>();
    List<GameObject> m_ControllerCopyList = new List<GameObject>();




    void HideInVisable()
    {
        m_ColorCopyGameObject.SetActive(false);
        m_SampleControllerGameObject.SetActive(false);
    }

    void Start()
    {
        HideInVisable();
    }


    void RefreshColorData()
    {
        string text = "";
        RandomSystemData.colors = new List<int>();
        foreach (var copy in m_ColorCopyList)
        {
            ColorSelectUI colorSelectUI = copy.GetComponent<ColorSelectUI>();
            if (colorSelectUI.IsSelected)
            {
                text += colorSelectUI.ID + ",";
                RandomSystemData.colors.Add(colorSelectUI.ID);
            }
        }
        text = text.TrimEnd(',');
        m_ColoredNumbs.text = text;
    }

    void LoadColor()
    {
        RandomSystemData.colorSampleDataList = ColorContainer.Instance.GetData();
        foreach (var copy in m_ColorCopyList)
        {
            GameObject.Destroy(copy);
        }
        m_ColorCopyList.Clear();
        foreach (ColorSampleData colorSampleData in RandomSystemData.colorSampleDataList)
        {
            GameObject colorCopy = GameObject.Instantiate(m_ColorCopyGameObject, m_ColorCopyGameObject.transform.parent);
            colorCopy.SetActive(true);
            ColorSelectUI colorSelectUI = colorCopy.GetComponent<ColorSelectUI>();
            colorSelectUI.SetColor(colorSampleData.id, new Color(colorSampleData.r, colorSampleData.g, colorSampleData.b, colorSampleData.a));
            colorSelectUI.SetSelected(RandomSystemData.colors.Contains(colorSampleData.id));
            colorSelectUI.SetClickAction(() =>
            {
                RefreshColorData();
            });
            m_ColorCopyList.Add(colorCopy);
        }
        RefreshColorData();
    }

    void LoadData()
    {
        string path = PathUtility.GetEditorPath("RandomSystemData.json");
        if (File.Exists(path))
        {
            string value = File.ReadAllText(path);
            RandomSystemData = JsonConvert.DeserializeObject<RandomSystemData>(value, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        }
        else
        {
            RandomSystemData = new RandomSystemData();
        }
        RandomSystemData.FixData();
    }

    void SaveData()
    {
        if (RandomSystemData == null)
        {
            RandomSystemData = new RandomSystemData();
        }
        string path = PathUtility.GetEditorPath("RandomSystemData.json");
        PathUtility.EnsureFolder(path);
        string value = JsonConvert.SerializeObject(RandomSystemData, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
        File.WriteAllText(path, value);
    }


    void InitTextEvent()
    {
        m_Folder.onValidateInput += delegate (string input, int charIndex, char addedChar)
        {
            if (char.IsLetterOrDigit(addedChar) || addedChar == '_')
            {
                return addedChar;
            }
            return '\0';
        };
        m_Folder.onValueChanged.AddListener(delegate
        {
            RandomSystemData.folder = m_Folder.text;
        });
        m_LevelBeginIndex.onValidateInput += delegate (string input, int charIndex, char addedChar)
        {
            if (char.IsDigit(addedChar))
            {
                return addedChar;
            }
            return '\0';
        };
        m_LevelBeginIndex.onValueChanged.AddListener(delegate
        {
            if (m_LevelBeginIndex.text != string.Empty)
                RandomSystemData.levelBeginIndex = int.Parse(m_LevelBeginIndex.text);
        });
        m_LevelEndIndex.onValidateInput += delegate (string input, int charIndex, char addedChar)
        {
            if (char.IsDigit(addedChar))
            {
                return addedChar;
            }
            return '\0';
        };
        m_LevelEndIndex.onValueChanged.AddListener(delegate
        {
            if (m_LevelEndIndex.text != string.Empty)
                RandomSystemData.levelEndIndex = int.Parse(m_LevelEndIndex.text);
        });
        m_ColoredTubes.onValidateInput += delegate (string input, int charIndex, char addedChar)
        {
            if (char.IsDigit(addedChar))
            {
                return addedChar;
            }
            return '\0';
        };
        m_ColoredTubes.onValueChanged.AddListener(delegate
        {
            if (m_ColoredTubes.text != string.Empty)
                RandomSystemData.coloredTubes = int.Parse(m_ColoredTubes.text);
        });
        m_NoColoredTubes.onValidateInput += delegate (string input, int charIndex, char addedChar)
        {
            if (char.IsDigit(addedChar))
            {
                return addedChar;
            }
            return '\0';
        };
        m_NoColoredTubes.onValueChanged.AddListener(delegate
        {
            if (m_NoColoredTubes.text != string.Empty)
                RandomSystemData.noColoredTubes = int.Parse(m_NoColoredTubes.text);
        });
    }

    void SetTextData()
    {
        m_Folder.text = RandomSystemData.folder;
        m_LevelBeginIndex.text = RandomSystemData.levelBeginIndex.ToString();
        m_LevelEndIndex.text = RandomSystemData.levelEndIndex.ToString();
        m_ColoredTubes.text = RandomSystemData.coloredTubes.ToString();
        m_NoColoredTubes.text = RandomSystemData.noColoredTubes.ToString();
    }

    void LoadController()
    {
        foreach (var controller in m_ControllerCopyList)
        {
            GameObject.Destroy(controller);
        }
        m_ControllerCopyList.Clear();
        foreach (var controller in RandomSystemData.controllers)
        {
            GameObject controllerCopy = GameObject.Instantiate(m_SampleControllerGameObject, m_SampleControllerGameObject.transform.parent);
            controllerCopy.SetActive(true);
            controller.RefreshUI(RandomSystemData, controllerCopy);
            m_ControllerCopyList.Add(controllerCopy);
        }
    }

    void OnEnable()
    {
        Load();
    }
    void Load()
    {
        // load data from local file 
        LoadData();
        // init text ui event
        InitTextEvent();
        // set text data to input field 
        SetTextData();
        // copy colored in scroll panel 
        LoadColor();
        // copy controller in scroll panel.
        LoadController();
    }

    void OnDisable()
    {
        SaveData();
    }


    public void AddController()
    {
        SingleTubesColorNumControllerData controllerData = new SingleTubesColorNumControllerData();
        controllerData.TubesColorsNum = 0;
        controllerData.TubesNum = 0;
        RandomSystemData.controllers.Add(controllerData);
        LoadController();
    }

    public void BtnGenerator()
    {
        var vaildData = VaildData();
        if (vaildData.Item1 == false)
        {
            Debug.Log(vaildData.Item2);
            return;
        }
        for (int i = RandomSystemData.levelBeginIndex; i <= RandomSystemData.levelEndIndex; i++)
        {
            string levelFilePath = PathUtility.GetLevelFilePath(RandomSystemData.folder, i.ToString());
            // 尝试生成一关数据
            var result = RandomFlow.Excute(RandomSystemData);
            var str = JsonConvert.SerializeObject(result, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });
            Debug.Log(str);
            DirectoryInfo directoryInfo = new DirectoryInfo(new FileInfo(levelFilePath).Directory.FullName);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            File.WriteAllText(levelFilePath, str);
        }
    }


    public Tuple<bool, string> VaildData()
    {
        // 检查数量是否合法
        if (RandomSystemData.coloredTubes < 2)
        {
            return new Tuple<bool, string>(false, "Colored Tubes must be greater than or equal to 2");
        }
        if (RandomSystemData.noColoredTubes < 1)
        {
            return new Tuple<bool, string>(false, "NoColored Tubes must be greater than or equal to 1");
        }
        if (RandomSystemData.levelBeginIndex < 1)
        {
            return new Tuple<bool, string>(false, "Level Begin Index must be greater than or equal to 1");
        }
        if (RandomSystemData.levelEndIndex < 0)
        {
            return new Tuple<bool, string>(false, "Level End Index must be greater than or equal to 0");
        }
        if (RandomSystemData.levelEndIndex < RandomSystemData.levelBeginIndex)
        {
            return new Tuple<bool, string>(false, "Level End Index must be greater than or equal to Level Begin Index");
        }
        if (RandomSystemData.colors.Count < 2)
        {
            return new Tuple<bool, string>(false, "The number of selected colors must be greater than 2");
        }
        if (RandomSystemData.colors.Count > RandomSystemData.coloredTubes)
        {
            return new Tuple<bool, string>(false, "The number of selected colors must be less than or equal to the number of colored tubes");
        }
        // 控制器内数量是否合法
        int totalNum = 0;
        foreach (var controller in RandomSystemData.controllers)
        {
            totalNum += controller.OccupiedColoredTubesCount();
            Tuple<bool, string> vaildData = controller.VaildData(RandomSystemData);
            if (vaildData.Item1 == false)
            {
                return vaildData;
            }
        }
        if (totalNum >= RandomSystemData.coloredTubes)
        {
            return new Tuple<bool, string>(false, "The sum of the number of occupied colored tubes in all controllers must be less than or equal to the number of colored tubes");
        }


        return new Tuple<bool, string>(true, "");
    }


}
