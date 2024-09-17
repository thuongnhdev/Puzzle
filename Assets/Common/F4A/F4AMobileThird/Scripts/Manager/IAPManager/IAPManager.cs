#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// You must obfuscate your secrets using Window > Unity IAP > Receipt Validation Obfuscator
// before receipt validation will compile in this sample.
#define RECEIPT_VALIDATION
#endif
namespace com.F4A.MobileThird
{
    using System;
    using System.Linq;
    using UnityEngine;
    using DataCore;

#if RECEIPT_VALIDATION
    using UnityEngine.Purchasing.Security;
#endif
#if DEFINE_IAP
    using UnityEngine.Purchasing;
#endif

    using Newtonsoft.Json;

    using System.Collections.Generic;
    using System.IO;
    using System.Globalization;



    public enum InAppProductType
    {
        Consumable = 0,
        NonConsumable = 1,
        Subscription = 2
    }

    /// <summary>
    /// 
    /// </summary>
    public enum EIapPopular
    {
        None,
        Hot,
        Best,
        Popular,
        Sale,
        New,
    }

    /// <summary>
	/// 
	/// </summary>
	public enum ETagProduct
    {
        None,
        Hot,
        Best,
        Popular,
        Sale,
        New,
    }

    public enum EIapReward
    {
        None = -1,
        Hint,
        Coin,
        RemoveAds,
        Vip,
        Unlock,
        Weekly,
        Monthly
    }

    public enum EIapNonConsumableType
    {
        None,
        RemoveAds,
    }
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    public class IAPProductInfo
    {
        public string Id = "";
        public string Name = "";
        public string Description = "";

        [Space(10)]
        [Header("Price of product ($)")]
        public float Price = 0.99f;
        public int Coin = 25;
        public int Discount = 0;
        public EIapReward eIapReward = EIapReward.None;
        public ETagProduct InAppTag = ETagProduct.None;
        public InAppProductType Type = InAppProductType.Consumable;
        public EIapNonConsumableType nonConsumableType = EIapNonConsumableType.None;
        public bool IsTypeRemoveAds()
        {
            return Type == InAppProductType.NonConsumable && nonConsumableType == EIapNonConsumableType.RemoveAds;
        }

        public bool IsConsumable()
        {
            return Type == InAppProductType.Consumable;
        }

        public bool IsNonConsumable()
        {
            return Type == InAppProductType.NonConsumable;
        }

        public bool IsSubscription()
        {
            return Type == InAppProductType.Subscription;
        }

        public bool HasTag()
        {
            return InAppTag != ETagProduct.None;
        }
    }

    public enum EStatusBuyIAP
    {
        None,
        NotInitializeIAP,
        NotContainInStore,
        InProcess,
        Success,
    }

    [System.Serializable]
    public class IAPSettingInfo
    {
        public bool isTestIAP = false;
        public bool enableAndroid = true;
        public bool enableIos = true;

        public bool EnableUnityIAP()
        {
#if UNITY_ANDROID
            return enableAndroid;
#elif UNITY_IOS
            return enableIos;
#else
            return false;
#endif
        }

        // Product identifiers for all products capable of being purchased:
        // "convenience" general identifiers for use with Purchasing, and their store-specific identifier
        // counterparts for use with and outside of Unity Purchasing. Define store-specific identifiers
        // also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)

        // General product identifiers for the consumable, non-consumable, and subscription products.
        // Use these handles in the code to reference which product to purchase. Also use these values
        // when defining the Product Identifiers on the store. Except, for illustration purposes, the
        // kProductIDSubscription - it has custom Apple and Google identifiers. We declare their store-
        // specific mapping to Unity Purchasing's AddProduct, below.
        public IAPProductInfo[] AndroidProductInfos;
        public IAPProductInfo[] IOSProductInfos;
    }

