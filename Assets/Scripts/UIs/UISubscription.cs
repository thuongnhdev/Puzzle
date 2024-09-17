using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using com.F4A.MobileThird;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UISubscription : BasePanel
{

    private const int TIME_REFRESH = 0;
    private const int SUBSCRIPTION_PER_DAY = 2;

    [SerializeField]
    private string productIdWeekAndroid = "", productIdWeekIOS;

    [SerializeField]
    private string productIdMonthAndroid = "", productIdMonthIOS;

    [SerializeField]
    private GameObject[] objChoiceHighLight;


    [SerializeField] private TextMeshProUGUI txtWeeklyPricing;
    [SerializeField] private TextMeshProUGUI txtMonthlyPricing;
    [SerializeField] private TextMeshProUGUI txtSubscriptionPeriods;

    [SerializeField] private GameObject loadingSubscription;

    private IAPProductInfo productInfo = null;

    private TypeBuyIap typeBuyIapCurrent;

    public override void Init()
    {
        if (IsNextDay())
        {
            RefreshData();
        }
    }

    public bool CanShow()
    {
        if (IsNextDay())
        {
            RefreshData();
        }
        if (GameData.Instance.SavedPack.SaveData.SubscriptionRemain > 0 && CheckShowAfter12h())
        {
            return true;
        }

        return false;
    }

    public string GetProductId(TypeBuyIap typeBuyIap)
    {
#if UNITY_ANDROID
        if (typeBuyIap == TypeBuyIap.WeekLy)
            return productIdWeekAndroid;
        else
            return productIdMonthAndroid;
#elif UNITY_IOS
        if (typeBuyIap == TypeBuyIap.WeekLy)
            return productIdWeekIOS;
        else
            return productIdMonthIOS;
#else
            return "";
#endif
    }
    public enum TypeBuyIap
    {
        WeekLy,
        MonthLy
    }
    public void InitData(TypeBuyIap typeBuyIap)
    {
        var productId = GetProductId(typeBuyIap);
#if DEFINE_IAP
        if (!string.IsNullOrEmpty(productId))
        {
            productInfo = IAPManager.Instance.GetProductInfoById(productId);

            if (productInfo != null)
            {
#if UNITY_ANDROID
                if (typeBuyIap == TypeBuyIap.WeekLy)
                    productIdWeekAndroid = productInfo.Id;
                else
                    productIdMonthAndroid = productInfo.Id;
#elif UNITY_IOS
                if (typeBuyIap == TypeBuyIap.WeekLy)
                    productIdWeekIOS = productInfo.Id;
                else
                    productIdMonthIOS = productInfo.Id;
#endif
                var price = GetSubscriptionLocalizePricing(typeBuyIap);
                txtSubscriptionPeriods.text = price;
            }
        }

#endif
    }


    string GetSubscriptionLocalizePricing(TypeBuyIap typeBuyIap)
    {
        var productId = GetProductId(typeBuyIap);
        if (!string.IsNullOrEmpty(productId))
        {
            var localPrice = IAPManager.Instance.GetProductPriceStringById(productId);
            if (localPrice != null)
            {
                if (typeBuyIap == TypeBuyIap.WeekLy)
                {
                    return $"3-days trial that auto-renews at {localPrice}/week.";
                }
                else if (typeBuyIap == TypeBuyIap.MonthLy)
                {
                    return $"Subscription will auto-renew at {localPrice}/month.";
                }
            }
        }
        return "";
    }
    string GetSubscriptionLocalizePricingForButtons(TypeBuyIap typeBuyIap)
    {
        var productId = GetProductId(typeBuyIap);
        if (!string.IsNullOrEmpty(productId))
        {
            var localPrice = IAPManager.Instance.GetProductPriceStringById(productId);
            if (localPrice != null)
            {
                if (typeBuyIap == TypeBuyIap.WeekLy)
                {
                    return $"{localPrice} per week";
                }
                else if (typeBuyIap == TypeBuyIap.MonthLy)
                {
                    return $"{localPrice} per month";
                }

            }
        }
        return "";
    }


    public override void SetData(object[] data)
    {
        base.SetData(data);

    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        ShareUIManager.Instance.ShowSharedUI(SceneSharedEle.SETTING, false);
        objChoiceHighLight[(int)TypeBuyIap.MonthLy].SetActive(false);
        objChoiceHighLight[(int)TypeBuyIap.WeekLy].SetActive(false);
        txtWeeklyPricing.text = GetSubscriptionLocalizePricingForButtons(TypeBuyIap.WeekLy);
        txtMonthlyPricing.text = GetSubscriptionLocalizePricingForButtons(TypeBuyIap.MonthLy);
        base.Open();
        DOVirtual.DelayedCall(0.1f, () =>
        {
            OnWeekLy();
        });


    }

    public void OpenAuto()
    {
        Open();
        GameData.Instance.SavedPack.SaveData.SubscriptionRemain--;
        GameData.Instance.RequestSaveGame();

        var timeBegin = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        PlayerPrefs.SetString(ConfigManager.KeyTimeOpenSubscription, timeBegin);
        PlayerPrefs.Save();
    }
    public void OnWeekLy()
    {
        SoundController.Instance.PlaySfxClick();

        objChoiceHighLight[(int)TypeBuyIap.MonthLy].SetActive(false);
        objChoiceHighLight[(int)TypeBuyIap.WeekLy].SetActive(true);
        typeBuyIapCurrent = TypeBuyIap.WeekLy;
        InitData(typeBuyIapCurrent);
    }

    public void OnMonthLy()
    {
        SoundController.Instance.PlaySfxClick();

        objChoiceHighLight[(int)TypeBuyIap.MonthLy].SetActive(true);
        objChoiceHighLight[(int)TypeBuyIap.WeekLy].SetActive(false);
        typeBuyIapCurrent = TypeBuyIap.MonthLy;
        InitData(typeBuyIapCurrent);
    }
    public void BtnContinuePress()
    {
        SoundController.Instance.PlaySfxClick();
        loadingSubscription.SetActive(true);
        DOVirtual.DelayedCall(3.0f, () =>
        {
            loadingSubscription.SetActive(false);
        });

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            UIManager.Instance.ShowPopupNoInternet();
            return;
        }

#if DEFINE_IAP
        if (productInfo != null)
        {
            IAPManager.Instance.BuyProductByID(productInfo.Id);
        }
        else
        {
            var productId = GetProductId(this.typeBuyIapCurrent);
            if (!string.IsNullOrEmpty(productId))
                IAPManager.Instance.BuyProductByID(productId);
        }
#endif
    }

    public void OnMoreInfo()
    {
        SoundController.Instance.PlaySfxClick();
        Application.OpenURL(ConfigManager.UrlMoreInfo);
    }

    public void OnRestore()
    {
        SoundController.Instance.PlaySfxClick();
        IAPManager.Instance.RestorePurchases((result) => {
            if (result)
                UIManager.Instance.ShowPopupNotice(ConfigManager.MSG_RESTORE_CODE_SUCCESS);
            else
                UIManager.Instance.ShowPopupNotice(ConfigManager.MSG_RESTORE_CODE_FAILED);
        });
    }
    public override void Close()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        GameManager.Instance.RemoveObjList(this);
    }

    public override void OnSwipeLeft()
    {
        Close();
    }

    private void RefreshData()
    {
        GameData.Instance.SavedPack.SaveData.SubscriptionRemain = SUBSCRIPTION_PER_DAY;

        GameData.Instance.RequestSaveGame();
    }
    private bool IsNextDay()
    {
        DateTime now = DateTime.UtcNow;
        int lastDay = GameData.Instance.SavedPack.SaveData.LastDaySubscription;
        if (lastDay != now.Day)
        {
            if (Mathf.Abs(lastDay - now.Day) > 1)
            {
                if (now.Hour >= TIME_REFRESH)
                {
                    lastDay = now.Day;
                    GameData.Instance.SavedPack.SaveData.LastDaySubscription = lastDay;
                }
                else
                {
                    lastDay = now.Day - 1;
                    GameData.Instance.SavedPack.SaveData.LastDaySubscription = lastDay;
                }

                return true;
            }
            else
            {
                if (now.Hour >= TIME_REFRESH)
                {
                    lastDay = now.Day;
                    GameData.Instance.SavedPack.SaveData.LastDaySubscription = lastDay;
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckShowAfter12h()
    {
        var timeBegin = PlayerPrefs.GetString(ConfigManager.KeyTimeOpenSubscription, "0");
        if (string.Compare(timeBegin, "0") == 0)
            return true;

        DateTime oDate = DateTime.ParseExact(timeBegin, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        System.TimeSpan diff1 = DateTime.Now.Subtract(oDate);
        var timeCheck = (diff1.TotalHours - 12);
        if (oDate.Day != DateTime.Now.Day)
            return false;

        if (timeCheck >= 0)
            return true;
        return false;
    }
}
