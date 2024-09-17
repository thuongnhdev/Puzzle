using System;
using System.Collections;
using System.Collections.Generic;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleInfoID
{
    public int BookID;
    public string PartID;
    public int PuzzleID;
    public int PartIndex;
}

public class UIBookDetail : BasePanel
{
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform rootContent;

    [Header("Book Title")]
    [SerializeField] private TextMeshProUGUI txtBookName;
    [SerializeField] private TextMeshProUGUI txtBookAuthor;
    [SerializeField] private TextMeshProUGUI txtBookAuthorLine2;
    [SerializeField] private Image imgThumbnail;
    [SerializeField] private GameObject bookThumbnail;

    [Header("Book Illustration")]
    //[SerializeField] private TextMeshProUGUI txtIllustrationTitle;
    [SerializeField] private TextMeshProUGUI txtIllustrationValue;

    [Header("Book Status")]
    //[SerializeField] private TextMeshProUGUI txtStatusTitle;
    [SerializeField] private TextMeshProUGUI txtStatusValue;

    [Header("Book Release")]
    //[SerializeField] private TextMeshProUGUI txtReleaseTitle;
    [SerializeField] private TextMeshProUGUI txtReleaseValue;

    [Header("Book Description")]
    [SerializeField] private RectTransform descriptionPanel;
    [SerializeField] private TextMeshProUGUI txtDescription;
    [SerializeField] private TextMeshProUGUI txtDescriptionLinked;
    [SerializeField] private GameObject moreBtn;
    [SerializeField] private TextMeshProUGUI txtFullDescription;

    [Header("Puzzle Part")]
    [SerializeField] private Transform partPuzzleParent;
    [SerializeField] private TextMeshProUGUI txtPartNumber;
    [SerializeField] private GameObject partPuzzleItemPrefab;

    [Header("Subscribe")]
    [SerializeField] private Image subscribeIcon;
    [SerializeField] private Sprite subscribeIconOn;
    [SerializeField] private Sprite subscribeIconOff;

    [Header("Button")]
    [SerializeField] private GameObject btnUnlockChapter;
    [SerializeField] private GameObject btnPlayNow;
    [SerializeField] private TextMeshProUGUI txtUnlockChapter;
    [SerializeField] private NumberCounter txtNumberCounter;
    //[SerializeField] private BasePanel popupGetMoreInks;

    [SerializeField] private GameObject[] txtBtnDownload;


    private string txtUnlockFormat = "{0} - Chapter {1}";

    private BookMasterData bookMasterData;
    private List<UIPartItem> listPartItem = new List<UIPartItem>();
    private string partNumberFormat = "{0} chapters, {1} puzzles";
    private string authorFormat = "by {0}";
    private bool currentSubscribe = true;
    private int bookID = 0;
    private BasePanel previousPanel;
    private PuzzleInfoID puzzleCanPlayNow;


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
        txtBookName.text = bookMasterData.BookName;

        if (txtBookAuthor == null || txtBookAuthorLine2 == null) return;

        txtBookAuthor.gameObject.SetActive(true);
        txtBookAuthorLine2.gameObject.SetActive(false);
        txtBookAuthor.text = string.Format(authorFormat, bookMasterData.Author);
        var tile = (float)Screen.height / Screen.width;
        if (tile > 1.5f && txtBookName.text.Length > 31)
        {
            txtBookAuthor.gameObject.SetActive(false);
            txtBookAuthorLine2.gameObject.SetActive(true);
        }

        var thumbnailBook = bookMasterData.Thumbnail.Thumbnail;
        if (string.IsNullOrEmpty(thumbnailBook))
            return;

        AssetManager.Instance.LoadPathAsync<Sprite>(thumbnailBook, (thumb) =>
        {
            if (thumb != null && imgThumbnail != null)
            {
                imgThumbnail.sprite = thumb;
            }
        });

        txtIllustrationValue.text = bookMasterData.Illustration;

