using System;
using System.Collections;
using System.Collections.Generic;
using com.F4A.MobileThird;
using DanielLochner.Assets.SimpleScrollSnap;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = DataCore.Debug;

public class UILiveEvent : BasePanel
{
    private const string REFRESH_TIME_IN = "Event ends in: {0}D {1}H {2}M";

    [Header("Main Config")]
    [SerializeField] private NumberCounter txtNumberCounter;

    [SerializeField] private GameObject playBtn;
    [SerializeField] private TextMeshProUGUI txtPlayBtn;
    [SerializeField] private GameObject claimBtn;
    [SerializeField] private GameObject progression;
    [SerializeField] private Text txtProgression;
    [SerializeField] private GameObject progressionIpad;
    [SerializeField] private Text txtProgressionIpad;
    [SerializeField] private TextMeshProUGUI txtTimeCount;

    [SerializeField] private Image inkTopCurrency;
    [SerializeField] private GameObject prefab;
    [SerializeField] private SimpleScrollSnap simpleScrollSnap;

    [Header("Page Dot")]
    [SerializeField] private RectTransform pagePrefab;
    [SerializeField] private Transform pageContainer;
    [SerializeField] private Vector2 sizeDots;
    [SerializeField] private Color normalDot;
    [SerializeField] private Color selectDot;


    private List<PostcardItem> listItems = new List<PostcardItem>();
    private List<RectTransform> _pageDots;
    private int _curDot;
    private string porgressionFormat = "{0} / {1}";

    [SerializeField] private SkeletonGraphic lightAnimation;

    public override void Init()
    {
        base.Init();

    }

    public override void SetData(object[] data)
    {
        base.SetData(data);
    }

    public override void Open()
    {
        if (_isOpen)
        {
            return;
        }
        base.Open();
        
        AdsService.Instance.SetAutoShowBanner(false);
        AdsService.Instance.HideBanner();

        ClearData();
        UpdateCurrencyData(0);
        UpdateUI();
        StartCoroutine(RunAfterOneFrame(() =>
        {
            GoToActivePanel();
        }));
        PanelChange();

        // Show tutorial Live event
        if (PlayerPrefs.GetInt(ConfigManager.KeyShowLiveEventTutorial, 0) == 0 && GameData.Instance.SavedPack.LiveEventSavedData.ShowTutorial == 0)
        {
            UIManager.Instance.ShowPopupLiveEvent(PopupLiveEventTutorial.TypeLiveTutorial.LiveEventClick);
            PlayerPrefs.SetInt(ConfigManager.KeyShowLiveEventTutorial, 1);
            PlayerPrefs.Save();
            GameData.Instance.SavedPack.LiveEventSavedData.ShowTutorial = 1;
            GameData.Instance.RequestSaveGame();
        }

        //Check if device ios xsmax
#if UNITY_IOS || UNITY_ANDROID
        progression.SetActive(true);
        progressionIpad.SetActive(false);
        if (!IsSpecialResolution())
        {
            progression.SetActive(false);
            progressionIpad.SetActive(true);
        }
#endif
    }

    public bool IsSpecialResolution()
    {
        return ((float)Screen.height / (float)Screen.width) >= 1.8f;
    }

    private IEnumerator RunAfterOneFrame(Action action)
    {
        yield return new WaitForEndOfFrame();
        action?.Invoke();
    }

    private void Update()
    {
        if (!_isOpen ||!LiveEventTimer.Instance.IsEventActive)
        {
            return;
        }

        TimeSpan timeRemain = LiveEventTimer.Instance.TimeEndEvent.Subtract(DateTime.Now);

        if (timeRemain.TotalSeconds > 0)
        {
            UpdateTimeRefresh(timeRemain.Days,  timeRemain.Hours, (int)timeRemain.Minutes, (int)timeRemain.Seconds);
        }
        else
        {
            UpdateTimeRefresh(0, 0, 0, 0);
        }
    }

    public override void Close()
    {
        base.Close();

     
    }

