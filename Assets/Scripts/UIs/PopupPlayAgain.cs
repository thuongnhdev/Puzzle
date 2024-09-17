using com.F4A.MobileThird;
using DataCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupPlayAgain : BasePanel
{

    [SerializeField] private UIPuzzleItem puzzlePlayAgain;

    private PuzzleLevelData masterData;
    private int bookID;
    private string partID;
    private string puzzleName;
    private string currPath = String.Empty;

    public override void Init()
    {
        base.Init();
    }

    public override void SetData(object[] data)
    {
        base.SetData(data);
        masterData = (PuzzleLevelData)data[0];
        bookID = (int)data[1];
        partID = (string)data[2];
        puzzlePlayAgain.SetData(masterData, bookID, partID, null, null);
        puzzleName = masterData.Name;
        if (String.IsNullOrEmpty(currPath)) {
            AssetManager.Instance.ReleasePath(currPath);
        }
        currPath = masterData.Thumbnail.Thumbnail;
        AssetManager.Instance.LoadPathAsync<Sprite>(masterData.CompletePuzzleImage.CompleteImage, (thumb) =>
        {
            if (thumb != null)
            {
                GameManager.Instance.PuzzleCompleteImg.enabled = true;
                GameManager.Instance.PuzzleCompleteImg.sprite = thumb;
            }
        });

    }


    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        GameManager.Instance.CapturePuzzle.ResetManual();
        GameManager.Instance.PuzzleCompleteImg.enabled = false;

        base.Open();
    }

    public override void Close()
    {
        GameManager.Instance.CapturePuzzle.ResetManual();
        GameManager.Instance.PuzzleCompleteImg.enabled = false;
        base.Close();
        GameManager.Instance.RemoveObjList(this);
    }

    public void OnShareTap()
    {
        GameManager.Instance.PuzzleCompleteImg.enabled = true;
        GameManager.Instance.CapturePuzzle.SharePuzzle(puzzleName);
    }

    public void OnDownloadTap()
    {
        GameManager.Instance.PuzzleCompleteImg.enabled = true;
        GameManager.Instance.CapturePuzzle.DownLoadPuzzle(puzzleName);
    }

    public void OnButtonPlayAgainTap()
    {
        Close();
        puzzlePlayAgain.OnPuzzlePlayAgainClick();

        bool isResumePlay = ConfigManager.GameData.PlayType.replay_puzzle == ConfigManager.GameData.PlayType.resume_puzzle ||
                            (GameData.Instance.SavedPack.GetCurrentPuzzleData() != null && GameData.Instance.SavedPack.GetCurrentPuzzleData().savedData.m_LsObjectDone.Count > 0);

        if (isResumePlay)
        {
            //AdsService.Instance.ShowInterstitial(AdsService.IntAdPlacementResume, (complete) =>
            //{
            //    SoundController.Instance.MuteBgMusic(false);
            //});
        }
    }

    public override void OnSwipeLeft()
    {
        Close();
    }
}

