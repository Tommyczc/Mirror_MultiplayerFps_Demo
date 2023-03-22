using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveModule : GameModule
{
    public static bool InitedWithData;
    
    private static string SavePath => Application.persistentDataPath + "/saves/";
    private static string SaveFile => SavePath + "game.sav";
    private static bool ExistSaveFile => File.Exists(SaveFile);
    private static Dictionary<string, string> _data = new();
    
    public override void OnRegister()
    {
        CreateSaveDir();
        InitWithData();
    }
    
    private static void CreateSaveDir() {
        if (!Directory.Exists(SavePath)) { 
            Directory.CreateDirectory(SavePath);
            Debug.Log("Save directory created.");
        }
    }
    
    private static void InitWithData()
    {
        if (ExistSaveFile)
        {
            ReadSaveFile();
            if (_data.Count != 0)
            {
                Debug.Log("Successfully inited with save data");
                InitedWithData = true;
            }
        }
        else
        {
            CreateSaveFile(true);
        }
    }
    
    public static void CreateSaveFile(bool overwrite = false)
    {
        if (ExistSaveFile)
        {
            if (overwrite)
            {
                File.Create(SaveFile).Dispose();
            }
            else
            {
                Debug.Log("Save file already exists");
            }
            return;
        }
        File.Create(SaveFile).Dispose();
        Debug.Log($"Save file created at {SaveFile}");
    }
    
    public static void WriteSaveFile() {
        if (File.Exists(SaveFile))
        {
            TryWriteToPath(SaveFile, _data);
        }
        else
        {
            Debug.Log("Save file not exist");
        }
    }
    
    public static void ReadSaveFile() {
        if (File.Exists(SaveFile)) {
            _data = TryReadFromPath(SaveFile);
        }
        else
        {
            Debug.Log("Save file not exist");
        }
    }
    
    public static void Save(string key, string value)
    {
        if (_data.ContainsKey(key))
        {
            _data[key] = value;
        }
        else
        {
            _data.Add(key, value);
        }
    }
    
    public static string Read(string key)
    {
        if (_data.ContainsKey(key))
        {
            return _data[key];
        }
        else
        {
            Debug.Log($"No data with key {key}");
            return string.Empty;
        }
    }

    #region Utils
    
    private static void TryWriteToPath(string path, Dictionary<string, string> data) {
        if (data.Count == 0)
        {
            Debug.Log($"Nothing to write to {path}");
            return;
        }
        StreamWriter sw = new StreamWriter(path);
        try
        {
            foreach (KeyValuePair<string, string> item in data)
            {
                sw.WriteLine(Encode(item));
            }
        }
        catch
        {

            Debug.Log($"Unknown error when writing to {path}");
        }
        finally { sw.Close(); }
    }
    
    private static Dictionary<string, string> TryReadFromPath(string path) {
        StreamReader sr = new StreamReader(path);
        Dictionary<string, string> data = new Dictionary<string, string>();
        try
        {
            while (sr.Peek() != -1)
            {
                KeyValuePair<string, string> item = Decode(sr.ReadLine());
                data.Add(item.Key, item.Value);
            }
        }
        catch
        {
            Debug.Log($"Unknown error when reading {path}");
        }
        finally { sr.Close(); }

        if (data.Count == 0)
        {
            Debug.Log($"Nothing read from {path}");
        }

        return data;
    }
    
    public static KeyValuePair<string, string> Decode(string save) {
        string[] segments = save.Split("|");
        if (segments.Length == 2) {
            return new KeyValuePair<string, string>(segments[0], segments[1]);
        }
        if (segments.Length < 1) {
            Debug.Log($"Empty or corrupted save item : {save}");
        }
        else {
            Debug.Log($"Invalid Save file #{segments[1]}");
        }
        return new KeyValuePair<string, string>();
    }

    public static string Encode(KeyValuePair<string, string> data) { 
        return data.Key + "|" + data.Value;
    }

    #endregion

    public override void OnUnregister()
    {
        //
    }
}
