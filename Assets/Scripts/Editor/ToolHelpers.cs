using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.IO;
using System.Linq;
using UnityEngine.U2D;
using UnityEditor.U2D;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor.PackageManager;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public static class ToolHelpers
{
    [MenuItem("Tools/clear addressables cache", false, 50)]
    public static void ClearAddressablesCache()
    {
        UnityEngine.Caching.ClearCache();
    }

    [MenuItem("Tools/Update Addressable Label/PuzzleLabels", false, 50)]
    public static void AddPuzzleLabels()
    {
        string sptSrcDir = $"{Application.dataPath}/Bundles/Puzzles";
        Debug.Log(sptSrcDir);
        DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);
        var tutorialPuzzle = new List<string>() { "Alice_6_Tut", "Frankenstein_6_Tut_Ver2", "Peter_Pan_19_Tut", "The_Secret_Garden_8_Tut" };
        IEnumerable<DirectoryInfo> dirInfo = rootDirInfo.EnumerateDirectories();
        var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Puzzles");        
        foreach (var dir in dirInfo)
        {
            if (tutorialPuzzle.Contains(dir.Name)) continue;
            string puzzlePath = $"Assets/Bundles/Puzzles/{dir.Name}";
            string label = $"Puzzle_{dir.Name}";
            AddressableAssetSettingsDefaultObject.Settings.AddLabel(label);
            if (group != null)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.AssetPath == puzzlePath)
                    {
                        if (!entry.labels.Contains(label))
                        {
                            Debug.Log($"{label} {puzzlePath}");
                            entry.labels.Clear();
                            entry.SetLabel(label, true, true);
                        }
                    }
                }
            }
        }

        EditorUtility.SetDirty(group);
    }
    [MenuItem("Tools/Update Addressable Label/MusicLabels", false, 50)]
    public static void AddMusicLabels()
    {
        string sptSrcDir = $"{Application.dataPath}/Bundles/Music";
        Debug.Log(sptSrcDir);
        DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);

        IEnumerable<FileInfo> fileInfo = rootDirInfo.EnumerateFiles("*.mp3");
        var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Music");
        foreach (var file in fileInfo)
        {
            var name = file.Name.Replace(".mp3", "");
            var label = $"Music_{name}";
            string puzzlePath = $"Assets/Bundles/Music/{file.Name}";

            AddressableAssetSettingsDefaultObject.Settings.AddLabel(label);
            if (group != null)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.AssetPath == puzzlePath)
                    {
                        if (!entry.labels.Contains(label))
                        {
                            Debug.Log($"{label} {puzzlePath}");
                            entry.labels.Clear();
                            entry.SetLabel(label, true, true);
                        }
                    }
                }
            }
        }
        EditorUtility.SetDirty(group);
    }
    [MenuItem("Tools/Update Addressable Label/SpinesLabels", false, 50)]
    public static void AddSpinesLabels()
    {
        string sptSrcDir = $"{Application.dataPath}/Bundles/Spines";
        Debug.Log(sptSrcDir);
        DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);

        IEnumerable<DirectoryInfo> dirInfo = rootDirInfo.EnumerateDirectories();
        var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup("Spines");
        foreach (var dir in dirInfo)
        {
            string puzzlePath = $"Assets/Bundles/Spines/{dir.Name}";
            string puzzle = dir.Name.Replace("Spine_", "");            
            string label = $"Spine_{puzzle}";
            if (group != null)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.AssetPath == puzzlePath)
                    {                        
                        if (!entry.labels.Contains(label))
                        {
                            Debug.Log($"{label} {puzzlePath}");
                            entry.labels.Clear();
                            entry.SetLabel(label, true, true);
                        }
                    }
                }
            }
        }
        EditorUtility.SetDirty(group);
    }

    [MenuItem("Tools/Update Addressable Label/LargeThumbnailsLabels", false, 50)]
    public static void AddLargeThumbnailLabels()
    {
        string sptSrcDir = $"{Application.dataPath}/Bundles/Thumbnails/Puzzle/Large";
        Debug.Log(sptSrcDir);
        DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);

        IEnumerable<FileInfo> fileInfo = rootDirInfo.EnumerateFiles("*.png");
        var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup("LargeThumbnails");
        foreach (var file in fileInfo)
        {
            var name = file.Name.Replace(".png", "");
            var label = $"Thumbnail_Large_{name}";
            string puzzlePath = $"Assets/Bundles/Thumbnails/Puzzle/Large/{file.Name}";
            AddressableAssetSettingsDefaultObject.Settings.AddLabel(label);

            if (group != null)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.AssetPath == puzzlePath)
                    {                        
                        if (!entry.labels.Contains(label))
                        {
                            Debug.Log($"{label} {puzzlePath}");
                            entry.labels.Clear();
                            entry.SetLabel(label, true, true);
                        }
                    }
                }
            }
        }
        EditorUtility.SetDirty(group);
    }

    [MenuItem("Tools/Update Addressable Label/SmallThumbnailLabels", false, 50)]
    public static void AddSmallThumbnailLabels()
    {
        string sptSrcDir = $"{Application.dataPath}/Bundles/Thumbnails/Puzzle/Small";
        Debug.Log(sptSrcDir);
        DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);
        var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup("SmallThumbnails");
        IEnumerable<FileInfo> fileInfo = rootDirInfo.EnumerateFiles("*.png");
        foreach (var file in fileInfo)
        {
            var name = file.Name.Replace(".png", "");
            var label = $"Thumbnail_Small_{name}";
            string puzzlePath = $"Assets/Bundles/Thumbnails/Puzzle/Small/{file.Name}";

            AddressableAssetSettingsDefaultObject.Settings.AddLabel(label);
            if (group != null)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.AssetPath == puzzlePath)
                    {
                        entry.labels.Clear();
                        entry.SetLabel(label, true, true);
                    }
                }
            }
        }
        EditorUtility.SetDirty(group);
    }

    [MenuItem("Tools/Update Addressable Label/FeatureBooks", false, 50)]
    public static void AddFeatureBooks()
    {
        var group = AddressableAssetSettingsDefaultObject.Settings.FindGroup("FeatureBooks");
        string sptSrcDir = $"{Application.dataPath}/Bundles/FeatureBooks/";
        string iPhoneSrcDir = "iPhone";
        string iPadSrcDir = "iPad";
        var folders = new List<string>() {
            iPhoneSrcDir,
            iPadSrcDir
        };
        foreach (var folder in folders)
        {
            var path = sptSrcDir + folder;
            Debug.Log(path);
            DirectoryInfo rootDirInfo = new DirectoryInfo(path);
            IEnumerable<FileInfo> fileInfo = rootDirInfo.EnumerateFiles("*.png");
            foreach (var file in fileInfo)
            {
                var name = file.Name.Replace(".png", "");
                var label = $"FeatureBooks_{name}";
                string puzzlePath = $"Assets/Bundles/FeatureBooks/{folder}/{file.Name}";
                Debug.Log($"{label} {puzzlePath}");
                AddressableAssetSettingsDefaultObject.Settings.AddLabel(label);
                if (group != null)
                {
                    foreach (var entry in group.entries)
                    {
                        if (entry.AssetPath == puzzlePath)
                        {
                            entry.labels.Clear();
                            entry.SetLabel(label, true, true);
                        }
                    }
                }
            }
            EditorUtility.SetDirty(group);
        }
    }

    [MenuItem("Tools/Update Addressable Label/RemoveOldAddressableLabels", false, 50)]
    private static void RemoveOldAddressablesLabels()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        HashSet<string> validLabels = new HashSet<string>(settings.GetLabels());

        foreach (AddressableAssetGroup group in settings.groups)
        {
            foreach (AddressableAssetEntry entry in group.entries)
            {
                HashSet<string> entryLabels = new HashSet<string>(entry.labels);
                foreach (string entryLabel in entryLabels)
                {
                    if (!validLabels.Contains(entryLabel))
                    {
                        entry.SetLabel(entryLabel, false);
                    }
                }
            }

            EditorUtility.SetDirty(group);
        }
    }



    [MenuItem("Tools/clear puzzle scriptable object", false, 50)]
    public static void ClearPuzzleScriptableObjects()
    {
        string root = $"{Application.dataPath}/ScriptableObjects/Puzzles/";
        string BackgroundMusic = root + "/BackgroundMusic/";
        string Photos = root + "/Photos/";
        string Puzzles = root + "/Puzzles/";
        string Spines = root + "/Spines/";
        string Thumbnails = root + "/Thumbnails/";

        var folders = new List<string>() { BackgroundMusic, Photos, Puzzles, Spines, Thumbnails };
        foreach (var path in folders)
        {
            Debug.Log(path);
            if (Directory.Exists(path)) { Directory.Delete(path, true); }
            Directory.CreateDirectory(path);
        }
    }

    [MenuItem("Tools/clear Chapter scriptable object", false, 50)]
    public static void ClearChapterScriptableObjects()
    {
        string root = $"{Application.dataPath}/ScriptableObjects/Chapters/";
        string BackgroundMusic = root + "/Chapters/";
        string Thumbnails = root + "/Thumbnails/";

        var folders = new List<string>() { BackgroundMusic, Thumbnails };
        foreach (var path in folders)
        {
            Debug.Log(path);
            if (Directory.Exists(path)) { Directory.Delete(path, true); }
            Directory.CreateDirectory(path);
        }
    }



    [MenuItem("Tools/clear Spine", false, 50)]
    public static void ClearSpine()
    {
        string sptSrcDir = $"{Application.dataPath}/Bundles/Puzzles";
        Debug.Log(sptSrcDir);
        DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);
        IEnumerable<DirectoryInfo> dirInfo = rootDirInfo.EnumerateDirectories();

        foreach (var dir in dirInfo)
        {
            var spinePath = $"Assets/Bundles/Puzzles/{dir.Name}/Spine_{dir.Name}";
            var metaSpinePath = $"Assets/Bundles/Puzzles/{dir.Name}/Spine_{dir.Name}.meta";
            Debug.Log(spinePath);
            if (Directory.Exists(spinePath)) {
                //Directory.Delete(spinePath, true);
                FileUtil.DeleteFileOrDirectory(spinePath);
                FileUtil.DeleteFileOrDirectory(metaSpinePath);
                Debug.Log($"Deleted {spinePath}");

            }
        }
    }

    [MenuItem("Tools/clear large photo", false, 50)]
    public static void ClearLargePhoto()
    {
        string sptSrcDir = $"{Application.dataPath}/Bundles/Puzzles";
        Debug.Log(sptSrcDir);
        DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);
        IEnumerable<DirectoryInfo> dirInfo = rootDirInfo.EnumerateDirectories();

        foreach (var dir in dirInfo)
        {
            var spinePath = $"Assets/Bundles/Puzzles/{dir.Name}/{dir.Name}.png";
            var metaSpinePath = $"Assets/Bundles/Puzzles/{dir.Name}/{dir.Name}.png.meta";            
            if (File.Exists(spinePath))
            {
                //Directory.Delete(spinePath, true);
                FileUtil.DeleteFileOrDirectory(spinePath);
                FileUtil.DeleteFileOrDirectory(metaSpinePath);
                Debug.Log($"Deleted {spinePath}");

            }
        }
    }

    [MenuItem("Tools/Create SpriteAtlas", false, 50)]
    public static void CreateSpriteAltasForPuzzles()
    {
        string sptSrcDir = $"{Application.dataPath}/Bundles/Puzzles";
        Debug.Log(sptSrcDir);
        DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);

        IEnumerable<DirectoryInfo> dirInfo = rootDirInfo.EnumerateDirectories();
        List<Object> spts = new List<Object>();
        foreach (var dir in dirInfo)
        {
            spts.Clear();
            string puzzlePath = $"{sptSrcDir}/{dir.Name}";
            var atlasPath = $"Assets/Bundles/Puzzles/{dir.Name}/Sprites.spriteatlas";
            if (File.Exists(atlasPath))
            {
                continue;
            }
            if (dir != null)
            {
                //string assetPath = $"{puzzlePath}/{dir.Name}";
                //Debug.Log($"assetPath: {assetPath}");
                var subDirs = dir.GetDirectories(dir.Name);
                if (subDirs.Length > 0)
                {
                    var subDir = subDirs.First();
                    var files = subDir.GetFiles($"*.png", SearchOption.TopDirectoryOnly);
                    for (int j = 0; j < files.Count(); j++)
                    {
                        var pngFile = files[j];
                        string allPath = pngFile.FullName;
                        string spritePath = allPath.Substring(allPath.IndexOf("Assets"));
                        //Debug.Log($"spritePath: {spritePath}");
                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                        if (IsPackable(sprite))
                            spts.Add(sprite);
                    }
                }
            }
            if (!File.Exists(atlasPath))
            {
                CreateAtlas(puzzlePath);
                SpriteAtlas sptAtlas = (SpriteAtlas)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(SpriteAtlas));
                Debug.Log($" File is exist. {atlasPath}");
                Debug.Log(sptAtlas.tag);
                AddPackAtlas(sptAtlas, spts.ToArray());

            }
        }
    }

    [MenuItem("Tools/Add Photo to Puzzle", false, 50)]
    public static void AddPhotoToPuzzle()
    {
        string sptSrcDir = $"{Application.dataPath}/Bundles/Puzzles";
        //Assets/Bundles/Thumbnails/Puzzle/Large
        var srcFolderPath = $"{Application.dataPath}/Bundles/Thumbnails/Puzzle/Large";
        DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);
        DirectoryInfo[] dirInfo = rootDirInfo.GetDirectories();
        foreach (var dir in dirInfo)
        {
            var photoPath = $"{sptSrcDir}/{dir.Name}/{dir.Name}.png";
            var photoMetaPath = $"{photoPath}.meta";
            var srcPNGPath = $"{srcFolderPath}/{dir.Name}.png";
            var srcMetaPath = $"{srcPNGPath}.meta";

            if (File.Exists(srcPNGPath))
            {
                Debug.Log($"{photoPath}");
                if (!File.Exists(photoPath))
                {
                    FileUtil.ReplaceFile(srcPNGPath, photoPath);
                }
            }
            else
            {
                Debug.LogError($"Failed to copy file: {srcPNGPath}");
            }
            if (File.Exists(srcMetaPath))
            {
                if (!File.Exists(photoMetaPath))
                {
                    FileUtil.ReplaceFile(srcMetaPath, photoMetaPath);
                }
            }
            else
            {
                Debug.LogError($"Failed to copy file: {srcPNGPath}");
            }

        }
    }

    [MenuItem("Tools/Add Spine to Puzzle", false, 50)]
    public static void AddSpineToPuzzle()
    {
        string sptSrcDir = $"{Application.dataPath}/Bundles/Puzzles";
        //Assets/Bundles/Thumbnails/Puzzle/Large
        var srcFolderPath = $"{Application.dataPath}/Bundles/Spines";
        DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);
        IEnumerable<DirectoryInfo> dirInfo = rootDirInfo.EnumerateDirectories();
        foreach (var dir in dirInfo)
        {
            var spinePath = $"{srcFolderPath}/Spine_{dir.Name}";
            var desSpinePath = $"{sptSrcDir}/{dir.Name}/Spine_{dir.Name}";
            if (Directory.Exists(spinePath))
            {
                if (!Directory.Exists(desSpinePath))
                {
                    Debug.Log($"Copy {spinePath} to {desSpinePath}");
                    FileUtil.CopyFileOrDirectory(spinePath, desSpinePath);
                }

            }
            else
            {
                Debug.LogError($"Missing {dir.Name}");
            }
        }
    }

    [MenuItem("Tools/Get Objects In Puzzle", false, 50)]
    public static void GetObjectsInPuzzles()
    {
        string sptSrcDir = $"{Application.dataPath}/Bundles/Puzzles";
        Debug.Log(sptSrcDir);
        DirectoryInfo rootDirInfo = new DirectoryInfo(sptSrcDir);
        DirectoryInfo[] dirInfo = rootDirInfo.GetDirectories();
        List<Object> spts = new List<Object>();
        var finalString = "";
        for (int i = 0; i < dirInfo.Count(); i++)
        {
            var dir = dirInfo[i];
            string puzzlePath = $"{sptSrcDir}/{dir.Name}/{dir.Name}";
            if (Directory.Exists(puzzlePath))
            {
                string[] fileEntries = Directory.GetFiles(puzzlePath, "*.png");
                var objects = (fileEntries.Count() - 1) / 2;
                var objectString = $"{dir.Name} {objects}\n";
                finalString += objectString;
            }

        }
        Debug.Log(finalString);
    }

    static bool IsPackable(Object o)
    {
        return o != null && (o.GetType() == typeof(Sprite) || o.GetType() == typeof(Texture2D) || (o.GetType() == typeof(DefaultAsset) && ProjectWindowUtil.IsFolder(o.GetInstanceID())));
    }


    public static void CreateAtlas(string sptDesDir)
    {
        string yaml = @"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!687078895 &4343727234628468602
SpriteAtlas:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Sprites
  m_EditorData:
    serializedVersion: 2
    textureSettings:
      serializedVersion: 2
      anisoLevel: 1
      compressionQuality: 50
      maxTextureSize: 2048
      textureCompression: 0
      filterMode: 1
      generateMipMaps: 1
      readable: 0
      crunchedCompression: 0
      sRGB: 1
    platformSettings:
    - serializedVersion: 3
      m_BuildTarget: iPhone
      m_MaxTextureSize: 2048
      m_ResizeAlgorithm: 0
      m_TextureFormat: 48
      m_TextureCompression: 1
      m_CompressionQuality: 50
      m_CrunchedCompression: 0
      m_AllowsAlphaSplitting: 0
      m_Overridden: 1
      m_AndroidETC2FallbackOverride: 0
      m_ForceMaximumCompressionQuality_BC6H_BC7: 0
    - serializedVersion: 3
      m_BuildTarget: Android
      m_MaxTextureSize: 2048
      m_ResizeAlgorithm: 0
      m_TextureFormat: 47
      m_TextureCompression: 1
      m_CompressionQuality: 50
      m_CrunchedCompression: 0
      m_AllowsAlphaSplitting: 0
      m_Overridden: 1
      m_AndroidETC2FallbackOverride: 0
      m_ForceMaximumCompressionQuality_BC6H_BC7: 0
    packingSettings:
      serializedVersion: 2
      padding: 4
      blockOffset: 1
      allowAlphaSplitting: 0
      enableRotation: 0
      enableTightPacking: 0
    secondaryTextureSettings: {}
    variantMultiplier: 1
    packables: []
    bindAsDefault: 1
    isAtlasV2: 0
    cachedData: {fileID: 0}
  m_MasterAtlas: {fileID: 0}
  m_PackedSprites: []
  m_PackedSpriteNamesToIndex: []
  m_RenderDataMap: {}
  m_Tag: Sprites
  m_IsVariant: 0
";
        AssetDatabase.Refresh();
        string atlasName = "Sprites.spriteatlas";

        if (!Directory.Exists(sptDesDir))
        {
            Directory.CreateDirectory(sptDesDir);
            AssetDatabase.Refresh();
        }
        string filePath = sptDesDir + "/" + atlasName;
        Debug.Log($"CreateAtlas: {filePath}");

        if (!File.Exists(filePath))
        {
            FileStream fs = new FileStream(filePath, FileMode.CreateNew);
            byte[] bytes = new UTF8Encoding().GetBytes(yaml);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
            AssetDatabase.Refresh();
        }

    }
    static void AddPackAtlas(SpriteAtlas atlas, Object[] spt)
    {
        atlas.Add(spt);
        SpriteAtlasUtility.PackAtlases(new[] { atlas }, EditorUserBuildSettings.activeBuildTarget);
    }
}