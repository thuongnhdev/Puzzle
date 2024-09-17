using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EventDispatcher;
using UnityEngine;
using com.F4A.MobileThird;
using DataCore;
using LeoScript.ArtBlitz;
public class UIManager : SingletonMonoAwake<UIManager>
{
    [SerializeField] private GameObject artBlitzCanvas;
    [SerializeField] private ShareUIManager shareUiManager;

    [SerializeField] private UIHomepage m_UIHomePage;
    [SerializeField] private UIGameplayController m_UIGameplayController;
    [SerializeField] private UIPuzzleIntroController m_UIIntroController;
    [SerializeField] private UIPuzzleCompleted m_UIPuzzleCompleted;
    [SerializeField] private UIArtBlitsCompleted m_UIArtBlitsCompleted;
    [SerializeField] private UIBookContent m_UIBookContent;
    [SerializeField] private UIPartDetail m_UIPartDetail;
    [SerializeField] private FaderController m_FaderController;
    [SerializeField] private UIDownloadAndShare m_uiDownloadAndShare;
    [SerializeField] private UIChooseFirstBook m_uiChooseFirstBook;
    [SerializeField] private UIChooseOtherBook m_uiChooseOtherBook;
    [SerializeField] private UIPushNotiPermission m_uiPushNotiPermission;
    [SerializeField] private UIWelcomeGift m_uiWelcomGift;
    [SerializeField] private UILoading m_uiLoading;
    [SerializeField] private UIShop m_uiShop;
    [SerializeField] private UISetting m_uiSetting;
    [SerializeField] private UILuckyDraw m_uiLuckyDraw;
    [SerializeField] private UILuckyDrawArtBlitz m_uiLuckyDrawArtBlitz;
    [SerializeField] private PopupPlayAgain m_uiPopupPlayAgain;
    [SerializeField] private PopupPlayAgainCollection m_uiPopupPlayAgainCollection;
    [SerializeField] private PopupGetMoreInk m_uiPopupGetMoreInk;
    [SerializeField] private PopupPromotion m_uiPopupPromotion;
    [SerializeField] private UILogin m_uiLogin;
    [SerializeField] private UILoginSuccess m_uiLoginSuccess;
    [SerializeField] private UISubscription m_uiSubscription;
    [SerializeField] private UISubscriptionDaily m_uiSubscriptionDaily;
    [SerializeField] private UICredits m_uiCredits;
    [SerializeField] private UIPreRegistration m_uiPreRegistration;
    [SerializeField] private UIAdsNoti m_uiAdsNoti;

    [SerializeField] private UILiveEvent m_uiLiveEvent;

    [SerializeField] private PopupNotice m_popupNotice;
    [SerializeField] private PopupPurchaseFail m_popupPuchaseFail;
    [SerializeField] private PopupPurchaseStatus m_popupPuchaseStatus;
    [SerializeField] private PopupNoInternet m_popupNoInternet;
    [SerializeField] private BasePopup m_popupNoAds;
    [SerializeField] private PopupDailyReward m_popupDailyReward;
    [SerializeField] private PopupChapterDetailComplete m_popupChapterDetailComplete;
    [SerializeField] private PopupQuit m_popupQuit;
    [SerializeField] private PopupHint m_popupHint;
    [SerializeField] private PopupHintArtBlitz m_popupHintArtBlitz;
    [SerializeField] private PopupLiveEventTutorial m_popupLiveEventTutorial;
    [SerializeField] private PopupLiveEventGift m_popupLiveEventGift;

    [SerializeField] private PopupLoadPuzzleFail m_popupLoadPuzzleFail;

    [SerializeField] private BasePopup m_popupUnlockSuccessfully;
    [SerializeField] private BasePopup m_popupCompleteThePrevious;
    [SerializeField] private BasePopup m_popupUnclockChapter;

    [SerializeField] private GameObject lockTouch;
    [SerializeField] private Transform m_TrsLoading;
    [SerializeField] private RectTransform anchorZoomIn;
    [SerializeField] private SpriteRenderer border;

