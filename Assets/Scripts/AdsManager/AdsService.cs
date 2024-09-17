using DataCore;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AudienceNetwork;

namespace com.F4A.MobileThird
{
    public class AdsService : SingletonMonoAwake<AdsService>
    {
        private readonly string max_sdk_key = "M06oIMR6ZTrZ8hI6MurS54GPdhblhih2kpq0AG7kqyIzea3_TvRTzizbOZL5G27-ry_0D6k131-hc2h4y8dvra";

#if UNITY_IOS
        private static readonly string BANNER_ID = "1c36cb043af63b85";
        private static readonly string INTERSTITIAL_AD_ID = "da080496c6ed7c02";
        private static readonly string REWARDED_AD_ID = "c9319f257860206b";
#else
        private static readonly string BANNER_ID = "73b174f0f8074167";
        private static readonly string INTERSTITIAL_AD_ID = "147ef94dcaac28f1";
        private static readonly string REWARDED_AD_ID = "627722653193ddb7";
#endif

        public const string IntAdPlacementBetween = "BETWEEN_CONTINUOUSLY_GAMES";
        public const string IntAdPlacementResume = "RESUME_GAME";
        public const string IntAdPlacementChapter = "CHAPTER_DETAIL";
        public const string IntAdPlacementSplash = "AD_FREQUENCY_FROM_SPLASH";
        public const string IntAdPlacementBackground = "AD_FREQUENCY_FROM_BACKGROUND";

        public const string RwAdUnitIdHint = "GET_HINT";
        public const string RwAdUnitIdFloatingHint = "GET_FLOATING_HINT";
        public const string RwAdUnitIdLucky = "LUCKY_DRAW";
        public const string RwAdUnitIdDaily = "DOUBLE_DAILY_REWARD";
        public const string RwAdUnitIdDraw = "DOUBLE_LUCKY_DRAW";
        public const string RwAdUnitIdDoublePuzzleReward = "DOUBLE_PUZZLE_REWARD";

        private bool isInited = false;
        private bool didLoadBanner = false;
        private bool didStartLoadAd = false;
        private bool didUpdateBannerPosition = false;

        public IEnumerator Initialize()
        {
            if (isInited)
                yield break;
            yield return InitMediation();
        }

        private IEnumerator InitMediation()
        {
            bool isCompleted = false;
            try
            {
                isInited = false;

                MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
                {
                    // AppLovin SDK is initialized, start loading ads
#if UNITY_IOS || UNITY_IPHONE
                    if (MaxSdkUtils.CompareVersions(UnityEngine.iOS.Device.systemVersion, "14.5") != MaxSdkUtils.VersionComparisonResult.Lesser)
                    {
                        // Note that App transparency tracking authorization can be checked via `sdkConfiguration.AppTrackingStatus` for Unity Editor and iOS targets
                        // 1. Set Facebook ATE flag here, THEN
                        bool advertiserTrackingEnabled = sdkConfiguration.AppTrackingStatus == MaxSdkBase.AppTrackingStatus.Authorized;
                        AdSettings.SetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
                    }
                    else
                    {
                        AdSettings.SetAdvertiserTrackingEnabled(true);
                    }
#endif
                    isCompleted = true;
                    StartLoadAd();
                    AnalyticManager.Instance.Initialize();
                };
                MaxSdk.SetUserId(SystemInfo.deviceUniqueIdentifier);
                MaxSdk.SetSdkKey(max_sdk_key);
                MaxSdk.InitializeSdk();
                MaxSdk.SetMuted(true);
                MaxSdk.SetVerboseLogging(CPlayerPrefs.GetBool(ConfigManager.ENABLE_DEBUG_MODE, false));

            }
            catch (Exception ex)
            {
                isCompleted = true;
            }
            yield return new WaitUntil(() => isCompleted);
        }

        public bool IsShowingVideoAd()
        {
            return isShowingInterstitial || isShowingRewardsAd;
        }
        public bool DidStartLoadAd()
        {
            return didStartLoadAd;
        }

