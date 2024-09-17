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

public class ChooseFirstBookData
{
    public int BookID;
    public string PartID;
    public int PuzzleID;
}

public class UIChooseFirstBook : BasePanel
{

    [SerializeField] private GameObject itemBookPrefab;
    [SerializeField] private SimpleScrollSnap simpleScrollSnap;
    [SerializeField] private TextMeshProUGUI txtPrize;
    [SerializeField] private GameObject playBtn;
    [SerializeField] private GameObject downloadBtn;
    [SerializeField] private List<BookMasterData> bookMasterDatas = new List<BookMasterData>();
    [SerializeField] private NumberCounter txtNumberCounter;

    public override void Init()
    {

    }
    private Action onComplete;
    public override void SetData(object[] data)
    {
        base.SetData(data);
        onComplete = (Action)data[0];
    }

    private void UpdateUI()
    {
        for (int i = 0; i < bookMasterDatas.Count; i++)
        {
            var itemBook = Instantiate(itemBookPrefab, simpleScrollSnap.Content, false).GetComponent<ItemChooseFirstBook>();
            itemBook.Init(bookMasterDatas[i]);
            simpleScrollSnap.AddToBack(itemBook.gameObject);
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
        //DataCore.Debug.Log(simpleScrollSnap.CurrentPanel);
        txtPrize.text = bookMasterDatas[simpleScrollSnap.CurrentPanel].ListChapters[0].Price.ToString();
    }
    private int _buyIndex = -1;
    private bool didUnlock = false;
    public void OnBtnBuyPress()
    {
        if (didUnlock) return;
        SoundController.Instance.PlaySfxClick();


        int coins = GameData.Instance.SavedPack.SaveData.Coin;
        _buyIndex = simpleScrollSnap.CurrentPanel;
        var book = bookMasterDatas[_buyIndex];
        var firstPart = book.ListChapters[0];
        firstPart.Price = 0;
        if (coins >= firstPart.Price)
        {
            DataCore.Debug.Log("Buy Chapter: " + bookMasterDatas[simpleScrollSnap.CurrentPanel].ListChapters[0].PartName);
            GameData.Instance.DecreaseInks(firstPart.Price, ConfigManager.GameData.ResourceSpentSource.unlock_chapter);
            GameData.Instance.RequestSaveGame();
            UpdateCurrencyData(1.0f, () =>
            {
                GameData.Instance.SavedPack.SaveData.ResumeBookId = book.ID;
                GameData.Instance.SavedPack.SaveData.ResumePartId = firstPart.ID;
                GameData.Instance.SavedPack.SaveData.ResumePuzzleId = firstPart.PuzzleLevels[0].ID;
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
                AnalyticManager.Instance.TrackUnlockFirstChapter((int)GameSession.Instance.SessionPlayedTime);
                //downloadBtn.SetActive(true);
                playBtn.SetActive(false);
                didUnlock = true;
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    UIManager.Instance.ShowPopupNoInternet();
                }
                else
                {
                    this.PostEvent(EventID.BackHome, buyBook);
                    Close();
                    onComplete?.Invoke();
                    DataCore.Debug.Log("Complete Buy Part: " + bookMasterDatas[simpleScrollSnap.CurrentPanel].ListChapters[0].PartName);
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
            var book = bookMasterDatas[_buyIndex];
            var firstPart = book.ListChapters[0];
            int firstPuzzleID = firstPart.PuzzleLevels[0].ID;
            var buyBook = new ChooseFirstBookData();
            buyBook.BookID = book.ID;
            buyBook.PartID = firstPart.ID;
            buyBook.PuzzleID = firstPuzzleID;
            this.PostEvent(EventID.BackHome, buyBook);
            Close();
            DataCore.Debug.Log("Complete Buy Part: " + bookMasterDatas[simpleScrollSnap.CurrentPanel].ListChapters[0].PartName);
        }

    }

    public override void Open()
    {

        base.Open();
        UpdateUI();
        UpdateCurrencyData(0);
        simpleScrollSnap.startingPanel = 0;
        txtPrize.text = bookMasterDatas[simpleScrollSnap.startingPanel].ListChapters[0].Price.ToString();
        
    }

    public override void Close()
    {
        base.Close();
    }


}
