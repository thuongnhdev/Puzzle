#define SAVE_BY_BINARY

using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using DataCore;

namespace DataCore
{
    [System.Serializable]
    public class PuzzleAssetResource
    {
        public List<PuzzleItem> PuzzleItemList;

        public PuzzleAssetResource()
        {
            PuzzleItemList = new List<PuzzleItem>();
        }
    }
    [System.Serializable]
    public class PuzzleItem
    {
        public string AssetGUID;
        public SaveAssetResource saveAssetResource;

        public PuzzleItem(string assetGUID, SaveAssetResource assetResource)
        {
            this.AssetGUID = assetGUID;
            this.saveAssetResource = assetResource;
        }
    }
    [System.Serializable]
    public class SaveAssetResource
    {
        public UnityEngine.Object Asset;
        public SaveAssetResource(UnityEngine.Object asset)
        {
            this.Asset = asset;
        }
    }
    public class PuzzleDataSave
    {
        #region singleton pattern
        public static PuzzleDataSave Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PuzzleDataSave(false);
                }
                return _instance;
            }
        }

        private static PuzzleDataSave _instance;

        public PuzzleAssetResource PuzzleAssetResource
        {
            get
            {
                return puzzleAssetResource;
            }
        }

        private PuzzleAssetResource puzzleAssetResource;


        public PuzzleDataSave(bool useThreadSave = true)
        {
            _instance = this;
        }

        ~PuzzleDataSave()
        {
        }

        public void LoadGameData()
        {
            DataCore.Debug.Log("LoadPuzzleData");
            try
            {
                if (DataManager.Exit(ConfigManager.FileNamePuzzleData))
                {
                    var data = DataManager.Load<string>(ConfigManager.FileNamePuzzleData);
                    puzzleAssetResource = JsonUtility.FromJson<PuzzleAssetResource>(data);
                }
                else
                {
                    puzzleAssetResource = new PuzzleAssetResource();
                }
            }
            catch (Exception ex)
            {
                DataCore.Debug.Log($"Failed LoadPuzzleData. Error: {ex.Message}");
            }

        }

        public void SaveData(string assetGUID, UnityEngine.GameObject obj)
        {
            SaveAssetResource dataPuzzle = new SaveAssetResource(obj);
            dataPuzzle.Asset = obj;
            var item = new PuzzleItem(assetGUID, dataPuzzle);
            puzzleAssetResource.PuzzleItemList.Add(item);
            var dataSave = JsonUtility.ToJson(puzzleAssetResource);
            DataManager.Save(ConfigManager.FileNamePuzzleData, dataSave);
        }
        #endregion




#if UNITY_EDITOR
        [MenuItem("HelpFunction/ClearSavePuzzle")]
        public static void DeleteFile()
        {
            // check if file exists
            if (!DataManager.Exit(ConfigManager.FileNamePuzzleData))
            {
                DataCore.Debug.Log("File not exist");
            }
            else
            {

                DataManager.DeleteAllData();

            }
            PlayerPrefs.DeleteAll();
            CPlayerPrefs.DeleteAll();
        }
#endif
    }
}