        txtStatusValue.text = bookMasterData.Status.ToString();

        txtReleaseValue.text = UIManager.ConvertReleaseDay(bookMasterData.Release, "updated");

        txtDescription.text = bookMasterData.Description;
        txtFullDescription.text = bookMasterData.Description;

        txtDescription.enabled = true;
        moreBtn.SetActive(false);
        DOVirtual.DelayedCall(0.1f, () =>
        {
            txtDescriptionLinked.enabled = txtDescription.isTextTruncated;
            var numberText = 250;
            if (Screen.width > 1500)
                numberText = 350;

            if (bookMasterData.Description.Length > numberText) moreBtn.SetActive(txtDescription.isTextTruncated);
        });


        txtFullDescription.enabled = false;

        descriptionPanel.sizeDelta = new Vector2(descriptionPanel.sizeDelta.x, 580);

        int partCount = bookMasterData.ListChapters.Count;
        int puzzleCount = 0;

        for (int i = 0; i < listPartItem.Count; i++)
        {
            Destroy(listPartItem[i].gameObject);
        }
        listPartItem.Clear();

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
                UIPartItem partItem = item.GetComponent<UIPartItem>();
                partItem.SetData(partData, bookID, UpdateButton, ShowGetMoreInkDropPopup);
                listPartItem.Add(partItem);
            }
        }

        txtPartNumber.text = string.Format(partNumberFormat, partCount, puzzleCount);

        UpdateSubscribeIcon();

        UpdateButton();
    }

    private void UpdateButton()
    {
        try
        {

            puzzleCanPlayNow = GetPuzzleCanPlayNow();
            if (btnPlayNow == null || btnUnlockChapter == null)
                return;

            if (puzzleCanPlayNow != null)
            {
                btnPlayNow.SetActive(true);
                btnUnlockChapter.SetActive(false);
            }            
            else
            {
                btnPlayNow.SetActive(false);
                var partCanUnlock = GetPartCanUnlock();
                if (partCanUnlock != null)
                {
                    btnUnlockChapter.SetActive(true);
                    var price = bookMasterData.ListChapters[partCanUnlock.PartIndex].Price;
                    txtUnlockChapter.text = string.Format(txtUnlockFormat, price, partCanUnlock.PartIndex + 1);

                    if (GameData.Instance.IsVipIap())
                    {
                        btnPlayNow.SetActive(true);
                        btnUnlockChapter.SetActive(false);
                    }
                }
                else
                {

                    btnUnlockChapter.SetActive(false);
                    var partCanUnlockFirst = GetPartCanUnlockFirst();
                    if (partCanUnlockFirst != null)
                    {
                        btnPlayNow.SetActive(true);
                    }
                    else
                    {
                        btnPlayNow.SetActive(false);
                    }

                }
            }
        }
        catch (Exception ex)
        {
            DataCore.Debug.Log($"Failed UIBookDetail UpdateButton {ex.Message}");
        }
    }

    private void ShowGetMoreInkDropPopup()
    {
        UIManager.Instance.ShowGetMoreInk(this);
        //popupGetMoreInks.Open();
    }

    private void UpdateSubscribeIcon()
    {
        subscribeIcon.sprite = currentSubscribe ? subscribeIconOff : subscribeIconOn;
    }

    public void OnSubscribeTouch()
    {
        SoundController.Instance.PlaySfxClick();
        currentSubscribe = GameData.Instance.SavedPack.SwitchIsSubcribeState(bookID);
        GameData.Instance.RequestSaveGame();
        UpdateSubscribeIcon();

        this.PostEvent(EventID.OnUpdateSubcribeState, bookID);
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
                    BookID = bookID,
                    PartID = partMasterData.ID,
                    PartIndex = i,
                    PuzzleID = -1
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
                BookID = bookID,
                PartID = partMasterData.ID,
                PartIndex = i,
                PuzzleID = -1
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
                        BookID = bookID,
                        PartID = partMasterData.ID,
                        PuzzleID = puzzleMasterData.ID
                    };
                }
            }
        }

        return null;
    }
   
    public void OnPlayNowTap()
    {
        if (SoundController.Instance != null) SoundController.Instance.PlaySfxClick();
        string partId = "-1";
        int puzzleId = -1;
        DataCore.Debug.Log($"bookID: {bookID}");

        if (puzzleCanPlayNow != null)
        {
            if (!string.IsNullOrEmpty(puzzleCanPlayNow.PartID))
                partId = puzzleCanPlayNow.PartID;

            if (puzzleCanPlayNow.PuzzleID > 0)
                puzzleId = puzzleCanPlayNow.PuzzleID;
        }
        DataCore.Debug.Log($"OnPlayNowTap bookID: {bookID} chapterId: {partId} puzzleId: {puzzleId}");

        if (string.Compare(partId, "-1") == 0 || puzzleId == -1)
        {
            if (Toast.instance != null)
                Toast.instance.ShowMessage("Don't have any puzzle available!");
        }
        else
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StartLevel(bookID, partId, puzzleId, ConfigManager.GameData.PlayType.new_puzzle);

        }

    }

    public void OnUnlockChapterTap()
    {
        SoundController.Instance.PlaySfxClick();
        var partCanUnlock = GetPartCanUnlock();
        if (partCanUnlock != null)
        {
            for (int i = 0; i < listPartItem.Count; i++)
            {
                if (listPartItem[i].MasterData.ID == partCanUnlock.PartID)
                {
                    listPartItem[i].OnBtnBuyClick();
                }
            }
        }
        UpdateButton();
    }

    public void OnUnlockChapterTapVip()
    {
        for (int i = 0; i < listPartItem.Count; i++)
        {
            listPartItem[i].SetStatusBtnVip();
            listPartItem[i].OnBtnBuyClick();
        }
        UpdateButton();
        //OnDownloadNowTap();
    }

    [ContextMenu("More")]
    public void OnMoreTap()
    {
        SoundController.Instance.PlaySfxClick();
        txtFullDescription.enabled = true;
        txtDescription.enabled = false;
        txtDescriptionLinked.enabled = false;
        moreBtn.SetActive(false);
        descriptionPanel.sizeDelta = new Vector2(descriptionPanel.sizeDelta.x, txtFullDescription.preferredHeight + 130);
        DOVirtual.DelayedCall(0.0f, () => { LayoutRebuilder.ForceRebuildLayoutImmediate(container); }).Play();

    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
        container.transform.localPosition = new Vector3(container.transform.localPosition.x, 0, container.transform.localPosition.z);
        UpdateCurrencyData(0);
        ShareUIManager.Instance.ShowSharedUI(SceneSharedEle.SETTING, false);
        bookThumbnail.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rootContent);
        //LayoutRebuilder.ForceRebuildLayoutImmediate(partPuzzleParent.GetComponent<RectTransform>());

        if (GameData.Instance.IsVipIap())
            OnUnlockChapterTapVip();

        GameManager.Instance.CurBookIdOpening = bookID;
    }

    public override void Close()
    {
        base.Close();
        bookThumbnail.SetActive(false);
        GameManager.Instance.RemoveObjList(this);
        GameManager.Instance.CurBookIdOpening = -1;
    }

    public override void OnCloseManual()
    {
        GameManager.Instance.CurBookIdOpening = -1;
        base.OnCloseManual();
        OnCloseManualClick();
    }

    public void OnCloseManualClick()
    {
        SoundController.Instance.PlaySfxClick();

        base.Close();
        bookThumbnail.SetActive(false);
        if (UIManager.Instance.UIHomepage._curId == 2)
        {
            UIManager.Instance.UIHomepage.OpenBackHome();
        }
        else
            UIManager.Instance.ShowHomePage();

        UIManager.Instance.ShowUISubscription(true);
        GameManager.Instance.RemoveObjList(this);
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
