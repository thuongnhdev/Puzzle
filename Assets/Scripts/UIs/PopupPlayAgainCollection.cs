using com.F4A.MobileThird;
using DataCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupPlayAgainCollection : BasePanel
{

    [SerializeField] private UIPuzzleCollectionItem puzzlePlayAgain;

    private PuzzleLevelData masterData;
    private int collectionId;
    private int collectionIndex;
    private string currPath = String.Empty;

    public override void Init()
    {
        base.Init();
    }

    public override void SetData(object[] data)
    {
        base.SetData(data);
        collectionId = (int)data[0];
        collectionIndex = (int)data[1];
        masterData = (PuzzleLevelData)data[2];
        puzzlePlayAgain.SetData( collectionId, collectionIndex, masterData, null, null);
        if (String.IsNullOrEmpty(currPath))
        {
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
        GameManager.Instance.CapturePuzzle.SharePuzzle(masterData.Name);
    }

    public void OnDownloadTap()
    {
        GameManager.Instance.PuzzleCompleteImg.enabled = true;
        GameManager.Instance.CapturePuzzle.DownLoadPuzzle(masterData.Name);
    }

    public void OnButtonPlayAgainTap()
    {
        Close();
        puzzlePlayAgain.OnPuzzlePlayAgainClick();
    }

    public override void OnSwipeLeft()
    {
        Close();
    }
}

