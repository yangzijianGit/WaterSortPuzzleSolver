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
    private readonly string LevelFolderPath = "Level";
    private readonly string EditorPath = "Editor";
    private readonly string extension = "json";

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

    public void DisplayLoadDialog()
    {
        // display UI
        go_dialogBox.SetActive(true);
        go_loadSpecifficElements.SetActive(true);
        StartCoroutine(DisplayFileList());
    }

    private IEnumerator DisplayFileList()
    {
        yield return new WaitForSeconds(0.1f);
        PopulateFileList();
    }

    private void PopulateFileList()
    {
        fileNamesContainer.ResetContents();
        var files = GetSaveFileNames();

        if (files.Count > 0)
        {
            fileNamesContainer.Display(files);
        }
    }

    private List<string> GetSaveFileNames()
    {
        var fileNames = new List<string>();
        string searchPattern = $"*.{extension}";
        try
        {
            var files = new DirectoryInfo(GetLevelFolderPath()).GetFiles(searchPattern, SearchOption.AllDirectories);

            foreach (var file in files)
            {
                fileNames.Add(file.Name.TrimEnd(('.' + extension).ToCharArray()));
            }
        }
        catch
        {
            // directory doesn't exist
        }

        return fileNames;
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
        string fileName = GetSelectedFileName();
        if (fileName == string.Empty)
        {
            Debug.LogError("You must provide a name for the file. Either by picking a file that already exists or by typing one.");
            return;
        }
        SaveEditorData();
        SaveLevelData(BeakerContainer.Instance.GetData(), fileName);
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
        if (fileName != string.Empty)
        {
            File.Delete(GetLevelFilePath(fileName));
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
        var beakerData = LoadLevelData(GetSelectedFileName());
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
            BeakerContainer.Instance.LoadData(beakerData, maxCapacity);

            // the data was loaded correctly
            Debug.Log("Success.");
        }
        catch // the data wasn't loaded correctly
        {
            // reset the containers
            BeakerContainer.Instance.ResetContents();
            ColorContainer.Instance.ResetContents();

            Debug.LogError("Couldn't load the configuration (beakers) from the given file.");
            yield break;
        }

        // hide UI
        go_dialogBox.SetActive(false);
        go_loadSpecifficElements.SetActive(false);
    }

    private IEnumerator ExportLevelData()
    {
        foreach (var fileName in GetSaveFileNames())
        {
            List<Beaker> beakerData = LoadLevelData(fileName);
            HsWaterSort.Level.LevelData levelData = new();

        }

        yield break;
    }

    void SaveColorData(List<ColorSampleData> data)
    {
        string path = GetEditorPath("ColorData.json");
        if (data == null)
        {
            Debug.LogError("SaveColorData No data to save.");
            return;
        }
        string value = JsonConvert.SerializeObject(data, Formatting.Indented);
        EnsureFolder(path);
        File.WriteAllText(path, value);
    }

    List<ColorSampleData> LoadColorData()
    {
        string path = GetEditorPath("ColorData.json");
        if (File.Exists(path))
        {
            string value = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<ColorSampleData>>(value);
        }
        return null;
    }

    void SaveTubesData(int capacity)
    {
        string path = GetEditorPath("CapacityData.json");
        string value = JsonConvert.SerializeObject(capacity, Formatting.Indented);
        EnsureFolder(path);
        File.WriteAllText(path, value);
    }

    int LoadTubesData()
    {
        string path = GetEditorPath("CapacityData.json");
        if (File.Exists(path))
        {
            string value = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<int>(value);
        }
        return 4;
    }

    public void SaveLevelData(List<Beaker> data, string fileName)
    {
        string value = JsonConvert.SerializeObject(data, Formatting.Indented);
        var savePath = GetLevelFilePath(fileName);
        EnsureFolder(savePath);
        File.WriteAllText(savePath, value);
    }

    public List<Beaker> LoadLevelData(string fileName)
    {
        string path = GetLevelFilePath(fileName);
        if (File.Exists(path))
        {
            string value = File.ReadAllText(path);
            // JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            // jsonSerializerSettings.
            return JsonConvert.DeserializeObject<List<Beaker>>(value);
        }
        return null;
    }

    private void EnsureFolder(string path)
    {
        FileInfo fileInfo = new FileInfo(path);
        string directoryPath = fileInfo.Directory.FullName;
        EnsureDirectoryExists(directoryPath);
    }
    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private string GetLevelFolderPath()
    {
        return Path.Combine(Application.dataPath, LevelFolderPath);
    }

    private string GetEditorFolderPath()
    {
        return Path.Combine(Application.dataPath, EditorPath);
    }

    private string GetLevelFilePath(string fileName)
    {
        if (!fileName.EndsWith("." + extension))
        {
            fileName += "." + extension;
        }
        return Path.Combine(GetLevelFolderPath(), fileName);
    }

    private string GetEditorPath(string fileName)
    {
        if (!fileName.EndsWith("." + extension))
        {
            fileName += "." + extension;
        }
        return Path.Combine(GetEditorFolderPath(), fileName);
    }

    private string GetSelectedFileName()
    {
        if (inputField_difficulty.text == string.Empty || inputField_levelId.text == string.Empty)
        {
            return string.Empty;
        }
        return Path.Combine(inputField_difficulty.text, inputField_levelId.text);
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
