namespace com.F4A.MobileThird
{
#if DEFINE_FIREBASE_MESSAGING
    using Firebase;
    using Firebase.Messaging;
    using System.Threading.Tasks;
    using Firebase.Extensions;
#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using System;

    public partial class FirebaseManager
    {
        private string topic = "art_story_notifications";

        protected void InitializeMessaging()
        {
#if DEFINE_FIREBASE_MESSAGING
            Debug.Log("FirebaseManager.InitializeMessaging");
            ToggleTokenOnInit();
#endif
        }

        public void RequestPermission(Action completed)
        {
            // This will display the prompt to request permission to receive
            // notifications if the prompt has not already been displayed before. (If
            // the user already responded to the prompt, their decision is cached by
            // the OS and can be changed in the OS settings).
            Debug.Log("FirebaseManager.RequestPermission");
            FirebaseMessaging.RequestPermissionAsync().ContinueWithOnMainThread(task =>
            {
                LogTaskCompletion(task, "RequestPermissionAsync");
                PlayerPrefs.SetInt("tokenSent", 1);
                PlayerPrefs.Save();
                ToggleTokenOnInit();
                Dispatcher.Instance.Invoke(() =>
                {
                    completed?.Invoke();
                });
            });
//#if UNITY_IOS
//            NotificationManager.Instance.RequestPushNotification(() =>
//            {
//                CPlayerPrefs.SetInt("tokenSent", 1);
//                CPlayerPrefs.Save();
//                ToggleTokenOnInit();
//                Dispatcher.Instance.Invoke(() =>
//                {
//                    completed?.Invoke();
//                });                
//            });
//#else

//#endif


        }


#if DEFINE_FIREBASE_MESSAGING
        public virtual void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.Log("FirebaseManager.OnMessageReceived Received a new message");
            var notification = e.Message.Notification;
            if (notification != null)
            {
                Debug.Log("FirebaseManager.OnMessageReceived title: " + notification.Title);
                Debug.Log("FirebaseManager.OnMessageReceived body: " + notification.Body);
                Debug.Log("FirebaseManager.OnMessageReceived body: " + notification.Icon);
            }
            if (e.Message.From.Length > 0)
            {
                Debug.Log("FirebaseManager.OnMessageReceived from: " + e.Message.From);
            }
            if (e.Message.Link != null)
            {
                Debug.Log("FirebaseManager.OnMessageReceived link: " + e.Message.Link.ToString());
            }
            if (e.Message.Data.Count > 0)
            {
                Debug.Log("FirebaseManager.OnMessageReceived data:");
                foreach (KeyValuePair<string, string> iter in e.Message.Data)
                {
                    Debug.Log("  " + iter.Key + ": " + iter.Value);
                }
            }
        }

        public virtual void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            Debug.Log("FirebaseManager.OnTokenReceived Received Registration Token: " + token.Token);
            //AdjustManager.Instance.EventMeasureUninstalls(token.Token);
        }
#endif

        public void ToggleTokenOnInit()
        {
#if DEFINE_FIREBASE_MESSAGING
            bool HasShowRequestPushNotif = PlayerPrefs.GetInt("tokenSent", 0) == 1;
            //FirebaseMessaging.TokenRegistrationOnInitEnabled = newValue;
            //Debug.Log("FirebaseManager.ToggleTokenOnInit Set TokenRegistrationOnInitEnabled to " + newValue);
            if (HasShowRequestPushNotif || Application.platform == RuntimePlatform.Android)
            {
                NotificationManager.Instance.SetupAllNotifications();
                FirebaseMessaging.MessageReceived += OnMessageReceived;
                FirebaseMessaging.TokenReceived += OnTokenReceived;
                FirebaseMessaging.TokenRegistrationOnInitEnabled = HasShowRequestPushNotif;
                FirebaseMessaging.SubscribeAsync(topic).ContinueWith(task =>
                {
                    LogTaskCompletion(task, "SubscribeAsync");
                });
            }


#endif
        }

#if DEFINE_FIREBASE_MESSAGING
        protected bool LogTaskCompletion(Task task, string operation)
        {
            bool complete = false;
            if (task.IsCanceled)
            {
                Debug.Log("FirebaseManager " + operation + " canceled.");
            }
            else if (task.IsFaulted)
            {
                Debug.Log("FirebaseManager " + operation + " uncounted an error.");
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    string errorCode = "";
                    FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        errorCode = String.Format("Error.{0}: ",
                          ((Firebase.Messaging.Error)firebaseEx.ErrorCode).ToString());
                    }
                    Debug.Log("FirebaseManager " + errorCode + exception.ToString());
                }
            }
            else if (task.IsCompleted)
            {
                Debug.Log("FirebaseManager " + operation + " completed");
                complete = true;
            }
            return complete;
        }
#endif
    }
}