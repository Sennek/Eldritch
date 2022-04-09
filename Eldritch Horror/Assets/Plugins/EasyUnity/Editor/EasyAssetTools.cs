#if UNITY_EDITOR
using MoreLinq;
using NoxLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
public static class EasyAssetTools
{
    [MenuItem("Assets/Easy Asset Tools/Find References In Project")]
    public static async void FindReferencesInProject()
    {
        string assetsFolder = Path.Combine(Environment.CurrentDirectory, "Assets");
        List<string> guids = Selection.objects
            .Select(o => AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(o)).ToString())
            .ToList();
        await FindReferencesInProject(assetsFolder, guids);
    }
    private static IEnumerable<string> GetProjectAssets(string assetsFolderPath)
        => Directory.EnumerateFiles(assetsFolderPath, "*.unity", SearchOption.AllDirectories)
             .Concat(Directory.EnumerateFiles(assetsFolderPath, "*.prefab", SearchOption.AllDirectories))
             .Concat(Directory.EnumerateFiles(assetsFolderPath, "*.asset", SearchOption.AllDirectories))
             .Concat(Directory.EnumerateFiles(assetsFolderPath, "*.anim", SearchOption.AllDirectories))
             .Concat(Directory.EnumerateFiles(assetsFolderPath, "*.mat", SearchOption.AllDirectories));
    public static async Task FindReferencesInProject(string assetsFolderPath, List<string> idList)
    {
        using Process notepadProcess = Process.Start(new ProcessStartInfo("notepad.exe"));
        IntPtr mainWindowHandle;
        IntPtr notepadWriteHandle;
        do
        {
            notepadProcess.WaitForInputIdle();
            notepadProcess.Refresh();
            mainWindowHandle = GetNotepadMainWindowHandle(notepadProcess);
            notepadWriteHandle = GetNotepadWriteHandle(mainWindowHandle);
            await Task.Delay(10);
        }
        while (notepadWriteHandle == IntPtr.Zero && !notepadProcess.HasExited);
        SetWindowText(mainWindowHandle, $"Listing project assets...");
        string[] filePaths = GetProjectAssets(assetsFolderPath).ToArray();
        long readFileSize = 0, totalFileSize = filePaths.AsParallel().Select(fp => new FileInfo(fp).Length).Sum();
        int fileCount = 0;
        int percentage, lastPercentage = -1;
        int lastSBLength = 0;
        TaskPool taskPool = new TaskPool();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < filePaths.Length && !notepadProcess.HasExited; i++)
        {
            if (lastPercentage != (percentage = (int)(readFileSize * 100f / totalFileSize)))
                SetWindowText(mainWindowHandle, $"Finding... {lastPercentage = percentage:00}%");
            if (sb.Length > lastSBLength)
            {
                lastSBLength = sb.Length;
                SetNotepadText(notepadWriteHandle, sb.ToString());
            }
            string path = filePaths[i]; //prevents issues with async
            await taskPool.EnqueueTaskAsync(() =>
            {
                if (FileContains(path, idList))
                {
                    fileCount++;
                    sb.AppendLine(path);
                }
                readFileSize += new FileInfo(path).Length;
            });
        }
        await taskPool.WaitAll();
        await Task.Delay(100); //Prevents some issues
        SetWindowText(mainWindowHandle, $"Done - {fileCount} files found");
    }
    private static bool FileContains(string filePath, List<string> idList)
    {
        using StreamReader reader = new StreamReader(filePath);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            foreach (string id in idList)
                if (line.Contains(id))
                    return true;
        }
        return false;
    }
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr hWnd);
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
    // Delegate to filter which windows to include 
    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    [DllImport("user32.dll")]
    public static extern int SetWindowText(IntPtr hWnd, string text);
    [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
    public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
    [DllImport("User32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);
    public static IntPtr GetNotepadMainWindowHandle(Process notepadProcess)
        => notepadProcess.MainWindowHandle != IntPtr.Zero
            ? notepadProcess.MainWindowHandle
            : FindWindows(delegate (IntPtr wnd, IntPtr param)
            { return GetWindowText(wnd).Contains("Untitled - Notepad"); })
                .FirstOrDefault();
    public static IntPtr GetNotepadWriteHandle(IntPtr mainWindowHandle)
        => FindWindowEx(mainWindowHandle, new IntPtr(0), "Edit", null);
    public static string GetWindowText(IntPtr hWnd)
    {
        int size = GetWindowTextLength(hWnd);
        if (size > 0)
        {
            var builder = new StringBuilder(size + 1);
            GetWindowText(hWnd, builder, builder.Capacity);
            return builder.ToString();
        }
        return string.Empty;
    }
    public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter)
    {
        IntPtr found = IntPtr.Zero;
        List<IntPtr> windows = new List<IntPtr>();
        EnumWindows(delegate (IntPtr wnd, IntPtr param)
        {
            if (filter(wnd, param))
                windows.Add(wnd);
            return true;
        }, IntPtr.Zero);
        return windows;
    }
    public static int SetNotepadText(IntPtr notepadWriteHandle, string text)
        => SendMessage(notepadWriteHandle, 0x000c, 0, text);
}
#endif