    public PopupDailyReward PopupDailyReward { get => m_popupDailyReward; set => m_popupDailyReward = value; }
    public PopupPromotion PopupPromotion { get => m_uiPopupPromotion; set => m_uiPopupPromotion = value; }
    public PopupGetMoreInk PopupGetMoreInk { get => m_uiPopupGetMoreInk; set => m_uiPopupGetMoreInk = value; }
    public UILoading UILoading { get => m_uiLoading; set => m_uiLoading = value; }
    public UIWelcomeGift UIWelcomeGift { get => m_uiWelcomGift; set => m_uiWelcomGift = value; }
    public UIPushNotiPermission UIPushNotiPermission { get => m_uiPushNotiPermission; set => m_uiPushNotiPermission = value; }
    public UIHomepage UIHomepage { get => m_UIHomePage; set => m_UIHomePage = value; }
    public UIBookContent UIBookContent { get => m_UIBookContent; set => m_UIBookContent = value; }
    public UIGameplayController UIGameplay { get => m_UIGameplayController; set => m_UIGameplayController = value; }
    public UIPuzzleCompleted UIPuzzleCompleted { get => m_UIPuzzleCompleted; set => m_UIPuzzleCompleted = value; }
    public UIArtBlitsCompleted UIArtBlitsCompleted { get => m_UIArtBlitsCompleted; set => m_UIArtBlitsCompleted = value; }
    public UIPuzzleIntroController UIIntro { get => m_UIIntroController; set => m_UIIntroController = value; }
    public UIDownloadAndShare UiDownloadAndShare { get => m_uiDownloadAndShare; set => m_uiDownloadAndShare = value; }
    public UIShop UIShop { get => m_uiShop; set => m_uiShop = value; }
    public UISetting UISetting { get => m_uiSetting; set => m_uiSetting = value; }
    public UILuckyDraw UiLuckyDraw { get => m_uiLuckyDraw; set => m_uiLuckyDraw = value; }
    public UILuckyDrawArtBlitz UILuckyDrawArtBlitz { get => m_uiLuckyDrawArtBlitz; set => m_uiLuckyDrawArtBlitz = value; }
    public FaderController Fader { get => m_FaderController; set => m_FaderController = value; }
    public Transform TrsLoading { get => m_TrsLoading; set => m_TrsLoading = value; }
    public PopupPlayAgain PopupPlayAgain { get => m_uiPopupPlayAgain; set => m_uiPopupPlayAgain = value; }
    public PopupPlayAgainCollection PopupPlayAgainCollection { get => m_uiPopupPlayAgainCollection; set => m_uiPopupPlayAgainCollection = value; }
    public UILogin UILogin { get => m_uiLogin; set => m_uiLogin = value; }
    public UILoginSuccess UILoginSuccess { get => m_uiLoginSuccess; set => m_uiLoginSuccess = value; }
    public UISubscription UISubscription { get => m_uiSubscription; set => m_uiSubscription = value; }
    public UISubscriptionDaily UISubscriptionDaily { get => m_uiSubscriptionDaily; set => m_uiSubscriptionDaily = value; }
    public UICredits UICredits { get => m_uiCredits; set => m_uiCredits = value; }
    public UIPreRegistration UIPreRegistration { get => m_uiPreRegistration; set => m_uiPreRegistration = value; }
    public PopupChapterDetailComplete PopupChapterDetailComplete { get => m_popupChapterDetailComplete; set => m_popupChapterDetailComplete = value; }
    public UIAdsNoti UIAdsNoti { get => m_uiAdsNoti; set => m_uiAdsNoti = value; }
    public UIChooseFirstBook UIChooseFirstBook { get => m_uiChooseFirstBook; set => m_uiChooseFirstBook = value; }
    public UIChooseOtherBook UIChooseOtherBook { get => m_uiChooseOtherBook; set => m_uiChooseOtherBook = value; }
    public PopupNoInternet PopupNoInternet { get => m_popupNoInternet; set => m_popupNoInternet = value; }
    public PopupHint PopupHint { get => m_popupHint; set => m_popupHint = value; }
    public PopupHintArtBlitz PopupHintArtBlitz { get => m_popupHintArtBlitz; set => m_popupHintArtBlitz = value; }
    public PopupQuit PopupQuit { get => m_popupQuit; set => m_popupQuit = value; }
    public PopupLoadPuzzleFail PopupLoadPuzzleFail { get => m_popupLoadPuzzleFail; set => m_popupLoadPuzzleFail = value; }
    public UILiveEvent UILiveEvent { get => m_uiLiveEvent; set => m_uiLiveEvent = value; }
    public PopupLiveEventTutorial PopupLiveEventTutorial { get => m_popupLiveEventTutorial; set => m_popupLiveEventTutorial = value; }
    public PopupLiveEventGift PopupLiveEventGift { get => m_popupLiveEventGift; set => m_popupLiveEventGift = value; }