        public void StartLoadAd()
        {
            if (didStartLoadAd) return;
            try
            {
                didStartLoadAd = true;
                isInited = true;
                InitializeInterstitialAds();
                InitializeRewardedAds();
                InitializeBanner();
                InitialPaidEvents();
            }
            catch (Exception e)
            {
                didStartLoadAd = false;
                isInited = false;
            }
        }
        private void InitialPaidEvents()
        {
            // Attach callbacks based on the ad format(s) you are using
            MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnAdRevenuePaidInterstitialEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnAdRevenuePaidRewardedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnAdRevenuePaidBannerEvent;

        }
        private void InitializeInterstitialAds()
        {
            //DataCore.Debug.Log("InitializeInterstitialAds");
            MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoaded;
            MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedToLoad;
            MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialAdClickedEvent;
            MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialShowing;
            MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialClosed;
            MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += OnInterstitialFailedToDisplay;
            _interstitialAdInfos = new Dictionary<string, MaxSdkBase.AdInfo>();
            // Load the first interstitial
            DOVirtual.DelayedCall(0.25f, () =>
            {
                LoadInterstitial();
            });

        }
        private void InitializeRewardedAds()
        {
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoaded;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedToLoad;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdShowing;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdClosed;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToPlay;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedReward;

            _rewardedAdInfos = new Dictionary<string, MaxSdkBase.AdInfo>();
            // Load the first RewardedAd
            DOVirtual.DelayedCall(0.25f, () =>
            {
                LoadRewardedAd();
            });
        }
        private void InitializeBanner()
        {
            //DataCore.Debug.Log("InitializeBanner");
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerFailedToLoad;
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerLoaded;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdLeaveApp;

            DOVirtual.DelayedCall(0.25f, () =>
            {
                LoadBanner();
            });

        }

        public void LoadBanner()
        {
            if (!isInited) return;
            DataCore.Debug.Log("LoadBanner");
            try
            {
                MaxSdk.SetBannerPlacement(BANNER_ID, GetPlacementFromId(BANNER_ID, "BANNER", $"bottom_banner_type_{AdsLogic.BannerType()}"));
                MaxSdk.SetBannerBackgroundColor(BANNER_ID, Color.clear);
                MaxSdk.CreateBanner(BANNER_ID, MaxSdkBase.BannerPosition.BottomCenter);
                MaxSdk.SetBannerExtraParameter(BANNER_ID, "adaptive_banner", "true");                
            }
            catch
            {
                DataCore.Debug.Log("Profile service should be initialed before LoadBanner");
            }
        }

        public void ShowBanner()
        {
            DataCore.Debug.Log($"ShowBanner {isInited} {AdsLogic.IsVipAds() || IAPManager.Instance.IsSubscriprion()}", false);
            if (!isInited) return;
            if (!autoShowBanner) return;
            if (AdsLogic.IsVipAds() || IAPManager.Instance.IsSubscriprion()) return;
            try
            {
                MaxSdk.ShowBanner(BANNER_ID);
            }
            catch (Exception e)
            {
                DataCore.Debug.Log("Failed to show banner with error" + e.ToString());
            }
        }

        bool autoShowBanner = false;
        public void SetAutoShowBanner(bool auto)
        {
            autoShowBanner = auto;
        }
        public void HideBanner()
        {
            if (!isInited) return;
            DataCore.Debug.Log("HideBanner", false);
            MaxSdk.HideBanner(BANNER_ID);
        }

        private void OnBannerLoaded(string data, MaxSdkBase.AdInfo adInfo)
        {
            OnBannerLoadedEvent?.Invoke(data);
            DataCore.Debug.Log($"OnBannerLoaded {adInfo}", false);
            if (!didLoadBanner && autoShowBanner)
            {
                didLoadBanner = true;
                ShowBanner();
            }
        }

        private void OnBannerFailedToLoad(string data, MaxSdkBase.ErrorInfo errorInfo)
        {
            DataCore.Debug.Log($"onBannerFailedToLoad {data}. Error info: {errorInfo}");
            try
            {
                OnBannerFailedEvent?.Invoke(data);
                didLoadBanner = false;
            }
            catch (System.Exception e)
            {
                DataCore.Debug.Log("onBannerFailedToLoad " + e.Message);
                didLoadBanner = false;
            }
        }

        private void OnBannerAdLeaveApp(string data, MaxSdkBase.AdInfo adInfo)
        {
            OnBannerLeaveAppEvent?.Invoke(data);
            DataCore.Debug.Log($"onBannerAdLeaveApp {data}. Ad info: {adInfo}");
        }

        //////////////////////////////////////////////////////////////////////////////////////

