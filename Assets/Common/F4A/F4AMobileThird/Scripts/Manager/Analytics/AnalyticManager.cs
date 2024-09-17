namespace com.F4A.MobileThird
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using DataCore;
    using Firebase.Analytics;
    using JimmysUnityUtilities;
    using UnityEngine;
    using UnityEngine.Analytics;

    public partial class AnalyticManager : SingletonMono<AnalyticManager>
    {
        public void Start()
        {
            //AdjustManager.Instance.Init(() => {
            //    UpdateFirebaseUserProperty();
            //});
        }

        private bool _didInit = false;
        public void Initialize()
        {
            _didInit = true;
            cachedRevenueEvents = new Dictionary<string, Dictionary<string, double>>();
            TenjinManager.Instance.Init(() =>
            {
                UpdateFirebaseUserProperty();
            });
        }

        private void UpdateFirebaseUserProperty()
        {
            if (!_didInit) return;
            FirebaseManager.Instance.UpdateFirebaseUserProperties();
        }
        public void LogAdEvent(string eventName, Dictionary<string, object> adInfo)
        {
            if (!_didInit) return;
            FirebaseManager.Instance.LogEvent(eventName, adInfo);
        }



        public void LogPayment(string productId, double quantity, double price, string currency, string transactionId, Dictionary<string, object> otherParameters = null)
        {
            if (!_didInit) return;
            TenjinManager.Instance.LogPayment(productId, quantity, price, currency, transactionId, otherParameters);
        }
        public void TrackPurchaseFailedVerification()
        {
            if (!_didInit) return;
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.PurchaseFailedVerification);
        }
        public void TrackPurchaseNotVerifiedVerification()
        {
            if (!_didInit) return;
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.PurchaseNotVerifiedVerification);
        }

        public void TrackPurchaseUnknownVerification()
        {
            if (!_didInit) return;
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.PurchaseUnknownVerification);
        }
        public void TrackConversionValue(int conversionValue)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.conversion_value, conversionValue);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.conversion_value, parameters);
        }

        public void TrackSubscriptionIOS(double price, string currency, string transactionId, string transactionDate, string productId)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.price, price,
                ConfigManager.TrackingEvent.EventParam.currency, currency,
                ConfigManager.TrackingEvent.EventParam.transaction_id, transactionId,
                ConfigManager.TrackingEvent.EventParam.transaction_date, transactionDate,
                ConfigManager.TrackingEvent.EventParam.product_id, productId);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.user_subscription, parameters);
        }

        public void TrackSubscriptionAndroid(double price, string currency, string orderId, string purchaseToken, string purchaseTime, string productId)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.price, price,
                ConfigManager.TrackingEvent.EventParam.currency, currency,
                ConfigManager.TrackingEvent.EventParam.order_id, orderId,
                ConfigManager.TrackingEvent.EventParam.purchase_time, purchaseTime,
                ConfigManager.TrackingEvent.EventParam.product_id, productId);
            var purchaseTokenParts = splitEventParams(purchaseToken, 99);
            for (int i = 0; i < purchaseTokenParts.Count; i++)
            {
                var paramName = ConfigManager.TrackingEvent.EventParam.purchase_token + "_" + i.ToString();
                parameters.Add(paramName, purchaseTokenParts[i]);
            }

            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.user_subscription, parameters);

        }
        Dictionary<string, int> _resourceEarnAndSpent = new Dictionary<string, int>();
        public void TrackResourceEarn(string triggerSource, string type, int amount)
        {
            if (!_didInit) return;
            var key = $"Earn_{triggerSource}_{type}";
            if (_resourceEarnAndSpent.ContainsKey(key))
            {
                _resourceEarnAndSpent[key] += amount;
            }
            else
            {
                _resourceEarnAndSpent[key] = amount;
            }
        }

        public void SendResourceEarnSpent()
        {
            var earnKey = "Earn";
            var spentKey = "Spent";
            foreach (var key in _resourceEarnAndSpent.Keys)
            {
                var components = key.Split('_');
                if (components.Count() > 2)
                {
                    var triggerSource = components[1];
                    var type = components[2];
                    var amount = _resourceEarnAndSpent[key];

                    if (components[0] == earnKey)
                    {
                        SendResourceEarn(triggerSource, type, amount);
                    }
                    else if (components[0] == spentKey) {
                        SendResourceSpent(triggerSource, type, amount);
                    }
                }
            }
            _resourceEarnAndSpent.Clear();
        }

        public void TrackResourceSpent(string triggerSource, string type, int amount)
        {
            if (!_didInit) return;
            var key = $"Spent_{triggerSource}_{type}";
            if (_resourceEarnAndSpent.ContainsKey(key))
            {
                _resourceEarnAndSpent[key] += amount;
            }
            else
            {
                _resourceEarnAndSpent[key] = amount;
            }
        }

        public void SendResourceEarn(string triggerSource, string type, int amount)
        {
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.trigger_source, triggerSource,
                                             ConfigManager.TrackingEvent.EventParam.resource_type, type,
                                             ConfigManager.TrackingEvent.EventParam.amount, amount);

            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.resource_earn, parameters);
        }
        public void SendResourceSpent(string triggerSource, string type, int amount)
        {
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.trigger_source, triggerSource,
                                             ConfigManager.TrackingEvent.EventParam.resource_type, type,
                                             ConfigManager.TrackingEvent.EventParam.amount, amount);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.resource_spent, parameters);
        }



        public void TrackStartPuzzleEvent(string puzzleName, string type)
        {
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.puzzle_name, puzzleName,
                ConfigManager.TrackingEvent.EventParam.play_type, type);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.start_puzzle, parameters);
        }

        public void TrackCompletePuzzle(string puzzleName, string type, int playedTimeInSeconds, int usedHints)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.puzzle_name, puzzleName,
                ConfigManager.TrackingEvent.EventParam.play_type, type,
                ConfigManager.TrackingEvent.EventParam.played_time, playedTimeInSeconds,
                ConfigManager.TrackingEvent.EventParam.used_hints, usedHints);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.completed_puzzle, parameters);
        }
        public void TrackSharePuzzleEvent(string puzzleName)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.puzzle_name, puzzleName);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.shared_puzzle, parameters);
        }
        public void TrackDownloadPuzzleEvent(string puzzleName)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.puzzle_name, puzzleName);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.downloaded_puzzle, parameters);
        }

        public void TrackFailedToLoadPuzzleEvent(string puzzleName)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.puzzle_name, puzzleName);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.failed_to_load_puzzle, parameters);
        }


        public void TrackUnlockChapter(string chapterName, string bookName)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.chapter_name, chapterName, ConfigManager.TrackingEvent.EventParam.book_name, bookName);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.unlock_chapter, parameters);
        }

        public void TrackOpenChapter(string chapterName, string bookName)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.chapter_name, chapterName, ConfigManager.TrackingEvent.EventParam.book_name, bookName);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.open_chapter, parameters);
        }

        public void TrackOpenBook(string bookName, string triggerSource)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.trigger_source, triggerSource, ConfigManager.TrackingEvent.EventParam.book_name, bookName);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.open_book, parameters);
        }

        public void TrackClaimChallenge(string challengeId)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.challenge_id, challengeId);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.claim_daily_challenge, parameters);
        }

        public void TrackCompleteTutorial(int playedTime)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.played_time, playedTime);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.tuto_completed, parameters);
            TenjinManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.tuto_completed);
        }
        public void TrackUnlockFirstChapter(int playedTime)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.played_time, playedTime);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.unlock_first_chapter, parameters);
        }

        public void TrackReceiveWelcomeGift()
        {
            if (!_didInit) return;
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.receive_welcome_gift);
        }

        public void TrackPlayedFivePuzzle()
        {
            if (!_didInit) return;
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.played_five_puzzle);
            TenjinManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.played_five_puzzle);
        }

        public void TrackPlayedThreePuzzle()
        {
            if (!_didInit) return;
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.played_three_puzzle);
            TenjinManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.played_three_puzzle);
        }

        public void TrackPlayedSevenPuzzle()
        {
            if (!_didInit) return;
            TenjinManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.played_seven_puzzle);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.played_seven_puzzle);
        }
        public void TrackCompletedSevenRewardedAd()
        {
            if (!_didInit) return;
            TenjinManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.completed_seven_rewarded_ad);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.completed_seven_rewarded_ad);
        }

        public void TrackCompletedThreeRewardedAd()
        {
            if (!_didInit) return;
            TenjinManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.completed_three_rewarded_ad);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.completed_three_rewarded_ad);
        }
        public void TrackCompletedFiveRewardedAd()
        {
            if (!_didInit) return;
            TenjinManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.completed_five_rewarded_ad);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.completed_five_rewarded_ad);
        }

        public void TrackShownThreeIntersitialAd()
        {
            if (!_didInit) return;
            TenjinManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.shown_three_interstitial_ad);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.shown_three_interstitial_ad);
        }

        public void TrackShownFiveIntersitialAd()
        {
            if (!_didInit) return;
            TenjinManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.shown_five_interstitial_ad);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.shown_five_interstitial_ad);
        }
        public void TrackShownSevenIntersitialAd()
        {
            if (!_didInit) return;
            TenjinManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.shown_seven_interstitial_ad);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.shown_seven_interstitial_ad);
        }

        public void TrackDownloadedBundle(long size)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.bundle_size, size);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.downloaded_asset_bundle, parameters);
        }

        public void TrackDownloadedPuzzle(string puzzleName, long size, float duration)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.puzzle_name, puzzleName, ConfigManager.TrackingEvent.EventParam.bundle_size, size, ConfigManager.TrackingEvent.EventParam.download_duration, duration);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.downloaded_puzzle_asset_bundle, parameters);
        }

        public void TrackAttribution(Dictionary<string, string> attributes)
        {
            if (!_didInit) return;
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.mobile_attribution, attributes);
        }

        #region Tutorial
        public void TrackStartPuzzleInTutorial(int index, int playedTimeInSeconds)
        {
            if (!_didInit) return;
            var eventName = $"{ConfigManager.TrackingEvent.EventName.start_tutorial_puzzle}_{index}";
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.played_time, playedTimeInSeconds);

            FirebaseManager.Instance.LogEvent(eventName, parameters);
        }
        public void TrackCompletedPuzzleInTutorial(int index, int playedTimeInSeconds)
        {
            if (!_didInit) return;
            var eventName = $"{ConfigManager.TrackingEvent.EventName.completed_tutorial_puzzle}_{index}";
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.played_time, playedTimeInSeconds);
            FirebaseManager.Instance.LogEvent(eventName, parameters);
        }

        public void TrackCompletedPuzzleStepInTutorial(int puzzleIndex, int layerIndex, int objectIndex, int playedTimeInSeconds)
        {
            var eventName = $"{ConfigManager.TrackingEvent.EventName.completed_step_tutorial_puzzle}_{puzzleIndex}_{layerIndex}_{objectIndex}";
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.played_time, playedTimeInSeconds);
            DataCore.Debug.Log($"Log event: {eventName} played_time {playedTimeInSeconds}", false);
            FirebaseManager.Instance.LogEvent(eventName, parameters);
        }

        public void LogFirstOpenMainGame(int playedTimeInSeconds)
        {
            if (!_didInit) return;
            var parameters = BuildParameters(ConfigManager.TrackingEvent.EventParam.played_time, playedTimeInSeconds);
            DataCore.Debug.Log($"Log event: {ConfigManager.TrackingEvent.EventName.first_open_main_game} played_time {playedTimeInSeconds}", false);
            FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.first_open_main_game, parameters);
        }

        public void LogEvent(string eventName)
        {
            if (!_didInit) return;
            DataCore.Debug.Log($"Log event: {eventName}", false);
            FirebaseManager.Instance.LogEvent(eventName);
        }

        #endregion

        #region Utils
        private static Dictionary<string, object> BuildParameters(params object[] keyValues)
        {
            var parameters = new Dictionary<string, object>();
            for (int i = 0, c = keyValues.Length; i < c; i += 2)
            {
                parameters[(string)keyValues[i]] = keyValues[i + 1];
            }
            return parameters;
        }

        public static List<string> splitEventParams(string param, int maxOffset)
        {
            var parts = new List<string>();
            var startIndex = 0;
            var offset = maxOffset;
            while (startIndex < param.Length)
            {
                var remainCharacter = param.Length - startIndex;
                if (remainCharacter < offset)
                {
                    offset = remainCharacter;
                }
                var subParam = param.Substring(startIndex, offset);
                parts.Add(subParam);
                startIndex += offset;
            }
            return parts;
        }
        #endregion


        #region ROAS
        private Dictionary<string, Dictionary<string, double>> cachedRevenueEvents;
        public void TrackAdRevenue(double revenue, string placement, string adType)
        {
            if (!_didInit) return;

            if (cachedRevenueEvents == null)
            {
                cachedRevenueEvents = new Dictionary<string, Dictionary<string, double>>();
            }

            if (revenue > 0)
            {
                var parameters = new List<Parameter>() {
                    new Parameter(FirebaseAnalytics.ParameterValue, revenue),
                    new Parameter(FirebaseAnalytics.ParameterCurrency, "USD"),
                };
                FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.ad_revenue, parameters);
            }

            var None = "None";
            placement = !string.IsNullOrEmpty(placement) ? placement : None;
            adType = !string.IsNullOrEmpty(adType) ? adType : None;
            var key = $"{placement}_{adType}";
            var impressionsKey = "impressions";
            var revenueKey = "revenue";
            if (!cachedRevenueEvents.ContainsKey(key))
            {
                var content = new Dictionary<string, double>() {
                    { impressionsKey , 1 },
                    { revenueKey , revenue },
                };
                cachedRevenueEvents.Add(key, content);
            }
            else
            {
                cachedRevenueEvents[key][impressionsKey] += 1;
                cachedRevenueEvents[key][revenueKey] += revenue;
            }

        }

        private void SendAdRevenue()
        {
            if (!_didInit) return;
            if (cachedRevenueEvents == null || cachedRevenueEvents.IsEmpty()) return;
            var revenueKey = "revenue";
            double revenue = 0f;
            foreach (var revenueEvent in cachedRevenueEvents.Values)
            {
                if (revenueEvent.ContainsKey(revenueKey))
                {
                    revenue += (double)revenueEvent[revenueKey];
                }
            }
            if (revenue > 0)
            {
                var parameters = new List<Parameter>() {
                    new Parameter(FirebaseAnalytics.ParameterValue, revenue),
                    new Parameter(FirebaseAnalytics.ParameterCurrency, "USD"),
                };
                DataCore.Debug.Log($"Log event: {ConfigManager.TrackingEvent.EventName.session_ad_revenue} revenue {revenue}", false);
                FirebaseManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.session_ad_revenue, parameters);
            }
            cachedRevenueEvents.Clear();
        }

        public void OnApplicationQuit()
        {
            if (!_didInit) return;
            SendAdRevenue();
            SendResourceEarnSpent();
        }
#if UNITY_ANDROID
        public void OnApplicationPause(bool pause)
        {
            if (!_didInit) return;
            if (AdsService.Instance.IsShowingVideoAd()) return;
            if (pause)
            {
                SendAdRevenue();
                SendResourceEarnSpent();
            }
        }
#else
        private void OnApplicationFocus(bool focus)
        {
            if (!_didInit) return;
            if (AdsService.Instance.IsShowingVideoAd()) return;
            if (!focus)
            {
                SendAdRevenue();
                SendResourceEarnSpent();
            }
        }
#endif

        #endregion

    }
}