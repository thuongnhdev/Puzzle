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

public class UIWelcome : BasePanel
{
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
        base.Open();
    }

    public void BtnContinuePress()
    {
        SoundController.Instance.PlaySfxClick();
        Close();
    }

    public void BtnTermsPress()
    {
        SoundController.Instance.PlaySfxClick();
        Application.OpenURL(ConfigManager.UrlTermsInfo);
    }

    public void BtnPolicyPress()
    {
        SoundController.Instance.PlaySfxClick();
        Application.OpenURL(ConfigManager.UrlPolicyInfo);
    }

    public override void Close()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        _onComplete?.Invoke();
        _onComplete = null;
    }

}