        private int intRetryAttempt = 0;
        private bool isLoadingInterstitial = false;
        private bool isShowingInterstitial = false;
        private Dictionary<string, MaxSdkBase.AdInfo> _interstitialAdInfos;

        public void LoadInterstitial()
        {
            if (!isInited) return;
            if (IsInterstitialLoaded()) return;
            if (isLoadingInterstitial) return;
            DataCore.Debug.Log($"Start LoadInterstitial {INTERSTITIAL_AD_ID}", false);
            isLoadingInterstitial = true;
            isShowingInterstitial = false;

            MaxSdk.LoadInterstitial(INTERSTITIAL_AD_ID);
        }

        public bool IsInterstitialLoaded()
        {
            var IsLoaded = false;
#if UNITY_IOS || UNITY_ANDROID
            IsLoaded = MaxSdk.IsInterstitialReady(INTERSTITIAL_AD_ID);
#else
            IsLoaded = PassIntersitialThresHold();
#endif
            return IsLoaded;
        }

        private string currentInterstitialPlacement = "";
        public void ShowInterstitial(string placement, Action<bool> onComplete)
        {
            //DataCore.Debug.Log($"ShowInterstitial {placement} isInited: {isInited} isShowingInterstitial {isShowingInterstitial} AdsLogic.IsVipAds() {AdsLogic.IsVipAds()} IsSubscriprion {IAPManager.Instance.IsSubscriprion()} IsInterstitialLoaded: {IsInterstitialLoaded()}", false);
            GC.Collect();
            Resources.UnloadUnusedAssets();
            OnInterstitialClosedEvent = onComplete;
            if (!isInited)
            {
                OnInterstitialClosedEvent?.Invoke(false);
                return;
            }
            if (isShowingInterstitial)
            {
                OnInterstitialClosedEvent?.Invoke(false);
                return;
            }
            if (!AdsLogic.IsPlayPuzzleIntertitialAds())
            {
                OnInterstitialClosedEvent?.Invoke(false);
                return;
            }
            if (AdsLogic.IsVipAds())
            {
                OnInterstitialClosedEvent?.Invoke(false);
                return;
            }

            if (IAPManager.Instance.IsSubscriprion()) {
                OnInterstitialClosedEvent?.Invoke(false);
                return;
            }

            if (IsInterstitialLoaded())
            {
                var creativeId = "";
                var networkAd = "";
                if (_interstitialAdInfos != null && _interstitialAdInfos.ContainsKey(INTERSTITIAL_AD_ID)) {
                    var adInfo = _interstitialAdInfos[INTERSTITIAL_AD_ID];
                    creativeId = adInfo.CreativeIdentifier;
                    networkAd = adInfo.NetworkName;
                }

                DataCore.Debug.Log($"Start ShowInterstitial {placement} NetworkName: {networkAd} creativeId: {creativeId} ");
                currentInterstitialPlacement = placement;
                isShowingInterstitial = true;
                SoundController.Instance.MuteBgMusic(true);
                MaxSdk.ShowInterstitial(INTERSTITIAL_AD_ID, GetPlacementFromId(INTERSTITIAL_AD_ID, "INT", placement));
            }
            else
            {
                OnInterstitialClosedEvent?.Invoke(false);
                LoadInterstitial();
            }
        }

        public bool IsRemoteIntertitial(string placement)
        {
            switch (placement)
            {
                case IntAdPlacementSplash:
#if UNITY_ANDROID
                    return FirebaseManager.GetValueBoolRemote(ConfigManager.keyAndroidLoadingAd);
#elif UNITY_IOS
                    return FirebaseManager.GetValueBoolRemote(ConfigManager.keyIosLoadingAd);
#endif
                case IntAdPlacementBackground:
#if UNITY_ANDROID
                    return FirebaseManager.GetValueBoolRemote(ConfigManager.keyAndroidBackgroundAd);
#elif UNITY_IOS
                    return FirebaseManager.GetValueBoolRemote(ConfigManager.keyIosBackgroundAd);
#endif


            }
            return false;
        }
        private void OnInterstitialLoaded(string data, MaxSdkBase.AdInfo adInfo)
        {
            DataCore.Debug.Log($"onInterstitialLoaded {data}. Ad info: {adInfo}", false);
            OnInterstitialLoadedEvent?.Invoke(data);
            isLoadingInterstitial = false;
            intRetryAttempt = 0;
            if (_interstitialAdInfos != null) {
                _interstitialAdInfos[data] = adInfo;
            }

        }

