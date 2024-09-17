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

public class PopupLoadPuzzleFail : BasePanel
{

    public override void Init()
    {
    }
    private string _bookName = "";
    private string _chapterName = "";
    private string _puzzleName = "";
    private Action _onComplete = null;
    public override void SetData(object[] data)
    {
        base.SetData(data);
        if (data.Length >= 1)
        {
            _onComplete = (Action)data[0];
            _bookName = (string)data[1];
            _chapterName = (string)data[2];
            _puzzleName = (string)data[3];
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
        _onComplete?.Invoke();
        _onComplete = null;

        GameManager.Instance.RemoveObjList(this);
    }

}
