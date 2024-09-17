using System;
using System.Collections;
using System.Collections.Generic;
using com.F4A.MobileThird;
using DanielLochner.Assets.SimpleScrollSnap;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseOtherBookData
{
    public int BookID;
    public string PartID;
    public int PuzzleID;
}

public class UIChooseOtherBook : BasePanel
{

    [SerializeField] private GameObject itemBookPrefab;
    [SerializeField] private SimpleScrollSnap simpleScrollSnap;
    [SerializeField] private TextMeshProUGUI txtPrize;
    [SerializeField] private GameObject playBtn;
    [SerializeField] private GameObject downloadBtn;
    [SerializeField] private List<BookMasterData> bookMasterDatas = new List<BookMasterData>();
    [SerializeField] private NumberCounter txtNumberCounter;

    private Action onComplete;
    public override void Init()
    {

    }

    public void OnEnable()
    {
        simpleScrollSnap.onPanelChanged.AddListener(PanelChange);
    }
    public void OnDisable()
    {
        simpleScrollSnap.onPanelChanged.RemoveListener(PanelChange);
    }
    public override void SetData(object[] data)
    {
        base.SetData(data);
        onComplete = (Action)data[0];
    }

    private List<BookMasterData> bookMasterDatasList = new List<BookMasterData>();
    private void UpdateUI()
    {
        bookMasterDatasList.Clear();
        for (int i = 0; i < bookMasterDatas.Count; i++)
        {
            if(bookMasterDatas[i].ID != GameManager.Instance.CurBookId)
            {
                var itemBook = Instantiate(itemBookPrefab, simpleScrollSnap.Content, false).GetComponent<ItemChooseFirstBook>();
                itemBook.Init(bookMasterDatas[i]);

                simpleScrollSnap.AddToBack(itemBook.gameObject);
                bookMasterDatasList.Add(bookMasterDatas[i]);
            }
        }
        PanelChange();
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
   
    public void PanelChange()
    {
        if (bookMasterDatasList[simpleScrollSnap.CurrentPanel] == null)
            return;
        //txtPrize.text = bookMasterDatasList[simpleScrollSnap.CurrentPanel].ListChapters[0].Price.ToString();
    }
    private int _buyIndex = -1;
    private bool didUnlock = false;

    public void OnBtnBuyPress()
    {
        if (didUnlock) return;
        SoundController.Instance.PlaySfxClick();

        int coins = GameData.Instance.SavedPack.SaveData.Coin;
        _buyIndex = simpleScrollSnap.CurrentPanel;
        var book = bookMasterDatasList[_buyIndex];
        var firstPart = book.ListChapters[0];
        var price = 0;
        if (coins >= price)
        {
            DataCore.Debug.Log("Buy Chapter: " + bookMasterDatasList[simpleScrollSnap.CurrentPanel].ListChapters[0].PartName);
            UpdateCurrencyData(1.0f, () =>
            {
                GameData.Instance.SavedPack.SaveUserChapterData(book.ID, firstPart.ID, ChapterStatus.UNLOCK);
                int firstPuzzleID = firstPart.PuzzleLevels[0].ID;
                GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.UNLOCK_NEW_CHAPTER, 1);
                GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.USE_INK_DROPS, firstPart.Price);
                GameData.Instance.SavedPack.SaveUserPuzzleData(book.ID, firstPart.ID, firstPuzzleID, PuzzleStatus.UNLOCK);
                GameData.Instance.RequestSaveGame();

                var buyBook = new ChooseFirstBookData();
                buyBook.BookID = book.ID;
                buyBook.PartID = firstPart.ID;
                buyBook.PuzzleID = firstPuzzleID;
                didUnlock = true;
                playBtn.SetActive(false);
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    UIManager.Instance.ShowPopupNoInternet();

                }
                else
                {
                    AnalyticManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.completed_unlock_new_book);
                    this.PostEvent(EventID.BackHome, buyBook);
                    Close();
                    onComplete?.Invoke();
                    DataCore.Debug.Log("Complete Buy Part: " + bookMasterDatasList[simpleScrollSnap.CurrentPanel].ListChapters[0].PartName);
                }
            });
        }
    }


    public void OnDownloadButtonClicked()
    {
        SoundController.Instance.PlaySfxClick();

        if (_buyIndex == -1) return;
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            UIManager.Instance.ShowPopupNoInternet();
        }
        else
        {
            var book = bookMasterDatasList[_buyIndex];
            var firstPart = book.ListChapters[0];
            int firstPuzzleID = firstPart.PuzzleLevels[0].ID;
            var buyBook = new ChooseFirstBookData();
            buyBook.BookID = book.ID;
            buyBook.PartID = firstPart.ID;
            buyBook.PuzzleID = firstPuzzleID;
            this.PostEvent(EventID.BackHome, buyBook);
            Close();
            DataCore.Debug.Log("Complete Buy Part: " + bookMasterDatasList[simpleScrollSnap.CurrentPanel].ListChapters[0].PartName);
        }

    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
        UpdateUI();
        UpdateCurrencyData(0);
        simpleScrollSnap.startingPanel = 3;
        simpleScrollSnap.CurrentPanel = 3;
        var price = bookMasterDatasList[simpleScrollSnap.startingPanel].ListChapters[0].Price;

        //txtPrize.text = price.ToString();
        AnalyticManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.open_unlock_new_book);
    }

    public override void Close()
    {
        GameManager.Instance.RemoveObjList(this);
        base.Close();
    }


}
