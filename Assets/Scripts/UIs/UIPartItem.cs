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

public enum DownloadStatus
{
    None,
    Downloading,
    DownloadComplete,
}

public class UIPartItem : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI txtPartNameAndDesc;
    [SerializeField] private TextMeshProUGUI txtNumberPuzzle;
    [SerializeField] private TextMeshProUGUI txtPrice;
    [SerializeField] private GameObject btnPrize;
    [SerializeField] private GameObject parentComplete;
    [SerializeField] private GameObject parentDownload;
    [SerializeField] private GameObject parentDownloading;
    [SerializeField] private Image iconProgressDownload;
    [SerializeField] private Image lockCover;
    [SerializeField] private Image thumbnail;
    [SerializeField] private SkeletonGraphic unLockCoverAnimation;

    private ChapterStatus status;
    private ChapterMasterData masterData;
    private Action onBuySuccessCallback;
    private Action onBuyFailedCallback;
    //private string partNameFormat = "{0}: {1}";
    private string numberPuzzleFormat = "{0} puzzles";
    private int bookID = 0;

    public ChapterMasterData MasterData => masterData;

    public void SetData(ChapterMasterData partMasterData, int _bookID, Action onBuySuccess, Action onBuyFailed)
    {
        onBuySuccessCallback = onBuySuccess;
        onBuyFailedCallback = onBuyFailed;
        masterData = partMasterData;
        bookID = _bookID;
        UpdateStatus();
        UpdateUI();
    }

    private void UpdateUI()
    {
        var nameAndDesc = masterData.PartName + ": " + masterData.Description;
        txtPartNameAndDesc.text = nameAndDesc;
        if (nameAndDesc.Length > 60)
            txtPartNameAndDesc.text = nameAndDesc.Substring(0, 60) + "...";

        txtNumberPuzzle.text = string.Format(numberPuzzleFormat, masterData.PuzzleLevels.Count);
        txtPrice.text = masterData.Price.ToString();

        var chapterThumbnail = masterData.ChapterThumbnailLabel();
        if (string.IsNullOrEmpty(chapterThumbnail))
            return;

        AssetManager.Instance.DownloadResource(chapterThumbnail, completed: (size) =>
        {
            AssetManager.Instance.LoadPathAsync<Sprite>(masterData.Thumbnail.Thumbnail, (thumb) =>
            {
                if (thumb != null && thumbnail != null)
                {
                    thumbnail.sprite = thumb;
                }
            });
        });
    }

    private void UpdateStatus()
    {
        status = GetStatusFromUserData();

        // Reset Button
        if(btnPrize != null) btnPrize.SetActive(false);
        if (parentComplete != null) parentComplete.SetActive(false);
        if (parentDownload != null)  parentDownload.SetActive(false);
        if (parentDownloading != null)  parentDownloading.SetActive(false);
        /////////

        if (status == ChapterStatus.LOCK)
        {
            if (lockCover != null) lockCover.gameObject.SetActive(true);
            if (btnPrize != null) btnPrize.SetActive(true);
        }
        else
        {
            // Check download
            if (lockCover != null) lockCover.gameObject.SetActive(false);
            if (btnPrize != null) btnPrize.SetActive(false);
            if (parentComplete != null) parentComplete.SetActive(true);
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
        else

    if (userDataBook == null)
        {
            return ChapterStatus.LOCK;
        }

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
        DataCore.Debug.Log("Click on Part: " + masterData.PartName);
        UIManager.Instance.CloseUIBookDetail();
        UIManager.Instance.ShowUIPartDetail(masterData, bookID, UIManager.Instance.UIBookContent, "book_detail");

        //// show intertial ads
        //if (CPlayerPrefs.GetInt(masterData.ID, 0) == 0)
        //{
        //    AdsService.Instance.ShowInterstitial(AdsService.IntAdPlacementChapter, (complete) =>
        //    {
        //        SoundController.Instance.MuteBgMusic(false);
        //    });
        //    CPlayerPrefs.SetInt(masterData.ID, 1);
        //    CPlayerPrefs.Save();
        //}
    }

    public void ActiveAnimationHint(string nameAnim)
    {
        unLockCoverAnimation.gameObject.SetActive(true);
        if (unLockCoverAnimation.skeletonDataAsset != null)
        {
            unLockCoverAnimation.AnimationState.SetAnimation(1, nameAnim, false);
            DOVirtual.DelayedCall(0.8f, () =>
            {
                unLockCoverAnimation.gameObject.SetActive(false);
                StartCoroutine(FadeOutText(1.0f, lockCover));
            });
        }
    }

    private IEnumerator FadeOutText(float timeSpeed, Image image)
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
        while (image.color.a > 0.0f)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - (Time.deltaTime * timeSpeed));
            yield return null;
        }
    }

    public void SetStatusBtnVip()
    {
        if (btnPrize != null) btnPrize.SetActive(false);
        if (parentComplete != null) parentComplete.SetActive(true);
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

        bool shouldUnlock = true;

        var partUserData = GameData.Instance.SavedPack.GetPartUserData(bookID, masterData.ID);
        if (partUserData != null)
        {
            DataCore.Debug.Log($"check chapter {bookID} {masterData.ID} {partUserData.Stt}");

            shouldUnlock = partUserData.Stt == ChapterStatus.LOCK;
        }
        DataCore.Debug.Log($"Buy chapter {masterData.PartName}:  {shouldUnlock}");

        if (shouldUnlock)
        {
            if (coins >= priceBook)
            {
                ActiveAnimationHint("Unlock");
                DOVirtual.DelayedCall(1.5f, () =>
                {
                    DataCore.Debug.Log("Buy chapter: " + masterData.PartName);
                    GameData.Instance.DecreaseInks(priceBook, ConfigManager.GameData.ResourceSpentSource.unlock_chapter);
                    ShareUIManager.Instance.UpdateCurrencyData(1.0f);
                    GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.USE_INK_DROPS, masterData.Price);
                    BuyChapter();
                    UpdateStatus();
                    SetStatusUnlock();
                    onBuySuccessCallback?.Invoke();

                });

            }
            else
            {
                DataCore.Debug.Log("Not enough coin");
                onBuyFailedCallback?.Invoke();
            }
        }

    }

    public void SetStatusUnlock()
    {
        for(var i =0;i< masterData.PuzzleLevels.Count;i++)
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
}
