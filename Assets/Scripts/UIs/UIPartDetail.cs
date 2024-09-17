using DataCore;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using com.F4A.MobileThird;
using DG.Tweening;
using EventDispatcher;

public class UIPartDetail : BasePanel
{

    [SerializeField] private TextMeshProUGUI txtPartName;
    [SerializeField] private TextMeshProUGUI txtAuthor;
    [SerializeField] private TextMeshProUGUI txtDescription;
    [SerializeField] private TextMeshProUGUI txtPrize;
    [SerializeField] private Transform puzzleParent;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private GameObject bottomTab;
    [SerializeField] private GameObject puzzleItemPrefab;
    [SerializeField] private GameObject panelPuzzlePlayAgain;
    [SerializeField] private UIPuzzleItem puzzlePlayAgain;
    [SerializeField] private BasePopup popupUnlockSuccessfully;
    [SerializeField] private BasePopup popupCompleteThePrevious;
    [SerializeField] private BasePopup popupUnclockChapter;
    [SerializeField] private GameObject btnUnlockNow;
    [SerializeField] private GameObject btnDownload;
    [SerializeField] private GameObject btnPlayNow;
    [SerializeField] private NumberCounter txtNumberCounter;
    //[SerializeField] private BasePanel popupPlayAgain;
    //[SerializeField] private BasePanel popupGetMoreInk;
    [SerializeField] private GameObject[] txtBtnDownload;

    private ChapterMasterData masterData;
    private List<UIPuzzleItem> listPuzzleLevelItem = new List<UIPuzzleItem>();
    private int bookID;
    private BasePanel previousPanel;
    private string puzzleName;

