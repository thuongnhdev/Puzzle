using System;
using System.Collections;
using System.Collections.Generic;
using com.F4A.MobileThird;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILogin : BasePanel
{
    [SerializeField] private NumberCounter txtNumberCounter;
    [SerializeField] private GameObject objAndroidButton;
    [SerializeField] private GameObject objIosButton;
    private bool _isLogin = false;
    public override void Init()
    {

    }

    public override void SetData(object[] data)
    {
        base.SetData(data);

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

    #region FaceBook
    public void FBLogin()
    {
        if (_isLogin) return;
        SoundController.Instance.PlaySfxClick();
        SocialManager.Instance.LoginFacebook();
        _isLogin = true;
    }

    public void FBLogout()
    {
        SoundController.Instance.PlaySfxClick();
        SocialManager.Instance.LogoutFB();
    }

    #endregion

    #region Google
    public void SignInWithGoogle()
    {
        if (_isLogin) return;
        SoundController.Instance.PlaySfxClick();
        GoogleSignInManager.Instance.SignInWithGoogle();
        _isLogin = true;
    }

    public void SignOutWithGoogle()
    {
        SoundController.Instance.PlaySfxClick();
        GoogleSignInManager.Instance.SignOutFromGoogle();
    }
    #endregion

    #region Apple
    public void SignInWithApple()
    {
        if (_isLogin) return;
        SoundController.Instance.PlaySfxClick();
        AppleSignInManager.Instance.SignInWithAppleButtonPressed();
        _isLogin = true;
    }


    #endregion
    public override void Open()
    {
        objIosButton.SetActive(false);
        objAndroidButton.SetActive(false);

#if UNITY_IOS || UNITY_EDITOR
        objIosButton.SetActive(true);
#else
        objAndroidButton.SetActive(true);
#endif

        UpdateCurrencyData(0);

        _isLogin = false;
        base.Open();

    }

    public void BtnContinuePress()
    {
        if (_isLogin) return;
        SoundController.Instance.PlaySfxClick();
        Close();
        this.PostEvent(EventID.BackBookDetail);
    }

    public override void Close()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
    }
    private void OnEnable()
    {
        DataCore.Debug.Log("UILogin OnEnable", false);
        FirebaseManager.OnLoginFacebookCompleted += FirebaseManager_OnLoginCompleted;
        GoogleSignInManager.OnLoginGoogleCompleted += FirebaseManager_OnLoginCompleted;
        AppleSignInManager.OnLoginAppleCompleted += FirebaseManager_OnLoginCompleted;
    }

    private void OnDisable()
    {
        DataCore.Debug.Log("UIManager OnDisable", false);
        FirebaseManager.OnLoginFacebookCompleted -= FirebaseManager_OnLoginCompleted;
        GoogleSignInManager.OnLoginGoogleCompleted -= FirebaseManager_OnLoginCompleted;
        AppleSignInManager.OnLoginAppleCompleted -= FirebaseManager_OnLoginCompleted;
    }

    private void FirebaseManager_OnLoginCompleted(bool success, string error, string userId)
    {
        _isLogin = false;

        if (success && !string.IsNullOrEmpty(userId))
        {
            GameData.Instance.SetUserId(userId);
            PlayerPrefs.SetInt(ConfigManager.LoginSuccess, 1);
            PlayerPrefs.Save();
            GameData.Instance.LoadOnlineConfig(userId, true, () =>
            {
                UIManager.Instance.UISetting.RefreshUILoginButton();
                UIManager.Instance.HideUILogin();
                UIManager.Instance.ShowUILoginSuccess(userId);
            });
        }
        else
        {
            UIManager.Instance.ShowPopupNotice(ConfigManager.MsgShowLoginFail);
        }
    }
}
