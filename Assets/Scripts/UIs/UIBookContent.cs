using System;
using System.Collections;
using System.Collections.Generic;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBookContent : BasePanel
{
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform rootContent;


    [Header("Puzzle Part")]
    [SerializeField] private Transform partPuzzleParent;
    [SerializeField] private GameObject partPuzzleItemPrefab;


    private BookMasterData bookMasterData;
    private List<UIPartPanel> listPartItem = new List<UIPartPanel>();
    private bool currentSubscribe = true;
    private int bookID = 0;
    private BasePanel previousPanel;
    private PuzzleInfoID puzzleCanPlayNow;

    [SerializeField] private NumberCounter txtNumberCounter;

    [Header("Book Illustration")]
    //[SerializeField] private TextMeshProUGUI txtIllustrationTitle;
    [SerializeField] private TextMeshProUGUI txtIllustrationValue;

    [Header("Book Status")]
    //[SerializeField] private TextMeshProUGUI txtStatusTitle;
    [SerializeField] private TextMeshProUGUI txtStatusValue;

    [Header("Book Release")]
    //[SerializeField] private TextMeshProUGUI txtReleaseTitle;
    [SerializeField] private TextMeshProUGUI txtReleaseValue;

    public override void Init()
    {

    }

    public override void OnUpdateInk(float animDuration, Action onComplete = null)
    {
        UpdateCurrencyData(animDuration, onComplete);
    }

    public override void SetData(object[] data)
    {
        base.SetData(data);
        bookMasterData = (BookMasterData)data[0];
        bookID = (int)data[1];
        previousPanel = (BasePanel)data[2];
        UpdateMasterData();

    }

    private void UpdateMasterData()
    {
        if (GameData.Instance.SavedPack == null || bookMasterData == null) return;

        currentSubscribe = GameData.Instance.SavedPack.GetSubcribeState(bookID);

        var thumbnailBook = bookMasterData.Thumbnail.Thumbnail;
        if (string.IsNullOrEmpty(thumbnailBook))
            return;

        int partCount = bookMasterData.ListChapters.Count;
        int puzzleCount = 0;

        for (int i = 0; i < listPartItem.Count; i++)
        {
            var partItem = listPartItem[i];
            partItem.OnRelease();
            Destroy(partItem.gameObject);
        }
        listPartItem.Clear();

        txtIllustrationValue.text = bookMasterData.Illustration;

        txtStatusValue.text = bookMasterData.Status.ToString();

        txtReleaseValue.text = UIManager.ConvertReleaseDay(bookMasterData.Release, "updated");

        // Load data for Parts
        for (int i = 0; i < bookMasterData.ListChapters.Count; i++)
        {
            var partData = bookMasterData.ListChapters[i];
            puzzleCount += partData.PuzzleLevels.Count;

            if (partPuzzleItemPrefab != null)
            {
                var item = Instantiate(partPuzzleItemPrefab);
                item.transform.SetParent(partPuzzleParent);
                item.transform.localScale = Vector3.one;
                UIPartPanel partItem = item.GetComponent<UIPartPanel>();
                partItem.SetData(partData, bookID, () => { }, ShowGetMoreInkDropPopup);
                listPartItem.Add(partItem);
                item.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(item.gameObject.GetComponent<RectTransform>().sizeDelta.x, ratioHightItem(partData.PuzzleLevels.Count, partData));
            }
        }
    }

    private int ratioHightItem(int size , ChapterMasterData masterData)
    {
        var defineSpaceOneLine = 600;
        if (size > 4)
            defineSpaceOneLine = 550;
        var tile = (int)size / 2;
        var space = tile * defineSpaceOneLine;
        if ((size % 2) > 0)
            space += defineSpaceOneLine;
        if (GetStatusFromUserData(masterData) == ChapterStatus.LOCK)
            space += 150;
        return space;
    }

    private ChapterStatus GetStatusFromUserData(ChapterMasterData masterData)
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

    private void ShowGetMoreInkDropPopup()
    {
        UIManager.Instance.ShowGetMoreInk(this);
    }

    private PuzzleInfoID GetPartCanUnlock()
    {
        string[] partIdList = bookMasterData.ListChapters[0].ID.Split('-');
        if (partIdList[1] == "1" || partIdList[1] == "2")
        {
            listPartItem[0].OnBtnBuyClick();
            return null;
        }

        for (int i = 0; i < bookMasterData.ListChapters.Count; i++)
        {
            var partMasterData = bookMasterData.ListChapters[i];
            var partStt = GameData.Instance.SavedPack.GetPartStatus(bookID, partMasterData.ID);

            if (partStt == ChapterStatus.LOCK)
            {
                return new PuzzleInfoID()
                {
                };

            }
        }

        return null;
    }


    private PuzzleInfoID GetPartCanUnlockFirst()
    {
        for (int i = 0; i < bookMasterData.ListChapters.Count; i++)
        {
            var partMasterData = bookMasterData.ListChapters[0];
            var partStt = GameData.Instance.SavedPack.GetPartStatus(bookID, partMasterData.ID);

            return new PuzzleInfoID()
            {
            };
        }

        return null;
    }
    private PuzzleInfoID GetPuzzleCanPlayNow()
    {
        for (int i = 0; i < bookMasterData.ListChapters.Count; i++)
        {
            var partMasterData = bookMasterData.ListChapters[i];
            var partStt = GameData.Instance.SavedPack.GetPartStatus(bookID, partMasterData.ID);

            if (partStt == ChapterStatus.LOCK)
            {
                continue;
            }
            for (int j = 0; j < partMasterData.PuzzleLevels.Count; j++)
            {
                var puzzleMasterData = partMasterData.PuzzleLevels[j];
                var puzzleStatus =
                    GameData.Instance.SavedPack.GetPuzzleStatus(bookID, partMasterData.ID,
                        puzzleMasterData.ID);

                if (puzzleStatus != PuzzleStatus.COMPLETE)
                {

                    DataCore.Debug.Log($"GetPuzzleCanPlayNow BookID: {bookID} chapter Id: {partMasterData.ID} puzzle id: {puzzleMasterData.ID}");

                    return new PuzzleInfoID()
                    {
                    };
                }
            }
        }

        return null;
    } 
   
    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
        container.transform.localPosition = new Vector3(container.transform.localPosition.x, 0, container.transform.localPosition.z);
        UpdateCurrencyData(0);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootContent);


        GameManager.Instance.CurBookIdOpening = bookID;
    }

    public override void Close()
    {
        DataCore.Debug.Log("UIBookContent Close", false);
        base.Close();
        GameManager.Instance.RemoveObjList(this);
        GameManager.Instance.CurBookIdOpening = -1;
        OnRelease();
    }

    public override void OnCloseManual()
    {
        GameManager.Instance.CurBookIdOpening = -1;
        base.OnCloseManual();
        OnCloseManualClick();
        
    }

    public void OnCloseManualClick()
    {
        DataCore.Debug.Log("UIBookContent OnCloseManual", false);
        SoundController.Instance.PlaySfxClick();

        base.Close();
        UIManager.Instance.UIHomepage.OpenBackHome();
        UIManager.Instance.ShowUISubscription(true);
        GameManager.Instance.RemoveObjList(this);
        OnRelease();
    }

    public void OnRelease()
    {
        DataCore.Debug.Log("UIBookContent OnRelease", false);
        for (int i = 0; i < listPartItem.Count; i++)
        {
            var partItem = listPartItem[i];
            partItem.OnRelease();
        }

        DOVirtual.DelayedCall(0.5f, () =>
        {
            this.PostEvent(EventID.OnUpdateResumePlaying);
        });
    }

    public override void ManualRefeshData()
    {
        UpdateMasterData();
    }

    public void UpdateCurrencyData(float animDuration, Action onComplete = null)
    {
        int newData = GameData.Instance.SavedPack.SaveData.Coin;
        txtNumberCounter.PlayAnim(newData, animDuration, onComplete);

    }

}
