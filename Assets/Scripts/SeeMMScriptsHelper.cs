using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SeeMMScriptsHelper
{
    public static string GetBasePath(string folderName = null)
    {
#if UNITY_EDITOR
        string path = Application.dataPath + $"/GameScripts/{folderName}";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return path;
#elif UNITY_ANDROID
        string path = Application.persistentDataPath + &"/GameScripts/{folderName}";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return path;
#elif UNITY_IPHONE
        string path = Application.persistentDataPath + &"/GameScripts/{folderName}";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return path;
#else
        string path = Application.dataPath + $"/GameScripts/{folderName}";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return path;
#endif
    }
}
