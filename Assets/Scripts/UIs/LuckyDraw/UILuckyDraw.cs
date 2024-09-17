using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DataCore;
using com.F4A.MobileThird;
public class UILuckyDraw : BasePanel
{
    private const int TIME_REFRESH = 0;
    private const int FREE_SPIN_PER_DAY = 5;
    private const int AMOUNT_PUZZLE_TO_SHOW = 3;
    private const float DELAY_SHOW_NO_THANKS = 2.0f;

    [SerializeField] private SpinController spinController;
    [SerializeField] private Button spinBtn;
    [SerializeField] private Transform noThanksBtn;
    [SerializeField] private TextMeshProUGUI remainTxt;

    [SerializeField] private Button playnowBtn;

    private Tween _scaleTween;
    private Action onPopupClose;

    public override void Init()
    {
        base.Init();
        spinController.Init();
        spinController.RegisterCompleteEvents(OnSpinCompleted);

        if (IsNextDay())
        {
            RefreshData();
        }
    }

    public bool CanShow()
    {
        if (!AdsService.Instance.IsRewardedAdLoaded())
        {
            AdsService.Instance.LoadRewardedAd();
            return false;
        }

        if (GameManager.Instance.GetStepGame() < StepGameConstants.StepComplete)
            return true;

        if (IsNextDay())
        {
            RefreshData();
        }
        if (GameData.Instance.SavedPack.SaveData.DailyLuckyDrawRemain > 0)
        {
            return true;
        }

        return false;
    }

    public override void SetData(object[] data)
    {
        base.SetData(data);
        onPopupClose = (Action)data[0];
    }

    public void OnWatchAdsTap()
    {
        if (GameData.Instance.IsVipIap())
        {
            OnCompleteAds();
            return;
        }
        AdsService.Instance.ShowRewardedAd(AdsService.RwAdUnitIdLucky,
        onRewardedAdClosedEvent: (complete) =>
        {
            SoundController.Instance.MuteBgMusic(false);
            if (!complete)
            {
                UIManager.Instance.ShowPopupNoAds();
            }
        }, onRewardedAdReceivedReward: (adId, placement) =>
        {
            OnCompleteAds();
        });
    }

