using com.F4A.MobileThird;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DataCore
{
    public class PlayerData : SingletonMonoAwake<PlayerData>
    {

        [SerializeField] UserProfile m_UserProfile;

        bool isNewer = false;

        public UserProfile UserProfile { get => m_UserProfile; set => m_UserProfile = value; }
        public bool IsNewer { get => isNewer; set => isNewer = value; }

        public void Init(Action OnComplete)
        {
            if (!DataManager.Exit(ConfigManager.FileNameProfile))
            {
                isNewer = true;

                m_UserProfile = new UserProfile();
                DataManager.Save<object>(ConfigManager.FileNameProfile, m_UserProfile);
            }
            else// read and update file
            {
                object userData = DataManager.Load<object>(ConfigManager.FileNameProfile);
                DataManager.Save(ConfigManager.FileNameProfile, userData);
            }

            OnComplete?.Invoke();
        }
        public void SaveData()
        {
            var dataSave = JsonUtility.ToJson(m_UserProfile);
            DataManager.Save(ConfigManager.FileNameProfile, dataSave);
        }


#if UNITY_ANDROID
        private void OnApplicationPause(bool pause)
        {
            if (pause) {
                SaveData();
            }            
        }
#else
        private void OnApplicationFocus(bool focus)
        {
            if (!focus) {
                SaveData();
            }            
        }
#endif


        private void OnApplicationQuit()
        {
            SaveData();
        }
    }
    [System.Serializable]
    public class UserProfile
    {
        public int m_IndexLevelOld;
        public LevelInfo m_LevelOldInfo;
        public UserProfile()
        {
            m_IndexLevelOld = -1;
            m_LevelOldInfo = new LevelInfo();
        }
    }
    [System.Serializable]
    public class LevelInfo
    {
        public int m_IndexCurrentLayerDone;
        public List<int> m_LsIndexLayer;
        public List<string> m_LsLayerDone;
        public List<string> m_LsObjectDone;
        public LevelInfo()
        {
            m_IndexCurrentLayerDone = -1;
            m_LsIndexLayer = new List<int>();
            m_LsLayerDone = new List<string>();
            m_LsObjectDone = new List<string>();
        }
    }
}