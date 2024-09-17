using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using com.F4A.MobileThird;
using DanielLochner.Assets.SimpleScrollSnap;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupLiveEventGift : BasePanel
{
    public override void Init()
    {
    }

    [SerializeField] private int[] inkReward;
    [SerializeField] private TextMeshProUGUI tmpInkReward;

    private Action _onComplete = null;
    public override void SetData(object[] data)
    {
        base.SetData(data);
        if (data.Length >= 0)
        {
            _onComplete = (Action)data[0];
            int ink = (int)data[1];
            tmpInkReward.text = "x" + inkReward[ink].ToString();
        }
    }


    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
    }

    public void BtnContinuePress()
    {
        SoundController.Instance.PlaySfxClick();
        _onComplete?.Invoke();
        _onComplete = null;
        Close();

    }

    public override void Close()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        _onComplete?.Invoke();
        _onComplete = null;

        GameManager.Instance.RemoveObjList(this);
    }

}
