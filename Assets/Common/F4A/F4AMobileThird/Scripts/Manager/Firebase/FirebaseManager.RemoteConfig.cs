namespace com.F4A.MobileThird
{
#if DEFINE_FIREBASE_REMOTECONFIG
    using Firebase.Extensions;
    using Firebase.RemoteConfig;
#endif
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using DataCore;

    public enum ERemoteKey
    {
        ConfigScores,
    }


    public partial class FirebaseManager
    {
        private Dictionary<string, object> _remoteConfigs = new Dictionary<string, object>();
        public Dictionary<string, object> RemoteConfigs
        {
            get { return _remoteConfigs; }
        }

        public bool IsGetRemoteConfigs { get; set; }

        private void InitializeRemote()
        {
#if DEFINE_FIREBASE_REMOTECONFIG
            SetDefaultRemoteConfig();
            FetchDataAsync();
#endif
        }

        void SetDefaultRemoteConfig()
        {
            System.Collections.Generic.Dictionary<string, object> defaults =
                new System.Collections.Generic.Dictionary<string, object>();

            // These are the values that are used if we haven't fetched data from the
            // server
            // yet, or if we ask for values that the server doesn't have:
            defaults.Add("Continuously3Puzzles", ConfigManager.Continuously3Puzzles);
            defaults.Add("CoinGiftIapSuccess", ConfigManager.CoinGiftLoginSuccess);
            defaults.Add("CoinDailyRewardGift", ConfigManager.InksDailyRewardGift);
            defaults.Add(ConfigManager.KeyTimeDelayShowHandTutorial, ConfigManager.TimeDelayShowHandTutorial);

            defaults.Add(ConfigManager.keyIosBackgroundAd, false);
            defaults.Add(ConfigManager.keyIosBetweenGamesAd, 2);
            defaults.Add(ConfigManager.keyIosDoubleDailyAd, false);
            defaults.Add(ConfigManager.keyIosDoubleLuckyAd, false);
            defaults.Add(ConfigManager.keyIosLoadingAd, false);

            defaults.Add(ConfigManager.keyAndroidBackgroundAd, false);
            defaults.Add(ConfigManager.keyAndroidBetweenGamesAd, 1);
            defaults.Add(ConfigManager.keyAndroidDoubleDailyAd, false);
            defaults.Add(ConfigManager.keyAndroidDoubleLuckyAd, false);
            defaults.Add(ConfigManager.keyAndroidLoadingAd, false);

            defaults.Add(ConfigManager.EnablePreRegistrationGift, false);
            defaults.Add(ConfigManager.InkPreRegistrationGift, 200);
            defaults.Add(ConfigManager.InkPreRegistrationGiftEndTime, 200);
            defaults.Add(ConfigManager.TimeBeginPreRegistrationGift, "20211108");
            defaults.Add(ConfigManager.TimeEndPreRegistrationGift, "20211130");
            defaults.Add(ConfigManager.TimePlayPuzzleShowIntertitialAds, 3);
            defaults.Add(ConfigManager.KeyGetExtraReward, ConfigManager.GetExtraRewardValue);
            defaults.Add(ConfigManager.KeyGetExtraRewardArtBlitz, ConfigManager.GetExtraRewardArtBlitzValue);
            defaults.Add(ConfigManager.KeyGetExtraRewardTutorial, 50);
            FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);
            DataCore.Debug.Log("Remote config ready!");
        }

        public static string GetValueRemote(string key)
        {
#if DEFINE_FIREBASE_REMOTECONFIG
            if (FirebaseRemoteConfig.DefaultInstance == null) return string.Empty;
            var str = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
            var strCache = PlayerPrefs.GetString(key, "0");

            if (string.IsNullOrEmpty(str))
            {
                if (string.IsNullOrEmpty(strCache) || string.Compare(strCache, "0") == 0)
                {
                    return strCache;
                }
            }
            else
            {
                if (string.Compare(str, strCache) != 0)
                {
                    PlayerPrefs.SetString(key, str);
                    PlayerPrefs.Save();
                    return str;
                }
            }
            return str;
#endif
            return string.Empty;
        }

        public static bool GetValueBoolRemote(string key)
        {
#if DEFINE_FIREBASE_REMOTECONFIG
            var isFlag = FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
            return isFlag;
#endif
            return false;
        }

#if DEFINE_FIREBASE_REMOTECONFIG
        public Task FetchDataAsync()
        {
            DataCore.Debug.Log("====> @LOG FetchDataAsync");
            // FetchAsync only fetches new data if the current data is older than the provided
            // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
            // By default the timespan is 12 hours, and for production apps, this is a good
            // number.  For this example though, it's set to a timespan of zero, so that
            // changes in the console will always show up immediately.
            Task fetchTask = FirebaseRemoteConfig.DefaultInstance.FetchAsync(System.TimeSpan.Zero);
            return fetchTask.ContinueWithOnMainThread(FetchComplete);
        }
#endif

        void FetchComplete(Task fetchTask)
        {
            if (fetchTask.IsCanceled)
            {
                DataCore.Debug.Log("Fetch canceled.");
            }
            else if (fetchTask.IsFaulted)
            {
                DataCore.Debug.Log("Fetch encountered an error.");
            }
            else if (fetchTask.IsCompleted)
            {
                DataCore.Debug.Log("Fetch completed successfully!");
            }

            var info = FirebaseRemoteConfig.DefaultInstance.Info;
            switch (info.LastFetchStatus)
            {
                case LastFetchStatus.Success:
                    FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync();
                    DataCore.Debug.Log(String.Format("Remote data loaded and ready (last fetch time {0}).",
                        info.FetchTime));
                    break;
                case LastFetchStatus.Failure:
                    switch (info.LastFetchFailureReason)
                    {
                        case FetchFailureReason.Error:
                            DataCore.Debug.Log("Fetch failed for unknown reason");
                            break;
                        case FetchFailureReason.Throttled:
                            DataCore.Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                            break;
                    }
                    break;
                case LastFetchStatus.Pending:
                    DataCore.Debug.Log("Latest Fetch call still pending.");
                    break;
            }
        }

        public void MergeAllKeys()
        {
#if DEFINE_FIREBASE_REMOTECONFIG
            IEnumerable<string> keys = FirebaseRemoteConfig.DefaultInstance.Keys;
            foreach (string key in keys)
            {
                try
                {
                    if (RemoteConfigs.ContainsKey(key))
                    {
                        if (RemoteConfigs[key] is string)
                        {
                            RemoteConfigs[key] = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
                        }
                        else if (RemoteConfigs[key] is float)
                        {
                            RemoteConfigs[key] = (float)FirebaseRemoteConfig.DefaultInstance.GetValue(key).DoubleValue;
                        }
                        else if (RemoteConfigs[key] is int)
                        {
                            RemoteConfigs[key] = (int)FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
                        }
                        else if (RemoteConfigs[key] is bool)
                        {
                            RemoteConfigs[key] = FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
                        }
                    }
                    else
                    {
                        RemoteConfigs[key] = FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
                    }
                    DataCore.Debug.Log("====>@LOG RemoteConfigs key: " + key + "/value:" + RemoteConfigs[key].ToString());
                }
                catch (Exception)
                {
                    DataCore.Debug.Log("====> @LOG Invalid key: " + key);
                }
            }

            if (RemoteConfigs != null) DataCore.Debug.Log("====> @LOG RemoteConfigs Lenght:" + RemoteConfigs.Count);
            IsGetRemoteConfigs = true;
#endif
        }
    }
}