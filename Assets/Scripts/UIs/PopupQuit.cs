using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupQuit : BasePopup
{
    public enum TypeExit
    {
        InGame = 0,
        HomePage = 1,
    }
    private TypeExit typeExit;
    public override void Init()
    {
        base.Init();
    }

    private Action _onComplete = null;
    public override void SetData(object[] data)
    {
        base.SetData(data);
        if (data.Length >= 1)
        {
            _onComplete = (Action)data[0];
            typeExit = (TypeExit)data[1];
        }
    }

    public override void Open()
    {
        base.Open();
        isCloseTime = false;
        DOVirtual.DelayedCall(0.5f, () =>
        {
            isCloseTime = true;
        });
    }
    private bool isCloseTime = false;
    private void FixedUpdate()
    {
        if (isCloseTime && Input.GetKey(KeyCode.Escape) && GameManager.Instance.GetStepGame() >= StepGameConstants.PlayPuzzleFour)
        {
            if (UIManager.Instance.PopupQuit.isActiveAndEnabled)
            {
                base.Close();
                GameManager.Instance.RemoveObjList(this);
            }
        }
        
    }
    public override void Close()
    {
        base.Close();
        GameManager.Instance.RemoveObjList(this);
    }

    public void OnClickQuit()
    {
        SoundController.Instance.PlaySfxClick();
        Close();

        if (UIManager.Instance.UIIntro.isActiveAndEnabled)
        {
            UIManager.Instance.UIIntro.Close();
            _onComplete?.Invoke();
            return;
        }
      
        if (typeExit == TypeExit.InGame)
            _onComplete?.Invoke();
        else
            Application.Quit();

    }
}
