using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public static class SaveSystem
{
    public static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";

    public static void Init() 
    {
        if (!Directory.Exists(SAVE_FOLDER)) {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
    }

    public static void Save(string saveString)
    {
        File.AppendAllText(SAVE_FOLDER + "/record.txt", saveString + "\n");
    }

    public static void Save(SaveObject saveObject){
        string saveString = JsonUtility.ToJson(saveObject);
        Save(saveString);
    }
}