    [AddComponentMenu("F4A/IAPManager")]
    public class IAPManager : SingletonMono<IAPManager>
#if DEFINE_IAP
    , IStoreListener
#endif
    {
        #region Delegates, Consts
        private const string KeyVersionIapConfig = "VersionIAPConfig";
        private const string NameFileIapConfig = "IAPInfo.txt";
        private bool _isGooglePlayStoreSelected;

        /// <summary>
        /// Occurs when on buy purchase successes.
        /// </summary>
        /// string id
        /// bool modeTest
        /// string receipt
        public static event Action<bool, string, string> OnBuyPurchaseSuccessed = delegate { };
        /// <summary>
        /// 
        /// </summary>
        /// string id, string failureReason
        public static event Action<bool, int, string> OnBuyPurchaseFailed = delegate { };
        //public static Action<IAPProductInfo> Action_Consumable_IAP;
        //      public static Action<IAPProductInfo> Action_NonConsumable_IAP;
        //      public static Action<IAPProductInfo> Action_Subscription_IAP;
        public static event Action<bool> OnRestorePurchases = delegate { };
        #endregion

        [SerializeField]
        private string urlConfigInApp = "";
        [SerializeField]
        private TextAsset textConfigDefault = null;
        private string developerPayload = "2968E3F488F834573401A50B54FA16F1"; // YOMIStudioDeveloperPayload MD5

        public IAPSettingInfo iapSettingInfo = new IAPSettingInfo();
        private bool _isPurchasing = false;
#if DEFINE_IAP
        private IStoreController m_StoreController;
        public IStoreController StoreController
        {
            get { return m_StoreController; }
        }
        // The Unity Purchasing system.
        private IExtensionProvider m_StoreExtensionProvider;
#if RECEIPT_VALIDATION
        private CrossPlatformValidator validator;
#endif
        // The store-specific Purchasing subsystems.

        //		private IAppleExtensions m_AppleExtensions;
        //		private IMoolahExtension m_MoolahExtensions;
        //		private ISamsungAppsExtensions m_SamsungExtensions;
        //		private IMicrosoftExtensions m_MicrosoftExtensions;
        //		private IUnityChannelExtensions m_UnityChannelExtensions;
#endif

        private bool _purchaseInProgress = false;


        private void Start()
        {
            F4ACoreManager.OnDownloadF4AConfigCompleted += F4ACoreManager_OnDownloadF4AConfigCompleted;
        }

        private void OnDestroy()
        {
            F4ACoreManager.OnDownloadF4AConfigCompleted -= F4ACoreManager_OnDownloadF4AConfigCompleted;
        }

        private void F4ACoreManager_OnDownloadF4AConfigCompleted(F4AConfigData configData, bool success)
        {

            if (configData != null && !string.IsNullOrEmpty(configData.urlInAppPurchase))
            {
                urlConfigInApp = configData.urlInAppPurchase;
            }

            if (F4ACoreManager.Instance.IsGetConfigOnline)
            {
                var dataLocal = CPlayerPrefs.GetString(NameFileIapConfig, "");
                if (configData.versionIAP == CPlayerPrefs.GetInt(KeyVersionIapConfig, 0)
                    && !string.IsNullOrEmpty(dataLocal)
                    )
                {
                    if (!string.IsNullOrEmpty(dataLocal))
                    {
                        iapSettingInfo = JsonConvert.DeserializeObject<IAPSettingInfo>(dataLocal);
                    }
                    InitializePurchasing();
                }
                else
                {
                    StartCoroutine(DMCMobileUtils.AsyncGetDataFromUrl(urlConfigInApp, textConfigDefault, (string data) =>
                    {
                        if (!string.IsNullOrEmpty(data))
                        {
                            DataCore.Debug.Log($"F4ACoreManager_OnDownloadF4AConfigCompleted {data}");
                            iapSettingInfo = JsonConvert.DeserializeObject<IAPSettingInfo>(data);
                            CPlayerPrefs.SetInt(KeyVersionIapConfig, configData.versionIAP);
                            CPlayerPrefs.SetString(NameFileIapConfig, data);
                            CPlayerPrefs.Save();
#if UNITY_EDITOR
                            DMCFileUtilities.SaveFile(data, NameFileIapConfig);
#endif
                        }
                        InitializePurchasing();
                    }));
                }
            }
            else
            {
                InitializePurchasing();
            }
        }

        public void InitializePurchasing()
        {
#if DEFINE_IAP
            // If we haven't set up the Unity Purchasing reference
            if (m_StoreController != null)
            {
                return;
            }

            // If we have already connected to Purchasing ...
            if (IsInitialized())
            {
                // ... we are done here.
                return;
            }

            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

#if UNITY_ANDROID
            foreach (var product in iapSettingInfo.AndroidProductInfos)
            {
                if (product.Type == InAppProductType.Consumable)
                {
                    builder.AddProduct(product.Id, ProductType.Consumable);
                }
                else if (product.Type == InAppProductType.NonConsumable)
                {
                    builder.AddProduct(product.Id, ProductType.NonConsumable);
                }
                else if (product.Type == InAppProductType.Subscription)
                {
                    builder.AddProduct(product.Id, ProductType.Subscription, new IDs()
                    { { product.Id, GooglePlay.Name } });
                }
            }

#elif UNITY_IOS
            foreach (var product in iapSettingInfo.IOSProductInfos)
            {
                if (product.Type == InAppProductType.Consumable)
                {
                    builder.AddProduct(product.Id, ProductType.Consumable);
                }
                else if (product.Type == InAppProductType.NonConsumable)
                {
                    builder.AddProduct(product.Id, ProductType.NonConsumable);
                }
                else if (product.Type == InAppProductType.Subscription)
                {
                    builder.AddProduct(product.Id, ProductType.Subscription, new IDs()
            { { product.Id, AppleAppStore.Name } });
                }
            }
#else
#endif
            var module = StandardPurchasingModule.Instance();
            _isGooglePlayStoreSelected = Application.platform == RuntimePlatform.Android && module.appStore == AppStore.GooglePlay;

#if RECEIPT_VALIDATION && !UNITY_EDITOR
            string appIdentifier;
#if UNITY_5_6_OR_NEWER
            appIdentifier = Application.identifier;
#else
            appIdentifier = Application.bundleIdentifier;
#endif
            validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), appIdentifier);
#endif

            // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
            // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
            UnityPurchasing.Initialize(this, builder);
#endif
        }


        public bool IsInitialized()
        {
#if DEFINE_IAP
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
#else
			return false;
#endif
        }

        public bool EnableUnityIAP()
        {
#if DEFINE_IAP
            return iapSettingInfo != null && iapSettingInfo.EnableUnityIAP();
#endif
            return false;
        }

        public bool IsConsumableById(string id)
        {
            var product = GetProductInfoById(id);
            return product != null && product.IsConsumable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public EStatusBuyIAP BuyProductByName(string productName)
        {
            IAPProductInfo[] products = GetAllProductInfo();

            if (products != null)
            {
                var product = products.Where(p => p.Name.Equals(productName)).FirstOrDefault();
                if (product != null)
                {
                    return BuyProductByID(product.Id);
                }
            }
            return EStatusBuyIAP.None;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public EStatusBuyIAP BuyProductByID(string productId)
        {
            DataCore.Debug.Log($"BuyProductByID {productId}");
#if DEFINE_IAP
            //if (iapSettingInfo.isTestIAP) // ONLY TEST
            //{
            //    OnBuyPurchaseSuccessed?.Invoke(productId, true, string.Empty);
            //    return EStatusBuyIAP.Success;
            //}

            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing 
                // system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    _isPurchasing = true;
                    DataCore.Debug.Log(string.Format("Purchasing product asynchronously: '{0}'", product.definition.id));
                    // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                    // asynchronously.
                    m_StoreController.InitiatePurchase(product, developerPayload);

                    //EventsManager.Instance.LogEvent("iap_buy_start", new Dictionary<string, object>
                    //{
                    //    { "id", productId },
                    //    { "name",product.metadata.localizedTitle }
                    //});
                    return EStatusBuyIAP.InProcess;
                }
                // Otherwise ...
                else
                {
                    OnBuyPurchaseFailed?.Invoke(true, 1, "");
                    // ... report the product look-up failure situation  
                    DataCore.Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    return EStatusBuyIAP.NotContainInStore;
                }
            }
            // Otherwise ...
            else
            {
                OnBuyPurchaseFailed?.Invoke(true, 1, "");
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
                // retrying initialization.
                DataCore.Debug.Log("BuyProductID FAIL. Not initialized.");
                return EStatusBuyIAP.NotInitializeIAP;
            }
#else
			return EStatusBuyIAP.None;
#endif
        }




        public IAPProductInfo GetProductRemoveAds()
        {
            IAPProductInfo[] products = GetAllProductInfo();

            if (products != null)
            {
                IAPProductInfo product = products.Where(p => p.IsTypeRemoveAds()).FirstOrDefault();
                return product;
            }
            return null;
        }

        public bool IsRemoveAdsById(string id)
        {
            var product = GetProductInfoById(id);
            return product != null && product.IsTypeRemoveAds();
        }

        public bool IsRemoveAdsByName(string itemName)
        {
            var product = GetProductInfoByName(itemName);
            return product != null && product.IsTypeRemoveAds();
        }

        public EStatusBuyIAP BuyProductRemoveAds()
        {
            IAPProductInfo[] products = GetAllProductInfo();

            if (products != null)
            {
                IAPProductInfo product = products.Where(p => p.IsTypeRemoveAds()).FirstOrDefault();
                return BuyProductByID(product.Id);
            }
            return EStatusBuyIAP.None;
        }

        private bool _restoreInProgress = false;

        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google.
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases(Action<bool> onComplete = null)
        {
            DataCore.Debug.Log("<color=blue>RestorePurchases Start</color>", false);
#if DEFINE_IAP
            // If Purchasing has not yet been set up ...
            if (!IsInitialized())
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                DataCore.Debug.Log("RestorePurchases FAIL. Not initialized.");
                onComplete?.Invoke(false);
                return;
            }

            // If we are running on an Apple device ... 
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                DataCore.Debug.Log("RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                _restoreInProgress = true;

                apple.RestoreTransactions((result) =>
                {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                    // no purchases are available to be restored.
                    DataCore.Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                    _restoreInProgress = false;

                    if (OnRestorePurchases != null)
                    {
                        onComplete?.Invoke(result);
                        OnRestorePurchases(result);
                    }
                });

            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                var android = m_StoreExtensionProvider.GetExtension<IGooglePlayStoreExtensions>();
                _restoreInProgress = true;

                android.RestoreTransactions((result) =>
                {
                    // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                    // no purchases are available to be restored.
                    DataCore.Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                    _restoreInProgress = false;

                    if (OnRestorePurchases != null)
                    {
                        onComplete?.Invoke(result);
                        OnRestorePurchases(result);
                    }
                });
            }
            // Otherwise ...
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                DataCore.Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
                onComplete?.Invoke(false);
            }