    public void CheatOpen24Item()
    {
        listItems[_curDot].CheatUnlock24Item();
        ClearData();
        UpdateCurrencyData(0);
        UpdateUI();
        GoToActivePanel();
    }

    public void CloseManually()
    {
        Close();
        UIManager.Instance.ShowHomePage();
    }

    private void UpdateProgression(int current, int total)
    {
        if (progression.gameObject.activeInHierarchy) txtProgression.text = string.Format(porgressionFormat, current, total);
        if (progressionIpad.gameObject.activeInHierarchy) txtProgressionIpad.text = string.Format(porgressionFormat, current, total);
    }

    public void UpdateUI()
    {
        int amount = LiveEventTimer.Instance.LiveEventData.PostCardDatas.Length;

       
        if (GameData.Instance.SavedPack.LiveEventSavedData == null || GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas == null
                       || GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas.Count == 0)
        {
        
            GameData.Instance.SavedPack.LiveEventSavedData = new LiveEventSaveData(amount);
            GameData.Instance.RequestSaveGame();
        }

        for (int i = 0; i < amount; i++)
        {

            var postCardItem = Instantiate(prefab, simpleScrollSnap.Content).GetComponent<PostcardItem>();
            simpleScrollSnap.AddToBack(postCardItem.gameObject);
            postCardItem.InitData(i, LiveEventTimer.Instance.LiveEventData.PostCardDatas[i]);
            listItems.Add(postCardItem);
          
        }
        
        CreatePageDots(amount);
        UpdateProgression(listItems[_curDot].TotalPuzzleComplete, listItems[_curDot].TotalPuzzle);
      
    }

    private void ClearData()
    {
        for (int i = listItems.Count-1; i >= 0; i--)
        {
            simpleScrollSnap.Remove(i);
        }
        listItems.Clear();

        if (_pageDots != null)
        {
            for (int i = 0; i < _pageDots.Count; i++)
            {
                Destroy(_pageDots[i].gameObject);
            }
            _pageDots.Clear();
        }

    }

    public override void OnUpdateInk(float animDuration, Action onComplete = null)
    {
        UpdateCurrencyData(animDuration, onComplete);
    }
    public override void OnPlayAnimInkIcon()
    {
        inkTopCurrency.transform.DOPunchScale(Vector3.one, 0.25f, 1, 0.1f);
    }
    public void UpdateCurrencyData(float animDuration, Action onComplete = null)
    {
        int newData = GameData.Instance.SavedPack.SaveData.Coin;
        txtNumberCounter.PlayAnim(newData, animDuration, onComplete);

    }

    public void CreatePageDots(int dots)
    {
        if (_pageDots == null)
        {
            _pageDots = new List<RectTransform>(dots);
        }

        for (int i = 0; i < dots; i++)
        {
            var newDot = Instantiate(pagePrefab, Vector3.zero, Quaternion.identity, pageContainer);
            _pageDots.Add(newDot);
            
        }

        _curDot = 0;
        UpdatePages();
    }

