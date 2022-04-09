using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using UnityEditor;

using UnityEngine;

public static class EasyGameLoader
{
    private static readonly Queue<(string json, string location)> saveQueue = new Queue<(string json, string location)>();
    private volatile static bool saveIssues;
    private volatile static Task saveTask;
    private volatile static byte currentSaveSlot;

    public static byte CurrentSaveSlot { get => currentSaveSlot; set => currentSaveSlot = value; }

    public static float SavedPlaytimeThisSession { get; private set; }
    public static bool LoadingComplete { get; private set; }

    public static string GameLogPath => Application.persistentDataPath + "/GameLog.txt";
    public static string SaveFolder => Application.persistentDataPath;

    public const string BACKUP_ADDITIONAL_EXTENSION = "bkp";

    public static string GetSaveSlotLocation() => GetSaveSlotLocation(CurrentSaveSlot);
    public static string GetSaveSlotLocation(int saveSlot, int trailIndex = 0)
        => Path.Combine(SaveFolder, $"Save_{saveSlot:00}_{trailIndex:00}.json");

    public static bool SaveIssues => saveIssues;
    public static bool SaveRunning => saveTask != null && !saveTask.IsCompleted && !saveTask.IsFaulted;
    public static Dictionary<byte, EasyGameSave> GameSaves { get; set; } = new Dictionary<byte, EasyGameSave>() { { 0, new EasyGameSave(Application.version) } };

    public static void ResetCurrentSave(bool save = true)
    {
        GameSaves[CurrentSaveSlot] = new EasyGameSave(Application.version);
        if (save) SaveGame();
    }

    public static void ResetAllSaves(bool save = true)
    {
        GameSaves = new Dictionary<byte, EasyGameSave>() { { 0, new EasyGameSave(Application.version) } };
        CurrentSaveSlot = 0;
        if (save) SaveGame();
    }

    /// <summary>
    /// Saves current game state to CurrentSaveSlot
    /// </summary>
    /// <returns>A boolean indicating if the saving worked or failed</returns>
    public static void SaveGame()
    {
        SetInfo();

        saveQueue.Enqueue((JsonConvert.SerializeObject(EasyGameSave.Current, Formatting.Indented), GetSaveSlotLocation()));

        if (saveTask == null || saveTask.IsCompleted)
            saveTask = Task.Run(SaveGameTask);
    }

    /// <summary>
    /// Saves current game state to CurrentSaveSlot
    /// </summary>
    /// <param name="screenshot">A screenshot that is a visual representation of this save</param>
    /// <returns>A boolean indicating if the saving worked or failed</returns>
    public static void SaveGame(Texture2D screenshot)
    {
        SetInfo();

        EasyGameSave.Current.Screenshot = screenshot.EncodeToPNG();

        saveQueue.Enqueue((JsonConvert.SerializeObject(EasyGameSave.Current, Formatting.Indented), GetSaveSlotLocation()));

        if (saveTask == null || saveTask.IsCompleted)
            saveTask = Task.Run(SaveGameTask);
    }

    /// <summary>
    /// Saves current game state to CurrentSaveSlot
    /// </summary>
    /// <param name="screenshotTaker">An instance of a ScreenshotTaker that can provide a visual representation of this save</param>
    /// <returns>A boolean indicating if the saving worked or failed</returns>
    public static void SaveGame(EasyScreenshotTaker screenshotTaker)
    {
        SetInfo();
        screenshotTaker.TakeScreenshot(OnScreenshotTaken);

        static void OnScreenshotTaken(Texture2D texture)
        {
            EasyGameSave.Current.Screenshot = texture.EncodeToPNG();

            saveQueue.Enqueue((JsonConvert.SerializeObject(EasyGameSave.Current, Formatting.Indented), GetSaveSlotLocation()));

            if (saveTask == null || saveTask.IsCompleted)
                saveTask = Task.Run(SaveGameTask);
        }
    }

    private static void SetInfo()
    {
        EasyGameSave.AddPlaytime(TimeSpan.FromSeconds(Time.realtimeSinceStartup - SavedPlaytimeThisSession), save: false);
        SavedPlaytimeThisSession = Time.realtimeSinceStartup;
        EasyGameSave.SetSuccessful();
    }

