using System;
using System.Collections;
using System.Collections.Generic;
using com.F4A.MobileThird;
using DataCore;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHomepage : BasePanel
{
    [Header("Main")]
    [SerializeField] private HomepageTab[] tabs;
    [SerializeField] private TabIcon[] tabIcons;
    [SerializeField] private Color32[] tabTitleColors;
    [SerializeField] private NumberCounter txtNumberCounter;
    [SerializeField] private GameObject objInk;
    [SerializeField] private GameObject objSetting;
    [SerializeField] private Image inkTopCurrency;

    public Vector3 InkTopCurrencyIconPos
    {
        get { return inkTopCurrency.transform.position; }
    }

    public bool DidInit { get => _DidInit; }

    [System.Serializable]
    private struct TabIcon
    {
        public Image IconImg;
        public TextMeshProUGUI Title;
        public Sprite[] IconSprites;
    }

    public int _curId;

    private bool _DidInit = false;
    
    public override void Init()
    {
        base.Init();
        _DidInit = true;
        for (int i = 0; i < tabs.Length; i++)
        {
            tabs[i].Init();
        }

        SetState(0);
    }

    public override void Open()
    {
        if (_isOpen)
        {
            return;
        }
        GameManager.Instance.AddObjList(this);
        base.Open();
        SetState(0);        
        UpdateCurrencyData(0);
    }

    public void OpenBackHome()
    {
        if (_isOpen)
        {
            return;
        }
        DataCore.Debug.Log($"OpenBackHome. Current index: {_curId}", false);
        GameManager.Instance.AddObjList(this);
        base.Open();
        SetState(_curId);
        UpdateCurrencyData(0);
    }

    public void ResetUI() {
        DataCore.Debug.Log($"ResetUI. Current index: {_curId}", false);
        GameManager.Instance.AddObjList(this);
        SetCurrentTab(_curId, true);
    }

    public override void OnCloseManual()
    {
        base.OnCloseManual();

        if (GameManager.Instance.objListOpen.Count <= 1)
        {
            if (GameManager.Instance.objListOpen.Contains(this) && !UIManager.Instance.UIPuzzleCompleted.isActiveAndEnabled)
            {
                UIManager.Instance.ShowUiQuit(() =>
                {
                    GameManager.Instance.RemoveObjList(this);
                }, PopupQuit.TypeExit.HomePage);
            }
            else
                GameManager.Instance.IsCloseEscape = false;
        }
        else
            GameManager.Instance.IsCloseEscape = false;
    }
    public void SetState(int state)
    {
        SetCurrentTab(state);
    }

    public void SetCurrentTab(int state, bool forceRefresh = false)
    {
        try
        {            
            _curId = state;
            DataCore.Debug.Log($"UIHomepage SetState. Current index: {_curId}", false);

            for (int i = 0; i < tabs.Length; i++)
            {
                if (i == state)
                {
                    var tab = tabs[i];
                    if (!tab.DidInit) {
                        tab.Init();
                    }
                    tab.Show(forceRefresh);
                    tabIcons[i].IconImg.sprite = tabIcons[i].IconSprites[1];
                    tabIcons[i].Title.color = tabTitleColors[1];
                }
                else
                {
                    tabs[i].Hide();

                    tabIcons[i].IconImg.sprite = tabIcons[i].IconSprites[0];
                    tabIcons[i].Title.color = tabTitleColors[0];
                }
            }
        }
        catch (Exception ex)
        {

        }

    }
    public override void OnUpdateInk(float animDuration, Action onComplete = null)
    {
        UpdateCurrencyData(animDuration, onComplete);
    }
    public override void OnPlayAnimInkIcon()
    {
        inkTopCurrency.transform.DOPunchScale(Vector3.one, 0.25f, 1, 0.1f);
    }
    public void UpdateCurrencyData(float animDuration, Action onComplete = null)
    {
        int newData = GameData.Instance.SavedPack.SaveData.Coin;
        txtNumberCounter.PlayAnim(newData, animDuration, onComplete);
    }
}
