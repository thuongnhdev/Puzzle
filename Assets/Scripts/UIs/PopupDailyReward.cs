using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataCore;
using TMPro;
using UnityEngine.UI;

public class PopupDailyReward : BasePanel
{
    private const string REFRESH_TIME_IN = "{0}H {1}M";
    private const int TIME_REFRESH = 0;

    [SerializeField] private DailyRewardItem[] items;
    [SerializeField] private Button claimBtn;
    [SerializeField] private TextMeshProUGUI timeLeftTxt;
    [SerializeField] private NumberCounter txtNumberCounter;

    private BasePanel previousPanel;
    private DateTime _nextTimeRefresh;
    private int _indexDailyReward;

    private bool _isClaimed;

    public override void SetData(object[] data)
    {
        base.SetData(data);
        if (data.Length > 0)
        {
            previousPanel = (BasePanel)data[0];
        }


    }

    public bool CanShow()
    {
        return IsNextDay();
    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
        RefreshData();
        ShareUIManager.Instance.UpdateCurrencyData(0.0f);
    }

    public override void OnSwipeLeft()
    {
        Close();
    }
    public override void Close()
    {
        base.Close();
        if (previousPanel != null)
        {
            previousPanel.Open();
            previousPanel = null;
        }

        if (!_isClaimed)
        {
            GameData.Instance.SavedPack.SaveData.DailyRewardIndex = 0;
            GameData.Instance.RequestSaveGame();
        }

        GameManager.Instance.RemoveObjList(this);
    }

    public void OnClaimTap()
    {
        claimBtn.interactable = false;

        items[_indexDailyReward].Claim(Close);
        _isClaimed = true;

        GameData.Instance.SavedPack.SaveData.DailyRewardIndex++;

        if (GameData.Instance.SavedPack.SaveData.DailyRewardIndex == items.Length)
        {
            GameData.Instance.SavedPack.SaveData.DailyRewardIndex = 0;
        }

        GameData.Instance.RequestSaveGame();
        //Close();
    }

    private void Update()
    {
        if (!_isOpen)
        {
            return;
        }

        TimeSpan timeRemain = _nextTimeRefresh.Subtract(DateTime.UtcNow);

        if (timeRemain.TotalSeconds > 0)
        {
            //DataCore.Debug.Log("Update: " + DateTime.Now  + "-" + timeRemain.TotalSeconds + "-" +lastDay +"-" + timeTick);
            UpdateTimeRefresh((int)(timeRemain.Days * 24 + timeRemain.Hours), (int)timeRemain.Minutes, (int)timeRemain.Seconds);
        }
        else
        {
            // DataCore.Debug.Log("NOW -------------------------  " + DateTime.Now );
            if (IsNextDay())
            {
                RefreshData();
            }

        }
    }

    private void RefreshData()
    {
        claimBtn.interactable = false;
        this._indexDailyReward = GameData.Instance.SavedPack.SaveData.DailyRewardIndex;
        if (GameData.Instance.IsResetDailyReward() == GameData.DailyRewardType.RESET)
            _indexDailyReward = 0;

        if (GameData.Instance.IsResetDailyReward() == GameData.DailyRewardType.DISABLE)
            _indexDailyReward = -1;

        DailyRewardStatus status;
        for (int i = 0; i < items.Length; i++)
        {
            if (i == this._indexDailyReward)
            {
                claimBtn.interactable = true;
                status = DailyRewardStatus.CAN_CLAIM;
            }
            else if (i < this._indexDailyReward)
            {
                status = DailyRewardStatus.CLAIMED;
            }
            else
            {
                status = DailyRewardStatus.LOCK;
            }
            items[i].SetData(i + 1, MCache.Instance.Config.DailyInkReward[i], status);

        }
        //UpdateCurrencyData(0.25f,);
        _isClaimed = false;

    }



    private bool IsNextDay()
    {
        DateTime now = DateTime.UtcNow;
        int lastDay = GameData.Instance.SavedPack.SaveData.LastDayDailyReward;
        if (lastDay != now.Day)
        {
            if (lastDay != 0 && lastDay > now.Day)
                return false;

            if (Mathf.Abs(lastDay - now.Day) > 1)
            {
                GameData.Instance.SavedPack.SaveData.DailyRewardIndex = 0;
                if (now.Hour >= TIME_REFRESH)
                {
                    lastDay = now.Day;
                    GameData.Instance.SavedPack.SaveData.LastDayDailyReward = lastDay;
                    var today = DateTime.UtcNow.Date;
                    _nextTimeRefresh = DateTime.UtcNow.Date.AddDays(1).AddHours(TIME_REFRESH);
                }
                else
                {
                    lastDay = now.Day - 1;
                    GameData.Instance.SavedPack.SaveData.LastDayDailyReward = lastDay;
                    _nextTimeRefresh = DateTime.UtcNow.Date.AddHours(TIME_REFRESH);
                }

                return true;
            }
            else
            {
                if (now.Hour >= TIME_REFRESH)
                {
                    _nextTimeRefresh = DateTime.UtcNow.Date.AddDays(1).AddHours(TIME_REFRESH);
                    lastDay = now.Day;
                    GameData.Instance.SavedPack.SaveData.LastDayDailyReward = lastDay;
                    return true;
                }
                else
                {
                    _nextTimeRefresh = DateTime.UtcNow.Date.AddHours(TIME_REFRESH);
                }
            }
        }
        else
        {
            _nextTimeRefresh = DateTime.UtcNow.Date.AddDays(1).AddHours(TIME_REFRESH);
        }


        return false;
    }


    private void UpdateTimeRefresh(int hour, int min, int seconds)
    {
        if (hour == 0 && min == 0 && seconds > 0)
        {
            min = 1;
        }

        string time = string.Format(REFRESH_TIME_IN, (hour).ToString("00"), min.ToString("00"));
        timeLeftTxt.SetText(time);
    }

    public override void OnUpdateInk(float animDuration, Action onComplete = null)
    {
        UpdateCurrencyData(animDuration, onComplete);
    }
    public void UpdateCurrencyData(float animDuration, Action onComplete = null)
    {
        int newData = GameData.Instance.SavedPack.SaveData.Coin;
        txtNumberCounter.PlayAnim(newData, animDuration, onComplete);

    }


}
