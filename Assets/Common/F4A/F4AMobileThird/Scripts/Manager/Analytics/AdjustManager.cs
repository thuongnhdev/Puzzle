namespace com.F4A.MobileThird
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    using DataCore;

    public class TenjinManager : SingletonMono<TenjinManager>
    {
        string AppToken = "9DFY1H9MY8VYUALWLXSJJ8MQYXAWO72T";
        private Action _completeInitial;
        BaseTenjin _instance;
        Dictionary<string, int> eventAndConversionMap = new Dictionary<string, int>()
        {
            {  ConfigManager.TrackingEvent.EventName.tuto_completed, 1 },
            {  ConfigManager.TrackingEvent.EventName.unlock_first_chapter, 2 },
            {  ConfigManager.TrackingEvent.EventName.receive_welcome_gift, 3 },
            {  ConfigManager.TrackingEvent.EventName.played_three_puzzle, 4 },
            {  ConfigManager.TrackingEvent.EventName.played_five_puzzle, 5 },
            {  ConfigManager.TrackingEvent.EventName.played_seven_puzzle, 6 },
            {  ConfigManager.TrackingEvent.EventName.purchase_iap, 7 },
            {  ConfigManager.TrackingEvent.EventName.completed_three_rewarded_ad, 8 },
            {  ConfigManager.TrackingEvent.EventName.completed_five_rewarded_ad, 9 },
            {  ConfigManager.TrackingEvent.EventName.completed_seven_rewarded_ad, 10 },


        };

        public void Init(Action completed)
        {
            DataCore.Debug.Log("TenjinManager init");
            _completeInitial = completed;
            TenjinConnect();
            SubscribeAppLovinImpressions();
        }
#if UNITY_IOS
        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                var lastTimeSentStr = CPlayerPrefs.GetString(ConfigManager.LastTimeSendSessionEventToTenjin, "20180127000000");
                DataCore.Debug.Log($"LastTimeSendSessionEventToTenjin: {lastTimeSentStr}", false);
                var lastTimeSent = System.DateTime.ParseExact(lastTimeSentStr, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                var duration = System.DateTime.Now - lastTimeSent;
                if (duration.TotalHours >= 6)
                {
                    TenjinConnect();
                }
            }
        }

#elif UNITY_ANDROID
        void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus)
            {
                var lastTimeSentStr = CPlayerPrefs.GetString(ConfigManager.LastTimeSendSessionEventToTenjin, "20180127000000");
                DataCore.Debug.Log($"LastTimeSendSessionEventToTenjin: {lastTimeSentStr}", false);
                var lastTimeSent = System.DateTime.ParseExact(lastTimeSentStr, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                var duration = System.DateTime.Now - lastTimeSent;
                if (duration.TotalHours >= 6)
                {
                    TenjinConnect();
                }
            }
        }
#endif
        public void TenjinConnect()
        {
            _instance = Tenjin.getInstance(AppToken);            

#if UNITY_IOS

            // Registers SKAdNetwork app for attribution
            _instance.RegisterAppForAdNetworkAttribution();
            // Sends install/open event to Tenjin
            _instance.Connect();

#elif UNITY_ANDROID

            // Sends install/open event to Tenjin
            _instance.Connect();
#endif
            CPlayerPrefs.SetString(ConfigManager.LastTimeSendSessionEventToTenjin, DateTime.Now.ToString("yyyyMMddHHmmss"));
            CPlayerPrefs.Save();
            _completeInitial?.Invoke();
        }
        public void SubscribeAppLovinImpressions() {
            if (_instance == null) return;
            _instance.SubscribeAppLovinImpressions();
        }


        public void LogEvent(string eventName)
        {
            if (_instance == null)
            {
                TenjinConnect();
            }
            DataCore.Debug.Log($"Start Log Event: {eventName}", false);
            _instance.SendEvent(eventName);
        }

        public void UpdateConversionValue(int value)
        {
#if UNITY_IOS
            if (_instance == null)
            {
                TenjinConnect();
            }
            if (value > 0)
            {
                _instance.UpdateConversionValue(value);
            }
#endif
        }



        public void LogPayment(string productId, double quantity, double price, string currency, string transactionId, Dictionary<string, object> otherParameters = null)
        {
            if (_instance == null)
            {
                TenjinConnect();
            }
            if (Application.platform == RuntimePlatform.Android)
            {
                if (otherParameters.ContainsKey("signature") && otherParameters.ContainsKey("receipt"))
                {
                    var signature = otherParameters["signature"].ToString();
                    var receipt = otherParameters["receipt"].ToString();
                    LogAndroidPurchase(productId, currency, (int)quantity, price, receipt, signature);
                }
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (otherParameters.ContainsKey("receipt"))
                {
                    var receipt = otherParameters["receipt"].ToString();
                    LogIosPurchase(productId, currency, (int)quantity, price, transactionId, receipt);
                }
            }

        }
        private void LogAndroidPurchase(string ProductId, string CurrencyCode, int Quantity, double UnitPrice, string Receipt, string Signature)
        {
            if (_instance == null)
            {
                TenjinConnect();
            }
            _instance.Transaction(ProductId, CurrencyCode, Quantity, UnitPrice, null, Receipt, Signature);
        }

        private void LogIosPurchase(string ProductId, string CurrencyCode, int Quantity, double UnitPrice, string TransactionId, string Receipt)
        {
            if (_instance == null)
            {
                TenjinConnect();
            }
            _instance.Transaction(ProductId, CurrencyCode, Quantity, UnitPrice, TransactionId, Receipt, null);
        }

        private int getEventConversionValue(string eventName) {
            if (eventAndConversionMap.ContainsKey(eventName))
            {
                return eventAndConversionMap[eventName];
            }
            return -1;
        }


        public void LogEvent(string eventName, Dictionary<string, object> parameters)
        {
            LogEvent(eventName, parameters, false);
        }
        public void LogEvent(string eventName, Dictionary<string, object> parameters, bool partnerEvent = false)
        {

        }

        public void LogPayment(string productName, string productId, double quantity, double price, string currency, string transactionId, Dictionary<string, object> otherParameters = null)
        {

        }

        public void TrackAdRevenue(double revenue, string countryCode, string networkName, string adUnitIdentifier, string placement)
        {

        }

        public void EventMeasureUninstalls(string token)
        {

        }
    }
}