    public void Init()
    {
        shareUiManager.Init();
        m_uiLoading.Init();
        m_UIBookContent.Init();
        m_UIGameplayController.Init();
        m_FaderController.Init();
        m_UIIntroController.Init();
        m_UIPuzzleCompleted.Init();
        m_UIArtBlitsCompleted.Init();
        m_uiChooseFirstBook.Init();
        m_uiChooseOtherBook.Init();
        m_uiShop.Init();
        m_uiSetting.Init();
        m_uiPopupPlayAgain.Init();
        m_uiPopupPlayAgainCollection.Init();
        m_uiPopupGetMoreInk.Init();
        m_uiLogin.Init();
        m_uiLoginSuccess.Init();
        m_uiSubscription.Init();
        m_uiSubscriptionDaily.Init();
        m_uiCredits.Init();
        m_uiPreRegistration.Init();
        m_uiWelcomGift.Init();
        m_uiLuckyDraw.Init();
        m_uiLuckyDrawArtBlitz.Init();
        m_uiPopupPromotion.Init();
        m_popupChapterDetailComplete.Init();
        m_uiAdsNoti.Init();
        m_uiLiveEvent.Init();

        //m_UIBookDetail.Close();
        m_UIGameplayController.Close();
        //m_UIIntroController.Close();
        //m_UIPuzzleCompleted.Close();
        //m_uiShop.Close();
        //m_uiSetting.Close();
        //m_uiLuckyDraw.Close();
        //m_uiPreRegistration.Close();

        UiDownloadAndShare.Init();
        m_popupNotice.Init();
        m_popupHint.Init();
        m_popupPuchaseStatus.Init();
        m_popupNoInternet.Init();
        m_popupQuit.Init();

        if (Tutorial.IsCompleted)
        {
            m_UIHomePage.Init();
        }
        m_popupNoAds.Init();
        m_popupPuchaseFail.Init();
        m_popupUnlockSuccessfully.Init();
        m_popupCompleteThePrevious.Init();
        m_popupUnclockChapter.Init();

        ActiveLockTouch(false);
        this.RegisterListener(EventID.PlayPuzzle, (o) => PlayPuzzle());
    }

    public void ShowHomePage()
    {
        if (!m_UIHomePage.DidInit)
        {
            m_UIHomePage.Init();
        }
        m_UIHomePage.Open();
        UIManager.Instance.ShowUISubscription(true);
    }

    public bool CanShowPopupDailyReward() {
        return PopupDailyReward.CanShow();
    }

    public void ShowDailyReward()
    {
        if (PopupDailyReward.CanShow())
        {
            PopupDailyReward.Open();
        }
    }
    public void ShowSetting()
    {
        m_uiSetting.Open();
    }

    public void ShowShop(BasePanel previousPanel = null)
    {        
        if (m_uiShop != null)
        {
            m_uiShop.SetData(new object[] { previousPanel });
            m_uiShop.Open();
        }
    }

    public void ShowGetMoreInk(BasePanel previousPanel = null)
    {
        if (m_uiPopupGetMoreInk != null)
        {
            m_uiPopupGetMoreInk.SetData(new object[] { previousPanel });
            m_uiPopupGetMoreInk.Open();
        }
    }
    public void ShowPurchaseFail(int type, Action onComplete)
    {
        if (m_popupPuchaseFail != null)
        {
            m_popupPuchaseFail.SetData(new object[] { type, onComplete });
            m_popupPuchaseFail.Open();
        }
    }


    public void ShowPopupLiveEvent(PopupLiveEventTutorial.TypeLiveTutorial typeLiveTutorial, Action onComplete = null)
    {
        if (PopupLiveEventTutorial != null)
        {
            PopupLiveEventTutorial.SetData(new object[] { typeLiveTutorial, onComplete });
            PopupLiveEventTutorial.Open();
        }
    }

