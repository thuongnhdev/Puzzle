using System;
using System.Collections;
using System.Collections.Generic;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using com.F4A.MobileThird;

public class UILiveEventReward : BasePanel
{
    
    
    [SerializeField]
    private TextMeshProUGUI tmpCoin;

    public override void Init()
    {

    }

    public override void SetData(object[] data)
    {
        base.SetData(data);
    }

    //private void CoundownTime()
    //{
    //    var today = System.DateTime.Now;
    //    var secondNow = (today.Hour * 3600) + (today.Minute * 60) + today.Second;
    //    var secondAdd = 86400 - secondNow;
    //    totalTime = secondAdd;
    //    var todayNow = System.DateTime.Now;
    //    var nextDay = todayNow.AddSeconds(totalTime);
    //    System.TimeSpan diff1 = nextDay.Subtract(today);
    //    tmpDescrription.text = ConfigManager.DescriptionDailyGift + " " + diff1.Hours + "H" + " " + diff1.Minutes + "M";
    //}
    //float totalTime = 0;

    private void Update()
    {
        //if (totalTime > 0)
        //{
        //    totalTime = Mathf.Max(0, totalTime - Time.deltaTime);
        //    var timeSpan = System.TimeSpan.FromSeconds(totalTime);
        //    tmpDescrription.text = ConfigManager.DescriptionDailyGift + " " + timeSpan.Hours.ToString("00") + "H" + " " +
        //                    timeSpan.Minutes.ToString("00") + "M";

        //}

    }


    public override void Open()
    {
        base.Open();

        //CoundownTime();
        //tmpCoin.text = "x" + ConfigManager.InksDailyRewardGift.ToString();
        //tmpDay.text = "Day " + IAPManager.Instance.GetDaySubscription();

    }

    public void BtnClaimRewardClick()
    {

    }

    public void BtnContinuePress()
    {
        GameData.Instance.IncreaseInks(ConfigManager.InksDailyRewardGift, ConfigManager.GameData.ResourceEarnSource.daily_subscription_reward);
        GameData.Instance.RequestSaveGame();
        ShareUIManager.Instance.UpdateCurrencyData(1.0f);

        Close();

        CPlayerPrefs.SetInt(ConfigManager.DailySubscriptionDone, IAPManager.Instance.GetDaySubscription());
        CPlayerPrefs.Save();
    }

    public override void Close()
    {
        base.Close();
    }

    public override void OnSwipeLeft()
    {
        Close();
    }
}
