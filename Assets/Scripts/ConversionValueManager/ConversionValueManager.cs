namespace com.F4A.MobileThird
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using com.F4A.MobileThird;
    using DataCore;
    using System.Linq;

    public class ConversionValueManager : SingletonMono<ConversionValueManager>
    {
        private readonly string key_user_revenue = "user_revenue";
        private readonly string key_last_time_sent_conversion_value = "last_time_sent_conversion_value";
        private readonly string key_previous_conversion_value = "previous_conversion_value ";
        private float _revenue;
        private List<float> _conversionValues = new List<float>()
        {
            0.01f, 0.02f, 0.03f, 0.04f,
            0.05f, 0.06f, 0.095f, 0.13f,
            0.165f, 0.2f, 0.235f, 0.32f,
            0.405f, 0.49f, 0.575f, 0.66f,
            0.82f, 0.98f, 1.14f, 1.3f,
            1.46f, 1.72f, 1.98f, 2.24f,
            2.5f, 2.76f, 3.145f, 3.53f,
            3.915f, 4.3f, 4.685f, 5.22f,
            5.755f, 6.29f, 6.825f, 7.36f,
            8.095f, 8.83f, 9.565f, 10.3f,
            11.035f, 12.07f, 13.105f, 14.14f,
            15.175f, 16.21f, 17.745f, 19.28f,
            20.815f, 22.35f, 23.885f, 26.17f,
            28.455f, 30.74f, 33.025f, 35.31f,
            38.595f, 41.88f, 45.165f, 48.45f,
            51.735f, 56.52f, 61.305f, 66.09f,
        };

        public void Start()
        {
            if (!CPlayerPrefs.HasKey(key_user_revenue))
            {
                _revenue = 0.0f;
            }
            else
            {
                _revenue = CPlayerPrefs.GetFloat(key_user_revenue, 0.0f);
            }
        }

        public void OnApplicationQuit()
        {
            SaveUserRevenue();
            SendConversionValueIfNeeded();
        }
#if UNITY_ANDROID
        public void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (AdsService.Instance.IsShowingVideoAd()) return;
                SaveUserRevenue();
                SendConversionValueIfNeeded();
            }
        }
#else
        private void OnApplicationFocus(bool focus)
        {
            if (!focus) {
                if (AdsService.Instance.IsShowingVideoAd()) return;
                SaveUserRevenue();
                SendConversionValueIfNeeded();      
            }
        }
#endif

        public void SendConversionValueIfNeeded(bool forced = false)
        {
            var dateSinceDateInstall = GetDateSinceDateInstall();
            if (dateSinceDateInstall < 0) { return; }
            if (dateSinceDateInstall == 0)
            {
                SendConversionValue();
            }
            else
            {
                var timeBetween = GetTimeBetweenLastTimeSentConversionValue();
                if (timeBetween >= 24 || forced)
                {
                    SendConversionValue();
                }
            }
        }

        private void SendConversionValue()
        {
            try
            {
                var minValue = _conversionValues.First();
                var maxValue = _conversionValues.Last();

                if (_revenue >= minValue)
                {
                    var value = 0;
                    if (_revenue >= maxValue)
                    {
                        value = 63;
                    }
                    else
                    {
                        value = _conversionValues.FindIndex(a => a > _revenue);
                    }

                    var previousConversionValue = CPlayerPrefs.GetInt(key_previous_conversion_value, -1);
                    //DataCore.Debug.Log($"SendConversionValue revenue: {_revenue} value: {value} previousConversionValue {previousConversionValue}");

                    if (value > previousConversionValue && value > 0)
                    {
                        //DataCore.Debug.Log($"Send Conversion Value {value} for revenue: {_revenue}");
                        TenjinManager.Instance.UpdateConversionValue(value);
                        //AnalyticManager.Instance.TrackConversionValue(value);
                        CPlayerPrefs.SetString(key_last_time_sent_conversion_value, DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture));
                        CPlayerPrefs.SetInt(key_previous_conversion_value, value);
                        CPlayerPrefs.Save();
                        SaveUserRevenue();
                    }
                }
            }
            catch (Exception ex)
            {
                DataCore.Debug.Log($"Failed SendConversionValue. Error: {ex.Message}");
            }
        }

        private int GetTimeBetweenLastTimeSentConversionValue()
        {
            if (CPlayerPrefs.HasKey(key_last_time_sent_conversion_value))
            {
                var lastTimeSentConversionValueStr = CPlayerPrefs.GetString(key_last_time_sent_conversion_value, string.Empty);
                if (!string.IsNullOrEmpty(lastTimeSentConversionValueStr)) {
                    var lastTimeSentConversionValue = DateTime.ParseExact(lastTimeSentConversionValueStr, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    var timeBetween = (DateTime.Now - lastTimeSentConversionValue).TotalHours;
                    return (int)Math.Ceiling(timeBetween);
                }                
            }
            return -1;
        }

        private int GetDateSinceDateInstall()
        {
            if (CPlayerPrefs.HasKey(ConfigManager.GameData.install_date))
            {
                var installDateStr = CPlayerPrefs.GetString(ConfigManager.GameData.install_date);
                var installDate = DateTime.ParseExact(installDateStr, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                var today = DateTime.Now.Date;
                var dateSinceDateInstall = (today - installDate).TotalDays;
                return (int)Math.Ceiling(dateSinceDateInstall);
            }
            return -1;
        }

        private void SaveUserRevenue()
        {
            CPlayerPrefs.SetFloat(key_user_revenue, _revenue);
            CPlayerPrefs.Save();
        }

        public void AddRevenue(float revenue)
        {
            if (revenue <= 0) return;
            _revenue += revenue;
        }
    }
}