#endif
        }


        #region IStoreListener

#if DEFINE_IAP
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            // Purchasing has succeeded initializing. Collect our Purchasing references.
            DataCore.Debug.Log("IAPManager OnInitialized: PASS");

            // Overall Purchasing system, configured with products for this application.
            m_StoreController = controller;
            // Store specific subsystem, for accessing device-specific store features.
            m_StoreExtensionProvider = extensions;

            //			m_AppleExtensions = extensions.GetExtension<IAppleExtensions> ();
            //			m_SamsungExtensions = extensions.GetExtension<ISamsungAppsExtensions> ();
            //			m_MoolahExtensions = extensions.GetExtension<IMoolahExtension> ();
            //			m_MicrosoftExtensions = extensions.GetExtension<IMicrosoftExtensions> ();
            //			m_UnityChannelExtensions = extensions.GetExtension<IUnityChannelExtensions> ();
            //


            Dictionary<string, string> introductory_info_dict = extensions.GetExtension<IAppleExtensions>().GetIntroductoryPriceDictionary();

            bool isActiveSubscription = false;
            bool didBuyRemoveAds = false;
            bool didBuyVIP = false;
            SubscriptionInfo subscriptionInfo = null;
            foreach (var product in m_StoreController.products.all)
            {
                if (product.availableToPurchase)
                {
                    //DataCore.Debug.Log($"product id is: {product.definition.id} - {product.metadata.localizedPriceString}");
                    // this is the usage of SubscriptionManager class

                    if (product.receipt != null)
                    {
                        if (product.definition.type == ProductType.Subscription)
                        {
                            string intro_json = (introductory_info_dict == null || !introductory_info_dict.ContainsKey(product.definition.storeSpecificId)) ? null : introductory_info_dict[product.definition.storeSpecificId];
                            SubscriptionManager p = new SubscriptionManager(product, intro_json);
                            SubscriptionInfo info = p.getSubscriptionInfo();
                            DataCore.Debug.Log("product id is: " + info.getProductId());
                            DataCore.Debug.Log("purchase date is: " + info.getPurchaseDate());
                            DataCore.Debug.Log("subscription next billing date is: " + info.getExpireDate());
                            DataCore.Debug.Log("is subscribed? " + info.isSubscribed().ToString());
                            DataCore.Debug.Log("is expired? " + info.isExpired().ToString());
                            DataCore.Debug.Log("is cancelled? " + info.isCancelled());
                            DataCore.Debug.Log("product is in free trial peroid? " + info.isFreeTrial());
                            DataCore.Debug.Log("product is auto renewing? " + info.isAutoRenewing());
                            DataCore.Debug.Log("subscription remaining valid time until next billing date is: " + info.getRemainingTime());
                            DataCore.Debug.Log("is this product in introductory price period? " + info.isIntroductoryPricePeriod());
                            DataCore.Debug.Log("the product introductory localized price is: " + info.getIntroductoryPrice());
                            DataCore.Debug.Log("the product introductory price period is: " + info.getIntroductoryPricePeriod());
                            DataCore.Debug.Log("the number of product introductory price period cycles is: " + info.getIntroductoryPricePeriodCycles());
                            if (info.isExpired() != Result.True) { 
                                subscriptionInfo = info;
                                isActiveSubscription = true;
                            }
                        }
                        else if (product.definition.type == ProductType.NonConsumable) {
                            DataCore.Debug.Log($"product {product.definition.id} is purchased: Receipt: {product.receipt}");
                            var p = GetProductInfoById(product.definition.id);
                            if (p.eIapReward == EIapReward.RemoveAds)
                            {
                                didBuyRemoveAds = true;
                            }
                            else if (p.eIapReward == EIapReward.Vip)
                            {
                                didBuyRemoveAds = true;
                                didBuyVIP = true;
                            }
                        }
                    }
                }
            }

            if (isActiveSubscription && subscriptionInfo != null)
            {
                UpdateSubscription(subscriptionInfo);
                var expiredTime = DateTime.Now + subscriptionInfo.getRemainingTime();
                var expired = expiredTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                CPlayerPrefs.SetString(ConfigManager.KeyTimeExpiredSubscription, expired);
                CPlayerPrefs.SetInt(ConfigManager.KeyBuySubscriptionSuccess, 1);
                CPlayerPrefs.SetInt(ConfigManager.keyVipIapAds, 0);
                CPlayerPrefs.Save();
            }
            else
            {
                CPlayerPrefs.SetInt(ConfigManager.KeyBuySubscriptionSuccess, 0);
                CPlayerPrefs.SetString(ConfigManager.KeyTimeExpiredSubscription, DateTime.Now.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
                CPlayerPrefs.Save();
            }

            if (didBuyRemoveAds)
            {
                CPlayerPrefs.SetInt(ConfigManager.keyVipIapAdsV2, 1);
                CPlayerPrefs.Save();
            }
            else {
                CPlayerPrefs.SetInt(ConfigManager.keyVipIapAdsV2, 0);
                CPlayerPrefs.Save();
            }

            if (didBuyVIP)
            {
                CPlayerPrefs.SetInt(ConfigManager.keyVipIapV2, 1);
                CPlayerPrefs.Save();
            }
            else {
                CPlayerPrefs.SetInt(ConfigManager.keyVipIapV2, 0);
                CPlayerPrefs.Save();
            }

#if UNITY_ANDROID
            RestorePurchases();
#endif
        }



        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            DataCore.Debug.Log("IAPManager OnInitializeFailed InitializationFailureReason:" + error);
        }
