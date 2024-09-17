using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EventDispatcher;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using DataCore;
using com.F4A.MobileThird;

public class UIArtBlitsCompleted : BasePanel
{
    [SerializeField] private Image puzzleImg;
    [SerializeField] private TextMeshProUGUI descriptionTxt;
    [SerializeField] private List<UIAnimation> uiAnimations = new List<UIAnimation>();
    [SerializeField] private Image inkIcon;
    [SerializeField] private Image inkIconMoving;
    [SerializeField] private NumberCounter txtNumberCounter;
    [SerializeField] private NumberCounter txtNumberCounterCurrent;
    [SerializeField] private Image inkTopCurrency;
    [SerializeField] private RectTransform btnContinue;
    [SerializeField] private Button btnClaim;

    [SerializeField] private RectTransform iconOnOffDes;
    [SerializeField] private Image backgroundOnOffDes;
    [SerializeField] private Sprite[] sprOnOffDes;
    [SerializeField] private RectTransform[] posOnOffDes;
    [SerializeField] private TextMeshProUGUI descriptionTxtOnOff;
    [SerializeField] private TextMeshProUGUI inkButtonTxt;

    [SerializeField] private RectTransform rectTransformMusic;
    [SerializeField] private RectTransform iconOnOffMusic;
    [SerializeField] private RectTransform[] posOnOffMusic;
    [SerializeField] private TextMeshProUGUI musicTxtOnOff;
    [SerializeField] private Image backgroundOnOffMusic;


    [SerializeField] private Button btnRemain;
    [SerializeField] private TextMeshProUGUI inkButtonRemainTxt;
    [SerializeField] private TextMeshProUGUI remainTodayTxt;

    [SerializeField] private GameObject currencyPanel;

    [SerializeField] private GameObject inkPanelNormal;


    [SerializeField] private GameObject infoCollection;
    [SerializeField] private TextMeshProUGUI tmpAuthorCollection;
    [SerializeField] private TextMeshProUGUI tmpBookNameCollection;
    private int ink;

