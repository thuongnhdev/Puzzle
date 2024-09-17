namespace com.F4A.MobileThird
{
#if DEFINE_FIREBASE_AUTH
    using Firebase.Auth;
    using Firebase.Database;
    using Firebase.Extensions;
    using Firebase;
    using System;
#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using DataCore;
    using Firebase.Firestore;

    public enum EAccoutLogin
    {
        None,
        Email,
        Facebook,
    }

    public partial class FirebaseManager
    {
#if DEFINE_FIREBASE_AUTH
        protected FirebaseUser _user;
#endif
        protected void InitializeAuth()
        {
#if DEFINE_FIREBASE_AUTH
            CheckFirebaseDependencies();
#endif
        }
        private void CheckFirebaseDependencies()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    if (task.Result == DependencyStatus.Available)
                    {
                        auth = FirebaseAuth.DefaultInstance;
                        try
                        {
                            GoogleSignInManager.Instance.InitializeGoogle(auth);
                        }
                        catch { }
#if UNITY_IOS
                        try
                        {
                            AppleSignInManager.Instance.InitializeApple(auth);
                        }
                        catch { }
#endif
                    }
                }
            });
        }

        private void OnEnable()
        {
            SocialManager.OnLoginFacebookSuccessed += SocialManager_OnInitFacebookCompleted;
            SocialManager.OnLoginFacebookFailed += SocialManager_OnLoginFacebookFailed;
        }

        private void OnDisable()
        {
            SocialManager.OnLoginFacebookSuccessed -= SocialManager_OnInitFacebookCompleted;
            SocialManager.OnLoginFacebookFailed -= SocialManager_OnLoginFacebookFailed;
        }

        private void SocialManager_OnInitFacebookCompleted()
        {
            if (SocialManager.Instance.IsLoginFacebook())
            {
                SignInWithFacebook(SocialManager.Instance.GetFacebookAccessTokenString(), true);
            }
        }

        private void SocialManager_OnLoginFacebookFailed()
        {
            Dispatcher.Instance.Invoke(() =>
            {
                OnLoginFacebookCompleted?.Invoke(false, string.Empty, "0");
            });

        }
        public bool IsSignIn()
        {
#if DEFINE_FIREBASE_AUTH
            return auth != null && auth.CurrentUser != null && _user != null && !string.IsNullOrEmpty(_user.UserId);
#else
            return false;
#endif
        }

        public void SignInWithFacebook(string accessToken, bool callDelegate = true)
        {
#if DEFINE_FIREBASE_AUTH
            try
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
#if DEFINE_FIREBASE_AUTH
                    Credential credential = FacebookAuthProvider.GetCredential(accessToken);
                    auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCanceled)
                        {
                            if (callDelegate)
                            {
                                Dispatcher.Instance.Invoke(() =>
                                {
                                    OnLoginFacebookCompleted?.Invoke(false, "SignInWithFacebook was canceled", "0");
                                });
                            }
                            
                            return;
                        }
                        else if (task.IsFaulted)
                        {
                            if (callDelegate)
                            {
                                Dispatcher.Instance.Invoke(() =>
                                {
                                    OnLoginFacebookCompleted?.Invoke(false, "SignInWithFacebook " + task.Exception.ToString(), "0");
                                });
                            }                            
                            return;
                        }
                        else
                        {
                            _user = task.Result;
                            if (callDelegate)
                            {
                                GameData.Instance.SetLoginType("Facebook");
                                Dispatcher.Instance.Invoke(() =>
                                {
                                    OnLoginFacebookCompleted?.Invoke(true, string.Empty, _user.UserId);
                                });
                            }
                        }
                    });
#endif
                }
            }
            catch (Exception ex)
            {
                DataCore.Debug.LogError("FBLogin.SignInWithFacebook/SignInWithCredentialAsync encountered an error: " + ex);
                if (callDelegate)
                {
                    Dispatcher.Instance.Invoke(() =>
                    {
                        OnLoginFacebookCompleted?.Invoke(false, ex.Message, "0");
                    });
                }
            }
#endif
        }

        public void LogoutWithFacebook()
        {
#if DEFINE_FIREBASE_AUTH
            if (auth != null && auth.CurrentUser != null) auth.SignOut();
            _user = null;
#endif
        }

#if DEFINE_FIREBASE_AUTH
        public void GetUserData(string databaseName, string uid, System.Action<bool, object> callBack)
        {
            if (_firebaseApp == null) return;
            FirebaseFirestore db = FirebaseFirestore.GetInstance(_firebaseApp);
            DocumentReference docRef = db.Collection(databaseName).Document(uid);
            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    DataCore.Debug.Log(String.Format("Document data for {0} document:", snapshot.Id));
                    Dictionary<string, object> jsonData = snapshot.ToDictionary();                    
                    callBack?.Invoke(true, jsonData[uid]);
                }
                else
                {
                    DataCore.Debug.Log(String.Format("Document {0} does not exist!", snapshot.Id));
                    callBack?.Invoke(false, null);
                }
            });
        }
#endif

#if DEFINE_FIREBASE_AUTH
        public void SaveDatabase(string databaseName, string json)
        {

            SaveDatabase(databaseName, json, null);
        }

        public void SaveDatabase(string databaseName, string json, System.Action callBack)
        {
            if (_firebaseApp == null) return;
            var uid = GameData.Instance.GetUserId();
            FirebaseFirestore db = FirebaseFirestore.GetInstance(_firebaseApp);
            DocumentReference docRef = db.Collection(databaseName).Document(uid);
            Dictionary<string, object> jsonData = new Dictionary<string, object>
            {
                { uid, json }
            };
            docRef.SetAsync(jsonData).ContinueWithOnMainThread(task =>
            {
                DataCore.Debug.Log("Added data to the LA document in the cities collection.");
            });

        }
#endif

    }
}
