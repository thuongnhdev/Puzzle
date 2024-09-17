using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using com.F4A.MobileThird;

using DataCore;

public class UIPushNotiPermission : BasePanel
{
    [SerializeField] private RectTransform titlePushNoti;

    private Action _onCompletePushPermission;
    public override void Init()
    {

    }

    public override void SetData(object[] data)
    {
        base.SetData(data);
        if (data.Length >= 1)
        {
            _onCompletePushPermission = (Action)data[0];
        }
    }


    public override void Open()
    {
        GameData.Instance.SavedPack.SaveData.IsShowPushNotiPermission = true;
        base.Open();
        //Check if device ios xsmax
#if UNITY_IOS
        if (SystemInfo.deviceModel.Contains("XS"))
        {
            var defineSpace = -290.0f;
            titlePushNoti.anchoredPosition = new Vector2(titlePushNoti.anchoredPosition.x, defineSpace);
        }
#endif
#if UNITY_ANDROID
        DOVirtual.DelayedCall(2f, () =>
        {
            FirebaseManager.Instance.RequestPermission(() =>
            {
            });
        });
#endif
    }

    public void OnCompletePushPermission(Action onComplete = null)
    {
        _onCompletePushPermission = onComplete;
    }
    public override void Close()
    {
        base.Close();
    }

    public void OnBtnContinuePress()
    {
        Close();
#if UNITY_ANDROID
        CompletePermission();
#elif UNITY_IOS
        DOVirtual.DelayedCall(0.1f, () =>
        {
            FirebaseManager.Instance.RequestPermission(() =>
            {
                PlayerPrefs.SetInt(ConfigManager.TokenSent, 1);
                PlayerPrefs.Save();
                CompletePermission();
            });
        });
#endif
    }

    void CompletePermission()
    {
        if (GameManager.Instance.GetStepGame() == StepGameConstants.PushNotificationPermission)
            GameManager.Instance.SetStepGame(StepGameConstants.PlayPuzzleTwo);

        _onCompletePushPermission?.Invoke();
    }

}