    private string placement;
    public override void Init()
    {

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

    public override void SetData(object[] data)
    {
        base.SetData(data);
        masterData = (ChapterMasterData)data[0];
        bookID = (int)data[1];
        previousPanel = (BasePanel)data[2];

        if (data.Length == 4 && string.Compare((string)data[3], "null") != 0)
            placement = (string)data[3];

        UpdateMasterData(bookID);
        UpdateStatus();
        float widthContent = puzzleParent.gameObject.GetComponent<RectTransform>().rect.width;
        float paddingLeft = (widthContent - gridLayoutGroup.cellSize.x * 2 - gridLayoutGroup.spacing.x) * 0.5f;
        gridLayoutGroup.padding.left = (int)paddingLeft;
    }

    private void UpdateMasterData(int bookID)
    {
        ClosePopupCompletePrevious();
        ClosePopupUnlockComplete();
        //ClosePopupGetMoreInk();

        var book = MasterDataStore.Instance.GetBookByID(bookID);
        txtPartName.text = book.BookName;
        txtAuthor.text = "by " + masterData.Author;
        txtDescription.text = masterData.PartName + ": " + masterData.Description;
        txtPrize.text = masterData.Price.ToString();

        for (int i = 0; i < listPuzzleLevelItem.Count; i++)
        {
            Destroy(listPuzzleLevelItem[i].gameObject);
        }
        listPuzzleLevelItem.Clear();

        int lastPuzzleCompleteID = GameData.Instance.SavedPack.GetLastCompletePuzzleData(bookID, masterData.ID);
        if (lastPuzzleCompleteID > 0)
        {
            for (int i = 0; i < masterData.PuzzleLevels.Count; i++)
            {
                if (masterData.PuzzleLevels[i].ID == lastPuzzleCompleteID)
                {
                    int nextIndex = i + 1;
                    if (nextIndex < masterData.PuzzleLevels.Count)
                    {
                        var status = GameData.Instance.SavedPack.GetPuzzleStatus(bookID, masterData.ID,
                            masterData.PuzzleLevels[nextIndex].ID);
                        if (status == PuzzleStatus.LOCK)
                        {
                            GameData.Instance.SavedPack.SaveUserPuzzleData(bookID, masterData.ID,
                                masterData.PuzzleLevels[nextIndex].ID, PuzzleStatus.UNLOCK);
                        }
                    }
                }
            }
        }

        for (int i = 0; i < masterData.PuzzleLevels.Count; i++)
        {
            var item = Instantiate(puzzleItemPrefab);
            item.transform.SetParent(puzzleParent);
            item.transform.localScale = Vector3.one;
            UIPuzzleItem puzzleItem = item.GetComponent<UIPuzzleItem>();
            puzzleItem.SetData(masterData.PuzzleLevels[i], bookID, masterData.ID, PuzzlePlayAgainCallback, PuzzleLockCallback);
            listPuzzleLevelItem.Add(puzzleItem);
        }
    }

    private void PuzzlePlayAgainCallback(PuzzleLevelData puzzleData)
    {
        //GameManager.Instance.CapturePuzzle.ResetManual();
        //GameManager.Instance.PuzzleCompleteImg.enabled = false;

        //panelPuzzlePlayAgain.SetActive(true);
        //popupPlayAgain.Open();
        for (int i = 0; i < masterData.PuzzleLevels.Count; i++)
        {
            if (masterData.PuzzleLevels[i].ID == puzzleData.ID)
            {
                //puzzlePlayAgain.SetData(masterData.PuzzleLevels[i], bookID, masterData.ID, null, null);
                //GameManager.Instance.PuzzleCompleteImg.sprite = masterData.PuzzleLevels[i].CompletePuzzleImage;
                //puzzleName = masterData.PuzzleLevels[i].Name;

                UIManager.Instance.PopupPlayAgain.SetData(new object[] { masterData.PuzzleLevels[i], bookID, masterData.ID });
                UIManager.Instance.PopupPlayAgain.Open();
            }
        }



    }

    public void PuzzleLockCallback()
    {
        var userDataBook = GameData.Instance.SavedPack.GetBookData(bookID);
        if (userDataBook == null)
            popupUnclockChapter.Open();
        else
            popupCompleteThePrevious.Open();
    }

    private void OnUnlockChapterTapVip()
    {
        btnUnlockNow.SetActive(false);
        btnDownload.SetActive(false);
        btnPlayNow.SetActive(true);
        bottomTab.SetActive(true);

        if (listPuzzleLevelItem == null || listPuzzleLevelItem.Count == 0)
            return;

        for (int i = 0; i < listPuzzleLevelItem.Count; i++)
        {
            listPuzzleLevelItem[i].UpdateStatusVip();
        }
        OnBtnBuyClick();
    }

    private void UpdateStatus()
    {
        var bookUserData = GameData.Instance.SavedPack.GetBookData(bookID);
        if (bookUserData == null)
        {
            btnUnlockNow.SetActive(true);
            btnDownload.SetActive(false);
            btnPlayNow.SetActive(false);
            bottomTab.SetActive(true);
            return;
        }

        var partUserData = bookUserData.GetChapterSaveData(masterData.ID);
        if (partUserData == null)
        {
            btnUnlockNow.SetActive(true);
            btnDownload.SetActive(false);
            btnPlayNow.SetActive(false);
            bottomTab.SetActive(true);
        }
        else
        {
            if (partUserData.Stt == ChapterStatus.LOCK)
            {
                btnUnlockNow.SetActive(true);
                btnDownload.SetActive(false);
                btnPlayNow.SetActive(false);
                bottomTab.SetActive(true);
            }
            else if (partUserData.Stt == ChapterStatus.UNLOCK)
            {
                btnUnlockNow.SetActive(false);
                btnDownload.SetActive(false);
                btnPlayNow.SetActive(true);
                bottomTab.SetActive(true);
            }
        }

    }

    public void OnBtnBuyClick()
    {
        int coins = GameData.Instance.SavedPack.SaveData.Coin;

        var priceBook = masterData.Price;
        string[] partIdList = masterData.ID.Split('-');

        if (GameData.Instance.IsVipIap() || partIdList[1] == "1" || partIdList[1] == "2")
            priceBook = 0;

        if (coins >= priceBook)
        {
            DataCore.Debug.Log("Buy Part: " + masterData.PartName);
            GameData.Instance.DecreaseInks(priceBook, ConfigManager.GameData.ResourceSpentSource.unlock_chapter);
            UpdateCurrencyData(1.0f);

            if (listPuzzleLevelItem != null && listPuzzleLevelItem.Count > 0)
            {
                for(var i =0; i< listPuzzleLevelItem.Count;i++)
                {
                    listPuzzleLevelItem[i].ActiveAnimationHint("Unlock");
                }
            }    
                
            
            DOVirtual.DelayedCall(2.0f, () =>
            {
                GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.USE_INK_DROPS, priceBook);
                GameData.Instance.SavedPack.SaveUserChapterData(bookID, masterData.ID, ChapterStatus.UNLOCK);
                var bundles = masterData.Labels();                
                GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.UNLOCK_NEW_CHAPTER, 1);
                for (var i = 0; i < listPuzzleLevelItem.Count; i++)
                {
                    int firstPuzzleID = masterData.PuzzleLevels[i].ID;
                    GameData.Instance.SavedPack.SaveUserPuzzleData(bookID, masterData.ID, firstPuzzleID, PuzzleStatus.UNLOCK);
                }
                UpdateStatus();
                UpdateMasterData(bookID);
            });


        }
        else
        {
            ShowPopupGetMoreInk();
        }

    }

    bool _isDownloading = false;
    public void OnBtnDownLoadClick(bool isSound = false)
    {
        if (_isDownloading) return;
        if (isSound) SoundController.Instance.PlaySfxClick();
        //var chapter = GameData.Instance.SavedPack.GetPartUserData(bookID, masterData.ID);
        //if (chapter.Stt != ChapterStatus.UNLOCK) return;
        //var bundles = masterData.Labels();

        //UpdateTextBtnDownload(false);

        ////show ison load of puzzle
        //if (listPuzzleLevelItem != null && listPuzzleLevelItem.Count > 0)
        //    listPuzzleLevelItem[0].StartDownload();
      
        //AssetManager.Instance.DownloadResources(bundles, completed: (size) =>
        //{
        //    if (listPuzzleLevelItem != null && listPuzzleLevelItem.Count > 0)
        //    {
        //        for(var i =0;i< listPuzzleLevelItem.Count;i++)
        //        {
        //            listPuzzleLevelItem[i].StopDownload();
        //        }
        //    }    

        //    GameData.Instance.SavedPack.SaveUserChapterData(bookID, masterData.ID, ChapterStatus.DOWNLOADED);
        //    for (var i = 0; i < listPuzzleLevelItem.Count; i++)
        //    {
        //        int firstPuzzleID = masterData.PuzzleLevels[i].ID;
        //        GameData.Instance.SavedPack.SaveUserPuzzleData(bookID, masterData.ID, firstPuzzleID, PuzzleStatus.UNLOCK);
        //    }
            
        //    UpdateStatus();
        
        //});
    }

  
    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        if (GameData.Instance.IsVipIap() && string.Compare(this.placement, "latest_update") == 0)
            OnUnlockChapterTapVip();

        string[] partIdList = masterData.ID.Split('-');
        if (partIdList[1] == "1" || partIdList[1] == "2")
        {
            OnUnlockChapterTapVip();
        }
        else
            UpdateCurrencyData(0);

        base.Open();

    }

    public void ClosePopupUnlockComplete()
    {
        popupUnlockSuccessfully.Close();
    }

    public void ClosePopupCompletePrevious()
    {
        popupCompleteThePrevious.Close();
    }

    public override void Close()
    {
        GameManager.Instance.CapturePuzzle.ResetManual();
        GameManager.Instance.PuzzleCompleteImg.enabled = false;
        base.Close();
        GameManager.Instance.RemoveObjList(this);
    }

    public override void OnCloseManual()
    {
        base.OnCloseManual();

        CloseManualClick();
        if (GameManager.Instance.CurBookIdOpening != -1)
            UIManager.Instance.ShowUIBookDetail(GameManager.Instance.CurBookIdOpening, "back_book_detail");
    }
    public void CloseManualClick()
    {
        Close();
        if (previousPanel != null)
        {
            previousPanel.ManualRefeshData();
            previousPanel.Open();
        }
        GameManager.Instance.RemoveObjList(this);
        if (UIManager.Instance.UIHomepage.isActiveAndEnabled)
            UIManager.Instance.UIHomepage.UpdateCurrencyData(0);
    }

    //public void ClosePopupGetMoreInk()
    //{
    //    popupGetMoreInk.Close();
    //}

    public void ShowPopupGetMoreInk()
    {
        //popupGetMoreInk.Open();
        //Close();
        UIManager.Instance.ShowGetMoreInk(this);
    }

    //public void OnMoreDealClick()
    //{
    //    //DataCore.Debug.Log("More Deal Click");
    //    ClosePopupGetMoreInk();
    //    //Close();
    //    UIManager.Instance.ShowShop(this);
    //}

    public void OnLoginClick()
    {
        DataCore.Debug.Log("Login Click");
    }

    public void OnPlayNowTap()
    {
        SoundController.Instance.PlaySfxClick();
        if (masterData == null)
            return;
        if (masterData.PuzzleLevels.Count == 0)
            return;

        var puzzle = GetPuzzleCanPlayNow();
        if (puzzle != null)
        {
            if (puzzle.PuzzleID != -1)
                GameManager.Instance.StartLevel(bookID, masterData.ID, puzzle.PuzzleID, ConfigManager.GameData.PlayType.new_puzzle);
        }
        else
        {
            var puzzleComplete = GetPuzzleFirstWhenCompleteAll();
            if (puzzleComplete != null)
            {
                if (puzzleComplete.PuzzleID != -1)
                    GameManager.Instance.StartLevel(bookID, masterData.ID, puzzleComplete.PuzzleID, ConfigManager.GameData.PlayType.new_puzzle);
                else
                    Toast.instance.ShowMessage("Don't have any puzzle available!");
            }
            else
                Toast.instance.ShowMessage("Don't have any puzzle available!");
        }
    }
    private PuzzleInfoID GetPuzzleFirstWhenCompleteAll()
    {

        var partStt = GameData.Instance.SavedPack.GetPartStatus(bookID, masterData.ID);
        if (partStt == ChapterStatus.LOCK)
        {
            return null;
        }

        var puzzleMasterData = masterData.PuzzleLevels[0];
        var puzzleStatus =
            GameData.Instance.SavedPack.GetPuzzleStatus(bookID, masterData.ID,
                puzzleMasterData.ID);
        if (puzzleStatus == PuzzleStatus.COMPLETE || puzzleStatus == PuzzleStatus.UNLOCK)
        {

            DataCore.Debug.Log($"GetPuzzleCanPlayNow BookID: {bookID} chapter Id: {masterData.ID} puzzle id: {puzzleMasterData.ID}");

            return new PuzzleInfoID()
            {
                BookID = bookID,
                PartID = masterData.ID,
                PuzzleID = puzzleMasterData.ID
            };

        }

        return null;
    }

    private PuzzleInfoID GetPuzzleCanPlayNow()
    {

        var partStt = GameData.Instance.SavedPack.GetPartStatus(bookID, masterData.ID);

        if (partStt == ChapterStatus.LOCK || partStt == ChapterStatus.UNLOCK)
        {
            return null;
        }

        for (int j = 0; j < masterData.PuzzleLevels.Count; j++)
        {
            var puzzleMasterData = masterData.PuzzleLevels[j];
            var puzzleStatus =
                GameData.Instance.SavedPack.GetPuzzleStatus(bookID, masterData.ID,
                    puzzleMasterData.ID);

            if (puzzleStatus == PuzzleStatus.UNLOCK)
            {

                DataCore.Debug.Log($"GetPuzzleCanPlayNow BookID: {bookID} chapter Id: {masterData.ID} puzzle id: {puzzleMasterData.ID}");

                return new PuzzleInfoID()
                {
                    BookID = bookID,
                    PartID = masterData.ID,
                    PuzzleID = puzzleMasterData.ID
                };

            }

        }

        return null;
    }
}