#endif


#if DEFINE_IAP
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {

            try
            {
                var pd = args.purchasedProduct;

                bool validPurchase = true; // Presume valid for platforms with no R.V.

#if RECEIPT_VALIDATION // Local validation is available for GooglePlay, and Apple stores
                DataCore.Debug.Log("IAPService RECEIPT_VALIDATION");
                if (Application.platform == RuntimePlatform.Android ||
                    Application.platform == RuntimePlatform.IPhonePlayer ||
                    Application.platform == RuntimePlatform.OSXPlayer ||
                    Application.platform == RuntimePlatform.tvOS)
                {
                    try
                    {
                        var result = validator.Validate(pd.receipt);
                    }
                    catch (Exception ex)
                    {
                        DataCore.Debug.Log("Invalid receipt, not unlocking content. " + ex.Message);
                        validPurchase = false;
                    }
                }
#endif
                OnBuyPurchaseSuccessed?.Invoke(_isPurchasing, args.purchasedProduct.definition.id, args.purchasedProduct.receipt);

                if (validPurchase)
                {
                    CheckSubscription(pd);
                }

                if (_isPurchasing) {
                    trackPayment(pd);
                }
                _isPurchasing = false;
            }
            catch (Exception ex)
            {
                DataCore.Debug.Log("Failed To ProcessPurchase. " + ex.Message);
            }
            _isPurchasing = false;
            return PurchaseProcessingResult.Complete;
        }

        private void trackPayment(Product p)
        {
            try
            {
                var productId = p.definition.id;
                var price = Decimal.ToDouble(p.metadata.localizedPrice);
                var currency = p.metadata.isoCurrencyCode;
                var transactionId = p.transactionID;

                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(p.receipt);
                    var payload = (string)receipt_wrapper["Payload"];
                    var others = new Dictionary<string, object>
                    {
                        { "receipt", payload }
                    };
                    AnalyticManager.Instance.LogPayment(productId, 1, price, currency, transactionId, others);
                }
                else if (Application.platform == RuntimePlatform.Android)
                {

                    var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(p.receipt);
                    var payload = (string)receipt_wrapper["Payload"];
                    var payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
                    var gpJson = (string)payload_wrapper["json"];
                    var gpSig = (string)payload_wrapper["signature"];


                    // Purchase verification request on Android.
                    var others = new Dictionary<string, object>
                            {
                                { "receipt", gpJson },
                                { "signature", gpSig },
                            };

                    AnalyticManager.Instance.LogPayment(productId, 1, price, currency, transactionId, others);
                }
                var productData  = GetProductInfoById(productId);
                if (productData != null) {
                    var revenue = productData.Price * 0.85f;
                    ConversionValueManager.Instance.AddRevenue(revenue);
                    ConversionValueManager.Instance.SendConversionValueIfNeeded(true);
                }
                GameData.Instance.OnMarkDirty();
                GameData.Instance.SyncUserProfileOnApplicationInBackground(true);
            }
            catch (Exception err)
            {
                DataCore.Debug.Log("Failed to tracking purchase " + err.ToString());
            }
        }


        public void CheckSubscription(Product product)
        {
            if (product.definition.type == ProductType.Subscription)
            {

                SubscriptionManager p = new SubscriptionManager(product, null);
                SubscriptionInfo info = p.getSubscriptionInfo();
                var productId = info.getProductId();
                var price = Decimal.ToDouble(product.metadata.localizedPrice);
                var currency = product.metadata.isoCurrencyCode;

#if UNITY_IOS
                    var transactionId = product.transactionID;
                    var transactionDate = info.getPurchaseDate().ToString("yyyyMMdd");
                    AnalyticManager.Instance.TrackSubscriptionIOS(price, currency, transactionId, transactionDate, productId);
#elif UNITY_ANDROID
                var receipt_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(product.receipt);
                var store = (string)receipt_wrapper["Store"];
                var payload = (string)receipt_wrapper["Payload"];
                var payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
                var original_json_payload_wrapper = (Dictionary<string, object>)MiniJson.JsonDecode((string)payload_wrapper["json"]);
                var sku = info.getSkuDetails();
                var orderId = (string)original_json_payload_wrapper["orderId"];
                var purchaseToken = (string)original_json_payload_wrapper["purchaseToken"];
                var purchaseTime = original_json_payload_wrapper["purchaseTime"].ToString();
                DataCore.Debug.Log("Receipt: " + product.receipt);
                DataCore.Debug.Log($"price: {price}, currency: {currency}, productId: {productId},orderId: {orderId} purchaseToken: {purchaseToken},purchaseTime {purchaseTime}, productId: {productId}");

                if (info.isExpired() == Result.True)
                {
                    var expired = info.getExpireDate().ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    CPlayerPrefs.SetString(ConfigManager.KeyTimeExpiredSubscription, expired);
                    CPlayerPrefs.Save();
                }
                else
                {
                    UpdateSubscription(info);
                }
                if (!_restoreInProgress)
                {
                    AnalyticManager.Instance.TrackSubscriptionAndroid(price, currency, orderId, purchaseToken, purchaseTime, productId);
                }
#endif
            }
            //if (!_restoreInProgress)
            //{
            //    AnalyticManager.Instance.LogPayment(product.definition.storeSpecificId, 1,
            //        Decimal.ToDouble(product.metadata.localizedPrice),
            //        product.metadata.isoCurrencyCode, product.transactionID);
            //}
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            string id = product.definition.storeSpecificId;
            if (OnBuyPurchaseFailed != null)
                OnBuyPurchaseFailed(_isPurchasing, 0, failureReason.ToString());
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.
            _isPurchasing = false;
            DataCore.Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", id, failureReason));
        }
