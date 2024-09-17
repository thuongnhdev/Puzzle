using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.F4A.MobileThird;
using DataCore;
using TMPro;
using System;

public class UIShop : BasePanel
{
    private BasePanel previousPanel;

    [SerializeField]
    private TextMeshProUGUI tmpPriceSubscription;

    [SerializeField]
    private NumberCounter txtNumberCounter;

    [SerializeField]
    private string productIdAndroidSubscriptionMin = "", productIdIOSSubscriptionMin;

    [SerializeField] private RectTransform container;

    [SerializeField] private GameObject IncredibleIapPanel;

    public override void SetData(object[] data)
    {
        base.SetData(data);
        previousPanel = (BasePanel)data[0];
    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        container.transform.localPosition = new Vector3(container.transform.localPosition.x, 0, container.transform.localPosition.z);

        UpdateCurrencyData(0);

#if UNITY_ANDROID
        var productInfo = IAPManager.Instance.GetProductInfoById(productIdAndroidSubscriptionMin);
        if (productInfo != null)
        {
            var price = IAPManager.Instance.GetProductPriceStringById(productInfo.Id);
            tmpPriceSubscription.text = "from " + price + "/ week";
        }
        else
        {
            var price = IAPManager.Instance.GetProductPriceStringById(productIdAndroidSubscriptionMin);
            tmpPriceSubscription.text = "from " + price + "/ week";
        }
#elif UNITY_IOS
        var productInfo = IAPManager.Instance.GetProductInfoById(productIdIOSSubscriptionMin);

        if (productInfo != null)
        {
            var price = IAPManager.Instance.GetProductPriceStringById(productInfo.Id);
            tmpPriceSubscription.text = "from " + price + "/ week";
        }
        else
        {
            var price = IAPManager.Instance.GetProductPriceStringById(productIdAndroidSubscriptionMin);
            tmpPriceSubscription.text = "from " + price + "/ week";
        }
#endif

        base.Open();

        if (CPlayerPrefs.GetInt(ConfigManager.KeyIncredibleIap, 0) == 1)
            IncredibleIapPanel.SetActive(false);
    }

    public override void OnUpdateInk(float animDuration, Action onComplete = null)
    {
        UpdateCurrencyData(animDuration, onComplete);
    }

    public void UpdateCurrencyData(float animDuration, Action onComplete = null)
    {
        int newData = GameData.Instance.SavedPack.SaveData.Coin;
        txtNumberCounter.PlayAnim(newData, animDuration, onComplete);

    }

    public void onClickSubscription()
    {
        UIManager.Instance.UISubscription.Open();
    }

    public void OnShowSubscription()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        UIManager.Instance.ShowUISubscription();
    }

    private bool isBuyIapCoinAndHint = false;
    public void OnAddHint()
    {
        isBuyIapCoinAndHint = true;
    }
    private void OnEnable()
    {
        ShareUIManager.OnUpdateInk += OnUpdateInk;
        IAPManager.OnBuyPurchaseSuccessed += IAPManager_OnBuyPurchaseSuccessed;
    }

    private void OnDisable()
    {
        ShareUIManager.OnUpdateInk -= OnUpdateInk;
        IAPManager.OnBuyPurchaseSuccessed -= IAPManager_OnBuyPurchaseSuccessed;
    }
    private void IAPManager_OnBuyPurchaseSuccessed(bool shouldShowUI, string id, string receipt)
    {
        var product = IAPManager.Instance.GetProductInfoById(id);
        if (product != null && isBuyIapCoinAndHint)
        {
            if (string.Compare(product.Id, "yomi.studio.art.story.package.promotion") == 0 && product.eIapReward == EIapReward.Coin)
            {
                GameData.Instance.IncreaseHint(5, ConfigManager.GameData.ResourceEarnSource.buy_iap);
                IncredibleIapPanel.SetActive(false);
                CPlayerPrefs.SetInt(ConfigManager.KeyIncredibleIap, 1);
                CPlayerPrefs.Save();
            }
        }
        isBuyIapCoinAndHint = false;
    }

    public override void Close()
    {
        base.Close();
        if (previousPanel != null)
        {
            previousPanel.Open();
            previousPanel = null;
        }
        GameManager.Instance.RemoveObjList(this);
    }

    public override void OnSwipeLeft()
    {
        Close();
    }
}

[System.Serializable]
public enum IAPType
{
    INCREDIBLE, INKS, REMOVE_ADS, SUBCRIPTION, PREMIUM_SUBCRIPTION
}
