using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using UnityEngine;
using UnityEngine.UI;
using DataCore;

namespace com.F4A.MobileThird
{
    public class GoogleSignInManager : SingletonMono<GoogleSignInManager>
    {
        // success, error
        public static event System.Action<bool, string, string> OnLoginGoogleCompleted = delegate { };
        public static event System.Action<bool> OnLogoutGoogleCompleted = delegate { };

        private GoogleSignInConfiguration configuration;

        public void InitializeGoogle(FirebaseAuth firebaseAuth)
        {
            if (configuration == null)
            {
                configuration = new GoogleSignInConfiguration
                {
                    WebClientId = ConfigManager.WebClientId,
                    RequestIdToken = true
                };
            }
        }


        public void SignInWithGoogle() { OnSignIn(); }
        public void SignOutFromGoogle() { OnSignOut(); }

        private void OnSignIn()
        {
            try
            {
                if (configuration == null)
                {
                    configuration = new GoogleSignInConfiguration
                    {
                        WebClientId = ConfigManager.WebClientId,
                        RequestIdToken = true,
                        ForceTokenRefresh = true
                    };
                }

                AddToInformation("OnSignIn");
                GoogleSignIn.Configuration = configuration;
                GoogleSignIn.Configuration.UseGameSignIn = false;
                GoogleSignIn.Configuration.RequestIdToken = true;
                AddToInformation("Calling SignIn");

                GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
            }
            catch (Exception ex)
            {
                Dispatcher.Instance.Invoke(() =>
                {
                    OnLoginGoogleCompleted?.Invoke(false, "Sign In fail", "0");
                    AddToInformation("Calling SignIn " + ex);
                });

            }

        }

        private void OnSignOut()
        {
            AddToInformation("Calling SignOut");
            GoogleSignIn.DefaultInstance.SignOut();
        }

        public void OnDisconnect()
        {
            AddToInformation("Calling Disconnect");
            GoogleSignIn.DefaultInstance.Disconnect();
        }

        internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
        {
            try
            {
                if (task.IsFaulted)
                {
                    using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                        {
                            GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                            AddToInformation("Got Error: " + error.Status + " " + error.Message);
                        }
                        else
                        {
                            AddToInformation("Got Unexpected Exception?!?" + task.Exception);
                        }
                    }
                    Dispatcher.Instance.Invoke(() =>
                    {
                        OnLoginGoogleCompleted?.Invoke(false, "Sign In fail", "0");
                    });
                }
                else if (task.IsCanceled)
                {
                    AddToInformation("Canceled");
                    Dispatcher.Instance.Invoke(() =>
                    {
                        OnLoginGoogleCompleted?.Invoke(false, "Sign In fail", "0");
                    });
                }
                else
                {
                    AddToInformation("Welcome: " + task.Result.DisplayName + "!");
                    AddToInformation("Email = " + task.Result.Email);
                    AddToInformation("Google ID Token = " + task.Result.IdToken);
                    SignInWithGoogleOnFirebase(task.Result.IdToken);
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Instance.Invoke(() =>
                {
                    OnLoginGoogleCompleted?.Invoke(false, "Sign In fail", "0");
                });
                AddToInformation("Calling SignIn " + ex);
            }

        }

        private void SignInWithGoogleOnFirebase(string idToken)
        {
            try
            {
                Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
                if (credential == null)
                {
                    Dispatcher.Instance.Invoke(() =>
                    {
                        OnLoginGoogleCompleted?.Invoke(false, "Sign In fail", "0");
                    });
                    return;
                }

                FirebaseManager.Instance.auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
                {
                    AggregateException ex = task.Exception;
                    if (ex != null)
                    {
                        Dispatcher.Instance.Invoke(() =>
                        {
                            OnLoginGoogleCompleted?.Invoke(false, "Sign In fail", "0");
                        });
                        if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                            AddToInformation("\nError code = " + inner.ErrorCode + " Message = " + inner.Message);
                    }
                    else
                    {
                        AddToInformation("Sign In Successful.");
                        GameData.Instance.SetLoginType("Google");
                        Dispatcher.Instance.Invoke(() =>
                        {
                            OnLoginGoogleCompleted?.Invoke(true, "Sign In Successful", task.Result.UserId);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Instance.Invoke(() =>
                {
                    OnLoginGoogleCompleted?.Invoke(false, "Sign In fail", "0");
                });
                AddToInformation("Calling SignIn " + ex);
            }

        }

        public void OnSignInSilently()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
            AddToInformation("Calling SignIn Silently");

            GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
        }

        public void OnGamesSignIn()
        {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = true;
            GoogleSignIn.Configuration.RequestIdToken = false;

            AddToInformation("Calling Games SignIn");

            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
        }

        private void AddToInformation(string str)
        {
            DataCore.Debug.Log(str, false);
        }
    }
}