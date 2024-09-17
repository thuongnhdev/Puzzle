using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using com.F4A.MobileThird;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupHint : BasePanel
{
    [SerializeField] private Image imageTime;
    private Tween _updateTween;

    [SerializeField] private SkeletonGraphic hintAnimation;

    [SerializeField] private GameObject inkIcon;
    [SerializeField] private GameObject inkIconMoving;
    [SerializeField] private GameObject inkIconMovingTarget;
    [SerializeField] private GameObject inkPanelNormal;

    public override void Init()
    {
    }

    private Action _onComplete = null;
    public override void SetData(object[] data)
    {
        base.SetData(data);
        if (data.Length >= 1)
        {
            _onComplete = (Action)data[0];
        }
    }


    public override void Open()
    {
        inkPanelNormal.SetActive(true);
        GameManager.Instance.AddObjList(this);
        base.Open();
        imageTime.fillAmount = 1;
        UpdateProgress(0);
        ActiveAnimationHint(true);
    }

    public void ActiveAnimationHint(bool isActive)
    {
        hintAnimation.gameObject.SetActive(isActive);
        if (hintAnimation.skeletonDataAsset != null)
        {
            hintAnimation.Clear();
            hintAnimation.Initialize(true);
            hintAnimation.AnimationState.SetAnimation(0, "Active", true);

        }
    }

    public void UpdateProgress(int endValue)
    {
        if (_updateTween != null)
        {
            _updateTween.Kill();
            _updateTween = null;
        }
        
        _updateTween = imageTime.DOFillAmount(endValue, 10).Play().OnComplete(()=> {
            Reset();
        });
    }

    private void Reset()
    {
        if (_updateTween != null)
        {
            _updateTween.Kill();
            _updateTween = null;
        }

        imageTime.fillAmount = 0;
        Close();
    }

    public void BtnContinuePress()
    {
        SoundController.Instance.PlaySfxClick();
        OnWatchAdsTap();
    }

    public override void Close()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        _onComplete?.Invoke();
        _onComplete = null;

        GameManager.Instance.RemoveObjList(this);
    }

    public void OnWatchAdsTap()
    {
        AdsService.Instance.ShowRewardedAd(AdsService.RwAdUnitIdFloatingHint,
            onRewardedAdClosedEvent: (complete) =>
            {
                SoundController.Instance.MuteBgMusic(false);
                if (!complete)
                {
                    UIManager.Instance.ShowPopupNoAds();
                }
            }, onRewardedAdReceivedReward: (adId, placement) =>
            {
                GameData.Instance.IncreaseHint(ConfigManager.NumberIncreasePopupHint, ConfigManager.GameData.ResourceEarnSource.puzzle_hint);
                UpdateHintUI();
            });
    }

    public void UpdateHintUI()
    {
        UIManager.Instance.UIGameplay.UpdateHintUI();
        MoveInkIconToTop();
    }

    private void MoveInkIconToTop()
    {
        inkIcon.SetActive(true);
        inkIconMoving.SetActive(true);
        inkIconMoving.transform.localScale = Vector3.one;
        inkIconMoving.transform.position = inkIcon.transform.position;
        inkIconMoving.transform.DOScale(Vector3.one * 0.85f, 1.0f);
        var posTaget = inkIconMovingTarget.gameObject.transform.position;
        inkIconMoving.transform.DOMove(posTaget, 1.0f).OnComplete(() =>
        {
            inkIconMoving.SetActive(false);
            inkPanelNormal.SetActive(false);
            Close();
        });
    }


}