    public void ShowPopupLiveEventGift(Action onComplete = null, int curDot = 0)
    {
        if (m_popupLiveEventGift != null)
        {
            m_popupLiveEventGift.SetData(new object[] { onComplete, curDot });
            m_popupLiveEventGift.Open();
        }
    }

    public void ShowUILiveEvent()
    {
        if (m_uiLiveEvent != null && LiveEventTimer.Instance.IsEventActive)
        {
            m_uiLiveEvent.Open();
        }
    }

    public void ShowPurchaseStatus(bool isSuccess, Action onComplete)
    {
        if (m_popupPuchaseStatus != null)
        {
            m_popupPuchaseStatus.SetData(new object[] { isSuccess, onComplete });
            m_popupPuchaseStatus.Open();
        }

    }

    public void ShowPopupNotice(string msg)
    {
        if (m_popupNotice != null)
        {
            m_popupNotice.SetData(new object[] { msg });
            m_popupNotice.Open();
        }
    }

    public void ShowPopupNoInternet(Action onComplete = null)
    {
        if (m_popupNoInternet != null)
        {
            m_popupNoInternet.SetData(new object[] { onComplete });
            m_popupNoInternet.Open();
        }
    }


    public void ShowPopupHint(Action onComplete = null)
    {
        if (m_popupHint != null)
        {
            m_popupHint.SetData(new object[] { onComplete });
            m_popupHint.Open();
        }
    }

    public void ShowPopupHintArtBlitz(Action onComplete = null)
    {
        if (m_popupHintArtBlitz != null)
        {
            m_popupHintArtBlitz.SetData(new object[] { onComplete });
            m_popupHintArtBlitz.Open();
        }
    }

    public void ShowPopupLoadPuzzleFail(Action onComplete = null , string bookName ="" , string chapterName = "", string puzzlename ="")
    {
        if (m_popupLoadPuzzleFail != null)
        {
            m_popupLoadPuzzleFail.SetData(new object[] { onComplete, bookName, chapterName, puzzlename });
            m_popupLoadPuzzleFail.Open();
        }
    }

    public void ShowPopupNoAds()
    {
        m_popupNoAds.Open();
    }

    public void ShowPopupUnlockSuccessfully()
    {
        m_popupUnlockSuccessfully.Open();
    }

    public void ShowPopupCompleteThePrevious()
    {
        m_popupCompleteThePrevious.Open();
    }

    public void ShowPopupUnclockChapter()
    {
        m_popupUnclockChapter.Open();
    }
    public void ActiveLockTouch(bool isActive)
    {
        lockTouch.SetActive(isActive);
    }

    public void ShowChapterIntro(PuzzleLevelData data, bool isShowIntroInGameUI)
    {
        if (data == null) return;
        if (m_UIHomePage != null)
            m_UIHomePage.Close();
        if (m_UIBookContent != null)
            m_UIBookContent.Close();
        if (m_UIPartDetail != null)
            m_UIPartDetail.Close();

        if (m_UIPuzzleCompleted != null && data != null)
        {
            m_UIPuzzleCompleted.SetData(new object[] { data.Thumbnail, data.Desc, data.Ink });
        }

        if (m_uiDownloadAndShare != null)
            m_uiDownloadAndShare.SetData(new object[] { data.Name });
        if (isShowIntroInGameUI && UIIntro != null)
        {
            UIIntro.SetData(new object[] { data });
            UIIntro.Open();
        }

        ShowUILoading(GameManager.Instance.MinLoadingIntro, () =>
        {
            this.PostEvent(EventID.PlayPuzzle);
            GameManager.Instance.OnResetTimeShowTutorial();
        });
    }

    private IEnumerator RunAfter(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }

    public void ShowUIBookDetail(int bookId, string placement, BasePanel previousPanel = null)
    {
        if (previousPanel != null)
        {
            previousPanel.Close();
        }

        var book = MasterDataStore.Instance.GetBookByID(bookId);
        if (book != null)
        {
            AnalyticManager.Instance.TrackOpenBook(book.BookName, placement);
        }

        m_UIBookContent.SetData(new object[] { book, bookId, previousPanel });
        m_UIBookContent.Open();
    }