    private static void SaveGameTask()
    {
        while (saveQueue.Count > 0)
        {
            (string json, string location) = saveQueue.Dequeue();
            if (SaveGame(location, location + BACKUP_ADDITIONAL_EXTENSION, json))
                continue;
            Thread.Sleep(100);
            if (SaveGame(location, location + BACKUP_ADDITIONAL_EXTENSION, json))
                continue;
            saveIssues = true;
            break;
        }
    }

    private static bool SaveGame(string saveLocation, string backupSaveLocation, string json)
    {
        if (SaveFile(json, saveLocation))
        {
            SaveFile(json, backupSaveLocation);
            return true;
        }
        else
        {
            if (File.Exists(saveLocation))
                File.Delete(saveLocation);
            return false;
        }
    }

    private static bool SaveFile(string json, string location)
    {
        if (File.Exists(location))
            File.Delete(location);
        File.WriteAllText(location, json);
        return File.ReadAllText(location) == json;
    }

    public static DateTime GetModifiedTime(byte key)
    {
        string location = GetSaveSlotLocation(key);
        return File.Exists(location)
                  ? File.GetLastAccessTime(location)
                  : File.Exists(location + BACKUP_ADDITIONAL_EXTENSION)
                      ? File.GetLastAccessTime(location + BACKUP_ADDITIONAL_EXTENSION)
                      : default;
    }

    public static async Task LoadAllAsync()
    {
        LoadingComplete = false;
        await Task.Yield();

        List<(byte slot, string location)> saves = Directory.EnumerateFiles(SaveFolder, $"Save_??_00.json")
            .Select(location => (byte.TryParse(location.Substring(location.Length - 10, 2), out byte b) ? b : (byte)255, location))
            .Where(b => b.Item1 < 100).ToList();
        await Task.Yield();

        saves.AddRange(Directory.EnumerateFiles(SaveFolder, $"Save_??_00.json" + BACKUP_ADDITIONAL_EXTENSION)
            .Select(location => (byte.TryParse(location.Substring(location.Length - 13, 2), out byte b) ? b : (byte)255, location))
            .Where(b => b.Item1 < 100 && !saves.Any(save => save.slot == b.Item1)));
        await Task.Yield();

        GameSaves = new Dictionary<byte, EasyGameSave>();
        foreach ((byte slot, string location) in saves)
            await Task.Run(() => LoadGame(slot, location));

        if (GameSaves.Count == 0) GameSaves.Add(0, new EasyGameSave(Application.version));
        LoadingComplete = true;
    }

    /// <summary>
    /// Attempts to load a save from disk using provided saveSlot
    /// </summary>
    /// <param name="saveSlot">The slot ID thet should be loaded from disk</param>
    /// <returns>A boolean indicating if the loading worked or failed</returns>
    private static void LoadGame(byte saveSlot, string location)
    {
        try
        {
            if (File.Exists(location))
                GameSaves[saveSlot] = JsonConvert.DeserializeObject<EasyGameSave>(File.ReadAllText(location));
        }
        catch (Exception e) { Debug.LogException(e); }
        try
        {
            if (!loadSuccessful() && File.Exists(location + BACKUP_ADDITIONAL_EXTENSION))
                GameSaves[saveSlot] = JsonConvert.DeserializeObject<EasyGameSave>(File.ReadAllText(location + BACKUP_ADDITIONAL_EXTENSION));
        }
        catch (Exception e) { Debug.LogException(e); }

        bool loadSuccessful() => GameSaves.TryGetValue(saveSlot, out EasyGameSave save) && save.Successful;
    }

    public static bool IsCompatibleSaveVersion(string version) => AreCompatibleSaveVersions(version, Application.version);


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Method temporarily commented out")]
    public static bool AreCompatibleSaveVersions(string versionA, string versionB) => true;
    //=> Version.TryParse(versionA, out Version vA) && Version.TryParse(versionB, out Version vB) &&
    //vA.Major == vB.Major && vA.Minor == vB.Minor && vA.Build == vB.Build;
}
