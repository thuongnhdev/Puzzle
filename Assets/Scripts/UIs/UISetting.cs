using DataCore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using com.F4A.MobileThird;
using EventDispatcher;

public class UISetting : BasePanel
{
    [SerializeField] private Image disableSoundImg;
    [SerializeField] private TextMeshProUGUI txtSound;
    [SerializeField] private Image disableMusicImg;
    [SerializeField] private TextMeshProUGUI txtMusic;
    [SerializeField] private Image disableHapticImg;
    [SerializeField] private TextMeshProUGUI txtHaptic;
    [SerializeField] private Image disableDescriptionImg;
    [SerializeField] private TextMeshProUGUI txtDescrition;
    [SerializeField] private TextMeshProUGUI versionTxt;
    [SerializeField] private TextMeshProUGUI signInTxt;
    [SerializeField] private TextMeshProUGUI signOutTxt;

    [SerializeField] private PopupLanguage popupLanguage;
    [SerializeField] private PopupRedeemCode popupRedeemCode;
    [SerializeField] private GameObject RedeemCodeButton;

    private IRateService rateService;

    public override void Init()
    {
        base.Init();

        popupLanguage.Init();
        popupRedeemCode.Init();
        InitUi();

    }

    private void InitUi()
    {

        disableSoundImg.enabled = !GameData.Instance.SavedPack.SaveData.IsSoundActive;
        disableMusicImg.enabled = !GameData.Instance.SavedPack.SaveData.IsMusicActive;
        disableHapticImg.enabled = !GameData.Instance.SavedPack.SaveData.IsHapticActive;
        disableDescriptionImg.enabled = !GameData.Instance.SavedPack.SaveData.IsDescriptionActive;

        txtSound.text = "Sound " + (GameData.Instance.SavedPack.SaveData.IsSoundActive ? "On" : "Off");
        txtMusic.text = "Music " + (GameData.Instance.SavedPack.SaveData.IsMusicActive ? "On" : "Off");
        txtHaptic.text = "Haptic " + (GameData.Instance.SavedPack.SaveData.IsHapticActive ? "On" : "Off");
        txtDescrition.text = "Description " + (GameData.Instance.SavedPack.SaveData.IsDescriptionActive ? "On" : "Off");

        versionTxt.SetText("Version " + Application.version);

        RefreshUILoginButton();

#if UNITY_IOS
        RedeemCodeButton.SetActive(false);
#endif
    }
    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
        InitUi();
    }

    public void RefreshUILoginButton() {
        signInTxt.gameObject.SetActive(false);
        signOutTxt.gameObject.SetActive(false);
        if (!GameData.Instance.IsUserLogin())
            signInTxt.gameObject.SetActive(true);
        else
            signOutTxt.gameObject.SetActive(true);
    }

    public void InitialRateService()
    {
#if UNITY_ANDROID
        rateService = new GoogleRateService();
        rateService.Initialize();
#elif UNITY_IOS
        rateService = new AppleRateService();
        rateService.Initialize();
#endif
    }

    public void OnSoundTap()
    {
        if (GameData.Instance.SavedPack.SaveData.IsSoundActive)
            SoundController.Instance.PlaySfxClick();

        GameData.Instance.SavedPack.SaveData.IsSoundActive = !GameData.Instance.SavedPack.SaveData.IsSoundActive;
        disableSoundImg.enabled = !GameData.Instance.SavedPack.SaveData.IsSoundActive;
        txtSound.text = "Sound " + (GameData.Instance.SavedPack.SaveData.IsSoundActive ? "On" : "Off");
        GameData.Instance.RequestSaveGame();
    }

    public void OnMusicTap()
    {
        SoundController.Instance.PlaySfxClick();

        GameData.Instance.SavedPack.SaveData.IsMusicActive = !GameData.Instance.SavedPack.SaveData.IsMusicActive;
        disableMusicImg.enabled = !GameData.Instance.SavedPack.SaveData.IsMusicActive;
        txtMusic.text = "Music " + (GameData.Instance.SavedPack.SaveData.IsMusicActive ? "On" : "Off");
        if (!GameData.Instance.SavedPack.SaveData.IsMusicActive)
        {
            SoundController.Instance.StopBGMusic();
        }
        else
        {
            SoundController.Instance.PlayMainBackgroundMusic();
        }
        GameData.Instance.RequestSaveGame();
    }

    public void OnHapticTap()
    {
        SoundController.Instance.PlaySfxClick();
        GameData.Instance.SavedPack.SaveData.IsHapticActive = !GameData.Instance.SavedPack.SaveData.IsHapticActive;
        disableHapticImg.enabled = !GameData.Instance.SavedPack.SaveData.IsHapticActive;
        txtHaptic.text = "Haptic " + (GameData.Instance.SavedPack.SaveData.IsHapticActive ? "On" : "Off");
        GameData.Instance.RequestSaveGame();
    }

    public void OnDescriptionTap()
    {
        SoundController.Instance.PlaySfxClick();
        GameData.Instance.SavedPack.SaveData.IsDescriptionActive = !GameData.Instance.SavedPack.SaveData.IsDescriptionActive;
        disableDescriptionImg.enabled = !GameData.Instance.SavedPack.SaveData.IsDescriptionActive;
        txtDescrition.text = "Description " + (GameData.Instance.SavedPack.SaveData.IsDescriptionActive ? "On" : "Off");
        GameData.Instance.RequestSaveGame();
    }

    public void OnSupportTap()
    {
        SoundController.Instance.PlaySfxClick();
        SocialManager.Instance.SendMail();
    }

    public void OnLanguageTap()
    {
        SoundController.Instance.PlaySfxClick();
        popupLanguage.Open();
    }

    public void OnRateUsTap()
    {
        DataCore.Debug.Log("OnRateUsTap");
        SoundController.Instance.PlaySfxClick();

#if UNITY_ANDROID
         if (rateService == null) {
            InitialRateService();            
        }
        rateService.ShowReviewInApp(true);
#elif UNITY_IOS
        SocialManager.Instance.OpenRateGame();
#endif
    }

    public void OnPrivacyAndTermTap()
    {
        SoundController.Instance.PlaySfxClick();
        SocialManager.Instance.OpenWebsite(ConfigManager.UrlMoreInfo);
    }

    public void OnInkDropShopTap()
    {
        SoundController.Instance.PlaySfxClick();
        UIManager.Instance.ShowShop();
    }

    public void OnCreditsAndFAQTap()
    {
        SoundController.Instance.PlaySfxClick();
        UIManager.Instance.ShowUICredit();
    }

    public void OnRedeemCodeTap()
    {
        SoundController.Instance.PlaySfxClick();
        popupRedeemCode.Open();
    }

    public void OnRestoreProgressTap()
    {
        SoundController.Instance.PlaySfxClick();        
        IAPManager.Instance.RestorePurchases((result)=> {
            if (result)
                UIManager.Instance.ShowPopupNotice(ConfigManager.MSG_RESTORE_CODE_SUCCESS);
            else
                UIManager.Instance.ShowPopupNotice(ConfigManager.MSG_RESTORE_CODE_FAILED);
        });
    }

    public void OnSignInTap()
    {
        SoundController.Instance.PlaySfxClick();

        if (!GameData.Instance.IsUserLogin()) {
            UIManager.Instance.ShowUILogin();
        }        
        else
        {
            UIManager.Instance.LogOut();
            PlayerPrefs.SetInt(ConfigManager.LoginSuccess, 0);
            PlayerPrefs.Save();
            RefreshUILoginButton();
        }
    }
    public override void Close()
    {
        GameManager.Instance.RemoveObjList(this);
        base.Close();
    }
}
