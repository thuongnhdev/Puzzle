using System;
using System.Collections;
using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DataCore;

public class AllCollectionTab : MonoBehaviour
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI authorTxt;
    [SerializeField] private TextMeshProUGUI authorLine2Txt;
    [SerializeField] private TextMeshProUGUI statusTxt;
    [SerializeField] private TextMeshProUGUI lastUpdateTxt;

    [SerializeField] private Image subcriberBtn;
    [SerializeField] private Sprite[] subcribeSprites;
    [SerializeField] private string[] subcribeString;
    [SerializeField] private Color32[] subcribeTextcolors;
    [SerializeField] private TextMeshProUGUI subcriberTxt;
    private int _bookId;
    private string _BookName;
    private long _lastUpdate;

    public void SetData(int bookId, string name, string author, string stt, long lastUpdate, Sprite sprite)
    {
        this._bookId = bookId;
        _BookName = name;
        this.nameTxt.SetText(name);
        this.authorTxt.SetText("by " + author);
        this.authorLine2Txt.SetText("by " + author);
        this.statusTxt.SetText(stt);

        this._lastUpdate = lastUpdate;
        this.lastUpdateTxt.SetText(UIManager.ConvertReleaseDay(lastUpdate, "updated"));
        bool isSubcribed = GameData.Instance.SavedPack.GetSubcribeState(_bookId);
        SetSubcribeBtnState(isSubcribed);

        thumbnail.sprite = sprite;
        authorTxt.gameObject.SetActive(true);
        authorLine2Txt.gameObject.SetActive(false);

        var tile = (float)Screen.height / Screen.width;
        if (tile > 1.5f && nameTxt.text.Length > 25)
        {
            authorTxt.gameObject.SetActive(false);
            authorLine2Txt.gameObject.SetActive(true);
        }
    }

    public void OnClick()
    {
        DataCore.Debug.Log($"AllCollectionTab {_bookId} {name} {_BookName}");
        SoundController.Instance.PlaySfxClick();
        UIManager.Instance.CloseUIHome();
        UIManager.Instance.ShowUIBookDetail(_bookId, "all_collections");
    }

    public long GetLastUpdate()
    {
        return _lastUpdate;
    }

    public void OnSubcriberTab()
    {
        SoundController.Instance.PlaySfxClick();
        bool isSubcribed = GameData.Instance.SavedPack.SwitchIsSubcribeState(_bookId);
        GameData.Instance.RequestSaveGame();
        SetSubcribeBtnState(isSubcribed);



        this.PostEvent(EventID.OnUpdateSubcribeState, _bookId);
    }

    public void SetSubcribeBtnState(bool isSubcribed)
    {
        subcriberBtn.sprite = subcribeSprites[isSubcribed ? 1 : 0];
        subcriberTxt.color = subcribeTextcolors[isSubcribed ? 1 : 0];
        subcriberTxt.text = subcribeString[isSubcribed ? 1 : 0];
    }

    public void UpdateData(int bookId)
    {
        if (bookId != _bookId)
        {
            return;
        }
        
        SetSubcribeBtnState(GameData.Instance.SavedPack.GetSubcribeState(_bookId));
    }
}