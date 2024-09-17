using System;
using System.Collections;
using System.Collections.Generic;
using DataCore;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using com.F4A.MobileThird;
using Spine.Unity;

public class UIPartPanel : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI txtPartDesc;
    [SerializeField] private TextMeshProUGUI txtPartName;
    [SerializeField] private TextMeshProUGUI txtPrice;
    [SerializeField] private GameObject btnPrize;
    [SerializeField] private GameObject parentComplete;
    [SerializeField] private GameObject parentDownload;
    [SerializeField] private GameObject parentDownloading;

    private ChapterStatus status;
    private ChapterMasterData masterData;
    private int bookID = 0;

    public ChapterMasterData MasterData => masterData;
    private List<UIPuzzleItem> listPuzzleLevelItem = new List<UIPuzzleItem>();


    [SerializeField] private Transform puzzleParent;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private GameObject puzzleItemPrefab;


    [SerializeField] private Image inkIcon;
    [SerializeField] private Image inkIconMoving;

    [SerializeField] private RectTransform container;

    public void SetData(ChapterMasterData partMasterData, int _bookID, Action onBuySuccess, Action onBuyFailed)
    {
        masterData = partMasterData;
        bookID = _bookID;
        UpdateStatus();
        UpdateUI();
        UpdateMasterData(bookID);

        string[] partIdList = masterData.ID.Split('-');
        if (partIdList[1] == "1" || GameData.Instance.IsVipIap() || partIdList[1] == "2")
        {
            OnUnlockChapterTapVip();
            SetLayoutPanel(false);
        }
        inkIconMoving.gameObject.SetActive(false);
    }

    public void OnRelease() {
        try
        {
            for (int i = 0; i < listPuzzleLevelItem.Count; i++)
            {
                var puzzleItem = listPuzzleLevelItem[i];
                puzzleItem.OnRelease();
            }
        }
        catch (Exception ex)
        {
            DataCore.Debug.Log($"Failed UIPartPanel OnRelease. Error: {ex.Message}");
        }

    }

    private void UpdateUI()
    {
        var nameAndDesc = masterData.Description;
        txtPartDesc.text = nameAndDesc;
        if (nameAndDesc.Length > 60)
            txtPartDesc.text = nameAndDesc.Substring(0, 60) + "...";
        txtPartDesc.text = $"'{nameAndDesc}!'"; 
        txtPartName.text = masterData.PartName;
        txtPrice.text = $"Unlock with {masterData.Price}";

        var chapterThumbnail = masterData.ChapterThumbnailLabel();
        if (string.IsNullOrEmpty(chapterThumbnail))
            return;

    }

    private void UpdateStatus()
    {
        status = GetStatusFromUserData();

        // Reset Button
        if (btnPrize != null)
        {
            btnPrize.SetActive(false);
            inkIconMoving.gameObject.SetActive(false);
        }
        if (parentComplete != null) parentComplete.SetActive(false);
        if (parentDownload != null) parentDownload.SetActive(false);
        if (parentDownloading != null) parentDownloading.SetActive(false);
        /////////

        if (status == ChapterStatus.LOCK)
        {
            SetLayoutPanel(true);
            if (btnPrize != null) btnPrize.SetActive(true);
        }
        else
        {
            SetLayoutPanel(false);
            // Check download
            if (btnPrize != null)
            {
                btnPrize.SetActive(false);
                inkIconMoving.gameObject.SetActive(false);
            }
            if (parentComplete != null) parentComplete.SetActive(false);
        }
     
    }

    private void SetLayoutPanel(bool isActive)
    {
        container.offsetMin = new Vector2(-20, container.offsetMin.y);
        container.offsetMax = new Vector2(20, container.offsetMax.y);
        container.offsetMax = new Vector2(container.offsetMax.x, -370);
        if (!isActive)
        {
            container.offsetMin = new Vector2(-20, container.offsetMin.y);
            container.offsetMax = new Vector2(20, container.offsetMax.y);
            container.offsetMax = new Vector2(container.offsetMax.x, -200);
        }
    }
    private ChapterStatus GetStatusFromUserData()
    {
        // Load from user data
        var userDataBook = GameData.Instance.SavedPack.GetBookData(bookID);
        string[] partIdList = masterData.ID.Split('-');
        if (partIdList[1] == "1" || partIdList[1] == "2")
        {
            return ChapterStatus.UNLOCK;
        }


        if (userDataBook == null)
            return ChapterStatus.LOCK;

        var userDataPart = userDataBook.GetChapterSaveData(masterData.ID);
        if (userDataPart != null)
        {
            if (partIdList[1] == "1" || partIdList[1] == "2")
            {
                userDataPart.Stt = ChapterStatus.UNLOCK;
                return ChapterStatus.UNLOCK;
            }
            else
                return userDataPart.Stt;
        }
        return ChapterStatus.LOCK;
    }

    public void OnPartClick()
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

    public void SetStatusBtnVip()
    {
        if (btnPrize != null) btnPrize.SetActive(false);
        if (parentComplete != null) parentComplete.SetActive(false);
        if (parentDownload != null) parentDownload.SetActive(false);
        if (parentDownloading != null) parentDownloading.SetActive(false);
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
            MoveInkIconToTop();
            if (listPuzzleLevelItem != null && listPuzzleLevelItem.Count > 0)
            {
                for (var i = 0; i < listPuzzleLevelItem.Count; i++)
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
                GameData.Instance.RequestSaveGame();
                //UpdateMasterData(bookID);
            });
        }
        else
        {
            UIManager.Instance.ShowGetMoreInk();
        }

    }

    public void SetStatusUnlock()
    {
        for (var i = 0; i < masterData.PuzzleLevels.Count; i++)
        {
            int firstPuzzleID = masterData.PuzzleLevels[i].ID;
            GameData.Instance.SavedPack.SaveUserPuzzleData(bookID, masterData.ID, firstPuzzleID, PuzzleStatus.UNLOCK);
        }
        GameData.Instance.SavedPack.SaveUserChapterData(bookID, masterData.ID, ChapterStatus.UNLOCK);


        GameData.Instance.RequestSaveGame();
    }
    private void BuyChapter()
    {
        GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.UNLOCK_NEW_CHAPTER, 1);
        GameData.Instance.SavedPack.SaveUserChapterData(bookID, masterData.ID, ChapterStatus.UNLOCK);
        GameData.Instance.RequestSaveGame();

    }

    private void PuzzlePlayAgainCallback(PuzzleLevelData puzzleData)
    {
        for (int i = 0; i < masterData.PuzzleLevels.Count; i++)
        {
            if (masterData.PuzzleLevels[i].ID == puzzleData.ID)
            {
                UIManager.Instance.PopupPlayAgain.SetData(new object[] { masterData.PuzzleLevels[i], bookID, masterData.ID });
                UIManager.Instance.PopupPlayAgain.Open();
            }
        }

    }

    public void OnBtnBuyPuzzleClick()
    {
        int coins = GameData.Instance.SavedPack.SaveData.Coin;

        var priceBook = masterData.Price;
        string[] partIdList = masterData.ID.Split('-');

        if (GameData.Instance.IsVipIap() || partIdList[1] == "1" || partIdList[1] == "2")
            priceBook = 0;

        if (coins >= priceBook)
        {
            DataCore.Debug.Log("Buy Part: " + masterData.PartName);
            MoveInkIconToTop();

            if (listPuzzleLevelItem != null && listPuzzleLevelItem.Count > 0)
            {
                for (var i = 0; i < listPuzzleLevelItem.Count; i++)
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
                //UpdateMasterData(bookID);
            });
        }
        else
        {
            UIManager.Instance.ShowGetMoreInk();
        }

    }

    private void UpdateMasterData(int bookID)
    {
        //ClosePopupGetMoreInk();

        var book = MasterDataStore.Instance.GetBookByID(bookID);
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
            puzzleItem.SetData(masterData.PuzzleLevels[i], bookID, masterData.ID, PuzzlePlayAgainCallback, () => { });
            listPuzzleLevelItem.Add(puzzleItem);
        }
    }

    public void UpdateCurrencyData(float animDuration, Action onComplete = null)
    {
        ShareUIManager.Instance.UpdateCurrencyData(animDuration);
    }

    private void OnUnlockChapterTapVip()
    {
        SetStatusBtnVip();

        if (listPuzzleLevelItem == null || listPuzzleLevelItem.Count == 0)
            return;

        for (int i = 0; i < listPuzzleLevelItem.Count; i++)
        {
            listPuzzleLevelItem[i].UpdateStatusVip();
        }
     
        DOVirtual.DelayedCall(2.0f, () =>
        {
            GameData.Instance.SavedPack.SaveUserChapterData(bookID, masterData.ID, ChapterStatus.UNLOCK);
            var bundles = masterData.Labels();
            GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.UNLOCK_NEW_CHAPTER, 1);
            for (var i = 0; i < listPuzzleLevelItem.Count; i++)
            {
                int firstPuzzleID = masterData.PuzzleLevels[i].ID;
                PuzzleStatus status = GameData.Instance.SavedPack.GetPuzzleStatus(bookID, masterData.ID, firstPuzzleID);
                if (status != PuzzleStatus.COMPLETE)
                {
                    GameData.Instance.SavedPack.SaveUserPuzzleData(bookID, masterData.ID, firstPuzzleID, PuzzleStatus.UNLOCK);
                }
                
            }
        });


    }

    private void MoveInkIconToTop()
    {
        inkIconMoving.gameObject.SetActive(true);
        inkIconMoving.transform.localScale = Vector3.one;
        inkIconMoving.transform.position = inkIcon.transform.position;
        inkIconMoving.transform.DOScale(Vector3.one * 0.5f, 1.0f);
        inkIcon.gameObject.SetActive(false);
        inkIconMoving.transform.DOMove(ShareUIManager.Instance.InkTopCurrencyIconPos, 1.0f).OnComplete(() =>
        {
            GameData.Instance.DecreaseInks(masterData.Price, ConfigManager.GameData.ResourceSpentSource.unlock_chapter);
            UpdateCurrencyData(1.0f);
        });
    }
}
