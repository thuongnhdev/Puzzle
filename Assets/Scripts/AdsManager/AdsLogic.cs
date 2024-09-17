using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.F4A.MobileThird;
using DataCore;
using System;
using System.Globalization;

namespace com.F4A.MobileThird
{
    public static class AdsLogic
    {
        public static void SetTimeBegin()
        {
            var timeCountDown = CPlayerPrefs.GetString(ConfigManager.TimeLoadAds2Day, "0");
            if (string.IsNullOrEmpty(timeCountDown) || string.Compare(timeCountDown, "0") == 0)
            {
                var timeCurrent = System.DateTime.Now.ToString("yyyyMMdd");
                CPlayerPrefs.SetString(ConfigManager.TimeLoadAds2Day, timeCurrent);
                CPlayerPrefs.Save();
            }

        }
        public static bool Is2DayInstallAds()
        {
#if UNITY_EDITOR
            return true;
#endif
            var timeBegin = CPlayerPrefs.GetString(ConfigManager.TimeLoadAds2Day, "0");
            DateTime oDate = DateTime.ParseExact(timeBegin, "yyyyMMdd", CultureInfo.InvariantCulture);
            System.TimeSpan diff1 = DateTime.Now.Subtract(oDate);
            if (diff1.TotalDays > ConfigManager.TimeShowIntConfig)
                return true;
            return false;
        }

        public static void SetTimeBeginArtBlitzComplete()
        {
            var timeCurrent = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            PlayerPrefs.SetString(ConfigManager.TimeShowintArtBlitz, timeCurrent);
            PlayerPrefs.Save();
        }

        public static bool IsTimeIntAdsArtBlitz()
        {
            var timeBegin = PlayerPrefs.GetString(ConfigManager.TimeShowintArtBlitz, "0");
            if (string.Compare(timeBegin, "0") == 0) return false;
            DateTime oDate = DateTime.ParseExact(timeBegin, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            System.TimeSpan diff1 = DateTime.Now.Subtract(oDate);
            if (diff1.TotalMinutes >= ConfigManager.TimeShowIntArtBlitzConfig)
                return true;
            return false;
        }

        public static bool IsPlayPuzzleIntertitialAds()
        {
            var playCount = GameData.Instance.PlayedGame();
            var xPlayRemote = FirebaseManager.GetValueRemote(ConfigManager.TimePlayPuzzleShowIntertitialAds);
            if (string.IsNullOrEmpty(xPlayRemote))
                xPlayRemote = ConfigManager.keyMaxPuzzlePlayShowIntertitialAds;
            if (playCount == 0 || string.IsNullOrEmpty(xPlayRemote))
                return false;

            if (playCount >= int.Parse(xPlayRemote))
                return true;
            return false;
        }

        public static bool Is3PuzzleShowINTAds()
        {
            var countUserPlayer = CPlayerPrefs.GetInt(ConfigManager.UserPlaysContinuously3Puzzles, 0);
            return countUserPlayer == ConfigManager.Continuously3Puzzles ? true : false;
        }
        public static void SetContinuously3Puzzles()
        {
            var countUserPlayer = CPlayerPrefs.GetInt(ConfigManager.UserPlaysContinuously3Puzzles, 0);
            if (countUserPlayer == ConfigManager.Continuously3Puzzles)
                countUserPlayer = 0;
            else
                countUserPlayer++;
            CPlayerPrefs.SetInt(ConfigManager.UserPlaysContinuously3Puzzles, countUserPlayer);
            CPlayerPrefs.Save();
        }

        public static bool IsBannerAds()
        {            
            return GameData.Instance.SavedPack.SaveData.IsTutorialCompleted ? true : false;
        }

        public static bool IsVipAds()
        {
            return CPlayerPrefs.GetInt(ConfigManager.keyVipIapAdsV2, 0) ==  1 || GameData.Instance.SavedPack.SaveData.DidRemoveAd;
        }

        public static bool IsLuckyRewardAds()
        {
            var data = CPlayerPrefs.GetString(ConfigManager.KeyMaxShowAdsLuckyInDay, "");
            string[] xData = data.Split('_');
            if (xData.Length == 0)
            {
                var dataCache = DateTime.Now.Day + "_" + 0;
                SetKeyMaxLucky(dataCache);
                xData = dataCache.Split('_');
            }
            if (int.Parse(xData[0]) == DateTime.Now.Day)
            {
                if (int.Parse(xData[1]) > ConfigManager.MaxShowAdsLuckyInDay)
                    return false;
                else
                {
                    var indexShow = int.Parse(xData[1]) + 1;
                    var dataCache = DateTime.Now.Day + "_" + indexShow;
                    SetKeyMaxLucky(dataCache);
                    return true;
                }
            }
            else
            {
                var dataCache = DateTime.Now.Day + "_" + 0;
                SetKeyMaxLucky(dataCache);
                return true;
            }
        }

        private static void SetKeyMaxLucky(string data)
        {
            CPlayerPrefs.SetString(ConfigManager.KeyMaxShowAdsLuckyInDay, data.ToString());
            CPlayerPrefs.Save();
        }
        public static bool IsTutorialComplete()
        {
            return GameData.Instance.SavedPack.SaveData.IsTutorialCompleted ? true : false;
        }
        public static void SetBannerType(int type)
        {
            CPlayerPrefs.SetInt(ConfigManager.BannerType, type);
            CPlayerPrefs.Save();
        }
        public static int BannerType()
        {
            var value = CPlayerPrefs.GetInt(ConfigManager.BannerType, 2);
            return value;
        }

        public static void SetTimeForeground()
        {
            var timeCurrent = System.DateTime.Now.ToString("yyyyMMddHHmmss");
            CPlayerPrefs.SetString(ConfigManager.KeyTimeForeground, timeCurrent);
            CPlayerPrefs.Save();
        }

        public static bool IsTimeForeground()
        {

            var timeForreg = CPlayerPrefs.GetString(ConfigManager.KeyTimeForeground, "0");
            if (string.IsNullOrEmpty(timeForreg) || timeForreg.CompareTo("0") == 0)
                return false;

            DateTime oDate = DateTime.ParseExact(timeForreg, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            System.TimeSpan diff1 = DateTime.Now.Subtract(oDate);
            if (diff1.TotalSeconds > ConfigManager.TimeForeground)
                return true;
            return false;
        }
    }
}