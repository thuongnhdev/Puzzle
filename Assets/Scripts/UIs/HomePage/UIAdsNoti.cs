using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.F4A.MobileThird;
using DataCore;
using TMPro;
using System;

public class UIAdsNoti : BasePanel
{
    private BasePanel previousPanel;

    [SerializeField]
    private TextMeshProUGUI tmpPriceSubscription;

    [SerializeField]
    private string productIdAndroidSubscriptionMin = "", productIdIOSSubscriptionMin;

    private Action _onComplete = null;
    public override void SetData(object[] data)
    {
        base.SetData(data);
        if (data.Length >= 1)
        {
            _onComplete = (Action)data[0];
        }
    }

    public override void Open()
    {
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

       AdsLogic.SetTimeBeginArtBlitzComplete();
    }

    public void onClickSubscription()
    {
        //Close();
        GameManager.Instance.SetStepGame(StepGameConstants.StepComplete);
        SoundController.Instance.PlaySfxClick();
        UIManager.Instance.ShowUISubscription();
    }

    public void onClickContinuePress()
    {
        AnalyticManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.shown_1st_interstitial_ad);
        SoundController.Instance.MuteBgMusic(false);
        Close();
    }

    public override void Close()
    {
        base.Close();
        _onComplete?.Invoke();
    }

    public override void OnSwipeLeft()
    {
        Close();
    }
}


