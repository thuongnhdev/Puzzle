using System;
using System.Collections;
using System.Collections.Generic;
using DataCore;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dayTxt;
    [SerializeField] private TextMeshProUGUI amountTxt;

    [SerializeField] private Image claimedBorderImg;
    [SerializeField] private Image completeImg;
    [SerializeField] private Image inkIcon;
    [SerializeField] private Image inkFlyIcon;

    private DailyRewardStatus _status;
    private int _amount;

    public void SetData(int day, int amount, DailyRewardStatus status)
    {
        dayTxt.SetText("Day " + day);
        amountTxt.SetText("x" + amount);

        this._amount = amount;
        inkFlyIcon.enabled = false;
        UpdateStatus(status);
    }

    public void Claim(Action onComplete)
    {
        if (_status != DailyRewardStatus.CAN_CLAIM)
        {
            DataCore.Debug.LogError("[DAILY REWARD] Incorrect Day");
            return;
        }

        MoveInkIconToTop(onComplete);
        GameData.Instance.IncreaseInks(_amount, ConfigManager.GameData.ResourceEarnSource.login_reward);
        UpdateStatus(DailyRewardStatus.CLAIMED);

        var timeClaim = System.DateTime.Now.ToString("yyyyMMdd");
        CPlayerPrefs.SetString(ConfigManager.TimeDailyRewardClaim, timeClaim);
        CPlayerPrefs.Save();
    }


    [ContextMenu("Move")]
    private void MoveInkIconToTop(Action onComplete)
    {
        inkFlyIcon.enabled = true;
        inkFlyIcon.transform.localScale = Vector3.one;
        inkFlyIcon.transform.position = inkIcon.transform.position;
        inkFlyIcon.transform.DOScale(Vector3.one * 0.5f, 1.0f);
        inkIcon.enabled = false;
        UIManager.Instance.ActiveLockTouch(true);
        inkFlyIcon.transform.DOMove(ShareUIManager.Instance.InkTopCurrencyIconPos, 1.0f).SetEase(Ease.InBack).OnComplete(() =>
        {
            // inkPanelParent.SetActive(false);
            //ShareUIManager.Instance.PlayAnimInkIcon();
            ShareUIManager.Instance.UpdateCurrencyData(1.0f, () =>
            {
                UIManager.Instance.ActiveLockTouch(false);
                inkFlyIcon.enabled = false;
                inkIcon.gameObject.SetActive(false);
                onComplete?.Invoke();
            });

        });
    }

    private void UpdateStatus(DailyRewardStatus status)
    {
        this._status = status;

        switch (status)
        {
            case DailyRewardStatus.CAN_CLAIM:
                claimedBorderImg.enabled = true;
                completeImg.enabled = false;
                inkIcon.enabled = true;
                break;
            case DailyRewardStatus.CLAIMED:
                claimedBorderImg.enabled = true;
                completeImg.enabled = true;
                inkIcon.gameObject.SetActive(false);
                break;
            case DailyRewardStatus.LOCK:
                claimedBorderImg.enabled = false;
                completeImg.enabled = false;
                inkIcon.enabled = true;
                break;
        }
    }
}

public enum DailyRewardStatus
{
    CLAIMED, CAN_CLAIM, LOCK
}
