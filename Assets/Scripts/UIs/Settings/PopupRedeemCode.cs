using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DataCore;
using com.F4A.MobileThird;
public class PopupRedeemCode : BasePopup
{
    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private TextMeshProUGUI tmpUserId;

    public override void Init()
    {
        UIReset();
    }

    protected override void UIReset()
    {
        base.UIReset();
        inputField.text = "";
    }

    public void OnRedeemTap()
    {
        SoundController.Instance.PlaySfxClick();
        var GET_DEVICE_ID = ConfigManager.GET_DEVICE_ID;
        var ENABLE_DEBUG_MODE = ConfigManager.ENABLE_DEBUG_MODE;

        if (inputField.text.Contains(GET_DEVICE_ID))
        {
            var userId = GameData.Instance.GetUserId();
            tmpUserId.text = userId.ToString();
        } else if (inputField.text.Contains(ENABLE_DEBUG_MODE))
        {
            CPlayerPrefs.SetBool(ConfigManager.EnableDebugMode, true);
            CPlayerPrefs.Save();
            Close();
        }
        else
        {
            var redeemCode = inputField.text;
            DataCore.Debug.Log($"redeemCode: {redeemCode}");
            if (string.IsNullOrEmpty(redeemCode)) {
                return;
            }

            if (FirebaseManager.Instance == null)
            {
                Close();
                UIManager.Instance.ShowPopupNotice(ConfigManager.MSG_REDOOM_CODE_ERROR);
            }
                

            FirebaseManager.Instance.Redeem(redeemCode, completed: (value) =>
            {
                if (string.Compare(value, "removeads") == 0)
                {
                    CPlayerPrefs.SetInt(ConfigManager.keyVipIapAdsV2, 1);
                    CPlayerPrefs.Save();
                    Close();
                    UIManager.Instance.ShowPopupNotice(ConfigManager.MSG_REDOOM_CODE_SUCCESS);

                }
                else
                {
                    bool isParseSuccess = false;
                    int inkValue = 0;
                    isParseSuccess = Int32.TryParse(value, out inkValue);
                    DataCore.Debug.Log($"isParseSuccess: {isParseSuccess} inkValue: {inkValue}");
                    if (isParseSuccess)
                    {
                        GameData.Instance.IncreaseInks(inkValue, ConfigManager.GameData.ResourceEarnSource.redoom_reward);
                        ShareUIManager.Instance.UpdateCurrencyData(1.0f);
                        UIManager.Instance.ShowPopupNotice(ConfigManager.MSG_REDOOM_CODE_SUCCESS);
                        Close();
                    }
                    else
                    {
                        UIManager.Instance.ShowPopupNotice(ConfigManager.MSG_REDOOM_CODE_ERROR);
                        Close();
                    }
                }
            }, () =>
            {
                Close();
                UIManager.Instance.ShowPopupNotice(ConfigManager.MSG_REDOOM_CODE_ERROR);
            });
        }

    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
    }

    public override void Close()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        GameManager.Instance.RemoveObjList(this);
    }
}
