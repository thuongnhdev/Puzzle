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

public class PopupChapterDetailComplete : BasePanel
{

    [SerializeField] private GameObject[] panelChapterComplete;
    [SerializeField] private TextMeshProUGUI priceBuyChapterTxt;

    [SerializeField] private GameObject[] btnClose;
    [SerializeField] private GameObject btnBuyChapter;
    [SerializeField] private GameObject btnUnlockChapter;


    private int priceBook = 0;
    public enum TypeChapterComplete
    {
        ChapterInk = 0,
        ChapterNoInk = 1,
        ChapterComplete = 2,
        ChapterCompleteWait = 3,
        ChapterCompleteFinish = 4
    }
    public override void Init()
    {

    }
    private Action<bool> _onComplete = null;
    private TypeChapterComplete _typeChapterComplete = TypeChapterComplete.ChapterInk;
    public override void SetData(object[] data)
    {
        if (data.Length >= 2)
        {
            _typeChapterComplete = (TypeChapterComplete)data[0];
            _onComplete = (Action<bool>)data[1];
            if(data.Length == 3 && !data[2].Equals("0"))
            {
                string[] chapId = GameManager.Instance.CurPartId.Split('-');
                var indexChapter = int.Parse(chapId[1]);
                priceBuyChapterTxt.text = (string)data[2] + " - Chapter " + indexChapter;
                priceBook = int.Parse((string)data[2]);
            }
        }
        base.SetData(data);
        for (var i = 0; i < panelChapterComplete.Length; i++)
        {
            panelChapterComplete[i].SetActive(false);
        }
        panelChapterComplete[(int)_typeChapterComplete].SetActive(true);

    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
        btnBuyChapter.SetActive(false);
        btnUnlockChapter.SetActive(false);
        if (GameManager.Instance.GetStepGame() == StepGameConstants.StepComplete)
        {
            btnBuyChapter.SetActive(true);
            for (int i = 0; i < btnClose.Length; i++)
            {
                btnClose[i].SetActive(true);
            }
        }
        else
        {
            string[] chapId = GameManager.Instance.CurPartId.Split('-');
            var indexChapter = int.Parse(chapId[1]);
            if (indexChapter == 2)
            {
                priceBook = 0;
                btnUnlockChapter.SetActive(true);
            }
            else
                btnBuyChapter.SetActive(true);
        }
           

    }

    public void BtnChapterComplete()
    {
        SoundController.Instance.PlaySfxClick();
        _onComplete?.Invoke(true);
        base.Close();
        GameManager.Instance.RemoveObjList(this);
    }

    public void BtnUnLockChapterOther()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        if (GameManager.Instance.GetStepGame() == StepGameConstants.StepComplete)
        {
            _onComplete?.Invoke(false);
            GameManager.Instance.RemoveObjList(this);
        }
        else
        {
            UIManager.Instance.ShowUIChooseOtherBook(() => {
                _onComplete?.Invoke(true);
                GameManager.Instance.RemoveObjList(this);
            });
        }
         
    }

    public void BtnBuyChapter()
    {
        SoundController.Instance.PlaySfxClick();
        int coins = GameData.Instance.SavedPack.SaveData.Coin;

        if (coins >= priceBook)
        {
            AnalyticManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.unlock_second_chapter);
            GameData.Instance.DecreaseInks(priceBook, ConfigManager.GameData.ResourceSpentSource.unlock_chapter);
            base.Close();
            _onComplete?.Invoke(true);
        }
        else
        {
            UIManager.Instance.ShowGetMoreInk(this);
        }

        GameManager.Instance.RemoveObjList(this);
    }

    public void BtnContinue()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
    }

    public void BtnChapterCompleteWait()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        _onComplete?.Invoke(true);
        GameManager.Instance.RemoveObjList(this);
    }

    public void BtnChapterCompleteFinishBook()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        _onComplete?.Invoke(true);
        GameManager.Instance.RemoveObjList(this);
    }
    public void OnCloseClick()
    {
        GameManager.Instance.RemoveObjList(this);
        SoundController.Instance.PlaySfxClick();
        base.Close();
        _onComplete?.Invoke(false);
    }


}
