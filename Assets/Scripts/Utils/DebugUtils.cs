using com.F4A.MobileThird;
using Firebase.Crashlytics;
using UnityEngine;

namespace DataCore
{
    public static class Debug
    {
        #region Private Methods

        private static string DebuggableString(object s)
        {
            //For debug purposes the array is checked to evidentiate null objects.
            return s != null ? s.ToString() : "null";
        }


        private static string[] DebuggableString(params object[] array)
        {
            string[] stringArray = new string[array.Length];
            for (int i = 0; i < array.Length; i++)
                stringArray[i] = DebuggableString(array[i]);
            return stringArray;
        }

        #endregion

        #region Log Methods
        public static void Log(string msgLog, bool logCrashlytic = true)
        {
            if (ConfigManager.Instance.IsLog)
            {
                var message = "Debug: " + msgLog;
                if (logCrashlytic || EnableDebugMode())
                {
                    UnityEngine.Debug.Log(message);
                }

                if (logCrashlytic) {
                    FirebaseManager.Instance.WriteCrashlyticsLog(message);
                }
                
            }

        }

        public static bool EnableDebugMode()
        {
#if UNITY_EDITOR
            return true;
#else
            return CPlayerPrefs.GetBool(ConfigManager.EnableDebugMode, false);
#endif
        }
        public static void LogFormat(string format, params object[] array)
        {
            if (ConfigManager.Instance.IsLogFormat)
            {
                if (EnableDebugMode())
                {
                    UnityEngine.Debug.LogFormat(format, array);
                }

                var message = "Debug: " + string.Format(format, array);
                FirebaseManager.Instance.WriteCrashlyticsLog(message);
            }
        }

        public static void LogInfo(params object[] array)
        {
            if (ConfigManager.Instance.IsLogInfor) {
                if (EnableDebugMode())
                {
                    UnityEngine.Debug.Log(StringUtils.Concat(DebuggableString(array)));
                }

                var message = "Info: " + StringUtils.Concat(DebuggableString(array));
                FirebaseManager.Instance.WriteCrashlyticsLog(message);
            }
        }

        public static void LogError(params object[] array)
        {
            if (ConfigManager.Instance.IsLogError)
            {
                if (EnableDebugMode())
                {
                    UnityEngine.Debug.Log(StringUtils.Concat(DebuggableString(array)));
                }

                var message = "Error: " + StringUtils.Concat(DebuggableString(array));
                FirebaseManager.Instance.WriteCrashlyticsLog(message);
            }
        }


        public static void LogError(Object context, params object[] array)
        {
            if (ConfigManager.Instance.IsLogError)
            {
                if (EnableDebugMode())
                {
                    UnityEngine.Debug.Log(StringUtils.Concat(DebuggableString(array)), context);
                }

                var message = "Error: " + StringUtils.Concat(DebuggableString(array));
                FirebaseManager.Instance.WriteCrashlyticsLog(message);
            }
        }

        public static void LogWarning(params object[] array)
        {
            if (ConfigManager.Instance.IsLogWarning)
                Debug.LogWarning(StringUtils.Concat(DebuggableString(array)));
        }


        public static void LogWarning(Object context, params object[] array)
        {
            if (ConfigManager.Instance.IsLogWarning)
                UnityEngine.Debug.LogWarning(StringUtils.Concat(DebuggableString(array)), context);
        }

        public static void LogException(System.Exception exception)
        {
            if (ConfigManager.Instance.IsLogException)
            {
                if (EnableDebugMode())
                {
                    UnityEngine.Debug.LogException(exception);
                }

                var message = "Exception: " + exception.Message;
                FirebaseManager.Instance.WriteCrashlyticsLog(message);
            }            
        }

        public static void LogException(Object context, System.Exception exception)
        {
            if (ConfigManager.Instance.IsLogException)
                UnityEngine.Debug.LogException(exception, context);
        }

#endregion


    }
}