using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Android.PerformanceTuner;
using DataCore;

namespace com.F4A.MobileThird
{
    public class GameSession : SingletonMono<GameSession>
    {
        AndroidPerformanceTuner<FidelityParams, Annotation> tuner =
        new AndroidPerformanceTuner<FidelityParams, Annotation>();
        private bool _isCompletedFirstSessionLoading = false;

        private float _sessionPlayedTime;

        public float SessionPlayedTime { get => _sessionPlayedTime; set => _sessionPlayedTime = value; }
        public bool IsCompletedFirstSessionLoading { get => _isCompletedFirstSessionLoading; set => _isCompletedFirstSessionLoading = value; }

        // Start is called before the first frame update
        void Start()
        {
            _sessionPlayedTime = 0;
            try
            {
                ErrorCode startErrorCode = tuner.Start();
                DataCore.Debug.Log("Completed Start PerformanceTuner");
            }
            catch (System.Exception ex)
            {
                DataCore.Debug.Log($"Failed to enable PerformanceTuner {ex.Message}");
            }

        }

        // Update is called once per frame
        void Update()
        {
            _sessionPlayedTime += Time.deltaTime;
        }


        //#if UNITY_ANDROID

        void OnApplicationPause(bool pauseStatus)
        {
            UnityEngine.Debug.Log($"OnApplicationPause {pauseStatus}");
            if (!GameSession.Instance.IsCompletedFirstSessionLoading) return;

            if (Application.platform == RuntimePlatform.Android)
            {
                if (pauseStatus)
                {
                    GameData.Instance.SyncUserProfileOnApplicationInBackground(true);
                }
            }
        }
        //#else
        private void OnApplicationFocus(bool focus)
        {
            UnityEngine.Debug.Log($"OnApplicationFocus {focus}");
            if (!GameSession.Instance.IsCompletedFirstSessionLoading) return;

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (!focus)
                {
                    GameData.Instance.SyncUserProfileOnApplicationInBackground(true);
                }
            }

        }
        //#endif

        private void OnApplicationQuit()
        {
            UnityEngine.Debug.Log($"OnApplicationQuit");
            GameData.Instance.OnApplicationQuit();
        }

    }
}