using System;
using System.Collections;
using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using DataCore;
using DG.Tweening;
using Spine.Unity;

public class UIPuzzleCollectionItem : MonoBehaviour
{
    [SerializeField] public Image thumbnail;
    [SerializeField] private Image iconStatus;
    [SerializeField] private Sprite sprIconPlay;
    [SerializeField] private Sprite sprIconLock;
    [SerializeField] private RectTransform parentDownloading;

    [SerializeField] private SkeletonGraphic unLockCoverAnimation;
    [SerializeField] private Image lockObject;

    private int _collectionId;
    private int _collectionIndex;
    private PuzzleStatus status;
    private PuzzleLevelData puzzleData;

    private Action<PuzzleLevelData> onPuzzlePlayAgain;
    private Action onPuzzleLock;

    public void SetData(int collectionId, int collectionIndex, PuzzleLevelData puzzleMasterData,  Action<PuzzleLevelData> puzzlePlayAgain, Action puzzleLock)
    {
        _collectionId = collectionId;
        _collectionIndex = collectionIndex;
        puzzleData = puzzleMasterData;
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

    private void UpdateUI()
    {
        if (parentDownloading != null)
            parentDownloading.gameObject.SetActive(true);
        if (puzzleData == null)
            return;
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
                //GameData.Instance.SavedPack.SaveUserPuzzleData(bookID, partID, puzzleData.ID, PuzzleStatus.UNLOCK);
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
        status = PuzzleStatus.UNLOCK;
        //status = GameData.Instance.SavedPack.ListCollectionSaveDatas[_collectionId].GetPuzzleCollectionSaveData(puzzleData.ID).Stt;
        DataCore.Debug.Log($"Puzzle: {status} {status}", false);
       
        UpdateIconLock();
    }


    public void StartDownload()
    {
        if (unLockCoverAnimation != null)
            unLockCoverAnimation.gameObject.SetActive(false);
        if (lockObject != null)
            lockObject.gameObject.SetActive(false);
        parentDownloading.gameObject.SetActive(true);
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
            GameManager.Instance.StartLevelCollection(_collectionId, _collectionIndex, puzzleData.ID, ConfigManager.GameData.PlayType.collection_play_puzzle);
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
        GameManager.Instance.StartLevelCollection(_collectionId, _collectionIndex, puzzleData.ID, ConfigManager.GameData.PlayType.collection_play_puzzle);
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
    }
}
