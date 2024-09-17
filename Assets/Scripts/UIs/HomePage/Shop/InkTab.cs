using DataCore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class InkTab : BaseShopTab
{
    [SerializeField] private TextMeshProUGUI amountTxt;
    [SerializeField] private int amount;
    private void Awake()
    {
        amountTxt.SetText("x" + amount);
    }

    public override void OnClick()
    {
        base.OnClick();

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            UIManager.Instance.ShowPopupNoInternet();
            return;
        }


        OnPurchaseSuccess();
    }

    protected override void OnPurchaseSuccess()
    {
        UIManager.Instance.ShowPurchaseStatus(true, () =>
        {
            GameData.Instance.IncreaseInks(amount, ConfigManager.GameData.ResourceEarnSource.buy_iap);
            ShareUIManager.Instance.UpdateCurrencyData(1.0f);
        });
    }
}