        private void OnInterstitialFailedToLoad(string data, MaxSdkBase.ErrorInfo errorInfo)
        {
            DataCore.Debug.Log("onInterstitialFailedToLoad " + data);
            OnInterstitialClosedEvent?.Invoke(false);
            intRetryAttempt++;
            isLoadingInterstitial = false;
            isShowingInterstitial = false;
            float retryDelay = (float)Math.Pow(2, Math.Min(6, intRetryAttempt));
            DOVirtual.DelayedCall(retryDelay, () =>
            {
                LoadInterstitial();
            });

        }

        private void OnInterstitialFailedToDisplay(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            DataCore.Debug.Log($"onInterstitialFailedToDisplay {adUnitId}. Error info: {errorInfo}, Ad info: {adInfo}");
            OnInterstitialClosedEvent?.Invoke(false);
            currentInterstitialPlacement = "";
            isLoadingInterstitial = false;
            isShowingInterstitial = false;
            LoadInterstitial();
            AnalyticManager.Instance.LogAdEvent(ConfigManager.TrackingEvent.EventName.failed_to_show_interstitial_ad, GetAdInfo(adInfo));
        }

        private void OnInterstitialClosed(string data, MaxSdkBase.AdInfo adInfo)
        {
            DataCore.Debug.Log($"onInterstitialClosed {data}. Ad info: {adInfo}");
            OnInterstitialClosedEvent?.Invoke(true);
            isShowingInterstitial = false;
            LoadInterstitial();
            AnalyticManager.Instance.LogAdEvent(ConfigManager.TrackingEvent.EventName.closed_interstitial_ad, GetAdInfo(adInfo));
        }

        private void OnInterstitialShowing(string data, MaxSdkBase.AdInfo adInfo)
        {
            DataCore.Debug.Log($"onInterstitialShowing {data}. Ad info: {adInfo}");
            OnInterstitialShowingEvent?.Invoke(data, currentInterstitialPlacement);
            currentInterstitialPlacement = "";
            isShowingInterstitial = true;
            AnalyticManager.Instance.LogAdEvent(ConfigManager.TrackingEvent.EventName.showed_interstitial_ad, GetAdInfo(adInfo));
            GameData.Instance.IncreaseShownIntersitialAd();
        }
        private void OnInterstitialAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            DataCore.Debug.Log($"OnInterstitialAdClickedEvent {adUnitId}. Ad info: {adInfo}");
            AnalyticManager.Instance.LogAdEvent(ConfigManager.TrackingEvent.EventName.clicked_interstitial_ad, GetAdInfo(adInfo));
        }




        //////////////////////////////////////////////////////////////////////////////////////


        private int rvRetryAttempt = 0;
        private bool isLoadingRewardAd = false;
        private bool isShowingRewardsAd = false;
        private string currentRewardAdPlacement = "";
        private Dictionary<string, MaxSdkBase.AdInfo> _rewardedAdInfos;
        public void LoadRewardedAd()
        {
            if (!isInited) return;
            DataCore.Debug.Log("Start LoadRewardedAd");
            if (IsRewardedAdLoaded()) return;
            if (isLoadingRewardAd) return;
            if (isShowingRewardsAd) return;
#if UNITY_IOS || UNITY_ANDROID
            DataCore.Debug.Log("Process LoadRewardedAd " + REWARDED_AD_ID, false);
            isLoadingRewardAd = true;
            isShowingRewardsAd = false;
            MaxSdk.LoadRewardedAd(REWARDED_AD_ID);
#endif
        }

        public bool IsRewardedAdLoaded()
        {
#if UNITY_ANDROID || UNITY_IOS
            return MaxSdk.IsRewardedAdReady(REWARDED_AD_ID);
#else
      return false;
#endif
        }

