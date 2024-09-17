using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DataCore;
using System.Linq;
#if UNITY_IOS
using Unity.Notifications.iOS;
#elif UNITY_ANDROID
using Unity.Notifications.Android;
#endif
using NotificationSamples;

namespace com.F4A.MobileThird
{

    public class LocalNotificationContent
    {
        public int DaysOfWeek;
        public int Hour;
        public int Minute;
        public List<string> Contents;
        public LocalNotificationContent(int weekday, int hour, int minute, List<String> contents)
        {
            this.DaysOfWeek = weekday;
            this.Hour = hour;
            this.Minute = minute;
            this.Contents = contents;
        }
    }

    public class LocalNotificationService : ILocalNotificationService
    {
        private Dictionary<int, List<LocalNotificationContent>> _weekDayNotificationContent;
        private bool _isInited = false;
        private readonly string androidNotificationChannelId = "yomi.artstory.unitylocalnotification.UnityNotification";
        private GameNotificationsManager _gameNotificationsManager;
        private readonly string androidNotificationChannelName = "Art Story Puzzle";

        public void Initialize(IList<Data.LocalNotification> localNotificationData, GameNotificationsManager notificationsManager)
        {

            _weekDayNotificationContent = new Dictionary<int, List<LocalNotificationContent>>();
            SetupAndroidNotificationChannel(notificationsManager);

            if (localNotificationData != null)
            {
                foreach (var item in localNotificationData)
                {
                    AddNotification(item);
                }
            }
            _isInited = true;
        }

        public void AddNotification(Data.LocalNotification localNotification)
        {
            try
            {

                if (_weekDayNotificationContent == null)
                {
                    _weekDayNotificationContent = new Dictionary<int, List<LocalNotificationContent>>();
                }
                foreach (var weekDay in localNotification.DaysOfWeek)
                {
                    var notification = new LocalNotificationContent(weekDay, localNotification.Hour, localNotification.Minute, localNotification.Contents);
                    if (!_weekDayNotificationContent.ContainsKey(weekDay))
                    {
                        _weekDayNotificationContent.Add(weekDay, new List<LocalNotificationContent>());
                    }
                    _weekDayNotificationContent.TryGetValue(weekDay, out List<LocalNotificationContent> weekDateNotifications);
                    weekDateNotifications.Add(notification);
                    _weekDayNotificationContent.Remove(weekDay);
                    _weekDayNotificationContent.Add(weekDay, weekDateNotifications);
                }
            }
            catch (Exception ex)
            {
                DataCore.Debug.Log($"Failed to AddNotification. {ex.Message}");
            }
        }

        public IEnumerator ScheduleAllNotifications()
        {
            if (!_isInited) yield break;
            if (!AuthorizedNotificationPermission()) yield break;
            if (_gameNotificationsManager == null) yield break;
            try
            {
                CancelAllNotifications();
                for (int i = 1; i <= 30; i++)
                {
                    var now = DateTime.Now;
                    var targetDate = now.AddDays(i);
                    var weekDay = (int)targetDate.DayOfWeek + 1;
                    if (_weekDayNotificationContent.ContainsKey(weekDay))
                    {
                        _weekDayNotificationContent.TryGetValue(weekDay, out List<LocalNotificationContent> listNotifications);
                        foreach (var notification in listNotifications)
                        {
                            SetNotification(targetDate, notification, weekDay);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataCore.Debug.Log($"Failed to SetupAllNotifications. {ex.Message}");
            }
            yield return new WaitForEndOfFrame();
            DataCore.Debug.Log("Complete ScheduleAllNotifications");

            //PlayerPrefs.SetInt(C.TotalNotificationKey, lastId);
        }

        public void SetNotification(DateTime fireDate, LocalNotificationContent notification, int weekDay)
        {
            if (_gameNotificationsManager == null) return;
            var contentId = UnityEngine.Random.Range(0, notification.Contents.Count);
            string content = notification.Contents[contentId];

            if (weekDay == 1 || weekDay == 7)
            {
                if (contentId == 0)
                {
                    //var bestScore = G.ProfileService.BestScore;
                    var bestScore = 0;
                    content = string.Format(content, bestScore);
                }
                else if (contentId == 1)
                {
                    var durationInSeconds = PlayerPrefs.GetFloat(ConfigManager.TotalDuration);
                    int minutes = Mathf.FloorToInt(durationInSeconds / 60f) + 1;
                    content = string.Format(content, minutes);
                }
            }
            var fireTime = new DateTime(fireDate.Year, fireDate.Month, fireDate.Day, notification.Hour, notification.Minute, 0, DateTimeKind.Local);

            IGameNotification notif = _gameNotificationsManager.CreateNotification();
            if (notification == null) { return; }
            notif.Title = androidNotificationChannelName;
            notif.Body = content;
            notif.Group = !string.IsNullOrEmpty(androidNotificationChannelId) ? androidNotificationChannelId : androidNotificationChannelId;
            notif.DeliveryTime = fireTime;

            //11
            PendingNotification notificationToDisplay =
                _gameNotificationsManager.ScheduleNotification(notif);
            //12
            notificationToDisplay.Reschedule = true;


        }

        public void CancelAllNotifications()
        {
            if (!_isInited)
                return;

            if (_gameNotificationsManager == null) return;
            _gameNotificationsManager.CancelAllNotifications();
        }


        public bool HasShowNotificationRequest()
        {
#if UNITY_IOS
            return iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus != AuthorizationStatus.NotDetermined;
#else            
            return true;
#endif
        }

        public bool AuthorizedNotificationPermission()
        {
#if UNITY_IOS
            return iOSNotificationCenter.GetNotificationSettings().AuthorizationStatus == AuthorizationStatus.Authorized;
#else
            return true;
#endif
        }

        public void SetupAndroidNotificationChannel(GameNotificationsManager notificationsManager)
        {
            if (notificationsManager == null) return;
            _gameNotificationsManager = notificationsManager;
            GameNotificationChannel notificationChannel = new GameNotificationChannel(androidNotificationChannelId,
                androidNotificationChannelName, androidNotificationChannelName,
                GameNotificationChannel.NotificationStyle.Default, highPriority: true);
            // Wrap channel in Android object            
            notificationsManager.Initialize(notificationChannel);
            
        }

        public void SetupAllNotifications(MonoBehaviour behaviour)
        {
            behaviour.StartCoroutine(ScheduleAllNotifications());
        }

        public void RequestAuthorization(Action completed)
        {
#if UNITY_IOS

            var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
            using (var req = new AuthorizationRequest(authorizationOption, true))
            {
                string res = "\n RequestAuthorization:";
                res += "\n finished: " + req.IsFinished;
                res += "\n granted :  " + req.Granted;
                res += "\n error:  " + req.Error;
                res += "\n deviceToken:  " + req.DeviceToken;
                DataCore.Debug.Log(res);
                //if (req.Granted)
                //{
                //    Adjust.setDeviceToken(req.DeviceToken);
                //}
                completed?.Invoke();
            }
#endif
        }

    }
}

