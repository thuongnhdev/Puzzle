namespace com.F4A.MobileThird
{
    using DataCore;
#if DEFINE_FIREBASE_ANALYTIC
    using Firebase.Analytics;
#endif
    using System;
    using System.Collections.Generic;
    //using System.Threading.Tasks;
    using UnityEngine;

    public partial class FirebaseManager
    {
        private bool _didInitial = false;
        protected void InitializeAnalytics()
        {
#if DEFINE_FIREBASE_ANALYTIC
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

            UnityEngine.Debug.Log("Set user properties.");
            // Set the user ID.
            FirebaseAnalytics.SetUserId(SystemInfo.deviceUniqueIdentifier);
            // Set default session duration values.
            //FirebaseAnalytics.SetMinimumSessionDuration(new TimeSpan(0, 0, 10));
            FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));
            _didInitial = true;
            UpdateFirebaseUserProperties();
#endif
        }

        public void UpdateFirebaseUserProperties()
        {
            if (!_didInitial) return;
            try
            {
                if (!CPlayerPrefs.HasKey(ConfigManager.GameData.install_version))
                {
                    CPlayerPrefs.SetString(ConfigManager.GameData.install_version, Application.version);
                    CPlayerPrefs.SetString(ConfigManager.GameData.install_date, DateTime.Now.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture));
                    CPlayerPrefs.Save();
                }
                var installVersion = CPlayerPrefs.GetString(ConfigManager.GameData.install_version, string.Empty);
                if (!string.IsNullOrEmpty(installVersion))
                {
#if UNITY_IOS
                    Firebase.Analytics.FirebaseAnalytics.SetUserProperty(ConfigManager.GameData.install_version, "ios_" + Application.version);
#elif UNITY_ANDROID
                    Firebase.Analytics.FirebaseAnalytics.SetUserProperty(ConfigManager.GameData.install_version, "android_" + Application.version);
#endif
                }

                Application.RequestAdvertisingIdentifierAsync(
                    (string advertisingId, bool trackingEnabled, string error) =>
                    {
                        UnityEngine.Debug.LogError("advertisingId " + advertisingId + " " + trackingEnabled + " " + error);
                        if (advertisingId != null && advertisingId.Length > 0 && trackingEnabled)
                        {
                            FirebaseAnalytics.SetUserProperty("advertising_id", advertisingId);
                        }
                    }
                );

                FirebaseAnalytics.SetUserProperty("device_unique_id", SystemInfo.deviceUniqueIdentifier);
                var adjustId = CPlayerPrefs.GetString(ConfigManager.GameData.adjustId, string.Empty);
                if (!string.IsNullOrEmpty(adjustId))
                {
                    FirebaseAnalytics.SetUserProperty(ConfigManager.GameData.adjustId, adjustId);
                }
                bool didSetABGroup = CPlayerPrefs.GetBool("did_set_ab_group", false);
                if (!didSetABGroup)
                {
                    var maxGroups = 10;
                    var group = UnityEngine.Random.Range(1, maxGroups + 1);
                    FirebaseAnalytics.SetUserProperty("ab_group", group.ToString());
                    CPlayerPrefs.SetBool("did_set_ab_group", true);
                    CPlayerPrefs.Save();
                }
                var loginType = GameData.Instance.GetLoginType();
                if (!string.IsNullOrEmpty(loginType))
                {
                    FirebaseAnalytics.SetUserProperty("login_type", loginType);
                }
                else {
                    FirebaseAnalytics.SetUserProperty("login_type", null);
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to add user properties. Error: {e.Message}");
            }
        }

        public void LogEvent(string name, string parameterName, long parameterValue)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            FirebaseAnalytics.LogEvent(name, parameterName, parameterValue);
#endif
        }
        public void LogEvent(string name, string parameterName, int parameterValue)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            FirebaseAnalytics.LogEvent(name, parameterName, parameterValue);
#endif
        }
        public void LogEvent(string name)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            FirebaseAnalytics.LogEvent(name);
#endif
        }
        public void LogEvent(string name, string parameterName, string parameterValue)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            FirebaseAnalytics.LogEvent(name, parameterName, parameterValue);
#endif
        }
        public void LogEvent(string name, string parameterName, double parameterValue)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            FirebaseAnalytics.LogEvent(name, parameterName, parameterValue);
#endif
        }

        public void LogEvent(string nameEvent, Dictionary<string, string> values)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            List<Parameter> parameters = new List<Parameter>();
            foreach (var pair in values)
            {
                parameters.Add(new Parameter(pair.Key, pair.Value.ToString()));
            }


            FirebaseAnalytics.LogEvent(nameEvent, parameters.ToArray());
#endif
        }

        public void LogEvent(string nameEvent, Dictionary<string, object> values)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            foreach (var pair in values)
            {
                if (pair.Value != null) parameters[pair.Key] = pair.Value.ToString();
                else parameters[pair.Key] = string.Empty;
            }
            LogEvent(nameEvent, parameters);
#endif
        }

        public void LogEvent(string nameEvent, List<Parameter> values) {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            FirebaseAnalytics.LogEvent(nameEvent, values.ToArray());
#endif
        }


        public void LogEventWithInfoCountry(string name, Dictionary<string, object> values)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            List<Parameter> parameters = new List<Parameter>();
            foreach (var pair in values)
            {
                parameters.Add(new Parameter(pair.Key, pair.Value.ToString()));
            }

            FirebaseAnalytics.LogEvent(name, parameters.ToArray());
#endif
        }


        public void AnalyticsLogin()
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            // Log an event with no parameters.
            UnityEngine.Debug.Log("Logging a login event.");
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
#endif
        }

        public void AnalyticsAppOpen()
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            // Log an event with no parameters.
            UnityEngine.Debug.Log("Logging a login event.");
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAppOpen);
#endif
        }

        public void AnalyticsLevelStart(int level)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            // Log an event with no parameters.
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelStart, "level", level);
#endif
        }

        public void AnalyticsLevelEnd(int level)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            // Log an event with no parameters.
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelEnd, "level", level);
#endif
        }

        public void AnalyticOpenScene(string sceneName)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            FirebaseAnalytics.LogEvent("OpenScene", "scene_name", sceneName);
#endif
        }

        public void AnalyticsScore(int score)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            // Log an event with an int parameter.
            UnityEngine.Debug.Log("Logging a post-score event.");
            FirebaseAnalytics.LogEvent(
              FirebaseAnalytics.EventPostScore,
              FirebaseAnalytics.ParameterScore,
              score);
#endif
        }

        public void AnalyticsLevelUp(int level, string parameterCharacter)
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            // Log an event with multiple parameters.
            UnityEngine.Debug.Log("Logging a level up event.");
            FirebaseAnalytics.LogEvent(
              FirebaseAnalytics.EventLevelUp,
              new Parameter(FirebaseAnalytics.ParameterLevel, level),
              new Parameter(FirebaseAnalytics.ParameterCharacter, parameterCharacter));
#endif
        }

        // Reset analytics data for this app instance.
        public void ResetAnalyticsData()
        {
            if (!_didInitial) return;
#if DEFINE_FIREBASE_ANALYTIC
            UnityEngine.Debug.Log("Reset analytics data.");
            FirebaseAnalytics.ResetAnalyticsData();
#endif
        }

    }
}