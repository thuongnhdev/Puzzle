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

public class PopupPurchaseFail : BasePanel
{
    private const string PURCHASE_LOGIN_MESSAGE = "Please login store";
    private const string PURCHASE_FAILED_MESSAGE = "Purchase unsuccessfully";

    [SerializeField] private TextMeshProUGUI msgTxt;

    public enum TypeFail
    {
        PurchaseFail = 0,
        Login = 1
    }
    public override void Init()
    {

    }

    private TypeFail _typeFail = TypeFail.PurchaseFail;
    public override void SetData(object[] data)
    {
        if (data.Length >= 2)
        {
            _typeFail = (TypeFail)data[0];
        }
        base.SetData(data);
        if (_typeFail == TypeFail.Login)
        {
            msgTxt.SetText(PURCHASE_LOGIN_MESSAGE);
        }
        else
        {
            msgTxt.SetText(PURCHASE_FAILED_MESSAGE);
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
    }

    public override void Close()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        GameManager.Instance.RemoveObjList(this);
    }


}
