using com.F4A.MobileThird;
using DataCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupPromotion : BasePopup
{

    public const int MIN_GAME_FOR_FIRST_TIME_SHOW = 5;
    public const int MAX_PROMOTION_PER_DAY = 4;
    public const int HOUR_NEED_TO_SHOW_PROMOTION = 6;
    private const int TIME_REFRESH = 0;

    private Action onPopupClose;

    public override void Init()
    {
        base.Init();

    }
    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {

    }

    private int promotionBannerRemain = MAX_PROMOTION_PER_DAY;
    public bool CanShow(bool isTutorial = false)
    {
        this.promotionBannerRemain = GameData.Instance.SavedPack.SaveData.PromotionBannerRemain;
        if (IsNextDay())
        {
            ResetData();
        }
        int currentHour = DateTime.Now.Hour;
        var LastTimeShowPromotion = GameData.Instance.SavedPack.SaveData.LastTimeShowPromotion;
        DataCore.Debug.Log($"PromotionBanner CanShow: isTutorial {isTutorial} PlayedGame: {GameData.Instance.PlayedGame()} LastTimeShowPromotion: {LastTimeShowPromotion}", false);
        if (isTutorial)
        {
            GameData.Instance.SavedPack.SaveData.LastDayPromotionBanner = DateTime.UtcNow.Day;
            GameData.Instance.SavedPack.SaveData.PromotionBannerRemain = MAX_PROMOTION_PER_DAY - 1;
            GameData.Instance.SavedPack.SaveData.LastTimeShowPromotion = currentHour;
            GameData.Instance.RequestSaveGame();
            return true;
        }

        if (GameData.Instance.PlayedGame() > MIN_GAME_FOR_FIRST_TIME_SHOW &&
            this.promotionBannerRemain >= 0 &&
            Mathf.Abs(currentHour - LastTimeShowPromotion) >= HOUR_NEED_TO_SHOW_PROMOTION)
        {
            GameData.Instance.SavedPack.SaveData.LastTimeShowPromotion = currentHour;
            GameData.Instance.RequestSaveGame();
            return true;
        }

        return false;
    }

    private void ResetData()
    {
        GameData.Instance.SavedPack.SaveData.PromotionBannerRemain = MAX_PROMOTION_PER_DAY;
        this.promotionBannerRemain = MAX_PROMOTION_PER_DAY;
        GameData.Instance.RequestSaveGame();
    }

    private bool IsNextDay()
    {
        DateTime now = DateTime.Now;
        int lastDay = GameData.Instance.SavedPack.SaveData.LastDayPromotionBanner;
        if (lastDay != now.Day)
        {
            if (Mathf.Abs(lastDay - now.Day) > 1)
            {
                if (now.Hour >= TIME_REFRESH)
                {
                    lastDay = now.Day;
                    GameData.Instance.SavedPack.SaveData.LastDayPromotionBanner = lastDay;
                }
                else
                {
                    lastDay = now.Day - 1;
                    GameData.Instance.SavedPack.SaveData.LastDayPromotionBanner = lastDay;
                }

                return true;
            }
            else
            {
                if (now.Hour >= TIME_REFRESH)
                {
                    lastDay = now.Day;
                    GameData.Instance.SavedPack.SaveData.LastDayPromotionBanner = lastDay;
                    return true;
                }
            }
        }
        return false;
    }

    public override void SetData(object[] data)
    {
        onPopupClose = (Action)data[0];
        base.SetData(data);
    }

    public override void Close()
    {
        GameData.Instance.SavedPack.SaveData.PromotionBannerRemain--;
        GameData.Instance.RequestSaveGame();
        onPopupClose?.Invoke();
        IAPManager.OnBuyPurchaseSuccessed -= IAPManager_OnBuyPurchaseSuccessed;
        IAPManager.OnBuyPurchaseFailed -= IAPManager_OnBuyPurchaseFailed;
        base.Close();
        GameManager.Instance.RemoveObjList(this);
    }


    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        ShareUIManager.Instance.ShowSharedUI(SceneSharedEle.COIN, false);
        GameManager.Instance.SetStepGame(StepGameConstants.StepComplete);
        IAPManager.OnBuyPurchaseSuccessed += IAPManager_OnBuyPurchaseSuccessed;
        IAPManager.OnBuyPurchaseFailed += IAPManager_OnBuyPurchaseFailed;
        base.Open();
    }    

    public void OnBtnBuy()
    {
    }

    private void IAPManager_OnBuyPurchaseSuccessed(bool shouldShowUI, string id, string receipt)
    {
        var product = IAPManager.Instance.GetProductInfoById(id);
        if (product != null)
        {
            GameData.Instance.IncreaseHint(5, ConfigManager.GameData.ResourceEarnSource.buy_iap);
            GameData.Instance.RequestSaveGame();
        }
        Close();
    }
    private void IAPManager_OnBuyPurchaseFailed(bool shouldShowUI, int type, string msg)
    {
        Close();
    }
}
