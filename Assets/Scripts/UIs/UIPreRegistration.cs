using System;
using System.Collections;
using System.Collections.Generic;
using com.F4A.MobileThird;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPreRegistration : BasePanel
{
    [SerializeField]
    private TextMeshProUGUI tmpInkGift;
    [SerializeField]
    private NumberCounter txtNumberCounter;

    public override void Init()
    {

    }

    public override void SetData(object[] data)
    {
        base.SetData(data);

    }


    public override void Open()
    {
        UpdateCurrencyData(0);
        var ink = int.Parse(FirebaseManager.GetValueRemote(ConfigManager.InkPreRegistrationGift));
        tmpInkGift.text = $"Here are your <size=55><b>{ink.ToString()} Free Ink Drops</b></size>. Enjoy \n reading and coloring.";
        ShareUIManager.Instance.ShowSharedUI(SceneSharedEle.COIN, true);
        GameData.Instance.IncreaseInks(ink, ConfigManager.GameData.ResourceEarnSource.pre_registration);
        GameData.Instance.RequestSaveGame();
        UpdateCurrencyData(1.0f);
        base.Open();

    }

    public void UpdateCurrencyData(float animDuration, Action onComplete = null)
    {
        int newData = GameData.Instance.SavedPack.SaveData.Coin;
        txtNumberCounter.PlayAnim(newData, animDuration, onComplete);

    }

    public void BtnContinuePress()
    {
        SoundController.Instance.PlaySfxClick();

        this.PostEvent(EventID.BackBookDetail);
        Close();
        UIManager.Instance.Fader.Hide();
    }

    public override void Close()
    {
        base.Close();
    }


}
