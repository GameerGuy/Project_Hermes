using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    public static readonly string SAVE_FOLDER = Application.persistentDataPath + "/Saves/";

    public static void Init() 
    {
        if (!Directory.Exists(SAVE_FOLDER)) {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
    }

    public static SaveObject CreateSaveObject()
    {
        SaveObject save = new SaveObject {
            courseName = SceneManager.GetActiveScene().name,
            lapTime = TimeManager.Instance.stopwatchTimer,
            date = DateTime.Today.ToShortDateString(),
            time = DateTime.Now.ToShortTimeString()
        };
        return save;
    }

    public static void SaveBySerialization()
    {
        SaveObject saveObject = CreateSaveObject();
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fileStream = new FileStream(SAVE_FOLDER + "/record.txt", FileMode.Append);

        bf.Serialize(fileStream, saveObject);
        fileStream.Close();
    }

    public static void LoadByDesirialization()
    {

    }



    public static void SaveAsJSON(string saveString)
    {
        File.AppendAllText(SAVE_FOLDER + "/record.txt", saveString + "\n");
    }

    public static void SaveAsJSON(){
        SaveObject saveObject = CreateSaveObject();
        string saveString = JsonUtility.ToJson(saveObject);
        SaveAsJSON(saveString);
    }
}
