using DataCore;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class UIDailyChallenge : HomepageTab
{
    private const string REFRESH_TIME_IN = "Time left: {0}H {1}M";

    [SerializeField] private DailyChallengeTab[] tabs;
    [SerializeField] private DailyChallengeData data;

    [SerializeField] private TextMeshProUGUI timeLeftTxt;
    [SerializeField] private Transform tabContainer;

    private DateTime _nextTimeRefresh;

    private void Update()
    {
        if (!_isOpen)
        {
            return;
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
        {
            RefreshDailyChallenge();
        }

#endif

        TimeSpan timeRemain = _nextTimeRefresh.Subtract(DateTime.UtcNow);

        if (timeRemain.TotalSeconds > 0)
        {
            //DataCore.Debug.Log("Update: " + DateTime.Now  + "-" + timeRemain.TotalSeconds + "-" +lastDay +"-" + timeTick);
            UpdateTimeRefresh((int)(timeRemain.Days * 24 + timeRemain.Hours), (int)timeRemain.Minutes, (int)timeRemain.Seconds);
        }
        else
        {
            // DataCore.Debug.Log("NOW -------------------------  " + DateTime.Now );
            UpdateData();
        }
    }

    public override void Init()
    {
        base.Init();
        if (!_didInit) {
            UpdateData();
        }
        
        _didInit = true;
    }

    protected override void AfterShowed()
    {
        base.AfterShowed();

        UpdateData();

        ShareUIManager.Instance.ShowSharedUI(SceneSharedEle.COIN);
        ShareUIManager.Instance.ShowSharedUI(SceneSharedEle.SETTING, false);
    }

    protected override void UIReset()
    {

    }


    private List<ChallengeSaveData> ChallengeSaveDatas = new List<ChallengeSaveData>();
    private void UpdateData()
    {

        int amountChallenge = GameConstants.AMOUNT_CHALLENGE_PER_DAY;
        ResetDailyChallengeIfNeeded();
        ChallengeSaveDatas.Clear();
        if (GameData.Instance.IsVipIap())
        {
            var challengeSaveDataList = GameData.Instance.SavedPack.DailyChallengeSavedData.ChallengeSaveDatas;
            for (var i = 0; i < challengeSaveDataList.Length; i++)
            {
                if (challengeSaveDataList[i].Type != ChallengeType.USE_INK_DROPS && challengeSaveDataList[i].Type != ChallengeType.UNLOCK_NEW_CHAPTER)
                {
                    ChallengeSaveDatas.Add(challengeSaveDataList[i]);
                }
            }
        }
        else
        {
            var challengeSaveDataList = GameData.Instance.SavedPack.DailyChallengeSavedData.ChallengeSaveDatas;
            for (var i = 0; i < challengeSaveDataList.Length; i++)
            {
                ChallengeSaveDatas.Add(challengeSaveDataList[i]);
            }
        }
        InitItem(amountChallenge, ChallengeSaveDatas);
        UpdateLines();

    }

    private void InitItem(int amountChallenge, List<ChallengeSaveData> challengeSaveDatas)
    {
        int dataId = 0;
        for (int i = 0; i < amountChallenge; i++)
        {
            if (i > challengeSaveDatas.Count - 1)
            {
                tabs[i].gameObject.SetActive(false);
            }
            else
            {
                var challengeSaveData = challengeSaveDatas[i];
                if (challengeSaveData != null)
                {
                    for (int j = 0; j < data.Data.Length; j++)
                    {
                        if (data.Data[j].Type == challengeSaveData.Type)
                        {
                            dataId = j;
                        }

                    }
                    tabs[i].SetData(challengeSaveData, data.Data[dataId], UpdateLines);
                }
            }
        }
    }
    private void UpdateLines()
    {
        int amountChallenge = GameConstants.AMOUNT_CHALLENGE_PER_DAY;
        for (int i = 0; i < amountChallenge; i++)
        {
            tabs[i].UpdateLine(amountChallenge);
        }
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

    public void ResetDailyChallengeIfNeeded()
    {
        DataCore.Debug.Log("ResetDailyChallengeIfNeeded");
        if (IsNextDay())
        {
            RefreshDailyChallenge();
        }

    }

    private void RefreshDailyChallenge()
    {
        DataCore.Debug.Log("Refresh Daily Challenge: " + GameData.Instance.SavedPack.DailyChallengeSavedData.LastDay);

        int amountChallenge = GameConstants.AMOUNT_CHALLENGE_PER_DAY;
        GameData.Instance.SavedPack.DailyChallengeSavedData.ChallengeSaveDatas = new ChallengeSaveData[amountChallenge];

        List<int> types = new List<int>(data.Data.Length);

        for (int i = 0; i < types.Capacity; i++)
        {
            types.Add(i);
        }

        int typeId;
        for (int i = 0; i < amountChallenge; i++)
        {
            typeId = types[Random.Range(0, types.Count)];
            types.Remove(typeId);

            GameData.Instance.SavedPack.DailyChallengeSavedData.ChallengeSaveDatas[i] = new ChallengeSaveData();
            GameData.Instance.SavedPack.DailyChallengeSavedData.ChallengeSaveDatas[i].Type = data.Data[typeId].Type;
            GameData.Instance.SavedPack.DailyChallengeSavedData.ChallengeSaveDatas[i].IsClaimed = false;
            GameData.Instance.SavedPack.DailyChallengeSavedData.ChallengeSaveDatas[i].ReachedValue = 0;
            GameData.Instance.SavedPack.DailyChallengeSavedData.ChallengeSaveDatas[i].ChallengeTargetId =
                Random.Range(0, data.Data[typeId].Amount.Length);

            if (data.Data[typeId].Type == ChallengeType.PLAY_DIFFERENCE_BOOK)
            {
                GameData.Instance.SavedPack.DailyChallengeSavedData.ChallengeSaveDatas[i].BookIds = new List<int>(data.Data[typeId].Amount[GameData.Instance.SavedPack.DailyChallengeSavedData.ChallengeSaveDatas[i].ChallengeTargetId]);
            }
        }

        GameData.Instance.RequestSaveGame();
    }
    private bool IsNextDay()
    {
        DateTime now = DateTime.UtcNow;
        int lastDay = GameData.Instance.SavedPack.DailyChallengeSavedData.LastDay;
        if (lastDay != now.Day)
        {
            if (Mathf.Abs(lastDay - now.Day) > 1)
            {
                if (now.Hour >= data.TimeReset)
                {
                    lastDay = now.Day;
                    GameData.Instance.SavedPack.DailyChallengeSavedData.LastDay = lastDay;
                    _nextTimeRefresh = DateTime.UtcNow.Date.AddDays(1).AddHours(data.TimeReset);
                }
                else
                {
                    lastDay = now.Day - 1;
                    GameData.Instance.SavedPack.DailyChallengeSavedData.LastDay = lastDay;
                    _nextTimeRefresh = DateTime.UtcNow.Date.AddHours(data.TimeReset);
                }

                return true;
            }
            else
            {
                if (now.Hour >= data.TimeReset)
                {
                    _nextTimeRefresh = DateTime.UtcNow.Date.AddDays(1).AddHours(data.TimeReset);
                    lastDay = now.Day;
                    GameData.Instance.SavedPack.DailyChallengeSavedData.LastDay = lastDay;
                    return true;
                }
                else
                {
                    _nextTimeRefresh = DateTime.UtcNow.Date.AddHours(data.TimeReset);
                }
            }
        }
        else
        {
            _nextTimeRefresh = DateTime.UtcNow.Date.AddDays(1).AddHours(data.TimeReset);
        }

        return false;
    }
}
