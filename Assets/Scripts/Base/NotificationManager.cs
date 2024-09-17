using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DataCore;
using NotificationSamples;
#if UNITY_IOS
using Unity.Notifications.iOS;
#elif UNITY_ANDROID
using Unity.Notifications.Android;
#endif


namespace com.F4A.MobileThird
{
    public class NotificationManager : SingletonMonoAwake<NotificationManager>
    {
        private ILocalNotificationService localNotificationService;
        [SerializeField]
        private GameNotificationsManager NotificationsManager;

        private List<string> NotificationList = new List<string>() {
            "📚 Ready to explore?\nPut 🧩 pieces together and discover a story! ",
            "👋Hey! Let's play Art Story and receive more 💰💰💰",
            "🤩 Sharpen your mind!\nTrain you 🧠 with magical puzzles! ",
            "⏰ Take a break! \nSolve relaxing and beautiful puzzles! 🧩🧩🧩",
            "You know, playing Art Story may slow the aging process and reduce stress ⚡🥊",
            "🌿🍎☘️Your brain needs exercise to stay healthy 🌿🍎☘️ \n🌈Let's train your brain today with Art Story 😘",
            "You must've had a hard day 😿 \n☘️Let's take a stretch break to loosen muscles and rid your body of tension 💓",
            "💓 HURRY!! 💓\bStarter pack provides x6 value, only cost $1.99 🤑🤑"
        };

        [SerializeField]
        private List<int> time = new List<int>() { 11, 15, 19 };

        private void Start()
        {
#if UNITY_ANDROID || UNITY_IOS
            localNotificationService = new LocalNotificationService();
            if (localNotificationService != null)
            {
                IList<Data.LocalNotification> localNotificationData = new List<Data.LocalNotification>();
                List<int> day = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
                foreach (var hour in time)
                {
                    var min = 0;
                    Data.LocalNotification data = new Data.LocalNotification(day, NotificationList, hour, min);
                    localNotificationData.Add(data);
                }
                localNotificationService.Initialize(localNotificationData, NotificationsManager); ;

            }
#endif            
        }
        public void SetupAllNotifications()
        {
            DataCore.Debug.Log("SetupAllNotifications");
            localNotificationService.CancelAllNotifications();
            localNotificationService.SetupAllNotifications(this);
        }
        public void RequestPushNotification(Action completed)
        {
            if (localNotificationService != null)
            {
                localNotificationService.RequestAuthorization(completed);
            }
        }
        public bool AuthorizedNotificationPermission()
        {
            if (localNotificationService != null)
            {
                return localNotificationService.AuthorizedNotificationPermission();
            }
            return false;
        }
    }
}
