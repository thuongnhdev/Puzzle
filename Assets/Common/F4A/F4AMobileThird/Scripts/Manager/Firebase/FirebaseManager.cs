namespace com.F4A.MobileThird
{
    using System;
#if DEFINE_FIREBASE_ANALYTIC || DEFINE_FIREBASE_CRASHLYTIC || DEFINE_FIREBASE_MESSAGING
    using Firebase;
#endif
    using UnityEngine;

    [System.Serializable]
    public class DMCFirebaseInfo
    {
        [SerializeField]
        private bool _isEnableFirebase;
        public bool IsEnableFirebase
        {
            get { return _isEnableFirebase; }
            set { _isEnableFirebase = value; }
        }


        [SerializeField]
        private string _urldatabase;
        public string Urldatabase
        {
            get { return _urldatabase; }
            set { _urldatabase = value; }
        }

        [SerializeField]
        private string _androidAppId;
        public string AndroidAppId
        {
            get { return _androidAppId; }
            set { _androidAppId = value; }
        }

        [SerializeField]
        private string _iosAppId;
        public string IosAppId
        {
            get { return _iosAppId; }
            set { _iosAppId = value; }
        }
    }

    public partial class FirebaseManager : SingletonMono<FirebaseManager>
    {
        // success, error
        public static event System.Action<bool, string, string> OnLoginFacebookCompleted = delegate { };
        //public static event System.Action<bool> OnLogoutFacebookCompleted = delegate { };

        public Firebase.Auth.FirebaseAuth auth;
        protected Firebase.Auth.FirebaseAuth otherAuth;

        //[SerializeField]
        private bool _firebaseInitialized = false;
        public bool FirebaseInitialized
        {
            get { return _firebaseInitialized; }
            set { _firebaseInitialized = value; }
        }

        private bool _firebaseCheckedDependencies = false;

        public bool FirebaseCheckedDependencies
        {
            get { return _firebaseCheckedDependencies; }
            set { _firebaseCheckedDependencies = value; }
        }

        [SerializeField]
        private DMCFirebaseInfo _firebaseInfo;
        public DMCFirebaseInfo FirebaseInfo
        {
            get { return _firebaseInfo; }
            set { _firebaseInfo = value; }
        }


#if DEFINE_FIREBASE_ANALYTIC || DEFINE_FIREBASE_CRASHLYTIC || DEFINE_FIREBASE_MESSAGING
        private DependencyStatus _dependencyStatus;
        private FirebaseApp _firebaseApp;
#endif

        private void Start()
        {
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {

#if DEFINE_FIREBASE_ANALYTIC || DEFINE_FIREBASE_CRASHLYTIC || DEFINE_FIREBASE_MESSAGING || DEFINE_FIREBASE_AUTH
            //_firebaseApp = FirebaseApp.DefaultInstance;
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                _dependencyStatus = task.Result;
                Debug.Log("@LOG Enabling data collection " + _dependencyStatus);
                if (_dependencyStatus == DependencyStatus.Available && task.IsCompleted)
                {
#if UNITY_EDITOR
#if UNITY_ANDROID
                    _firebaseApp = FirebaseApp.Create(new AppOptions() { AppId = _firebaseInfo.AndroidAppId });
#else
                    _firebaseApp = FirebaseApp.Create(new AppOptions() { AppId = _firebaseInfo.IosAppId });
#endif
#else
                    _firebaseApp = FirebaseApp.DefaultInstance;
#endif


                    Dispatcher.Instance.Invoke(() =>
                    {
                        if (_firebaseApp != null)
                        {
                            Debug.Log("@LOG _firebaseApp is not null");
#if DEFINE_FIREBASE_ANALYTIC
                            try
                            {
                                InitializeAnalytics();
                            }
                            catch { }
#endif
#if DEFINE_FIREBASE_CRASHLYTIC
                            try
                            {
                                InitializeCrashlytics();
                            }
                            catch { }
#endif
#if DEFINE_FIREBASE_MESSAGING
                            try
                            {
                                InitializeMessaging();
                            }
                            catch { }
#endif
#if DEFINE_FIREBASE_STORAGE
                            try
                            {
                                InitializeStorage();
                            }
                            catch { }
#endif
#if DEFINE_FIREBASE_REMOTECONFIG
                            try
                            {
                                InitializeRemote();
                            }
                            catch { }
#endif
#if DEFINE_FIREBASE_AUTH
                            try
                            {
                                InitializeAuth();
                            }
                            catch { }
#endif


                            _firebaseInitialized = true;
                        }
                        else
                        {
                            _firebaseInitialized = false;
                            Debug.Log("@LOG _firebaseApp is null");
                        }
                        _firebaseCheckedDependencies = true;
                    });

                }
                else
                {
                    _firebaseInitialized = false;
                    _firebaseApp = null;
                    _firebaseCheckedDependencies = true;
                    Debug.Log("@LOG Could not resolve all Firebase dependencies: " + _dependencyStatus);
                }
                
            });
#endif
        }

#if !DEFINE_FIREBASE_AUTH
        public void SaveDatabase(string databaseName, string json)
        {
        }

        public void SaveDatabase(string databaseName, string json, System.Action callBack)
        {

        }

        public void GetUserData(string databaseName, System.Action<bool, object> callBack)
        {

        }

        public void GetUserData(string databaseName, string uid, System.Action<bool, object> callBack)
        {

        }
#endif
    }
}