using System;
using System.Collections;
using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using DataCore;
using DG.Tweening;
using Spine.Unity;

public enum PuzzleItemStatus
{
    Lock,
    Unlock,
    Complete
}


public class UIPuzzleItem : MonoBehaviour
{
    [SerializeField] public Image thumbnail;
    [SerializeField] private Image iconStatus;
    [SerializeField] private Sprite sprIconPlay;
    [SerializeField] private Sprite sprIconLock;
    [SerializeField] private RectTransform parentDownloading;

    [SerializeField] private SkeletonGraphic unLockCoverAnimation;
    [SerializeField] private Image lockObject;

    private PuzzleStatus status;
    private PuzzleLevelData puzzleData;
    private int bookID;
    private string partID;

    private Action<PuzzleLevelData> onPuzzlePlayAgain;
    private Action onPuzzleLock;

    public void SetData(PuzzleLevelData puzzleMasterData, int _bookID, string _partID, Action<PuzzleLevelData> puzzlePlayAgain, Action puzzleLock)
    {

        partID = _partID;
        puzzleData = puzzleMasterData;
        bookID = _bookID;
        onPuzzlePlayAgain = puzzlePlayAgain;
        onPuzzleLock = puzzleLock;
        if (unLockCoverAnimation != null)
        {
            unLockCoverAnimation.gameObject.SetActive(true);
        }
        if (lockObject != null)
        {
            lockObject.gameObject.SetActive(true);
        }
        if (parentDownloading != null)
            parentDownloading.gameObject.SetActive(false);
        UpdateStatus();
        UpdateUI();
    }

    public void OnRelease()
    {
        if (puzzleData == null) return;
        try
        {
            AssetManager.Instance.ReleasePath(puzzleData.Thumbnail.Thumbnail);
        }
        catch (Exception ex)
        {
            DataCore.Debug.Log($"Failed to release UIPuzzleItem. Error: {ex.Message}");
        }

    }

    private void UpdateUI()
    {
        if (parentDownloading != null)
            parentDownloading.gameObject.SetActive(true);
        AssetManager.Instance.DownloadResource(puzzleData.ThumbnailLabel(), completed: (size) =>
        {
            AssetManager.Instance.LoadPathAsync<Sprite>(puzzleData.Thumbnail.Thumbnail, (thumb) =>
            {
                if (thumb != null && thumbnail != null)
                {
                    if (parentDownloading != null)
                        parentDownloading.gameObject.SetActive(false);
                    thumbnail.sprite = thumb;
                }
            });
        });
    }

