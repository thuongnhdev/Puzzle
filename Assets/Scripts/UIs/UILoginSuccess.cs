using System;
using System.Collections;
using System.Collections.Generic;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoginSuccess : BasePanel
{
    [SerializeField] private TextMeshProUGUI tmpMsg;
    [SerializeField] private NumberCounter txtNumberCounter;

    public override void Init()
    {

    }

    private string _userId;
    public override void SetData(object[] data)
    {
        base.SetData(data);
        if (data.Length >= 1)
        {
            _userId = (string)data[0];
        }
    }


    public override void Open()
    {
        try
        {
            GameManager.Instance.AddObjList(this);
            UpdateCurrencyData(0);
            var login = PlayerPrefs.GetInt(ConfigManager.LoginFirstSuccess, 0);
            if (login == 0)
            {
                tmpMsg.enabled = true;
                int coinLogin = ConfigManager.CoinGiftLoginSuccess;
                PlayerPrefs.SetInt(ConfigManager.LoginFirstSuccess, 1);
                PlayerPrefs.Save();
                GameData.Instance.IncreaseInks(coinLogin, ConfigManager.GameData.ResourceEarnSource.login_reward);               
                ShareUIManager.Instance.UpdateCurrencyData(0.0f);
                UpdateCurrencyData(1.0f);                               
            }
            else
            {
                tmpMsg.enabled = false;
            }

            base.Open();
        }
        catch(Exception ex)
        {
          UIManager.Instance.ShowPopupNotice(ConfigManager.MsgShowLoginFail);
        }
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

    public void BtnContinuePress()
    {
        GameData.Instance.SyncUserProfileOnApplicationInBackground();
        Close();
    }

    public override void Close()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        GameManager.Instance.RemoveObjList(this);
    }


}