        public void ShowRewardedAd(string placement, Action<bool> onRewardedAdClosedEvent, Action<string, string> onRewardedAdReceivedReward)
        {

            if (!isInited) onRewardedAdClosedEvent?.Invoke(false);
            if (isShowingRewardsAd) onRewardedAdClosedEvent?.Invoke(false);
            GC.Collect();
            Resources.UnloadUnusedAssets();
            if (IsRewardedAdLoaded())
            {
                var creativeId = "";
                var networkAd = "";
                if (_rewardedAdInfos != null && _rewardedAdInfos.ContainsKey(REWARDED_AD_ID))
                {
                    var adInfo = _rewardedAdInfos[REWARDED_AD_ID];
                    creativeId = adInfo.CreativeIdentifier;
                    networkAd = adInfo.NetworkName;
                }

                OnRewardedAdClosedEvent = onRewardedAdClosedEvent;
                OnRewardedAdReceivedRewardEvent = onRewardedAdReceivedReward;
                DataCore.Debug.Log($"ShowRewardedAd {placement} NetworkName: {networkAd} creativeId: {creativeId}");
                isShowingRewardsAd = true;
                currentRewardAdPlacement = placement;
                SoundController.Instance.MuteBgMusic(true);
                MaxSdk.ShowRewardedAd(REWARDED_AD_ID, GetPlacementFromId(REWARDED_AD_ID, "RV", placement));
            }
            else
            {
                onRewardedAdClosedEvent?.Invoke(false);
                LoadRewardedAd();
            }
        }

        private void OnRewardedAdLoaded(string data, MaxSdkBase.AdInfo adInfo)
        {
            rvRetryAttempt = 0;
            isLoadingRewardAd = false;
            OnRewardedAdLoadedEvent?.Invoke(data);
            if (_rewardedAdInfos != null) {
                _rewardedAdInfos[data] = adInfo;
            }
            DataCore.Debug.Log($"onRewardedAdLoaded {data}. Ad info: {adInfo}", false);
        }

        private void OnRewardedAdFailedToLoad(string data, MaxSdkBase.ErrorInfo errorInfo)
        {
            rvRetryAttempt++;
            isLoadingRewardAd = false;
            float retryDelay = (float)Math.Pow(2, Math.Min(6, rvRetryAttempt));
            DOVirtual.DelayedCall(retryDelay, () =>
            {
                LoadRewardedAd();
            });

            DataCore.Debug.Log($"onRewardedAdFailedToLoad {data}. errorInfo: {errorInfo}");
        }
        private void OnRewardedAdFailedToPlay(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            currentRewardAdPlacement = "";
            isLoadingRewardAd = false;
            isShowingRewardsAd = false;
            LoadRewardedAd();
            OnRewardedAdClosedEvent?.Invoke(false);
            OnRewardedAdClosedEvent = null;
            OnRewardedAdReceivedRewardEvent = null;
            DataCore.Debug.Log($"onRewardedAdFailedToPlay {adUnitId}. errorInfo: {errorInfo}. adInfo: {adInfo}");
            AnalyticManager.Instance.LogAdEvent(ConfigManager.TrackingEvent.EventName.failed_to_show_rewarded_ad, GetAdInfo(adInfo));

        }
        private void OnRewardedAdClosed(string data, MaxSdkBase.AdInfo adInfo)
        {
            isShowingRewardsAd = false;
            LoadRewardedAd();
            OnRewardedAdClosedEvent?.Invoke(true);
            OnRewardedAdClosedEvent = null;
            DataCore.Debug.Log($"onRewardedAdClosed {data}. adInfo: {adInfo}");
            AnalyticManager.Instance.LogAdEvent(ConfigManager.TrackingEvent.EventName.closed_rewarded_ad, GetAdInfo(adInfo));

        }

        private void OnRewardedAdReceivedReward(string adId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            isShowingRewardsAd = false;
            OnRewardedAdReceivedRewardEvent?.Invoke(adId, currentRewardAdPlacement);
            OnRewardedAdReceivedRewardEvent = null;
            DataCore.Debug.Log($"onRewardedAdReceivedReward {adId}. adInfo: {adInfo}");
            AnalyticManager.Instance.LogAdEvent(ConfigManager.TrackingEvent.EventName.completed_rewarded_ad, GetAdInfo(adInfo));
            GameData.Instance.IncreaseCompletedRewardedAd();
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            OnRewardedAdClosedEvent?.Invoke(true);
            OnRewardedAdClosedEvent = null;
            DataCore.Debug.Log($"OnRewardedAdClickedEvent {adUnitId}. adInfo: {adInfo}");
            AnalyticManager.Instance.LogAdEvent(ConfigManager.TrackingEvent.EventName.clicked_rewarded_ad, GetAdInfo(adInfo));
        }

