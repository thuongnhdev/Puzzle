using EventDispatcher;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using DataCore;

public class ShareUIManager : MonoBehaviour
{
    public static event System.Action<float, Action> OnUpdateInk = delegate { };
    public static event System.Action OnPlayAnimInkIcon = delegate { };

    public static ShareUIManager Instance;

    public Action OnBackBtnTap;

    public SceneSharedEle CurSceneSharedEle
    {
        get { return _curSceneSharedEle; }
    }

    [Header("CurrencyUI")]
    [SerializeField] private Image inkTopCurrency;
    [SerializeField] private NumberCounter txtNumberCounter;

    [Header("Share UI")]
    [SerializeField] private GameObject coinBar;
    //[SerializeField] private GameObject settingBtn;
    [SerializeField] private GameObject backBtn;
    [SerializeField] private GameObject coinAnchor;

    public Vector3 InkTopCurrencyIconPos
    {
        get { return inkTopCurrency.transform.position; }
    }


    private UIManager _uiManager;

    private SceneSharedEle _baseShowEle;
    private SceneSharedEle _curSceneSharedEle;
    private bool _isInit = false;

    public void Init()
    {


        if (_isInit)
        {
            return;
        }
        Instance = this;
        UpdateCurrencyData(0);
        _isInit = true;
        _uiManager = UIManager.Instance;

    }

    public void ShowSharedUI(SceneSharedEle ele, bool isActive = true)
    {
    }

    public void UpdateCurrencyData(float animDuration, Action onComplete = null)
    {
        OnUpdateInk?.Invoke(animDuration, onComplete);

    }

    public void PlayAnimInkIcon()
    {
        OnPlayAnimInkIcon?.Invoke();
    }

    public void ShowBaseSharedUI()
    {
        ShowSharedUI(_baseShowEle);
    }

    public void SetBaseSharedUIEle(SceneSharedEle ele)
    {
        _baseShowEle = ele;
        ShowSharedUI(ele);
    }

    public void OnEnableCoin(bool isActive)
    {
        coinBar.SetActive(isActive);
        if (isActive) UpdateCurrencyData(0);
    }
    public void ForceActiveSharedUI(SceneSharedEle ele, bool isActive)
    {
        //if ((ele & SceneSharedEle.COIN) != 0)
        //{
        //    coinBar.SetActive(isActive);
        //}

        //if ((ele & SceneSharedEle.SETTING) != 0)
        //{
        //    settingBtn.SetActive(isActive);
        //}

        if ((ele & SceneSharedEle.BACK) != 0)
        {
            backBtn.SetActive(isActive);
        }
    }

    public void OnBackBtnPress()
    {
        SoundController.Instance.PlaySfxClick();
        OnBackBtnTap?.Invoke();
    }

    public void OnSettingBtnPress()
    {
        SoundController.Instance.PlaySfxClick();
        _uiManager.ShowSetting();
    }

    public void OnAddCoinPress()
    {
        SoundController.Instance.PlaySfxClick();       
        _uiManager.ShowShop();
    }

    public Vector3 GetDiamondAnchorPos()
    {
        return coinAnchor.transform.position;
    }
}