    private void OnCompleteAds()
    {
        spinBtn.interactable = false;

        GameData.Instance.SavedPack.SaveData.DailyLuckyDrawRemain--;
        GameData.Instance.SavedPack.SaveData.DailyLuckyDrawShowFree--;
        remainTxt.SetText("REMAINING TODAY: " + GameData.Instance.SavedPack.SaveData.DailyLuckyDrawRemain);
        GameData.Instance.RequestSaveGame();

        GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.SPIN_LUCKY_DRAW, 1);
        spinController.Spin();
    }

    public void OnClickPlayNow()
    {
        if (GameManager.Instance.GetStepGame() == StepGameConstants.StepComplete)
            GameData.Instance.SavedPack.SaveData.DailyLuckyDrawShowFree--;
        if (SoundController.Instance != null) SoundController.Instance.PlaySfxClick();

        GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.SPIN_LUCKY_DRAW, 1);
        spinController.Spin();
    }
    public override void Open()
    {
        if (_isOpen)
        {
            return;
        }
        base.Open();
        spinController.Reset();
        UIReset();

        ShowNoThanks();
        remainTxt.SetText("REMAINING TODAY: " + GameData.Instance.SavedPack.SaveData.DailyLuckyDrawRemain);


        spinBtn.interactable = true;
        GameData.Instance.SavedPack.SaveData.CountCompletedPuzzleLuckyDraw = 0;
        GameData.Instance.RequestSaveGame();

        playnowBtn.gameObject.SetActive(false);
        spinBtn.gameObject.SetActive(true);
        noThanksBtn.gameObject.SetActive(true);
        var stepGame = GameManager.Instance.GetStepGame();
   
    
        if (stepGame < StepGameConstants.StepComplete)
        {
            if (stepGame == StepGameConstants.PlayPuzzleThree)
                GameManager.Instance.SetStepGame(StepGameConstants.LuckyDrawOne);
            else if (stepGame == StepGameConstants.PlayPuzzleFour)
                GameManager.Instance.SetStepGame(StepGameConstants.LuckyDrawTwo);
            else if (stepGame == StepGameConstants.PlayPuzzleFive)
                GameManager.Instance.SetStepGame(StepGameConstants.LuckyDrawThree);

            if (stepGame == StepGameConstants.LuckyDrawOne)
            {
                ActiveFree();
            }else
            {
                if (GameData.Instance.IsVipIap())
                {
                    playnowBtn.gameObject.SetActive(true);
                    spinBtn.gameObject.SetActive(false);
                    noThanksBtn.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            if (GameData.Instance.SavedPack.SaveData.DailyLuckyDrawShowFree == 4)
                ActiveFree();
            else
            {
                if (GameData.Instance.IsVipIap())
                {
                    playnowBtn.gameObject.SetActive(true);
                    spinBtn.gameObject.SetActive(false);
                    noThanksBtn.gameObject.SetActive(true);
                }
            }
        }
    }

    private void ActiveFree()
    {
            playnowBtn.gameObject.SetActive(true);
            spinBtn.gameObject.SetActive(false);
            noThanksBtn.gameObject.SetActive(false);
    }

    public override void Close()
    {
        if (spinController.IsSpining || !_isOpen)
        {
            return;
        }
        base.Close();
        onPopupClose?.Invoke();
        

    }

    protected override void UIReset()
    {
        base.UIReset();

        if (_scaleTween != null)
        {
            _scaleTween.Kill();
            _scaleTween = null;
        }

        spinController.Reset();
        noThanksBtn.transform.localScale = Vector3.zero;
    }

    private void OnSpinCompleted()
    {
        Close();
    }

    private void ShowNoThanks()
    {
        _scaleTween = DOVirtual.DelayedCall(DELAY_SHOW_NO_THANKS,
             () =>
             {
                 _scaleTween = noThanksBtn.transform.DOScale(1f, 0.25f).SetEase(Ease.OutCubic);
             }).Play();

    }

    public void OnClickNoThank()
    {
        //GameData.Instance.SavedPack.SaveData.DailyLuckyDrawRemain--;
        remainTxt.SetText("REMAINING TODAY: " + GameData.Instance.SavedPack.SaveData.DailyLuckyDrawRemain);
        GameData.Instance.RequestSaveGame();
        Close();
    }

    private void RefreshData()
    {
        GameData.Instance.SavedPack.SaveData.CountCompletedPuzzleLuckyDraw = 0;
        GameData.Instance.SavedPack.SaveData.DailyLuckyDrawRemain = FREE_SPIN_PER_DAY;
        GameData.Instance.SavedPack.SaveData.DailyLuckyDrawShowFree = FREE_SPIN_PER_DAY;

        GameData.Instance.RequestSaveGame();
    }

    private bool IsNextDay()
    {
        DateTime now = DateTime.UtcNow;
        int lastDay = GameData.Instance.SavedPack.SaveData.LastDayLuckyDraw;

        if (lastDay != now.Day)
        {
            if (Mathf.Abs(lastDay - now.Day) > 1)
            {
                if (now.Hour >= TIME_REFRESH)
                {
                    lastDay = now.Day;
                    GameData.Instance.SavedPack.SaveData.LastDayLuckyDraw = lastDay;
                }
                else
                {
                    lastDay = now.Day - 1;
                    GameData.Instance.SavedPack.SaveData.LastDayLuckyDraw = lastDay;
                }

                return true;
            }
            else
            {
                if (now.Hour >= TIME_REFRESH)
                {
                    lastDay = now.Day;
                    GameData.Instance.SavedPack.SaveData.LastDayLuckyDraw = lastDay;
                    return true;
                }
            }
        }
        return false;
    }
}
