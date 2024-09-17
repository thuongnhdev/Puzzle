#define SAVE_BY_BINARY

using UnityEngine;
using System;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using com.F4A.MobileThird;
using Newtonsoft.Json;
using System.Collections;
using System.Globalization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DataCore
{
    public class GameData
    {
        #region singleton pattern
        public static GameData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameData(false);
                }
                return _instance;
            }
        }

        public static event Action<bool> OnMusicMute = delegate { };

        private static GameData _instance;

        public PlayerSavePack SavedPack
        {
            get
            {
                return savedPack;
            }
        }

        private PlayerSavePack savedPack;

        private bool isDirty;
        private bool isRequestSave;
        private bool useThreadSave;
        private bool isThreadSaving;

        public enum DailyRewardType
        {
            RESET,
            DISABLE,
            NORMAL
        }

        public enum LoadType
        {
            LOCAL,
            ONLINE,
            NONE
        }
        public GameData(bool useThreadSave = true)
        {
            _instance = this;

            this.useThreadSave = useThreadSave;
            this.isThreadSaving = false;
            this.isDirty = false;
            this.isRequestSave = false;
            ThreadPool.SetMaxThreads(1, 1);
        }

        ~GameData()
        {
#if TRANSFER_DATA
            apiManager.OnRequestUserInforSuccess -= OnRequestUserInforSuccess;
            apiManager.OnRequestUserInforFailed -= OnRequestUserInforFailed;
            apiManager.OnUpdateUserInforSuccess -= OnUpdateUserInforSuccess;
            apiManager.OnUpdateUserInforFailed -= OnUpdateUserInforFailed;
#endif
        }


        #endregion
        public void saveGameData()
        {
            if (isThreadSaving)
            {
                isDirty = true;
                return;
            }

#if IGNORE_SAVEGAME
            isDirty = false;
            isRequestSave = false;
            return;
#endif

            if (useThreadSave)
                ThreadPool.QueueUserWorkItem(ThreadSave);
            else
                ThreadSave(null);

            isRequestSave = false;
        }

        public void LoadGameData(LoadType type)
        {
            switch (type)
            {
                case LoadType.LOCAL:
                    LoadLocalConfig();
                    break;
                case LoadType.ONLINE:
                    var idUser = GameData.Instance.GetUserId();
                    if (IsUserLogin())
                        LoadOnlineConfig(idUser);
                    else
                        LoadLocalConfig();
                    break;
            }
        }

        public bool IsUserLogin() {
            var idUser = GameData.Instance.GetUserId();
            return !string.IsNullOrEmpty(idUser);
        }

        private void LoadLocalConfig()
        {
            savedPack = LoadLocalDataFromFile();
            if (savedPack == null) {
                savedPack = new PlayerSavePack();
            }                        
            savedPack.ConvertDataUpdateCollection();
        }


        private PlayerSavePack LoadLocalDataFromFile()
        {
            if (DataManager.Exit(ConfigManager.FileNameGameData))
            {
                try
                {
                    var data = DataManager.Load<string>(ConfigManager.FileNameGameData);
                    savedPack = JsonUtility.FromJson<PlayerSavePack>(data);
                    return savedPack;
                }
                catch (Exception ex)
                {
                    DataCore.Debug.Log($"Failed LoadLocalConfig. Error: {ex.Message}");
                }
            }
            return null;
        }

        public void LoadOnlineConfig(string idUser, bool forced = false, Action completed = null)
        {
            if (string.IsNullOrEmpty(idUser)) return;
            FirebaseManager.Instance.GetUserData(ConfigManager.FileNameGameData, idUser, (ok, data) =>
            {
                if (ok)
                {
                    try
                    {
                        if (data != null)
                        {
                           
                            var newSavePack = JsonUtility.FromJson<PlayerSavePack>(data.ToString());
                            if (forced)
                            {
                                savedPack = newSavePack;
                                savedPack.ConvertDataUpdateCollection();
                            }
                            else {
                                if (newSavePack.VERSION > savedPack.VERSION) {
                                    savedPack = newSavePack;
                                    savedPack.ConvertDataUpdateCollection();
                                }
                            }
                            PlayerPrefs.SetInt(ConfigManager.LoginFirstSuccess, 1);
                            PlayerPrefs.Save();

                            DataCore.Debug.Log($"LoadOnlineConfig: {data}", false);

                            if (GameSession.Instance.IsCompletedFirstSessionLoading)
                            {
                                Dispatcher.Instance.Invoke(() =>
                                {
                                    UIManager.Instance.UIHomepage.gameObject.SetActive(false);
                                    UIManager.Instance.UIHomepage.gameObject.SetActive(true);
                                    UIManager.Instance.UIHomepage.ResetUI();

                                    ShareUIManager.Instance.UpdateCurrencyData(0.0f);
                                });
                            }

                            completed?.Invoke();

                            CPlayerPrefs.SetString(ConfigManager.TimeSyncWithCloud, DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                            CPlayerPrefs.Save();
                            RequestSaveGame();
                        }
                        else {
                            ThreadSaveOnline();
                            completed?.Invoke();
                        }

                    }
                    catch (Exception ex)
                    {
                        DataCore.Debug.Log($"Failed LoadOnlineConfig. Error: {ex.Message}");
                        completed?.Invoke();
                    }
                }
                else {

                    completed?.Invoke();
                }

            });
        }

        public void MigrateIAPAndPremium() {
            var didBuyVIP = CPlayerPrefs.GetInt(ConfigManager.keyVipIap, 0) == 1;
            var didBuyRemoveAds = CPlayerPrefs.GetInt(ConfigManager.keyVipIapAds, 0) == 1;
            if (didBuyRemoveAds) {
                savedPack.SaveData.DidRemoveAd = true;
            }
            if (didBuyVIP)
            {
                savedPack.SaveData.DidVIP = true;
            }
            RequestSaveGame();
        }

        public void OnMarkDirty()
        {
            isDirty = true;
        }

        public void SaveIfDirty()
        {
            if (isDirty)
            {
                isRequestSave = true;
            }
        }

        public void SyncUserProfileOnApplicationInBackground(bool shouldSaveData = true)
        {

            if (shouldSaveData)
            {
                DataCore.Debug.Log($"SyncUserProfileOnApplicationInBackground. isDirty: {isDirty}");
                if (isDirty)
                {
                    saveGameData();
                    ThreadSaveOnline();
                    isDirty = false;
                }
            }
        }

        public void OnApplicationQuit()
        {
            SyncUserProfileOnApplicationInBackground();
        }
        public void OnLateUpdate()
        {
            if (isRequestSave)
            {
                saveGameData();
            }
        }
        public void LogOut()
        {
            PlayerPrefs.SetString(ConfigManager.IdUser, String.Empty);
            PlayerPrefs.SetString(ConfigManager.LoginType, String.Empty);
            PlayerPrefs.Save();
        }
        public void SetUserId(string userID)
        {
            if (!string.IsNullOrEmpty(userID))
                PlayerPrefs.SetString(ConfigManager.IdUser, userID);
            PlayerPrefs.Save();

        }

        public void SetLoginType(string type) {
            if (!string.IsNullOrEmpty(type))
                PlayerPrefs.SetString(ConfigManager.LoginType, type);
            PlayerPrefs.Save();
        }

        public string GetUserId()
        {
            var idUser = PlayerPrefs.GetString(ConfigManager.IdUser, String.Empty);
            return idUser;
        }

        public string GetLoginType() {
            var LoginType = PlayerPrefs.GetString(ConfigManager.LoginType, String.Empty);
            return LoginType;
        }

        void ThreadSave(object stateInfo)
        {
            if (savedPack == null) return;
            isThreadSaving = true;
            savedPack.VERSION = DateTime.Now.Ticks;
            savedPack.DataCleanCollectionPuzzlePlayings();
            var dataSave = JsonUtility.ToJson(savedPack);
            if (string.IsNullOrEmpty(dataSave)) return;
            DataManager.Save(ConfigManager.FileNameGameData, dataSave);
            isThreadSaving = false;
            PlayerPrefs.SetString(ConfigManager.VersionConfig, savedPack.VERSION.ToString());
            PlayerPrefs.Save();
        }

        void ThreadSaveOnline()
        {
            //Check if the device can reach the internet via a carrier data network
            if (!SocialManager.Instance.isConnectionNetwork())
                return;
            // save on firebase
            var idUser = GameData.Instance.GetUserId();
            DataCore.Debug.Log($"ThreadSaveOnline idUser: {idUser}", false);
            if (!string.IsNullOrEmpty(idUser))
            {
                var nameDataBase = ConfigManager.FileNameGameData;
                PlayerSavePack playerSavePack = savedPack;
                playerSavePack.SaveData.IsTutorialCompleted = true;
                var dataSave = JsonUtility.ToJson(playerSavePack);
                FirebaseManager.Instance.SaveDatabase(nameDataBase, dataSave);
            }
        }

        public void EventSettingSound(bool isMute)
        {
            OnMusicMute?.Invoke(isMute);
        }

        public void RequestSaveGame()
        {
            isRequestSave = true;
            OnMarkDirty();
        }
        public int PlayedGame() {
            return savedPack.SaveData.playedPuzzle;
        }
        public void IncreasePlayedGame()
        {
            savedPack.SaveData.playedPuzzle += 1;
            if (savedPack.SaveData.playedPuzzle < 3)
            {
                var event_name = $"{ConfigManager.TrackingEvent.EventName.completed_puzzle}_{savedPack.SaveData.playedPuzzle}";
                AnalyticManager.Instance.LogEvent(event_name);
            }
            else if (savedPack.SaveData.playedPuzzle == 3)
            {
                AnalyticManager.Instance.TrackPlayedThreePuzzle();
            }
            else if (savedPack.SaveData.playedPuzzle == 5)
            {
                AnalyticManager.Instance.TrackPlayedFivePuzzle();
            }
            else if (savedPack.SaveData.playedPuzzle == 7)
            {
                AnalyticManager.Instance.TrackPlayedSevenPuzzle();
            }
            RequestSaveGame();
        }
        public void IncreaseCompletedRewardedAd()
        {
            savedPack.SaveData.completedRewardedAd += 1;
            if (savedPack.SaveData.completedRewardedAd == 3)
            {
                AnalyticManager.Instance.TrackCompletedThreeRewardedAd();
            }
            else if (savedPack.SaveData.completedRewardedAd == 5)
            {
                AnalyticManager.Instance.TrackCompletedFiveRewardedAd();
            }
            else if (savedPack.SaveData.completedRewardedAd == 7)
            {
                AnalyticManager.Instance.TrackCompletedThreeRewardedAd();
            }
            RequestSaveGame();
        }
        public void IncreaseShownIntersitialAd()
        {
            savedPack.SaveData.shownInterstitial += 1;
            if (savedPack.SaveData.shownInterstitial == 3)
            {
                AnalyticManager.Instance.TrackShownThreeIntersitialAd();
            }
            else if (savedPack.SaveData.shownInterstitial == 5)
            {
                AnalyticManager.Instance.TrackShownFiveIntersitialAd();
            }
            else if (savedPack.SaveData.shownInterstitial == 7)
            {
                AnalyticManager.Instance.TrackShownSevenIntersitialAd();
            }
            RequestSaveGame();
        }

        public void IncreaseInks(int amount, string placement)
        {
            SavedPack.SaveData.Coin += amount;
            if (amount > 0)
            {
                AnalyticManager.Instance.TrackResourceEarn(placement, ConfigManager.GameData.ResourceType.ink, amount);
            }
            RequestSaveGame();
        }

        public void DecreaseInks(int amount, string placement)
        {
            SavedPack.SaveData.Coin -= amount;
            if (SavedPack.SaveData.Coin < 0)
                SavedPack.SaveData.Coin = 0;
            RequestSaveGame();
            if (amount > 0)
            {
                AnalyticManager.Instance.TrackResourceSpent(placement, ConfigManager.GameData.ResourceType.ink, amount);
            }

        }

        public void IncreaseHint(int amount, string placement)
        {
            SavedPack.SaveData.Hint += amount;
            if (amount > 0)
            {
                AnalyticManager.Instance.TrackResourceEarn(placement, ConfigManager.GameData.ResourceType.hint, amount);
            }

            RequestSaveGame();
        }

        public void UpdateHint(int amount)
        {
            SavedPack.SaveData.Hint = amount;
            RequestSaveGame();
        }

        public void DecreaseHint(int amount, string placement)
        {
            if (amount > 0)
            {
                AnalyticManager.Instance.TrackResourceSpent(placement, ConfigManager.GameData.ResourceType.hint, amount);
            }
            if (!GameData.Instance.IsVipIap())
            {
                SavedPack.SaveData.Hint -= amount;
                RequestSaveGame();
            }

        }

        public void AddRemovedNewChapter(int id)
        {
            if (savedPack.SaveData.NewChapterRemoved == null)
            {
                savedPack.SaveData.NewChapterRemoved = new List<int>();
            }

            savedPack.SaveData.NewChapterRemoved.Add(id);
            RequestSaveGame();
        }

        public bool IsVipIap()
        {
            return CPlayerPrefs.GetInt(ConfigManager.keyVipIapV2, 0) == 1 || SavedPack.SaveData.DidVIP;
        }

        public void UpdateBuyPremium() {
            savedPack.SaveData.DidRemoveAd = true;
            savedPack.SaveData.DidVIP = true;
            RequestSaveGame();
        }
        public void UpdateRemoveAd() {
            savedPack.SaveData.DidRemoveAd = true;            
            RequestSaveGame();
        }

        public DailyRewardType IsResetDailyReward()
        {
            var timeClaim = CPlayerPrefs.GetString(ConfigManager.TimeDailyRewardClaim, String.Empty);
            if (string.Compare(timeClaim, String.Empty) == 0)
                return DailyRewardType.NORMAL;

            DateTime oDate = DateTime.ParseExact(timeClaim, "yyyyMMdd", CultureInfo.InvariantCulture);
            System.TimeSpan diff1 = DateTime.Now.Subtract(oDate);
            if (diff1.TotalDays > ConfigManager.DayDailyReward)
                return DailyRewardType.RESET;
            else if (diff1.TotalDays < 0)
                return DailyRewardType.DISABLE;
            return DailyRewardType.NORMAL;
        }





#if UNITY_EDITOR
        [MenuItem("HelpFunction/ClearSave")]
        public static void DeleteFile()
        {
            CPlayerPrefs.DeleteAll();
            PlayerPrefs.DeleteAll();
            // check if file exists
            if (!DataManager.Exit(ConfigManager.FileNameGameData))
            {
                DataCore.Debug.Log("File not exist");
            }
            else
            {

                DataManager.DeleteAllData();

            }
        }

        //        [MenuItem("HelpFunction/AddCoin")]
        //        public static void AddCoin()
        //        {
        //            GameData.Instance.IncreaseInks(10000);
        //            ShareUIManager.Instance.UpdateCurrencyData(0.0f);
        //        }
#endif
    }
}