    public void ActiveAnimationHint(string nameAnim)
    {
        if (unLockCoverAnimation == null)
            return;
        if (lockObject != null)
        {
            lockObject.gameObject.SetActive(true);
        }
        unLockCoverAnimation.gameObject.SetActive(true);
        if (unLockCoverAnimation.skeletonDataAsset != null)
        {
            unLockCoverAnimation.AnimationState.SetAnimation(1, nameAnim, false);
            DOVirtual.DelayedCall(0.8f, () =>
            {
                unLockCoverAnimation.gameObject.SetActive(false);
                lockObject.gameObject.SetActive(false);
                StartCoroutine(FadeOutText(1.0f, lockObject));
                GameData.Instance.SavedPack.SaveUserPuzzleData(bookID, partID, puzzleData.ID, PuzzleStatus.UNLOCK);
                status = PuzzleStatus.UNLOCK;
                GameData.Instance.RequestSaveGame();

                StartDownload();
                DOVirtual.DelayedCall(0.8f, () =>
                {
                    StopDownload();
                });

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
    private void UpdateStatus()
    {
        EnableGrayScaleThumbnail(true);
        var unlocked = GameData.Instance.SavedPack.GetPartStatus(bookID, partID) != ChapterStatus.LOCK || GameData.Instance.IsVipIap();
        status = GameData.Instance.SavedPack.GetPuzzleStatus(bookID, partID, puzzleData.ID);
        string[] partIdList = partID.Split('-');
        if (partIdList[1] == "1" || partIdList[1] == "2")
        {
            if (status != PuzzleStatus.COMPLETE)
            {
                GameData.Instance.SavedPack.SaveUserPuzzleData(bookID, partID, puzzleData.ID, PuzzleStatus.UNLOCK);
                status = PuzzleStatus.UNLOCK;
            }
        }
        else
        {
            DataCore.Debug.Log($"Puzzle: {unlocked} {status}");
            if (unlocked)
            {
                if (status != PuzzleStatus.COMPLETE)
                    status = PuzzleStatus.UNLOCK;
            }
            else
                status = PuzzleStatus.LOCK;
        }

        UpdateIconLock();
    }


    public void StartDownload()
    {
        if (unLockCoverAnimation != null)
            unLockCoverAnimation.gameObject.SetActive(false);
        if (lockObject != null)
            lockObject.gameObject.SetActive(false);
        parentDownloading.gameObject.SetActive(true);
        //DOVirtual.DelayedCall(1.0f, () =>
        //{
        //    parentDownloading.SetActive(false);
        //});
    }

    public void StopDownload()
    {
        if (unLockCoverAnimation != null)
            unLockCoverAnimation.gameObject.SetActive(false);
        if (lockObject != null)
            lockObject.gameObject.SetActive(false);
        iconStatus.gameObject.SetActive(false);
        parentDownloading.gameObject.SetActive(false);
    }

    public void UpdateStatusVip()
    {
        if (lockObject != null) lockObject.gameObject.SetActive(false);
        if (unLockCoverAnimation != null) unLockCoverAnimation.gameObject.SetActive(false);
        iconStatus.gameObject.SetActive(false);
        iconStatus.sprite = sprIconPlay;
    }

    private void UpdateIconLock()
    {
        switch (status)
        {
            case PuzzleStatus.NONE:
                iconStatus.gameObject.SetActive(false);
                iconStatus.sprite = sprIconLock;
                if (lockObject != null) lockObject.gameObject.SetActive(true);
                if (unLockCoverAnimation != null) unLockCoverAnimation.gameObject.SetActive(true);
                break;
            case PuzzleStatus.LOCK:
                iconStatus.gameObject.SetActive(false);
                iconStatus.sprite = sprIconLock;
                if (lockObject != null) lockObject.gameObject.SetActive(true);
                if (unLockCoverAnimation != null) unLockCoverAnimation.gameObject.SetActive(true);
                break;
            case PuzzleStatus.UNLOCK:
                if (lockObject != null) lockObject.gameObject.SetActive(false);
                if (unLockCoverAnimation != null) unLockCoverAnimation.gameObject.SetActive(false);
                iconStatus.gameObject.SetActive(false);
                iconStatus.sprite = sprIconPlay;
                break;
            case PuzzleStatus.COMPLETE:
                if (lockObject != null) lockObject.gameObject.SetActive(false);
                if (unLockCoverAnimation != null) unLockCoverAnimation.gameObject.SetActive(false);
                iconStatus.gameObject.SetActive(false);
                EnableGrayScaleThumbnail(false);
                break;
        }
    }

    private void EnableGrayScaleThumbnail(bool enable)
    {
        Material mat = new Material(thumbnail.material);
        mat.SetFloat("_EffectAmount", enable ? 1 : 0);
        thumbnail.material = mat;
    }

    public void OnPuzzlePlayClick()
    {
        if (status == PuzzleStatus.UNLOCK)
        {
            GameManager.Instance.StartLevel(bookID, partID, puzzleData.ID, ConfigManager.GameData.PlayType.new_puzzle);
            if (!GameManager.Instance.IsCompleteStepGame()) StepGame();
            GameData.Instance.SavedPack.SaveMyLibraryBookData(bookID);
        }
        else if (status == PuzzleStatus.COMPLETE)
        {
            onPuzzlePlayAgain?.Invoke(puzzleData);
        }
        else
        {
            onPuzzleLock?.Invoke();
        }
    }

    public void OnPuzzlePlayAgainClick()
    {
        GameManager.Instance.StartLevel(bookID, partID, puzzleData.ID, ConfigManager.GameData.PlayType.replay_puzzle);
    }

    private void StepGame()
    {
        var playGame = GameManager.Instance.GetStepGame();
        if (playGame == StepGameConstants.Tutorial)
            GameManager.Instance.SetStepGame(StepGameConstants.PlayPuzzleOne);
        else if (playGame == StepGameConstants.HomePage)
            GameManager.Instance.SetStepGame(StepGameConstants.PlayPuzzleTwo);
        else if (playGame == StepGameConstants.Rating)
            GameManager.Instance.SetStepGame(StepGameConstants.PlayPuzzleThree);
        else if (playGame == StepGameConstants.PlayPuzzleThree)
            GameManager.Instance.SetStepGame(StepGameConstants.LuckyDrawOne);
        //else if (playGame == StepGameConstants.Login)
        //    GameManager.Instance.SetStepGame(StepGameConstants.PlayPuzzleFour);
        //else if (playGame == StepGameConstants.LuckyDraw)
        //    GameManager.Instance.SetStepGame(StepGameConstants.PlayPuzzleFive);
    }
}