        private void OnRewardedAdShowing(string data, MaxSdkBase.AdInfo adInfo)
        {
            OnRewardedAdShowingEvent?.Invoke(data, currentRewardAdPlacement);
            DataCore.Debug.Log($"onRewardedAdShowing {data}. adInfo: {adInfo}");
            AnalyticManager.Instance.LogAdEvent(ConfigManager.TrackingEvent.EventName.showed_rewarded_ad, GetAdInfo(adInfo));
        }

        //////////////////////////////////////////////////////////////////////////////////////
        #region Events

        private void OnAdRevenuePaidBannerEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            double revenue = adInfo.Revenue;
            string placement = "Bottom";
            AnalyticManager.Instance.TrackAdRevenue(revenue, placement, "BANNER");
            ConversionValueManager.Instance.AddRevenue((float)revenue);
        }

        private void OnAdRevenuePaidInterstitialEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            double revenue = adInfo.Revenue;            
            string placement = adInfo.Placement; // The placement this ad's postbacks are tied to
            AnalyticManager.Instance.TrackAdRevenue(revenue, placement, "REWARD");
            ConversionValueManager.Instance.AddRevenue((float)revenue);
        }

        private void OnAdRevenuePaidRewardedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            double revenue = adInfo.Revenue;
            string placement = adInfo.Placement; // The placement this ad's postbacks are tied to
            AnalyticManager.Instance.TrackAdRevenue(revenue, placement, "INTER");
            ConversionValueManager.Instance.AddRevenue((float)revenue);
        }


        // Fired when banner ad is loaded
        private static event Action<string> OnBannerLoadedEvent;

        // Fired when banner ad fails to load
        private static event Action<string> OnBannerFailedEvent;

        // Fired when tap banner ad
        private static event Action<string> OnBannerLeaveAppEvent;

        // Fired when interstitial ad is loaded and ready to be shown
        private static event Action<string> OnInterstitialLoadedEvent;

        // Fired when interstitial ad fails to Show
        private static event Action<string> OnInterstitialShowFailedEvent;

        // Fired when interstitial ad closes
        private static event Action<bool> OnInterstitialClosedEvent;

        // Fired when interstitial ad showing
        private static event Action<string, string> OnInterstitialShowingEvent;

        // Fired when tap interstitial ad
        private static event Action<string> OnInterstitialLeaveAppEvent;

        // Fired when rewarded ad is loaded and ready to be shown
        private static event Action<string> OnRewardedAdLoadedEvent;

        // Fired when a rewarded ad completes
        public static event Action<string, string> OnRewardedAdReceivedRewardEvent;

        // Fired when a rewarded ad closes
        public static event Action<bool> OnRewardedAdClosedEvent;

        // Fired when rewarded ad showing
        private static event Action<string, string> OnRewardedAdShowingEvent;

        #endregion
        #region  Utils

        public string getCurrentBannerId()
        {
            return BANNER_ID;
        }

        public string GetPlacementFromId(string adId, string type, string placement)
        {
#if UNITY_IOS
            return $"{adId}_{type}_{placement}";
#elif UNITY_ANDROID
            return $"{adId}_{type}_{placement}";
#else
      return $"{adId}_{type}_{placement}";
#endif
        }

        private Dictionary<string, object> GetAdInfo(MaxSdkBase.AdInfo adInfo)
        {
            if (adInfo == null) return null;
            try
            {
                var result = new Dictionary<string, object>();
                result.Add(ConfigManager.TrackingEvent.EventParam.revenue, adInfo.Revenue);
                result.Add(ConfigManager.TrackingEvent.EventParam.ad_unit_identifier, adInfo.AdUnitIdentifier);
                result.Add(ConfigManager.TrackingEvent.EventParam.country_code, MaxSdk.GetSdkConfiguration().CountryCode);
                result.Add(ConfigManager.TrackingEvent.EventParam.network_name, adInfo.NetworkName);
                result.Add(ConfigManager.TrackingEvent.EventParam.network_placement, adInfo.NetworkPlacement);
                result.Add(ConfigManager.TrackingEvent.EventParam.placement, adInfo.Placement);
                result.Add(ConfigManager.TrackingEvent.EventParam.creative_identifier, adInfo.CreativeIdentifier);
                return result;
            }
            catch
            {
                return null;
            }

        }

        #endregion



    }
}