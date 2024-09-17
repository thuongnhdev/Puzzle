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

public class UIWelcomeGift : BasePanel
{
    [SerializeField]
    private TextMeshProUGUI tmpInkGift;

    [SerializeField] private NumberCounter txtNumberCounter;

    private int inkGift = 200;
    public override void Init()
    {
        InitPreregistration();
    }

    private void InitPreregistration()
    {
        var timeBegin = FirebaseManager.GetValueRemote(ConfigManager.TimeBeginPreRegistrationGift);
        var timeEnd = FirebaseManager.GetValueRemote(ConfigManager.TimeEndPreRegistrationGift);
        if (string.IsNullOrEmpty(timeBegin) || string.IsNullOrEmpty(timeEnd))
            return;
        if (string.Compare(timeBegin, "0") == 0 || string.Compare(timeEnd, "0") == 0)
            return;
     
        DateTime oDateBegin = DateTime.ParseExact(timeBegin, "yyyyMMdd", CultureInfo.InvariantCulture);
        DateTime oDateEnd = DateTime.ParseExact(timeEnd, "yyyyMMdd", CultureInfo.InvariantCulture);
        var timeNow = DateTime.Now;
        if (timeNow > oDateBegin && timeNow < oDateEnd)
            this.inkGift = int.Parse(FirebaseManager.GetValueRemote(ConfigManager.InkPreRegistrationGift));
        else
            this.inkGift = int.Parse(FirebaseManager.GetValueRemote(ConfigManager.InkPreRegistrationGiftEndTime));
    }

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
        tmpInkGift.text = $"{this.inkGift.ToString()} Free Ink Drops";

        UIManager.Instance.Fader.Hide();
        this.PostEvent(EventID.BackBookDetail);
        UpdateCurrencyData(0);

        GameManager.Instance.SetStepGame(StepGameConstants.WelcomeGift);
        base.Open();

        isGetGift = false;
    }

    private bool isGetGift = false;
    public void BtnContinuePress()
    {
        SoundController.Instance.PlaySfxClick();
        if (isGetGift) return;

        isGetGift = true;
        GameData.Instance.IncreaseInks(this.inkGift, ConfigManager.GameData.ResourceEarnSource.welcome_gift);
        GameData.Instance.RequestSaveGame();
        ShareUIManager.Instance.UpdateCurrencyData(1.0f, () =>
        {
            GameManager.Instance.SetStepGame(StepGameConstants.HomePage);
            AnalyticManager.Instance.TrackReceiveWelcomeGift();
            Close();
            //GameManager.Instance.CheckFlowGame();
        });

    }

    public override void Close()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        _onComplete?.Invoke();
        _onComplete = null;
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
}
