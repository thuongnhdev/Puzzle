using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageTab : BaseShopTab
{
    public override void OnClick()
    {
        base.OnClick();

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            UIManager.Instance.ShowPopupNoInternet();
            return;
        }

        // Init IAP Native
        OnPurchaseFailed();
    }

    protected override void OnPurchaseSuccess()
    {
        UIManager.Instance.ShowPurchaseStatus(true, () =>
        {
            // Init logic success here
        });
    }

    public void onClickSubscription()
    {
        UIManager.Instance.ShowUISubscription();
    }
}
