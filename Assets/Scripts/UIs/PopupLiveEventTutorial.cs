using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using com.F4A.MobileThird;
using DanielLochner.Assets.SimpleScrollSnap;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupLiveEventTutorial : BasePanel
{
    public enum TypeLiveTutorial
    {
        LiveEventClick = 0,
        LiveEventOpen = 1,
        LiveEventClock = 2,
        LiveEventGift = 3,
    }
    [SerializeField] private SimpleScrollSnap simpleScrollSnap;

    [SerializeField] private GameObject[] panelLiveEvent;
    private TypeLiveTutorial _typeLiveTutorial = TypeLiveTutorial.LiveEventClick;
    public override void Init()
    {
    }

    private Action _onComplete = null;
    public override void SetData(object[] data)
    {
        base.SetData(data);
        if (data.Length >= 2)
        {
            _typeLiveTutorial = (TypeLiveTutorial)data[0];

            _onComplete = (Action)data[1];
        }
        for (var i = 0; i < panelLiveEvent.Length; i++)
        {
            panelLiveEvent[i].SetActive(false);
        }
        panelLiveEvent[(int)_typeLiveTutorial].SetActive(true);
    }


    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
        for (var i = 0; i < panelLiveEvent.Length; i++)
        {
            if (simpleScrollSnap != null) simpleScrollSnap.AddToBack(panelLiveEvent[i]);
        }
        simpleScrollSnap.GoToPanel((int)_typeLiveTutorial);
    }

    public void BtnContinuePress()
    {
        SoundController.Instance.PlaySfxClick();
        _onComplete?.Invoke();
        _onComplete = null;
        simpleScrollSnap.GoToNextPanel();

    }

    public void PanelChange()
    {
        int _curDot = simpleScrollSnap.CurrentPanel;
        _typeLiveTutorial = (TypeLiveTutorial)_curDot;
        for (var i = 0; i < panelLiveEvent.Length - 1; i++)
        {
            if (panelLiveEvent[i].activeInHierarchy) panelLiveEvent[i].SetActive(false);
        }
        if (_typeLiveTutorial < TypeLiveTutorial.LiveEventGift)
        {
            panelLiveEvent[(int)_typeLiveTutorial].SetActive(true);
        }
        else
            Close();
    }

    public override void Close()
    {
        SoundController.Instance.PlaySfxClick();
        base.Close();
        _onComplete?.Invoke();
        _onComplete = null;

        GameManager.Instance.RemoveObjList(this);
    }

}