    private MasterDataStore _masterDataStore;
    private PuzzleLevelData puzzleLevelData;
    private Action onComplete;
    public Vector3 InkTopCurrencyIconPos
    {
        get { return inkTopCurrency.transform.position; }
    }

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
        if (data.Length == 0)
        {
            DataCore.Debug.LogWarning("UI Puzzle Completed needs sprite and description");
            return;
        }
        onComplete = (Action)data[1];
        puzzleLevelData = (PuzzleLevelData)data[0];
        txtNumberCounterCurrent.PlayAnim(GameData.Instance.SavedPack.SaveData.Coin, 0);
        ink = puzzleLevelData.Ink;
        descriptionTxt.SetText(puzzleLevelData.Desc.ToString().Replace("\\n", "\n"));
        _masterDataStore = MasterDataStore.Instance;
        
    }

    public override void Open()
    {
        infoCollection.SetActive(true);
        var bookList = _masterDataStore.BookDatas;
        string author = "";
        string bookName = "";
        var puzzleName = puzzleLevelData.Name;
        puzzleName = puzzleName.Replace("_", " ");
        string[] subs = puzzleName.Split(' ');
        string name = subs[0];
        bool isNumber = int.TryParse(subs[1], out int n);
        if (!isNumber)
            name = subs[0] + " " + subs[1];
        foreach (var itemBook in bookList)
        {
            if (itemBook.BookName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                author = "by " + itemBook.Author;
                bookName = itemBook.BookName;
            }
        }
        if(string.IsNullOrEmpty(bookName))
        {
            foreach (var itemBook in bookList)
            {
                string nameBook = subs[1];
                if (itemBook.BookName.IndexOf(nameBook, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    author = "by " + itemBook.Author;
                    bookName = itemBook.BookName;
                }
            }
        }
        if (string.IsNullOrEmpty(bookName) && subs.Length > 2)
        {
            foreach (var itemBook in bookList)
            {
                string nameBook = subs[2];
                if (itemBook.BookName.IndexOf(nameBook, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    author = "by " + itemBook.Author;
                    bookName = itemBook.BookName;
                }
            }
        }
        if (string.IsNullOrEmpty(bookName) && subs.Length > 3)
        {
            foreach (var itemBook in bookList)
            {
                string nameBook = subs[3];
                if (itemBook.BookName.IndexOf(nameBook, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    author = "by " + itemBook.Author;
                    bookName = itemBook.BookName;
                }
            }
        }
        tmpAuthorCollection.SetText(author);
        tmpBookNameCollection.SetText(bookName);
      

        txtNumberCounterCurrent.PlayAnim(GameData.Instance.SavedPack.SaveData.Coin, 0);
        inkButtonTxt.SetText(ink.ToString());
        ShowUiCurrency();
        base.Open();

        disableButton(false);
        rectTransformMusic.gameObject.SetActive(false);
        var stepGame = GameManager.Instance.GetStepGame();

        btnClaim.gameObject.SetActive(true);
        rectTransformMusic.gameObject.SetActive(true);

        var isMusic = GameData.Instance.SavedPack.SaveData.IsMusicActive;
        var colorBg = isMusic == true ? backgroundOnOffMusic.sprite = sprOnOffDes[0] : backgroundOnOffMusic.sprite = sprOnOffDes[1];
        var pos = isMusic == true ? iconOnOffMusic.localPosition = posOnOffMusic[0].localPosition : iconOnOffMusic.localPosition = posOnOffMusic[1].localPosition;
        var msg = isMusic == true ? musicTxtOnOff.text = "MUSIC: ON" : musicTxtOnOff.text = "MUSIC: OFF";


        if (CanShowRemain()) btnRemain.gameObject.SetActive(true);
        var inkRatio = float.Parse(FirebaseManager.GetValueRemote(ConfigManager.KeyGetExtraRewardArtBlitz));
        if (inkRatio == 0)
            inkRatio = ConfigManager.GetExtraRewardArtBlitzValue;

        var inkRaise = (int)(ink * inkRatio);
        inkButtonRemainTxt.SetText(inkRaise.ToString());
        remainTodayTxt.SetText("REMAINING TODAY: " + GameData.Instance.SavedPack.SaveData.PuzzleCompleteRemain);
       

        SetStatusDescription();


        if (GameData.Instance.IsVipIap())
            btnRemain.gameObject.SetActive(false);

        AssetManager.Instance.DownloadResource(puzzleLevelData.ThumbnailLabel(), completed: (size) =>
        {
            AssetManager.Instance.LoadPathAsync<Sprite>(puzzleLevelData.CompletePuzzleImage.CompleteImage, (thumb) =>
            {
                if (thumb != null)
                    puzzleImg.sprite = thumb;
            });
        });

    }


    private void disableButton(bool isActive)
    {
        btnClaim.gameObject.SetActive(isActive);
        btnRemain.gameObject.SetActive(isActive);
    }
    private void SetStatusDescription()
    {
        var isDes = GameData.Instance.SavedPack.SaveData.IsDescriptionActive;
        uiAnimations[1].gameObject.SetActive(isDes);
        var colorBg = isDes == true ? backgroundOnOffDes.sprite = sprOnOffDes[0] : backgroundOnOffDes.sprite = sprOnOffDes[1];
        var pos = isDes == true ? iconOnOffDes.localPosition = posOnOffDes[0].localPosition : iconOnOffDes.localPosition = posOnOffDes[1].localPosition;
        var msg = isDes == true ? descriptionTxtOnOff.text = "DESCRIPTION: ON" : descriptionTxtOnOff.text = "DESCRIPTION: OFF";
    }

    private void SetStatusMusic()
    {
        var isMusic = GameData.Instance.SavedPack.SaveData.IsMusicActive;
        var colorBg = isMusic == true ? backgroundOnOffMusic.sprite = sprOnOffDes[0] : backgroundOnOffMusic.sprite = sprOnOffDes[1];
        var pos = isMusic == true ? iconOnOffMusic.localPosition = posOnOffMusic[0].localPosition : iconOnOffMusic.localPosition = posOnOffMusic[1].localPosition;
        var msg = isMusic == true ? musicTxtOnOff.text = "MUSIC: ON" : musicTxtOnOff.text = "MUSIC: OFF";
        if (!GameData.Instance.SavedPack.SaveData.IsMusicActive)
        {
            SoundController.Instance.StopBGMusic();
        }
        else
        {
            SoundController.Instance.PlayBgMusicAgain();
        }
    }

    public void UpdateCurrencyData(float animDuration, Action onComplete = null)
    {
        int newData = GameData.Instance.SavedPack.SaveData.Coin;
        txtNumberCounterCurrent.PlayAnim(newData, animDuration, onComplete);

    }

    public override void Close()
    {
        for (int i = 0; i < uiAnimations.Count; i++)
        {
            uiAnimations[i].ResetPositionToInitStatus(Status.Out);
        }
        base.Close();
    }

    public void OnClickClaim()
    {
        GameData.Instance.SavedPack.SaveData.IsResumeComplete = false;
        SoundController.Instance.PlaySfxClick();
        disableButton(false);

        txtNumberCounter.ShowText(false, () =>
        {
            MoveInkIconToTop();
        });
    }

    public void OnClickRemain()
    {
        GameData.Instance.SavedPack.SaveData.IsResumeComplete = false;
        disableButton(false);
        SoundController.Instance.PlaySfxClick();
        AdsService.Instance.ShowRewardedAd(AdsService.RwAdUnitIdDoublePuzzleReward,
         onRewardedAdClosedEvent: (complete) =>
         {
             SoundController.Instance.MuteBgMusic(false);
             if (!complete)
             {
                 UIManager.Instance.ShowPopupNoAds();
                 GameData.Instance.SavedPack.SaveData.PuzzleCompleteRemain--;
                 txtNumberCounter.ShowText(false, () =>
                 {
                     MoveInkIconToTop();
                 });
                 GameData.Instance.RequestSaveGame();
             }
         }, onRewardedAdReceivedReward: (adId, placement) =>
         {
             GameData.Instance.SavedPack.SaveData.PuzzleCompleteRemain--;
             txtNumberCounter.Reset();
             var inkRatio = float.Parse(FirebaseManager.GetValueRemote(ConfigManager.KeyGetExtraReward));
             if (inkRatio == 0)
                 inkRatio = ConfigManager.GetExtraRewardArtBlitzValue;
             ink *= (int)inkRatio;
             if (GameManager.Instance.GetStepGame() < StepGameConstants.PlayPuzzleOne)
             {
                 ink = int.Parse(FirebaseManager.GetValueRemote(ConfigManager.KeyGetExtraRewardTutorial));
             }
             txtNumberCounter.PlayAnim(ink, 1.0f, () =>
             {
                 txtNumberCounter.ShowText(false, () =>
                 {
                     MoveInkIconToTop();
                 });
             });

             GameData.Instance.RequestSaveGame();
         });

    }

    public void OnClickDescription()
    {
        if (SoundController.Instance != null) SoundController.Instance.PlaySfxClick();
        GameData.Instance.SavedPack.SaveData.IsDescriptionActive = !GameData.Instance.SavedPack.SaveData.IsDescriptionActive;
        GameData.Instance.RequestSaveGame();
        SetStatusDescription();
    }

    public void OnClickMusic()
    {
        if (SoundController.Instance != null) SoundController.Instance.PlaySfxClick();
        GameData.Instance.SavedPack.SaveData.IsMusicActive = !GameData.Instance.SavedPack.SaveData.IsMusicActive;
        GameData.Instance.RequestSaveGame();
        SetStatusMusic();
    }

    private void OnShowIntWhenCompletePuzzle(Action completed)
    {
        // check time show int
        if(AdsLogic.IsTimeIntAdsArtBlitz())
        {
            AdsService.Instance.ShowInterstitial(AdsService.IntAdPlacementBetween, (complete) =>
            {
                GameData.Instance.SavedPack.SaveData.CountCompletedPuzzle = 0;
                SoundController.Instance.MuteBgMusic(false);
                completed?.Invoke();
                AdsLogic.SetTimeBeginArtBlitzComplete();
            });
        }
        else
        {
            completed?.Invoke();
        }
    }

    private void ShowUiCurrency()
    {
        var timeDelay = 1.0f;
        currencyPanel.SetActive(true);

        inkPanelNormal.SetActive(true);
        inkIcon.gameObject.SetActive(true);
        inkIcon.transform.DOPunchScale(Vector3.one, 0.25f, 1, 0.5f);
        txtNumberCounter.Reset();
        txtNumberCounter.PlayAnim(ink, timeDelay, () =>
        {
        });
    }

    private void MoveInkIconToTop()
    {
        inkIconMoving.transform.localScale = Vector3.one;
        inkIconMoving.transform.position = inkIcon.transform.position;
        inkIconMoving.transform.DOScale(Vector3.one * 0.5f, 1.0f);
        inkIcon.gameObject.SetActive(false);
        inkIconMoving.transform.DOMove(ShareUIManager.Instance.InkTopCurrencyIconPos, 1.0f).OnComplete(() =>
        {
            inkPanelNormal.SetActive(false);
            GameData.Instance.IncreaseInks(ink, ConfigManager.GameData.ResourceEarnSource.puzzle_reward);
            UpdateCurrencyData(1.0f, () =>
            {
                if (GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
                {
                    OnShowIntWhenCompletePuzzle(() =>
                    {
                        // Init logic success here
                        GameData.Instance.RequestSaveGame();

                        GameData.Instance.SavedPack.LiveEventSavedData.UpdateCompletePuzzle(GameManager.Instance.CurPostCardId, GameManager.Instance.CurEventPuzzleId);
                        if (UIManager.Instance.UiLuckyDraw.CanShow())
                        {
                            Action onClose = CheckStepByPlayedGame;

                            UIManager.Instance.UILuckyDrawArtBlitz.SetData(new object[] { onClose });
                            UIManager.Instance.ShowUiLuckyArtBlitzDraw();
                        }
                        else
                        {
                            CheckStepByPlayedGame();
                        }
                    });
                
                 
                }

            });

        });
    }

    private void CheckStepByPlayedGame()
    {
        Close();
        onComplete?.Invoke();
        onComplete = null;
    }

    public void PlayAnimInkIcon()
    {
        inkTopCurrency.transform.DOPunchScale(Vector3.one, 0.25f, 1, 0.1f);
    }

    public bool CanShowRemain()
    {
        if (!AdsService.Instance.IsRewardedAdLoaded())
        {
            AdsService.Instance.LoadRewardedAd();
            return false;
        }

        if (IsNextDay())
        {
            RefreshData();
        }
        if (GameData.Instance.SavedPack.SaveData.PuzzleCompleteRemain > 0)
        {
            return true;
        }

        return false;
    }


    private const int TIME_REFRESH = 0;
    private const int REMAIN_PER_DAY = 5;
    private void RefreshData()
    {
        GameData.Instance.SavedPack.SaveData.PuzzleCompleteRemain = REMAIN_PER_DAY;

        GameData.Instance.RequestSaveGame();
    }

    private bool IsNextDay()
    {
        DateTime now = DateTime.UtcNow;
        int lastDay = GameData.Instance.SavedPack.SaveData.LastDayCompleteRemain;

        if (lastDay != now.Day)
        {
            if (Mathf.Abs(lastDay - now.Day) > 1)
            {
                if (now.Hour >= TIME_REFRESH)
                {
                    lastDay = now.Day;
                    GameData.Instance.SavedPack.SaveData.LastDayCompleteRemain = lastDay;
                }
                else
                {
                    lastDay = now.Day - 1;
                    GameData.Instance.SavedPack.SaveData.LastDayCompleteRemain = lastDay;
                }

                return true;
            }
            else
            {
                if (now.Hour >= TIME_REFRESH)
                {
                    lastDay = now.Day;
                    GameData.Instance.SavedPack.SaveData.LastDayCompleteRemain = lastDay;
                    return true;
                }
            }
        }
        return false;
    }
}
