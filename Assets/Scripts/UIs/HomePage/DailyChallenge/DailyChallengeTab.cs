using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DataCore;
using com.F4A.MobileThird;

public class DailyChallengeTab : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rewardAmountTxt;
    [SerializeField] private TextMeshProUGUI titleTxt;

    [SerializeField] private Button claimBtn;
    [SerializeField] private Image claimBtnImg;
    [SerializeField] private TextMeshProUGUI claimTxt;

    [SerializeField] private Image completeTick;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI reachedValueTxt;
    [SerializeField] private Image inkIcon;
    [SerializeField] private Image inkIconMoving;

    [SerializeField] private Color32[] progressColors;
    [SerializeField] private Image lineImg;

    private ChallengeSaveData _challengeSaveData;
    private bool _canClaim = false;
    private int _rewardAmount;
    private Action _onUpdateLines;

    public void SetData(ChallengeSaveData challengeSaveData, ChallengeData data, Action onUpdateLines)
    {
        _challengeSaveData = challengeSaveData;

        if (challengeSaveData.IsClaimed)
        {
            completeTick.enabled = true;
            claimBtn.gameObject.SetActive(false);
            transform.SetAsLastSibling();
        }
        else
        {
            completeTick.enabled = false;
            claimBtn.gameObject.SetActive(true);
        }

        int targetValue = data.Amount[challengeSaveData.ChallengeTargetId];
        _rewardAmount = data.InkDrops[challengeSaveData.ChallengeTargetId];

        rewardAmountTxt.SetText("x" + _rewardAmount.ToString());
        titleTxt.SetText(GetTitle(data.Type, targetValue));

        reachedValueTxt.SetText(challengeSaveData.ReachedValue + "/" + targetValue);

        _canClaim = challengeSaveData.ReachedValue >= targetValue;
        claimBtn.enabled = _canClaim;
        lineImg.enabled = true;
        if (_canClaim)
        {
            if (!challengeSaveData.IsClaimed)
            {
                transform.SetAsFirstSibling();

                claimBtnImg.color = new Color32(71, 181, 99, 255);
                claimTxt.color = Color.white;
            }

            progressBar.fillAmount = 1;
            progressBar.color = progressColors[0];
        }
        else
        {
            //DataCore.Debug.Log("--------------" + ((float)challengeSaveData.ReachedValue) / targetValue);
            progressBar.fillAmount = ((float)challengeSaveData.ReachedValue) / targetValue;
            progressBar.color = progressColors[1];

            claimBtnImg.color = new Color32(229, 224, 208, 255);
            claimTxt.color = new Color32(187, 187, 187, 255);
        }

        _onUpdateLines = onUpdateLines;
    }

    public void UpdateLine(int amountChallenge)
    {
        if (transform.GetSiblingIndex() == amountChallenge - 1)
        {
            lineImg.enabled = false;
        }
        else
        {
            lineImg.enabled = true;
        }
    }

    public void OnClaimTap()
    {
        if (!_canClaim)
        {
            return;
        }

        SoundController.Instance.PlaySfxClick();
        // Update Claim
        _challengeSaveData.IsClaimed = true;

        completeTick.enabled = true;
        claimBtn.gameObject.SetActive(false);

        GameData.Instance.IncreaseInks(_rewardAmount, ConfigManager.GameData.ResourceEarnSource.daily_challenge);

        MoveInkIconToTop(() =>
        {
            transform.SetAsLastSibling();
            lineImg.enabled = false;
            _onUpdateLines?.Invoke();
            AnalyticManager.Instance.TrackClaimChallenge(_challengeSaveData.ChallengeTargetId.ToString());
        });
    }

    private string GetTitle(ChallengeType type, int targetValue)
    {
        switch (type)
        {
            case ChallengeType.COMPLETE_PUZZLE:
                return "Play " + targetValue + " puzzles";
            case ChallengeType.PLAY_DIFFERENCE_BOOK:
                return "Play " + targetValue + " different books";
            case ChallengeType.USE_INK_DROPS:
                return "Spend " + targetValue + " Ink Drops";
            case ChallengeType.UNLOCK_NEW_CHAPTER:
                return "Unlock " + targetValue + " new chapter";
            case ChallengeType.USE_HINT:
                return "Use " + targetValue + " Hint";
            case ChallengeType.SHARE_PUZZLE:
                return "Share Puzzle";
            case ChallengeType.SPIN_LUCKY_DRAW:
                return "Spin " + targetValue + " times";
        }

        return "";
    }

    [ContextMenu("Move")]
    private void MoveInkIconToTop(Action onComplete)
    {
        inkIconMoving.enabled = true;
        inkIconMoving.transform.localScale = Vector3.one;
        inkIconMoving.transform.position = inkIcon.transform.position;
        inkIconMoving.transform.DOScale(Vector3.one * 0.5f, 1.0f);
        inkIcon.enabled = false;
        UIManager.Instance.ActiveLockTouch(true);
        inkIconMoving.transform.DOMove(ShareUIManager.Instance.InkTopCurrencyIconPos, 1.0f).SetEase(Ease.InBack).OnComplete(() =>
        {
            // inkPanelParent.SetActive(false);
            //ShareUIManager.Instance.PlayAnimInkIcon();
            ShareUIManager.Instance.UpdateCurrencyData(1.0f, () =>
            {
                UIManager.Instance.ActiveLockTouch(false);
                inkIconMoving.enabled = false;
                inkIcon.enabled = true;
                onComplete?.Invoke();
            });

        });
    }
}
