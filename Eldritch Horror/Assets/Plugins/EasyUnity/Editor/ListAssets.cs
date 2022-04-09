#if UNITY_EDITOR
using NoxLibrary.Excel;

using System.Linq;

using UnityEditor;

using UnityEngine;

public static class ListAssets
{
    [MenuItem("Assets/Textures/Disable Mip Maps")]
    public static void DisableMipMaps()
    {
        string[] paths = AssetDatabase.GetAllAssetPaths();
        for (int i = 0; i < paths.Length; i++)
            if (AssetImporter.GetAtPath(paths[i]) is TextureImporter importer && importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
                Debug.Log($"Disabled mipmaps on {paths[i]}");
            }
    }

    [MenuItem("Assets/List/BadArt")]
    public static void ListBadArt()
    {
        Workbook workbook = new Workbook();
        Sheet sheet = workbook.CreateSheet("Assets");
        sheet.Get(0).SetNext("Path").SetNext("Width").SetNext("Height").SetNext("NPOT").SetNext("2K+");
        string[] array = AssetDatabase.GetAllAssetPaths();
        int rowID = 1;
        for (int i = 0; i < array.Length; i++)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(array[i]);
            if (!texture || array[i].StartsWith("Package")) continue;
            bool NPOT = !Mathf.IsPowerOfTwo(texture.width) || !Mathf.IsPowerOfTwo(texture.height);
            bool tooBig = texture.width > 2000 || texture.height > 2000;
            if (NPOT || tooBig)
                sheet.Get(rowID++).SetNext(array[i]).SetNext(texture.width).SetNext(texture.height).SetNext(NPOT).SetNext(tooBig);
        }
        workbook.Save(@"R:\Temp\LargeNPOTAssets");
        Debug.Log("Done");
    }

    [MenuItem("Assets/Fix/AudioLoadType")]
    public static void FixAudioLoadType()
    {
        foreach (string path in AssetDatabase.GetAllAssetPaths())
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (!clip) continue;
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (!importer) continue;

            bool modified = importer.preloadAudioData != (importer.preloadAudioData = false);

            AudioClipLoadType loadType =
                clip.length > 10 ? AudioClipLoadType.Streaming :
                clip.length > 1 ? AudioClipLoadType.CompressedInMemory :
                AudioClipLoadType.DecompressOnLoad;

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            modified |= settings.loadType != (settings.loadType = loadType);

            if (modified)
            {
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
            }
        }
    }

    [MenuItem("Assets/List/BadGamblingArt")]
    public static void ListBadGamblingArt()
    {
        Workbook workbook = new Workbook();
        Sheet sheet = workbook.CreateSheet("Assets");
        sheet.Get(0).SetNext("Path").SetNext("Width").SetNext("Height").SetNext("NPOT").SetNext("2K+");
        string[] array = AssetDatabase.GetAllAssetPaths();
        int rowID = 1;
        for (int i = 0; i < array.Length; i++)
        {
            if (!array[i].Contains("Gambling")) continue;
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(array[i]);
            if (!texture || array[i].StartsWith("Package")) continue;
            bool NPOT = !Mathf.IsPowerOfTwo(texture.width) || !Mathf.IsPowerOfTwo(texture.height);
            bool twoK = texture.width > 2000 || texture.height > 2000;
            if (NPOT || twoK)
                sheet.Get(rowID++).SetNext(array[i]).SetNext(texture.width).SetNext(texture.height).SetNext(NPOT).SetNext(twoK);
        }
        workbook.Save(@"R:\Temp\LargeNPOTAssets");
        Debug.Log("Done");
    }

    [MenuItem("Assets/Remove/Non-Sprite Textures")]
    public static void RemoveTextures()
    {
        foreach (string path in AssetDatabase.GetAllAssetPaths().Where(s => s.StartsWith("Assets/Sprites")))
        {
            if (path.EndsWith(".gif") || (AssetImporter.GetAtPath(path) is TextureImporter importer && importer.textureType != TextureImporterType.Sprite))
            {
                AssetDatabase.DeleteAsset(path);
                Debug.Log($"Removed asset at {path}");
            }
        }
    }

    //[MenuItem("Assets/Temp/Temp")]
    //public static void RunTemp()
    //{
    //    FileManager fm = FileManager.Create(@"R:\Temp\TempLog.txt");
    //    if (fm == null) return;
    //    fm.AutoFlush = true;
    //    //foreach (string guid in AssetDatabase.FindAssets("(800, 600) t:texture2D", new string[] { @"Assets/Sprites/DT Baseball" }))
    //    //{
    //    //    AssetImporter assetImporter = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
    //    //    if (assetImporter is TextureImporter textureImporter && textureImporter.spritePixelsPerUnit != 200)
    //    //    {
    //    //        textureImporter.spritePixelsPerUnit = 200;
    //    //        textureImporter.SaveAndReimport();
    //    //        fm.WriteLine(assetImporter.name);
    //    //    }
    //    //}

    //    HashSet<Character_SSI> validSSIs = new HashSet<Character_SSI>();
    //    AddValidCharacterSSIFrom(@"C:\MegaCat\DeadToons\Assets\Sprites\Baseball\Characters", validSSIs);
    //    AddValidCharacterSSIFrom(@"C:\MegaCat\DeadToons\Assets\Sprites\DT Baseball", validSSIs);

    //    foreach (string guid in AssetDatabase.FindAssets("t:texture2D", new string[] { @"Assets/Sprites/DT Baseball" }))
    //    {
    //        string filePath = AssetDatabase.GUIDToAssetPath(guid);
    //        Character_SSI ssi = new Character_SSI(Path.GetFileName(filePath));
    //        if (ssi == null || ssi.Pivot.x + ssi.Pivot.y != 0) continue;
    //        Character_SSI correct = validSSIs.FirstOrDefault(a => a.Pivot.x + a.Pivot.y != 0 && a.CharacterName.Substring(0, 3) == ssi.CharacterName.Substring(0, 3) && a.CategoryName == ssi.CategoryName && a.ActionName == ssi.ActionName && a.Direction == ssi.Direction && a.SpriteSize == ssi.SpriteSize);
    //        //if (correct != null)
    //        {
    //            fm.WriteLine(filePath);
    //            fm.WriteLine(ssi.ToString());
    //            fm.WriteLine(correct?.ToString());
    //        }
    //        if (correct != null)
    //        {
    //            ssi.Pivot = correct.Pivot;
    //            fm.WriteLine(AssetDatabase.RenameAsset(filePath, ssi.ToString()));
    //        }
    //        fm.WriteLine("");
    //    }
    //    fm.Dispose();
    //    AssetDatabase.SaveAssets();
    //}

    //private static void AddValidCharacterSSIFrom(string folder, HashSet<Character_SSI> validSSIs)
    //{
    //    foreach (string file in Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories).Where(f => !f.StartsWith("f_") /*&& !f.Contains("Silly") && !f.Contains("UnforcedError")*/))
    //    {
    //        Character_SSI ssi = new Character_SSI(Path.GetFileName(file));
    //        if ((ssi?.IsValid() ?? false) && validSSIs.Add(ssi))
    //            ;//WriteLine($"{validSSIs.Count}) {ssi}");
    //    }
    //}

    //[MenuItem("Assets/ConvertToPOT")]
    //public static void ConvertToPOT()
    //{
    //    foreach (Object o in Selection.objects)
    //    {
    //        string path = AssetDatabase.GetAssetPath(o);
    //        if (AssetImporter.GetAtPath(path) is TextureImporter importer)
    //        {
    //            if (o is Texture2D texture) texture.Resize(512, 512);                
    //            importer.npotScale = TextureImporterNPOTScale.None;
    //            importer.SaveAndReimport();
    //            AssetDatabase.ForceReserializeAssets(new List<string>() { path }, ForceReserializeAssetsOptions.ReserializeAssets);
    //            EditorUtility.SetDirty(o);
    //            AssetDatabase.SaveAssets();
    //        }
    //    }
    //}

}
#endif