    private void UpdatePages()
    {
        for (int i = 0; i < _pageDots.Count; i++)
        {
            _pageDots[i].GetComponent<Image>().color = i == _curDot ? selectDot : normalDot;
            _pageDots[i].sizeDelta = new Vector2(i == _curDot ? sizeDots.y : sizeDots.x, _pageDots[i].sizeDelta.y);
        }

    }
    public void CompletePostCard(int index)
    {
        int curDotActive = index;
        listItems[curDotActive].AnimScaleBackGround(() =>
        {
            ActiveAnimationHint(true);
            DOVirtual.DelayedCall(2f, () =>
            {
                ActiveAnimationHint(false);
                UIManager.Instance.ShowPopupLiveEventGift(() =>
                {
                    GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas[curDotActive].ClaimReward = 1;

                    GameData.Instance.IncreaseInks(ConfigManager.KeyRewardLiveEventInk, ConfigManager.GameData.ResourceEarnSource.live_event);
                    GameData.Instance.IncreaseHint(ConfigManager.KeyRewardLiveEventHint, ConfigManager.GameData.ResourceEarnSource.live_event);
                    ShareUIManager.Instance.UpdateCurrencyData(0.0f);
                    if (curDotActive < listItems.Count - 1)
                    {
                        simpleScrollSnap.GoToPanel(curDotActive + 1);
                    }
                    GameData.Instance.RequestSaveGame();
                },curDotActive);
            });

        });
    }
    public void PanelChange()
    {
        _curDot = simpleScrollSnap.CurrentPanel;
        switch (listItems[_curDot].Status)
        {
            case LiveEventPostCardState.Claimed:
                claimBtn.SetActive(false);
                playBtn.SetActive(false);
                break;
            case LiveEventPostCardState.Completed:
                claimBtn.SetActive(false);
                playBtn.SetActive(false);
                break;
            case LiveEventPostCardState.Lock:
                claimBtn.SetActive(false);
                playBtn.SetActive(true);
                txtPlayBtn.SetText("Active Postcard");
                break;
            case LiveEventPostCardState.Unlock:
                claimBtn.SetActive(false);
                playBtn.SetActive(true);
                txtPlayBtn.SetText("Play Now");
                break;
        }

        UpdateProgression(listItems[_curDot].TotalPuzzleComplete, listItems[_curDot].TotalPuzzle);
        UpdatePages();
    }

    public void OnClaimBtnTap()
    {
        listItems[_curDot].UpdateClaimed();
        UIManager.Instance.ShowPopupLiveEventGift(()=> {
            UpdateCurrencyData(0.25f);
        }, _curDot);

        GameData.Instance.IncreaseInks(listItems[_curDot].InkReward, ConfigManager.GameData.ResourceEarnSource.puzzle_reward);
        simpleScrollSnap.GoToPanel(_curDot);
        claimBtn.SetActive(false);
    }

    public void OnPlayBtnTap()
    {
        if (listItems[_curDot].Status == LiveEventPostCardState.Lock)
        {
            GoToActivePanel();
        }
        else
        {
            listItems[_curDot].PlayNow();
            Close();
        }
    }

    private void UpdateTimeRefresh(int day, int hour, int min, int seconds)
    {
        if (hour == 0 && min == 0 && seconds > 0)
        {
            min = 1;
        }

        if (seconds < 0)
        {
            seconds = 0;
        }

        string time = string.Format(REFRESH_TIME_IN, day.ToString(), (hour).ToString("00"), min.ToString("00"));
        txtTimeCount.SetText(time);
    }

    private void GoToActivePanel()
    {
        int activePostcardId = 0;
        bool isGoToPanel = false;
        for (int i = 0; i < listItems.Count; i++)
        {
            if (listItems[i].Status == LiveEventPostCardState.Unlock || listItems[i].Status == LiveEventPostCardState.Completed)
            {
                activePostcardId = i;
                if (listItems[activePostcardId].Status == LiveEventPostCardState.Completed && GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas[activePostcardId].ClaimReward == 0)
                {
                    if (activePostcardId < listItems.Count)
                    {
                        simpleScrollSnap.GoToPanel(activePostcardId);
                    }
                    isGoToPanel = !isGoToPanel;
                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        CompletePostCard(activePostcardId);
                    });
                    break;
                }
            }
        }

        if(!isGoToPanel)
        {
            if (listItems[activePostcardId].Status != LiveEventPostCardState.Completed &&
                      GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas[activePostcardId].ClaimReward == 0)
                simpleScrollSnap.GoToPanel(activePostcardId);
        }
    }

    public void ActiveAnimationHint(bool isActive)
    {
        lightAnimation.gameObject.SetActive(isActive);
        if (lightAnimation.skeletonDataAsset != null)
        {
            lightAnimation.Clear();
            lightAnimation.Initialize(true);
            lightAnimation.AnimationState.SetAnimation(0, "animation", false);

        }

    }
}
