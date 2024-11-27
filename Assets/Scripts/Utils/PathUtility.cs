

using System.IO;
using UnityEngine;

public static class PathUtility
{
    public const string LevelFolderPath = "Level";
    public const string EditorPath = "Editor";
    public const string extension = "json";
    public static void EnsureFolder(string path)
    {
        FileInfo fileInfo = new FileInfo(path);
        string directoryPath = fileInfo.Directory.FullName;
        EnsureDirectoryExists(directoryPath);
    }
    public static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static string GetLevelFolderPath()
    {
        return Path.Combine(Application.dataPath, LevelFolderPath);
    }

    public static string GetEditorFolderPath()
    {
        return Path.Combine(Application.dataPath, EditorPath);
    }

    public static string GetLevelFilePath(string subFolder, string fileName)
    {
        if (string.IsNullOrEmpty(subFolder) || string.IsNullOrEmpty(fileName))
        {
            return string.Empty;
        }

        if (!fileName.EndsWith("." + extension))
        {
            fileName += "." + extension;
        }
        return Path.Combine(GetLevelFolderPath(), subFolder, fileName);
    }

    public static string GetEditorPath(string fileName)
    {
        if (!fileName.EndsWith("." + extension))
        {
            fileName += "." + extension;
        }
        return Path.Combine(GetEditorFolderPath(), fileName);
    }

}