    public void ShowUIPartDetail(string partID, int bookID, BasePanel previousPanel = null)
    {

        ChapterMasterData chapterMasterData = MasterDataStore.Instance.GetPartById(bookID, partID);
        var book = MasterDataStore.Instance.GetBookByID(bookID);
        if (chapterMasterData != null && book != null)
        {
            AnalyticManager.Instance.TrackOpenChapter(chapterMasterData.PartName, book.BookName);
        }
        m_UIPartDetail.SetData(new object[] { chapterMasterData, bookID, previousPanel });
        m_UIPartDetail.Open();
    }

    public void ShowUIPartDetail(ChapterMasterData masterData, int bookID, BasePanel previousPanel = null, string placement = "null")
    {
        var book = MasterDataStore.Instance.GetBookByID(bookID);
        if (masterData != null && book != null)
        {
            AnalyticManager.Instance.TrackOpenChapter(masterData.PartName, book.BookName);
        }
        m_UIPartDetail.SetData(new object[] { masterData, bookID, previousPanel, placement });
        m_UIPartDetail.Open();
    }

    public void CloseUIBookDetail()
    {
        m_UIBookContent.Close();
    }

    public void CloseUIHome()
    {
        m_UIHomePage.Close();
    }

    public void ShowUIPushNotiPermission(Action onComplete = null)
    {
        SoundController.Instance.PlayMainBackgroundMusic();
        m_uiPushNotiPermission.SetData(new object[] { onComplete });
        m_uiPushNotiPermission.Open();
    }

    public void ShowUIWelcomeGift(Action onComplete = null)
    {
        m_uiWelcomGift.SetData(new object[] { onComplete });
        m_uiWelcomGift.Open();
    }

    public void ShowPuzzleCompleted()
    {
        m_UIGameplayController.Close();
        //m_uiDownloadAndShare.Open(); // Waiting to camera reset to default
        StartCoroutine(IEWaitCameraUpdate(() =>
        {
            m_uiDownloadAndShare.Open();
        }));
    }

    public IEnumerator IEWaitCameraUpdate(Action onComplete)
    {
        yield return new WaitUntil(() => !ZoomInZoomOut.Instance.IsUpdateCamera);
        onComplete?.Invoke();

    }

    public void ShowPuzzleCompletedFinalPage()
    {
        m_uiDownloadAndShare.Close();
        if (GameManager.Instance.GetStepGame() < StepGameConstants.PlayPuzzleOne)
        {
            float posY = Camera.main.ScreenToWorldPoint(anchorZoomIn.transform.position).y;
            GameManager.Instance.PlayAnimMoveAndScaleCompletePuzzle(posY, 0.65f, () =>
            {
                m_UIPuzzleCompleted.Open();
            });
        }
        else
        {
            ShareUIManager.Instance.OnEnableCoin(true);
            float posY = Camera.main.ScreenToWorldPoint(anchorZoomIn.transform.position).y;
            GameManager.Instance.PlayAnimMoveAndScaleCompletePuzzle(posY, 0.65f, () =>
            {
                ShareUIManager.Instance.OnEnableCoin(false);
                m_UIPuzzleCompleted.Open();
            });
        }
    }

    public void ShowArtBlitsCompleted(PuzzleLevelData data, Action onComplete)
    {
        if (m_UIArtBlitsCompleted != null && data != null)
        {
            m_UIArtBlitsCompleted.SetData(new object[] { data, onComplete });
            m_UIArtBlitsCompleted.Open();
        }
    }

    public void ShowUIArtBlitzGame(bool isActive)
    {
        if (artBlitzCanvas != null)
            artBlitzCanvas.gameObject.SetActive(isActive);
    }
	
    public void ShowUILogin()
    {
        m_uiLogin.Open();
    }

    public void HideUILogin()
    {
        m_uiLogin.Close();
    }

    public void ShowUILoginSuccess(string userId)
    {
        if (m_uiLoginSuccess != null)
        {
            m_uiLoginSuccess.SetData(new object[] { userId });
            m_uiLoginSuccess.Open();
        }

    }
    public void ShowUISubscription(bool isCanshow = false)
    {
        if (!GameManager.Instance.IsCompleteStepGame())
            return;

        if (isCanshow)
        {
            if (PlayerPrefs.GetInt(ConfigManager.KeyTotalPlayPuzzleFinish, 0) < StepGameConstants.HomePage)
                return;

            if (UISubscription.CanShow() && !PopupDailyReward.gameObject.activeInHierarchy)
            {
                m_uiSubscription.OpenAuto();
            }
        }
        else
            m_uiSubscription.Open();
    }

