using com.F4A.MobileThird;
using DataCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardBuyIap : SingletonMono<RewardBuyIap>
{
    private void OnEnable()
    {
        IAPManager.OnBuyPurchaseSuccessed += IAPManager_OnBuyPurchaseSuccessed;
        IAPManager.OnBuyPurchaseFailed += IAPManager_OnBuyPurchaseFailed;
    }

    private void OnDisable()
    {
        IAPManager.OnBuyPurchaseSuccessed -= IAPManager_OnBuyPurchaseSuccessed;
        IAPManager.OnBuyPurchaseFailed -= IAPManager_OnBuyPurchaseFailed;
    }


    private void IAPManager_OnBuyPurchaseSuccessed(bool shouldShowUI, string id, string receipt)
    {
        if (IAPManager.Instance == null) return;

        try
        {
            var product = IAPManager.Instance.GetProductInfoById(id);
            if (product != null)
            {
                switch (product.eIapReward)
                {
                    case EIapReward.Coin:
                        {
                            ShowPurchaseStatus(shouldShowUI, () =>
                            {
                                // Init logic success here
                                GameData.Instance.IncreaseInks(product.Coin, ConfigManager.GameData.ResourceEarnSource.buy_iap);
                                ShareUIManager.Instance.UpdateCurrencyData(0.0f);
                            });
                        }
                        break;
                    case EIapReward.Hint:
                        {
                            ShowPurchaseStatus(shouldShowUI, () =>
                            {
                                // Init logic success here
                                GameData.Instance.IncreaseHint(product.Coin, ConfigManager.GameData.ResourceEarnSource.buy_iap);
                                ShareUIManager.Instance.UpdateCurrencyData(0.0f);
                            });
                        }
                        break;
                    case EIapReward.RemoveAds:
                        {
                            ShowPurchaseStatus(shouldShowUI, () =>
                            {
                                CPlayerPrefs.SetInt(ConfigManager.keyVipIapAdsV2, 1);
                                CPlayerPrefs.Save();
                                GameData.Instance.UpdateRemoveAd();

                            });
                        }
                        break;
                    case EIapReward.Vip:
                        {
                            ShowPurchaseStatus(shouldShowUI, () =>
                            {
                                CPlayerPrefs.SetInt(ConfigManager.keyVipIapV2, 1);
                                CPlayerPrefs.SetInt(ConfigManager.keyVipIapAdsV2, 1);
                                CPlayerPrefs.Save();
                                GameData.Instance.UpdateBuyPremium();
                                //GameData.Instance.UpdateHint(ConfigManager.keyVipNumberUntimitedHint);
                                ShareUIManager.Instance.UpdateCurrencyData(0.0f);
                            });
                        }
                        break;
                    case EIapReward.Unlock:
                        {
                            ShowPurchaseStatus(shouldShowUI, () =>
                            {
                            });
                        }
                        break;
                    case EIapReward.Weekly:
                        {
                            ShowPurchaseStatus(shouldShowUI, () =>
                            {
                                GameData.Instance.IncreaseInks(product.Coin, ConfigManager.GameData.ResourceEarnSource.daily_subscription_reward);
                                ShareUIManager.Instance.UpdateCurrencyData(0.0f);
                            });
                        }
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            DataCore.Debug.LogException(ex);
        }

    }

    private void ShowPurchaseStatus(bool shouldShowUI, Action completed) {
        if (shouldShowUI)
        {
            UIManager.Instance.ShowPurchaseStatus(true, () =>
            {
                completed?.Invoke();
            });
        }
        else {
            completed?.Invoke();
        }
    }

    private void IAPManager_OnBuyPurchaseFailed(bool shouldShowUI, int type, string msg)
    {
        if (shouldShowUI) {
            UIManager.Instance.ShowPurchaseFail(type, () =>
            {
            });
        }        
    }
}
