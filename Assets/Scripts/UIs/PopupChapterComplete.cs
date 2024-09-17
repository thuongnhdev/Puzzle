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

public class PopupChapterComplete : BasePanel
{

    [SerializeField] private GameObject[] panelChapterComplete;

    public enum TypeChapterComplete
    {
        ChapterComplete = 0,
        ChapterCompleteWait = 1,
        ChapterCompleteFinish = 2
    }
    public override void Init()
    {

    }
    private Action _onComplete = null;
    private TypeChapterComplete _typeChapterComplete = TypeChapterComplete.ChapterComplete;
    public override void SetData(object[] data)
    {
        if (data.Length >= 2)
        {
            _typeChapterComplete = (TypeChapterComplete)data[0];
            _onComplete = (Action)data[1];
        }
        base.SetData(data);
        for(var i =0;i< panelChapterComplete.Length;i++)
        {
            panelChapterComplete[i].SetActive(false);
        }
        panelChapterComplete[(int)_typeChapterComplete].SetActive(true);
    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
    }

    public void BtnChapterComplete()
    {
        SoundController.Instance.PlaySfxClick();
        _onComplete?.Invoke();
        base.Close();
        
    }

    public void BtnChapterCompleteWait()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        _onComplete?.Invoke();
    }

    public void BtnChapterCompleteFinishBook()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        _onComplete?.Invoke();
    }

    public override void Close()
    {
        GameManager.Instance.RemoveObjList(this);
        SoundController.Instance.PlaySfxClick();
        base.Close();
    }


}
