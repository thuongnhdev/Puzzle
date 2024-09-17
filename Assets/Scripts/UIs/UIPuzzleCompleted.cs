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

public class UIPuzzleCompleted : BasePanel
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
        if (data.Length < 2)
        {
            DataCore.Debug.LogWarning("UI Puzzle Completed needs sprite and description");
            return;
        }
        txtNumberCounterCurrent.PlayAnim(GameData.Instance.SavedPack.SaveData.Coin, 0);
        ink = (int)data[2];
        descriptionTxt.SetText(data[1].ToString().Replace("\\n", "\n"));

        _masterDataStore = MasterDataStore.Instance;
        DataCore.Debug.Log($"UIPuzzleCompleted Set data: {data}", false);
    }

    public override void Open()
    {
        if (GameManager.Instance.PuzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
        {
            infoCollection.SetActive(true);
            var bookList = _masterDataStore.BookDatas;
            string author = "";
            string bookName = "";
            var puzzle = _masterDataStore.GetCollectionPuzzleByIndex(GameManager.Instance._collectionId, GameManager.Instance._collectionIndex);
            var puzzleName = puzzle.Name;
            puzzleName = puzzleName.Replace("_", " ");
            string[] subs = puzzleName.Split(' ');
            string name = subs[0];
            bool isNumber = int.TryParse(subs[1], out int _);
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
            tmpAuthorCollection.SetText(author);
            tmpBookNameCollection.SetText(bookName);
        }
        else
        {
            infoCollection.SetActive(false);
        }

        txtNumberCounterCurrent.PlayAnim(GameData.Instance.SavedPack.SaveData.Coin, 0);
        inkButtonTxt.SetText(ink.ToString());
        ShowUiCurrency();
        base.Open();

        DisableButton(false);
        rectTransformMusic.gameObject.SetActive(false);
        var stepGame = GameManager.Instance.GetStepGame();
        if (stepGame > StepGameConstants.Tutorial)
        {
            btnClaim.gameObject.SetActive(true);
            rectTransformMusic.gameObject.SetActive(true);
            UpdateMusicSwitch();            
        }

        if (stepGame > StepGameConstants.Tutorial)
        {
            if (CanShowRemain()) btnRemain.gameObject.SetActive(true);
            var inkRatio = float.Parse(FirebaseManager.GetValueRemote(ConfigManager.KeyGetExtraReward));
            if (inkRatio == 0)
                inkRatio = ConfigManager.GetExtraRewardValue;
            var inkRaise = ink * inkRatio;
            inkButtonRemainTxt.SetText(inkRaise.ToString());
            remainTodayTxt.SetText("REMAINING TODAY: " + GameData.Instance.SavedPack.SaveData.PuzzleCompleteRemain);
        }

        SetStatusDescription();

        if (GameManager.Instance.PuzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
        {
            int index = GameManager.Instance._collectionIndex + 1;
            var puzzle = _masterDataStore.GetCollectionPuzzleByIndex(GameManager.Instance._collectionId, GameManager.Instance._collectionIndex);
            GameData.Instance.SavedPack.DataSetCollectionPuzzlePlaying(GameManager.Instance._collectionId, GameManager.Instance._collectionIndex, PuzzleStatus.COMPLETE);
            var nextPuzzle = _masterDataStore.GetCollectionPuzzleByIndex(GameManager.Instance._collectionId, index);
            
            if (nextPuzzle != null)
            {
                var savedPuzzle = GameData.Instance.SavedPack.DataGetCollectionPuzzlePlaying(GameManager.Instance._collectionId, index);
                if (savedPuzzle == null || savedPuzzle.PuzzleStatus != PuzzleStatus.COMPLETE)
                {
                    GameData.Instance.SavedPack.DataSetCollectionPuzzlePlaying(GameManager.Instance._collectionId, index, PuzzleStatus.UNLOCK);
                }                
            }
        }

        if (GameData.Instance.IsVipIap())
            btnRemain.gameObject.SetActive(false);


    }
    private void UpdateMusicSwitch()
    {
        var isMusic = GameData.Instance.SavedPack.SaveData.IsMusicActive;
        if (isMusic)
        {
            backgroundOnOffMusic.sprite = sprOnOffDes[0];
            iconOnOffMusic.localPosition = posOnOffMusic[0].localPosition;
            musicTxtOnOff.text = "MUSIC: ON";
        }
        else
        {
            iconOnOffMusic.localPosition = posOnOffMusic[1].localPosition;
            backgroundOnOffMusic.sprite = sprOnOffDes[1];
            musicTxtOnOff.text = "MUSIC: OFF";
        }
    }


    private void DisableButton(bool isActive)
    {
        btnClaim.gameObject.SetActive(isActive);
        btnRemain.gameObject.SetActive(isActive);
    }
    private void SetStatusDescription()
    {
        var isDes = GameData.Instance.SavedPack.SaveData.IsDescriptionActive;
        uiAnimations[1].gameObject.SetActive(isDes);
        if (isDes)
        {
            backgroundOnOffDes.sprite = sprOnOffDes[0];
            iconOnOffDes.localPosition = posOnOffDes[0].localPosition;
            descriptionTxtOnOff.text = "DESCRIPTION: ON";
        }
        else
        {
            backgroundOnOffDes.sprite = sprOnOffDes[1];
            iconOnOffDes.localPosition = posOnOffDes[1].localPosition;
            descriptionTxtOnOff.text = "DESCRIPTION: OFF";
        }
    }

    private void SetStatusMusic()
    {
        UpdateMusicSwitch();

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
        DisableButton(false);

        txtNumberCounter.ShowText(false, () =>
        {
            MoveInkIconToTop();
        });
    }

    public void OnClickRemain()
    {
        GameData.Instance.SavedPack.SaveData.IsResumeComplete = false;
        DisableButton(false);
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
                 inkRatio = ConfigManager.GetExtraRewardValue;
             ink *= (int)inkRatio;

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

    public void OnClickNextPuzzle()
    {
        if (SoundController.Instance != null) SoundController.Instance.PlaySfxClick();
        if (GameData.Instance == null || UIManager.Instance == null) return;
        DataCore.Debug.Log($"StepGame {GameManager.Instance.GetStepGame()}", false);
        if (GameManager.Instance.GetStepGame() < StepGameConstants.PlayPuzzleOne)
        {
            StepTutorial();
        }
        else {
            CompletedEndingPuzzle();            
        }        
    }

    private void CompletedEndingPuzzle()
    {
        try
        {
            GameData.Instance.SavedPack.SaveData.IsResumeComplete = false;
            if (GameData.Instance.SavedPack == null)
                return;

            if (GameData.Instance.SavedPack.SaveData == null)
                return;


            if (GameManager.Instance.PuzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
            {
                OnShowIntWhenCompletePuzzle(() =>
                {
                    this.PostEvent(EventID.BackPuzzleTab);
                });
            }
            else
            {
                GameManager.Instance.ShowChapterCompleteMode((isChapterNext, index) =>
                {
                    OnShowIntWhenCompletePuzzle(() =>
                    {
                        if (isChapterNext)
                            this.PostEvent(EventID.BackHomeNextChapter, isChapterNext);
                        else
                            this.PostEvent(EventID.BackHome);
                    });
                });
            }
        }
        catch (Exception ex)
        {
            DataCore.Debug.LogError(ex);
        }
    }
    private void OnShowIntWhenCompletePuzzle(Action completed)
    {
        var countFinishPuzzle = GameData.Instance.SavedPack.SaveData.CountCompletedPuzzle;
        var countRemote = int.Parse(FirebaseManager.GetValueRemote(ConfigManager.keyAndroidBetweenGamesAd));
        //var playCountFinish = PlayerPrefs.GetInt(ConfigManager.KeyTotalPlayPuzzleFinish, 0);
        if (GameManager.Instance.GetStepGame() < StepGameConstants.StepComplete && GameData.Instance.PlayedGame() < 5)
        {
            completed?.Invoke();
            return;
        }
        DataCore.Debug.Log($"OnShowIntWhenCompletePuzzle: countFinishPuzzle {countFinishPuzzle} countRemote {countRemote}");
#if UNITY_ANDROID
        if (countFinishPuzzle >= int.Parse(FirebaseManager.GetValueRemote(ConfigManager.keyAndroidBetweenGamesAd)))
        {
            AdsService.Instance.ShowInterstitial(AdsService.IntAdPlacementBetween, (complete) =>
            {
                GameData.Instance.SavedPack.SaveData.CountCompletedPuzzle = 0;
                SoundController.Instance.MuteBgMusic(false);
                completed?.Invoke();
            });
        }
        else
        {
            completed?.Invoke();
        }
#elif UNITY_IOS
        if (countFinishPuzzle >= int.Parse(FirebaseManager.GetValueRemote(ConfigManager.keyIosBetweenGamesAd)))
        {
            AdsService.Instance.ShowInterstitial(AdsService.IntAdPlacementBetween, (complete) =>
            {
                GameData.Instance.SavedPack.SaveData.CountCompletedPuzzle = 0;
                SoundController.Instance.MuteBgMusic(false);
                completed?.Invoke();
            });
        } else {
            completed?.Invoke();
        }
#endif
    }


    private void ShowUiCurrency()
    {
        var timeDelay = 1.0f;
        currencyPanel.SetActive(true);
        if (GameManager.Instance.GetStepGame() < StepGameConstants.PlayPuzzleOne)
        {
            timeDelay = 0;
            //currencyPanel.SetActive(false);
            inkPanelNormal.SetActive(true);
            uiAnimations[2].Run(Status.In);
            txtNumberCounter.Reset();
            txtNumberCounter.PlayAnim(ink, timeDelay, () =>
            {
            });
        }
        else
        {
            inkPanelNormal.SetActive(true);
            inkIcon.gameObject.SetActive(true);
            inkIcon.transform.DOPunchScale(Vector3.one, 0.25f, 1, 0.5f);
            txtNumberCounter.Reset();
            txtNumberCounter.PlayAnim(ink, timeDelay, () =>
            {
                var stepGame = GameManager.Instance.GetStepGame();
                if (stepGame < StepGameConstants.PlayPuzzleOne)
                {
                    txtNumberCounter.ShowText(false, () =>
                    {
                        MoveInkIconToTop();
                    });
                }

            });
        }

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
                     var stepGame = GameManager.Instance.GetStepGame();
                     if (stepGame >= StepGameConstants.StepComplete)
                     {
                         var playCountFinish = PlayerPrefs.GetInt(ConfigManager.KeyTotalPlayPuzzleFinish, 0);
                         
                         if (UIManager.Instance.PopupPromotion.CanShow(GameData.Instance.PlayedGame() == PopupPromotion.MIN_GAME_FOR_FIRST_TIME_SHOW))
                         {
                             ShowPromotion();
                         }
                         else if (UIManager.Instance.UiLuckyDraw.CanShow())
                         {
                             Action onClose = CheckStepByPlayedGame;

                             UIManager.Instance.UiLuckyDraw.SetData(new object[] { onClose });
                             UIManager.Instance.ShowUiLuckyDraw();
                         }
                         else
                         {
                             CheckStepByPlayedGame();
                         }
                     }
                     else
                     {
                         CheckStepByPlayedGame();
                     }
                 }

             });

        });
    }

    private void ShowPromotion()
    {
        Action onClose = CheckStepByPlayedGame;
        UIManager.Instance.PopupPromotion.SetData(new object[] { onClose });
        UIManager.Instance.PopupPromotion.Open();
    }

    private bool isGetInk = false;
    void StepTutorial()
    {
        if (isGetInk)
            return;
        isGetInk = !isGetInk;
        //if (GameManager.Instance.GetStepGame() < StepGameConstants.PlayPuzzleOne)
        //{
        //    ink = int.Parse(FirebaseManager.GetValueRemote(ConfigManager.KeyGetExtraRewardTutorial));
        //}
        txtNumberCounter.PlayAnim(ink, 1.0f, () =>
        {
            txtNumberCounter.ShowText(false, () =>
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
                        isGetInk = !isGetInk;
                        Close();
                        GameManager.Instance.SetStepGame(StepGameConstants.PlayPuzzleOne);
                        GameData.Instance.RequestSaveGame();
                        this.PostEvent(EventID.BackPuzzleTab);
                    });
                });
            });
        });
    }

    private void CheckStepByPlayedGame()
    {
        DataCore.Debug.Log("CheckStepByPlayedGame", false);
        var playCountFinish = GameData.Instance.PlayedGame();
        if (playCountFinish == 1)
        {            
            UIManager.Instance.ShowUIPushNotiPermission(() =>
            {
                ShowUiDescriptionAnimation();
            });
            GameManager.Instance.SetStepGame(StepGameConstants.PushNotificationPermission);
        }
        else if (playCountFinish == 2)
        {
            Action onClose = OnShowRating;
            GameManager.Instance.SetStepGame(StepGameConstants.LuckyDrawOne);
            UIManager.Instance.UiLuckyDraw.SetData(new object[] { onClose });
            UIManager.Instance.ShowUiLuckyDraw();
        }
        else if (playCountFinish >= 3 && !GameManager.Instance.IsCompleteStepGame())
        {
            GameManager.Instance.SetStepGame(StepGameConstants.StepComplete);
            Action onClose = showAdNotif;
            UIManager.Instance.UiLuckyDraw.SetData(new object[] { onClose });
            UIManager.Instance.ShowUiLuckyDraw();
        }
        else
        {
            ShowUiDescriptionAnimation();
        }        
    }

    public void showAdNotif() {
        UIManager.Instance.ShowUiAdsNoti(() =>
        {
            CompletedEndingPuzzle();
        });
    }

    private void OnShowRating() {
        GameManager.Instance.OnRating();
        ShowUiDescriptionAnimation();
    }

    private void ShowUiDescriptionAnimation()
    {
        DataCore.Debug.Log("ShowUiDescriptionAnimation");
        if (GameManager.Instance.PuzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
        {
            CompletedEndingPuzzle();            
        }
        else
        {
            var isDes = GameData.Instance.SavedPack.SaveData.IsDescriptionActive;
            if (isDes)
            {
                for (int i = 0; i < uiAnimations.Count; i++)
                {
                    uiAnimations[i].Run(Status.In);
                }
            }
            else {
                CompletedEndingPuzzle();
            }

        }
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
