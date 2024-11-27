using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class SaveSystem : MonoBehaviour
{
    [SerializeField]
    private GameObject go_dialogBox;
    [SerializeField]
    private GameObject go_loadSpecifficElements;
    [SerializeField]

    private FileContainer fileNamesContainer;

    [SerializeField]
    private InputField inputField_difficulty;

    [SerializeField]
    private InputField inputField_levelId;


    private void Start()
    {
        StartCoroutine(StartLoad());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        OnSavePressed();
    }

    private IEnumerator StartLoad()
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("Loading...");
        yield return LoadData();
    }

    private List<KeyValuePair<string, List<string>>> GetSaveFileNames()
    {
        List<KeyValuePair<string, List<string>>> res = new List<KeyValuePair<string, List<string>>>();
        string searchPattern = $"*.{PathUtility.extension}";
        try
        {
            var rootDirInfo = new DirectoryInfo(PathUtility.GetLevelFolderPath());
            var subFolder = rootDirInfo.GetDirectories();
            foreach (var folder in subFolder)
            {
                var fileNames = new List<string>();

                var files = folder.GetFiles(searchPattern, SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    fileNames.Add(file.Name.TrimEnd(('.' + PathUtility.extension).ToCharArray()));
                }
                res.Add(new KeyValuePair<string, List<string>>(folder.Name, fileNames));
            }
        }
        catch
        {
            // directory doesn't exist
        }

        return res;
    }


    private void SaveEditorData()
    {
        SaveColorData(ColorContainer.Instance.GetData());
        SaveTubesData(BeakerUI.MaxCapacity);
    }

    // buttons

    public void OnSavePressed()
    {
        // create file
        var fileName = GetSelectedFileName();
        if (fileName == null)
        {
            Debug.LogError("You must provide a name for the file. Either by picking a file that already exists or by typing one.");
            return;
        }
        SaveEditorData();
        SaveLevelData(BeakerContainer.Instance.GetData(), fileName.Item1, fileName.Item2);
        Debug.Log("Saved.");
        // hide UI
        go_dialogBox.SetActive(false);
    }

    public void OnLoadPressed()
    {
        StartCoroutine(LoadData());
    }

    public void OnExportPressed()
    {
        StartCoroutine(LoadData());
    }

    public void OnDeletePressed()
    {
        var fileName = GetSelectedFileName();
        if (fileName != null)
        {
            File.Delete(PathUtility.GetLevelFilePath(fileName.Item1, fileName.Item2));
        }
        else
        {
            Debug.LogError("No file was selected");
        }
    }

    public void OnCancelPressed()
    {
        go_loadSpecifficElements.SetActive(false);
        go_dialogBox.SetActive(false);
    }


    // functionality

    private IEnumerator LoadData()
    {
        // gather data
        var colorData = LoadColorData();
        var fileName = GetSelectedFileName();
        List<Beaker> beakerData = null;
        if (fileName != null)
        {
            beakerData = LoadLevelData(fileName.Item1, fileName.Item2);
        }
        else
        {
            Debug.LogError("No file was selected");
        }
        var maxCapacity = LoadTubesData();

        // load fill the containers
        try
        {
            ColorContainer.Instance.LoadData(colorData);
        }
        catch // the data wasn't loaded correctly
        {
            // reset the container
            ColorContainer.Instance.ResetContents();

            Debug.LogError("Couldn't load the configuration (color samples) from the given file.");
            yield break;
        }

        yield return new WaitForSeconds(0.1f);

        try
        {
            if (beakerData != null)
            {
                BeakerContainer.Instance.LoadData(beakerData, maxCapacity);
            }
            else
            {
                BeakerContainer.Instance.ResetContents();
            }
            // the data was loaded correctly
            Debug.Log("Success.");
        }
        catch // the data wasn't loaded correctly
        {
            // reset the containers
            BeakerContainer.Instance.ResetContents();

            Debug.LogError("Couldn't load the configuration (beakers) from the given file.");
            yield break;
        }

        // hide UI
        go_dialogBox.SetActive(false);
        go_loadSpecifficElements.SetActive(false);
    }

    private IEnumerator ExportLevelData()
    {
        foreach (var fileCache in GetSaveFileNames())
        {
            foreach (var fileName in fileCache.Value)
            {
                // todo export level data
                List<Beaker> beakerData = LoadLevelData(fileCache.Key, fileName);
                HsWaterSort.Level.LevelData levelData = new();
            }
        }

        yield break;
    }

    void SaveColorData(List<ColorSampleData> data)
    {
        string path = PathUtility.GetEditorPath("ColorData.json");
        if (data == null)
        {
            Debug.LogError("SaveColorData No data to save.");
            return;
        }
        string value = JsonConvert.SerializeObject(data, Formatting.Indented);
        PathUtility.EnsureFolder(path);
        File.WriteAllText(path, value);
    }

    List<ColorSampleData> LoadColorData()
    {
        string path = PathUtility.GetEditorPath("ColorData.json");
        if (File.Exists(path))
        {
            string value = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<ColorSampleData>>(value);
        }
        return null;
    }

    void SaveTubesData(int capacity)
    {
        string path = PathUtility.GetEditorPath("CapacityData.json");
        string value = JsonConvert.SerializeObject(capacity, Formatting.Indented);
        PathUtility.EnsureFolder(path);
        File.WriteAllText(path, value);
    }

    int LoadTubesData()
    {
        string path = PathUtility.GetEditorPath("CapacityData.json");
        if (File.Exists(path))
        {
            string value = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<int>(value);
        }
        return 4;
    }

    public void SaveLevelData(List<Beaker> data, string subFolder, string fileName)
    {
        string value = JsonConvert.SerializeObject(data, Formatting.Indented);
        var savePath = PathUtility.GetLevelFilePath(subFolder, fileName);
        PathUtility.EnsureFolder(savePath);
        File.WriteAllText(savePath, value);
    }

    public List<Beaker> LoadLevelData(string subFolder, string fileName)
    {
        string path = PathUtility.GetLevelFilePath(subFolder, fileName);
        if (File.Exists(path))
        {
            string value = File.ReadAllText(path);
            // JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            // jsonSerializerSettings.
            return JsonConvert.DeserializeObject<List<Beaker>>(value);
        }
        return null;
    }

    private Tuple<string, string> GetSelectedFileName()
    {
        if (inputField_difficulty.text == string.Empty || inputField_levelId.text == string.Empty)
        {
            return null;
        }
        return new Tuple<string, string>(inputField_difficulty.text, inputField_levelId.text);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                OnSavePressed();
            }
        }
    }

}