#endif
        #endregion


        public IAPProductInfo[] GetAllProductInfo()
        {
            IAPProductInfo[] products = null;
#if UNITY_ANDROID
            products = iapSettingInfo.AndroidProductInfos;
#elif UNITY_IOS
            products = iapSettingInfo.IOSProductInfos;
#endif
            if (products != null)
            {
                return products;
            }
            else
            {
                return new IAPProductInfo[0];
                //return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public IAPProductInfo GetProductInfoById(string productId)
        {
            IAPProductInfo[] products = GetAllProductInfo();
            if (!string.IsNullOrEmpty(productId) && products != null)
            {
                return products.Where(item => String.Equals(productId, item.Id)).FirstOrDefault();
            }
            else return null;
        }

        public IAPProductInfo GetProductInfoByName(string productName)
        {
            IAPProductInfo[] products = GetAllProductInfo();

            if (products != null)
            {
                return products.Where(item => String.Equals(productName, item.Name)).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public IAPProductInfo GetProductInfoByIndex(int index)
        {
            IAPProductInfo[] products = GetAllProductInfo();
            if (products != null && 0 <= index && index < products.Length)
            {
                return products[index];
            }
            else return null;
        }

        public bool ProductAvailableToPurchase(IAPProductInfo product)
        {
            if (product != null)
            {
#if DEFINE_IAP
                if (IsInitialized() && DMCMobileUtils.IsInternetAvailable())
                {
                    var productStore = m_StoreController.products.all.Where(pro => pro.definition.id.Equals(product.Id)).FirstOrDefault();
                    if (productStore != null && productStore.metadata != null)
                    {
                        return productStore.availableToPurchase;
                    }
                }
                return false;
#endif
            }
            return false;

        }

        public string GetIsoCurrencyCodeByName(string productName)
        {
            var product = GetProductInfoByName(productName);
            return GetIsoCurrencyCode(product);
        }

        public string GetIsoCurrencyCodeById(string productId)
        {
            var product = GetProductInfoById(productId);
            return GetIsoCurrencyCode(product);
        }

        private string GetIsoCurrencyCode(IAPProductInfo product)
        {
            if (product != null)
            {
#if DEFINE_IAP
                if (IsInitialized() && DMCMobileUtils.IsInternetAvailable())
                {
                    var productStore = m_StoreController.products.all.Where(pro => pro.definition.id.Equals(product.Id)).FirstOrDefault();
                    if (productStore != null && productStore.metadata != null)
                    {
                        return productStore.metadata.isoCurrencyCode;
                    }
                }
                return "$";
#endif
            }

            return "$";
        }

        public float GetProductPriceByName(string productName)
        {
            var product = GetProductInfoByName(productName);
            return GetProductPrice(product);
        }

        public float GetProductPriceById(string productId)
        {
            var product = GetProductInfoById(productId);
            return GetProductPrice(product);
        }

        private float GetProductPrice(IAPProductInfo product)
        {
            if (product != null)
            {
#if DEFINE_IAP
                if (IsInitialized() && DMCMobileUtils.IsInternetAvailable())
                {
                    var productStore = m_StoreController.products.all.Where(pro => pro.definition.id.Equals(product.Id)).FirstOrDefault();
                    if (productStore != null && productStore.metadata != null)
                    {
                        return (float)productStore.metadata.localizedPrice;
                    }
                }
                return product.Price;
#endif
            }

            return 1.99f;
        }

        public string GetProductPriceStringByName(string productName)
        {
            var product = GetProductInfoByName(productName);
            return GetProductPriceString(product);
        }

        public string GetProductPriceStringById(string productId)
        {
            var product = GetProductInfoById(productId);
            return GetProductPriceString(product);
        }

        private string GetProductPriceString(IAPProductInfo product)
        {
            if (product != null)
            {
                DataCore.Debug.Log($"GetProductPriceString {product.Id}");
#if DEFINE_IAP
#if !UNITY_EDITOR
                if (IsInitialized() && DMCMobileUtils.IsInternetAvailable())
                {
                    var productStore = m_StoreController.products.all.Where(pro => pro.definition.id.Equals(product.Id)).FirstOrDefault();
                    if (productStore != null && productStore.metadata != null)
                    {
                        return productStore.metadata.localizedPriceString;
                    }
                }
#endif
                return product.Price.ToString() + "$";
#endif
            }

            return "1.99$";
        }

        public IAPProductInfo[] GetProductHasTag()
        {
            var products = GetAllProductInfo();
            if (products != null)
            {
                return products.Where(product => product.HasTag()).ToArray();
            }
            return null;
        }

        public IAPProductInfo[] GetProductWithTag(ETagProduct tagProduct)
        {
            var products = GetAllProductInfo();
            if (products != null)
            {
                return products.Where(product => product.InAppTag == tagProduct).ToArray();
            }
            return null;
        }

        public int GetIndexProduct(string id)
        {
            var products = GetAllProductInfo();
            if (products == null) return -1;
            for (int counter = 0; counter < products.Length; counter++)
            {
                if (products[counter].Id.Equals(id)) return counter;
            }
            return -1;
        }

        public bool CheckProductBought(string productID)
        {
#if DEFINE_IAP
            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(productID);
                return product != null && product.hasReceipt;
            }
            else
            {
                return false;
            }
#else
            return false;
#endif
        }

        public bool IsSubscriprion()
        {
            var isBuySubscription = CPlayerPrefs.GetInt(ConfigManager.KeyBuySubscriptionSuccess, 0);
            if (isBuySubscription == 0)
                return false;

            var timeEnd = CPlayerPrefs.GetString(ConfigManager.KeyTimeExpiredSubscription, "20180127000000");
            DateTime expired = DateTime.ParseExact(timeEnd, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            DataCore.Debug.Log($"IsSubscriprion {isBuySubscription} {expired}");
            return expired >= DateTime.Now;
        }

        private void UpdateSubscription(SubscriptionInfo info)
        {
            try
            {
                if (info.isExpired() == Result.False)
                {
                    CPlayerPrefs.SetInt(ConfigManager.KeyBuySubscriptionSuccess, 1);
                    var expiredTime = DateTime.Now + info.getRemainingTime();
                    var expired = expiredTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    CPlayerPrefs.SetString(ConfigManager.KeyTimeExpiredSubscription, expired);
                    var purchaseTime = info.getPurchaseDate().ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    CPlayerPrefs.SetString(ConfigManager.KeyTimeBeginSubscription, purchaseTime);
                    CPlayerPrefs.Save();
                }
            }
            catch (Exception ex)
            {
                DataCore.Debug.LogError($"Failed UpdateSubscription: {ex.Message}");
            }

        }
        public int GetDaySubscription()
        {
            var isBuySubscription = CPlayerPrefs.GetInt(ConfigManager.KeyBuySubscriptionSuccess, 0);
            if (isBuySubscription == 0)
                return 0;

            var timeBegin = CPlayerPrefs.GetString(ConfigManager.KeyTimeBeginSubscription, "20180127000000");
            DateTime oDate = DateTime.ParseExact(timeBegin, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            System.TimeSpan diff1 = DateTime.Now.Subtract(oDate);

            var dayCountDown = 0;
            if (diff1.TotalDays < 1)
                dayCountDown = 0;
            else
                dayCountDown = (int)diff1.TotalDays;
            return dayCountDown;
        }

#if UNITY_EDITOR
        [ContextMenu("Save Info In App Purchase")]
        public void SaveInfo()
        {
            string pathFolder = Application.dataPath + "/Common/Data";
            DMCMobileUtils.CreateDirectory(pathFolder);
            string path = Path.Combine(pathFolder, NameFileIapConfig);
            string str = JsonConvert.SerializeObject(iapSettingInfo);
            System.IO.StreamWriter file = new System.IO.StreamWriter(path);
            file.WriteLine(str);
            file.Close();
            UnityEditor.AssetDatabase.Refresh();
        }

        [ContextMenu("Load Info In App Purchase")]
        public void LoadInfo()
        {
            string path = Application.dataPath + "/Common/Data/" + NameFileIapConfig;
            string text = System.IO.File.ReadAllText(path);
            iapSettingInfo = JsonConvert.DeserializeObject<IAPSettingInfo>(text);
        }
#endif
    }
}