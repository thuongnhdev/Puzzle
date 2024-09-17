using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
using UnityEngine;
using DataCore;
using Firebase;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace com.F4A.MobileThird
{
    public class AppleSignInManager : SingletonMono<AppleSignInManager>
    {
        // success, error
        public static event System.Action<bool, string, string> OnLoginAppleCompleted = delegate { };
        public static event System.Action<bool> OnLogoutAppleCompleted = delegate { };

        private const string AppleUserIdKey = "AppleUserId";

        private IAppleAuthManager _appleAuthManager;


        public void InitializeApple(FirebaseAuth firebaseAuth)
        {
#if !UNITY_EDITOR
            try
            {
                DataCore.Debug.Log("InitializeApple");

                // If the current platform is supported
                if (AppleAuthManager.IsCurrentPlatformSupported)
                {
                    // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
                    var deserializer = new PayloadDeserializer();
                    // Creates an Apple Authentication manager with the deserializer
                    this._appleAuthManager = new AppleAuthManager(deserializer);
                }
            }
            catch (Exception ex)
            {
                AddToInformation("Exception " + ex);
            }
#endif
        }

        private void Update()
        {
            // Updates the AppleAuthManager instance to execute
            // pending callbacks inside Unity's execution loop
            if (this._appleAuthManager != null)
            {
                this._appleAuthManager.Update();
            }

        }

        public void SignInWithAppleButtonPressed() { this.SignInWithApple(); }

        private void InitializeLoginMenu()
        {
            // Check if the current platform supports Sign In With Apple
            if (this._appleAuthManager == null)
                return;
            DataCore.Debug.Log("InitializeLoginMenu");

            // If at any point we receive a credentials revoked notification, we delete the stored User ID, and go back to login
            this._appleAuthManager.SetCredentialsRevokedCallback(result =>
            {
                AddToInformation("Received revoked callback " + result);
                CPlayerPrefs.DeleteKey(AppleUserIdKey);
            });

            // If we have an Apple User Id available, get the credential status for it
            if (CPlayerPrefs.HasKey(AppleUserIdKey))
            {
                var storedAppleUserId = CPlayerPrefs.GetString(AppleUserIdKey);
                this.CheckCredentialStatusForUserId(storedAppleUserId);
            }
            // If we do not have an stored Apple User Id, attempt a quick login
            else
            {
                SignInWithApple();
            }
        }

        private void CheckCredentialStatusForUserId(string appleUserId)
        {
            // If there is an apple ID available, we should check the credential state
            this._appleAuthManager.GetCredentialState(
                appleUserId,
                state =>
                {
                    switch (state)
                    {
                        // If it's authorized, login with that user id
                        case CredentialState.Authorized:
                            this.AttemptQuickLogin();
                            return;

                        // If it was revoked, or not found, we need a new sign in with apple attempt
                        // Discard previous apple user id
                        case CredentialState.Revoked:
                        case CredentialState.NotFound:
                            CPlayerPrefs.DeleteKey(AppleUserIdKey);
                            SignInWithApple();
                            return;
                    }
                },
                error =>
                {
                    var authorizationErrorCode = error.GetAuthorizationErrorCode();
                    AddToInformation("Error while trying to get credential state " + authorizationErrorCode.ToString() + " " + error.ToString());
                });
        }

        private void AttemptQuickLogin()
        {
            DataCore.Debug.Log("AttemptQuickLogin");
            var quickLoginArgs = new AppleAuthQuickLoginArgs();

            // Quick login should succeed if the credential was authorized before and not revoked
            this._appleAuthManager.QuickLogin(
                quickLoginArgs,
                credential =>
                {
                    // If it's an Apple credential, save the user ID, for later logins
                    var appleIdCredential = credential as IAppleIDCredential;
                    if (appleIdCredential != null)
                    {
                        CPlayerPrefs.SetString(AppleUserIdKey, credential.User);
                        string authCode = System.Text.Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                        string idToken = System.Text.Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                        var rawNonce = GenerateRandomString(32);
                        AuthWithApple(idToken, rawNonce);
                    }
                },
                error =>
                {
                    // If Quick Login fails, we should show the normal sign in with apple menu, to allow for a normal Sign In with apple
                    var authorizationErrorCode = error.GetAuthorizationErrorCode();
                    AddToInformation("Quick Login Failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                });
        }

        public void SignInWithApple()
        {
            var loginArgs = new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName);
            if (_appleAuthManager == null)
            {
                if (AppleAuthManager.IsCurrentPlatformSupported)
                {
                    // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
                    var deserializer = new PayloadDeserializer();
                    // Creates an Apple Authentication manager with the deserializer
                    this._appleAuthManager = new AppleAuthManager(deserializer);

                }
                AddToInformation("SignInWithApple was canceled. ");
                //OnLoginAppleCompleted?.Invoke(false, "SignInWithApple was canceled.","0");
                //return;
            }

            this._appleAuthManager.LoginWithAppleId(
                loginArgs,
                credential =>
                {
                    // If a sign in with apple succeeds, we should have obtained the credential with the user id, name, and email, save it
                    CPlayerPrefs.SetString(AppleUserIdKey, credential.User);
                    // success
                    var appleIDCredential = credential as IAppleIDCredential;
                    string authCode = System.Text.Encoding.UTF8.GetString(appleIDCredential.AuthorizationCode);
                    string idToken = System.Text.Encoding.UTF8.GetString(appleIDCredential.IdentityToken);
                    var rawNonce = GenerateRandomString(32);
                    AuthWithApple(idToken, rawNonce);
                },
                error =>
                {
                    var authorizationErrorCode = error.GetAuthorizationErrorCode();
                    AddToInformation("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " + error.ToString());
                    OnLoginAppleCompleted?.Invoke(false, "Sign in with Apple failed","0");
                });
        }

        private void AuthWithApple(string appleIdToken, string rawNonce)
        {
            Firebase.Auth.Credential credential =
    Firebase.Auth.OAuthProvider.GetCredential("apple.com", appleIdToken, rawNonce, null);
           
            if (credential == null)
            {
                Dispatcher.Instance.Invoke(() => {
                    OnLoginAppleCompleted?.Invoke(false, "SignInWithCredentialAsync was canceled.","0");
                });                
                return;
            }
            FirebaseManager.Instance.auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    AddToInformation("SignInWithCredentialAsync was canceled.");
                    Dispatcher.Instance.Invoke(() =>
                    {
                        OnLoginAppleCompleted?.Invoke(false, "SignInWithCredentialAsync was canceled.","0");
                    });
                    return;
                }
                if (task.IsFaulted)
                {
                    AddToInformation("SignInWithCredentialAsync encountered an error: " + task.Exception);
                    Dispatcher.Instance.Invoke(() =>
                    {
                        OnLoginAppleCompleted?.Invoke(false, "SignInWithCredentialAsync was canceled.","0");
                    });
                    return;
                }
                GameData.Instance.SetLoginType("Apple");
                Dispatcher.Instance.Invoke(() =>
                {
                    OnLoginAppleCompleted?.Invoke(true, "User signed in successfully" , task.Result.UserId);
                });

                Firebase.Auth.FirebaseUser newUser = task.Result;
            });
        }

        private void AddToInformation(string str)
        {
            DataCore.Debug.Log(str, false);
        }

        private static string GenerateRandomString(int length)
        {
            if (length <= 0)
            {
                DataCore.Debug.Log("Expected nonce to have positive length");
            }

            const string charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._";
            var cryptographicallySecureRandomNumberGenerator = new RNGCryptoServiceProvider();
            var result = string.Empty;
            var remainingLength = length;

            var randomNumberHolder = new byte[1];
            while (remainingLength > 0)
            {
                var randomNumbers = new List<int>(16);
                for (var randomNumberCount = 0; randomNumberCount < 16; randomNumberCount++)
                {
                    cryptographicallySecureRandomNumberGenerator.GetBytes(randomNumberHolder);
                    randomNumbers.Add(randomNumberHolder[0]);
                }

                for (var randomNumberIndex = 0; randomNumberIndex < randomNumbers.Count; randomNumberIndex++)
                {
                    if (remainingLength == 0)
                    {
                        break;
                    }

                    var randomNumber = randomNumbers[randomNumberIndex];
                    if (randomNumber < charset.Length)
                    {
                        result += charset[randomNumber];
                        remainingLength--;
                    }
                }
            }

            return result;
        }
    }
}