    public void ShowUISubscriptionDaily()
    {
        m_uiSubscriptionDaily.Open();
    }

    public void ShowUICredit()
    {
        m_uiCredits.Open();
    }

    public void ShowUiLuckyDraw()
    {
        m_uiLuckyDraw.Open();
    }

    public void ShowUiLuckyArtBlitzDraw()
    {
        m_uiLuckyDrawArtBlitz.Open();
    }

    public void ShowUiPreRegistration()
    {
        m_uiPreRegistration.Open();
    }

    public void ShowUiAdsNoti(Action onComplete = null)
    {
        m_uiAdsNoti.SetData(new object[] { onComplete });
        m_uiAdsNoti.Open();
        AnalyticManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.shown_ad_notice);
    }

    public void ShowUiQuit(Action onComplete = null, PopupQuit.TypeExit typeExit = PopupQuit.TypeExit.HomePage)
    {
        m_popupQuit.SetData(new object[] { onComplete, typeExit });
        m_popupQuit.Open();
    }

    public void ShowUiPopupChapterDetailComplete(PopupChapterDetailComplete.TypeChapterComplete mode, Action<bool> OnComplete = null, string price = "0")
    {
        m_popupChapterDetailComplete.SetData(new object[] { mode, OnComplete, price });
        m_popupChapterDetailComplete.Open();
    }
    public void LogOut()
    {
        try
        {
            var loginType = PlayerPrefs.GetString(ConfigManager.LoginType, String.Empty);
            GameData.Instance.LogOut();
            switch (loginType)
            {
                case "Facebook":
                    SocialManager.Instance.LogoutFB();
                    break;
                case "Google":
                    GoogleSignInManager.Instance.SignOutFromGoogle();
                    break;

                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            DataCore.Debug.Log($"Failed LogOut. Error: {ex.Message}");
        }

    }
    private void PlayPuzzle()
    {
        m_UIGameplayController.Open();
        //GameManager.Instance.ShowPuzzle();
    }

    public void EnablePuzzleBorder(bool value)
    {
        border.enabled = value;
    }

    public static string ConvertReleaseDay(long ticks, string type)
    {

        string data = "Last " + type + " {0} {1} ago."; ;

        DateTime releaseTime = new DateTime(ticks);
        TimeSpan subTime = DateTime.Now.Subtract(releaseTime);

        string timeType = "";
        int time = 0;

        if (subTime.TotalSeconds < 60)
        {
            timeType = "seconds";
            time = (int)subTime.TotalSeconds;
        }
        else if (subTime.TotalMinutes < 60)
        {
            timeType = "minutes";
            time = (int)subTime.TotalMinutes;
        }
        else if (subTime.TotalHours < 24)
        {
            timeType = "hours";
            time = (int)subTime.TotalHours;
        }
        else if (subTime.TotalDays < 7)
        {
            timeType = "days";
            time = (int)subTime.TotalDays;
        }
        else if (subTime.TotalDays < 30)
        {
            timeType = "wk.";
            time = (int)(subTime.TotalDays / 7);
        }
        else if (subTime.TotalDays < 365)
        {
            timeType = "months";
            time = (int)(subTime.TotalDays / 30);
        }
        else
        {
            timeType = "years";
            time = (int)(subTime.TotalDays / 365);
        }

        if (time < 0)
            time = 10;

        data = string.Format(data, time, timeType);

        return data;
    }

    [ContextMenu("Show Choose First Book")]
    public void ShowUIChooseFirstBook(Action onComplete = null)
    {
        m_uiChooseFirstBook.SetData(new object[] { onComplete });
        m_uiChooseFirstBook.Open();
    }

    public void ShowUIChooseOtherBook(Action onComplete = null)
    {
        m_uiChooseOtherBook.SetData(new object[] { onComplete });
        m_uiChooseOtherBook.Open();
    }

    public void ShowUILoading(float minWaitingTime = 2.0f, Action onComplete = null)
    {
        m_uiLoading.SetData(new object[] { minWaitingTime, onComplete });
        m_uiLoading.Open();
    }

    public void UpdateProgressLoading(float value)
    {
        m_uiLoading.UpdateProgress(value);
    }
    public void CloseUILoading()
    {
        m_uiLoading.Close();
    }

}
