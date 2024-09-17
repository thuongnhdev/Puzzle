using System;
using System.Collections;
using System.Collections.Generic;
using DanielLochner.Assets.SimpleScrollSnap;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using com.F4A.MobileThird;
using System.Linq;

namespace DataCore
{
    public class LiveEventTimer : SingletonMono<LiveEventTimer>
    {
        private const string REFRESH_TIME_IN = "Event ends in: {0}D {1}H {2}M";

        private bool _isEventActive;
        public string PorgressionFormat = "{0} / {1}";
        private LiveEventData _liveEventData;

        public LiveEventData LiveEventData
        {
            get
            {
                return _liveEventData;
            }
        }

        public bool IsEventActive
        {
            get
            {
                UpdateEventTime();
                return _isEventActive;
            }
        }
        private void Start()
        {
            UpdateEventTime();
        }

        private DateTime _timeEndEvent;

        public DateTime TimeEndEvent
        {
            get
            {
                return _timeEndEvent;
            }
        }

        public void UpdateEventTime()
        {
            InitData();
            _timeEndEvent = new DateTime(_liveEventData.EndTime.Year, _liveEventData.EndTime.Month, _liveEventData.EndTime.Day);
            DateTime timeStartEvent = new DateTime(_liveEventData.StartTime.Year, _liveEventData.StartTime.Month, _liveEventData.StartTime.Day);

            if (timeStartEvent < DateTime.Now && DateTime.Now < _timeEndEvent)
            {
                _isEventActive = true;
            }
            else
            {
                _isEventActive = false;
            }
        }

        private void Update()
        {
            TimeSpan timeRemain = _timeEndEvent.Subtract(DateTime.Now);

            if (timeRemain.TotalSeconds > 0)
            {
                UpdateTimeRefresh(timeRemain.Days, timeRemain.Hours, (int)timeRemain.Minutes, (int)timeRemain.Seconds);
            }
            else
            {
                TimeoutEvent();
            }
        }

        private void TimeoutEvent()
        {
            _isEventActive = false;
            UpdateTimeRefresh(0, 0, 0, 0);
            this.PostEvent(EventID.LiveEventTimeout);
        }

        private string UpdateTimeRefresh(int day, int hour, int min, int seconds)
        {
            if (hour == 0 && min == 0 && seconds > 0)
            {
                min = 1;
            }

            if (seconds < 0)
            {
                seconds = 0;
            }
            dayRemainEvent = day.ToString();
            hourRemainEvent = hour.ToString();
            string time = string.Format(REFRESH_TIME_IN, day.ToString(), (hour).ToString("00"), min.ToString("00"));
            return time;
        }

        private string dayRemainEvent = "0";
        private string hourRemainEvent = "0";
        public string GetDay()
        {
            if(dayRemainEvent == "0")
            {
                if (_liveEventData == null) InitData();
                _timeEndEvent = new DateTime(_liveEventData.EndTime.Year, _liveEventData.EndTime.Month, _liveEventData.EndTime.Day);
                DateTime timeStartEvent = new DateTime(_liveEventData.StartTime.Year, _liveEventData.StartTime.Month, _liveEventData.StartTime.Day);
                if (timeStartEvent < DateTime.Now && DateTime.Now < _timeEndEvent)
                {
                    TimeSpan timeRemain = _timeEndEvent.Subtract(DateTime.Now);
                    if (timeRemain.TotalSeconds > 0)
                    {
                        dayRemainEvent = timeRemain.Days.ToString();
                    }
                }
            }
            return dayRemainEvent;
        }

        public string GetHour()
        {
            if (hourRemainEvent == "0")
            {
                if (_liveEventData == null) InitData();
                _timeEndEvent = new DateTime(_liveEventData.EndTime.Year, _liveEventData.EndTime.Month, _liveEventData.EndTime.Day);
                DateTime timeStartEvent = new DateTime(_liveEventData.StartTime.Year, _liveEventData.StartTime.Month, _liveEventData.StartTime.Day);
                if (timeStartEvent < DateTime.Now && DateTime.Now < _timeEndEvent)
                {
                    TimeSpan timeRemain = _timeEndEvent.Subtract(DateTime.Now);
                    if (timeRemain.TotalSeconds > 0)
                    {
                        hourRemainEvent = timeRemain.Hours.ToString();
                    }
                }
               
            }
            return hourRemainEvent;
        }

        private void InitData()
        {
            if(_liveEventData == null)
            {
                _liveEventData = MasterDataStore.Instance.LiveEventData;
                for (int i = 0; i < _liveEventData.PostCardDatas.Length; i++)
                {
                    if(PlayerPrefs.GetInt("Random",0)== 0)
                    {
                        _liveEventData.PostCardDatas[i].PuzzleData = RandomIndexItem(_liveEventData, _liveEventData.PostCardDatas[i].PuzzleData);
                        PlayerPrefs.SetInt("Random", 1);
                        PlayerPrefs.Save();
                    }
                 
                }
            }
        }

        private LiveEventPuzzleData[] RandomIndexItem(LiveEventData data, LiveEventPuzzleData[] listLiveItem)
        {
            LiveEventPuzzleData[] liveEventPuzzleDatas = new LiveEventPuzzleData[listLiveItem.Length];
            Dictionary<int, int> indexRandomArray = new Dictionary<int, int>();

            for (int i = 0; i < listLiveItem.Length; i++)
                indexRandomArray.Add(i, i);

            System.Random rand = new System.Random();
            int[] intList = new int[listLiveItem.Length];
            for (int i = 0; i < listLiveItem.Length; ++i)
            {
                int index = rand.Next(indexRandomArray.Count);

                KeyValuePair<int, int> pair = indexRandomArray.ElementAt(index);
                if (indexRandomArray.ContainsValue(pair.Value))
                {
                    intList[i] = pair.Value;
                    liveEventPuzzleDatas[i] = listLiveItem[intList[i]];
                    indexRandomArray.Remove(pair.Key);
                }
            }

            return liveEventPuzzleDatas;

        }

    }

}
