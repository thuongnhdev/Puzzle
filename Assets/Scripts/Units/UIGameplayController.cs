using System;
using DG.Tweening;
using System.Collections.Generic;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DataCore;
using com.F4A.MobileThird;
using Spine.Unity;
using System.Globalization;

public class UIGameplayController : BasePanel
{
    [SerializeField] ScrollObjectManager scrollObject;
    [SerializeField] Button btnHint;
    [SerializeField] Button btnReplay;
    [SerializeField] Button btnBack;
    [SerializeField] List<GameObject> ObjUI;
    [SerializeField] private ProgressBar progressBar;
    [SerializeField] private Image turorialCover;
    [SerializeField] private Image iconLightTutorial;
    [SerializeField] private SpriteRenderer borderPuzzle;
    [SerializeField] private TextMeshProUGUI txtHintNumber;
    [SerializeField] private GameObject iconAds;
    [SerializeField] private SkeletonGraphic lightHintAnimation;
    
    [Header("UI Zoom")]
    [SerializeField] private List<UIAnimation> listUIAnimation = new List<UIAnimation>();
    [SerializeField] private UIAnimation btnCloseZoom;

    [SerializeField]
    bool bol_AllObjUIHasFade = false;
    private bool isZoomState = false;

    private bool _isPlayingPuzzle = false;
    public bool IsPlayingPuzzle { get => _isPlayingPuzzle; }

    public ScrollObjectManager ScrollObject { get => scrollObject; set => scrollObject = value; }

    bool _isHint = false;
    public override void Init()
    {
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.UpdateProgressLayer, UpdateProgress);
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.OnInitPuzzleCompleted, (o) =>
            {
                borderPuzzle.enabled = true;
            }
        );

        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.OnPhysicalBack, (o) =>
        {
            if (IsOpen)
            {
                OnBackButtonClicked();
            }
        });
        
        btnHint.onClick.AddListener(() =>
        {
            //GameManager.Instance.FinishPuzzle();
            SoundController.Instance.PlaySfxClick();

            HideTutorial();
            if (GameData.Instance.SavedPack.SaveData.Hint == 0 && !GameData.Instance.IsVipIap())
            {
                OnWatchAdsTap();
            }
            else
            {
                if (didUsedHint) return;
                didUsedHint = true;
                Hint();
            }
        });

        btnReplay.onClick.AddListener(() =>
        {
            SoundController.Instance.PlaySfxClick();
            GameManager.Instance.LoadReplayPuzzle();
        });

        btnBack.onClick.AddListener(() =>
        {
            OnBackButtonClicked();
        });

        bol_AllObjUIHasFade = AddFadeControllerIntoObjUI();
        isZoomState = false;
        progressBar.Init();
        progressBar.SetActive(false);
        borderPuzzle.enabled = false;

    }

    public void OnBackButtonClicked()
    {
        DataCore.Debug.Log("OnBackButtonClicked");
        SoundController.Instance.PlaySfxClick();
        AdsService.Instance.HideBanner();
        bool isZooming = ZoomInZoomOut.Instance.Bol_Zooming;
        if (isZooming)
        {
            ZoomInZoomOut.Instance.Bol_Zooming = false;
            UIManager.Instance.UIGameplay.ShowUI();
        }

        btnBack.interactable = false;
        ZoomInZoomOut.Instance.SetToDefault(() =>
        {
            UIManager.Instance.UIGameplay.ScrollObject.AllowScroll();

            btnBack.interactable = true;
        });
        SoundController.Instance.PlayMainBackgroundMusic();
        GameManager.Instance.RemoveObjList(this);
        GameManager.Instance.ClearPuzzleAsset();
        StopShowPopupHintTask();
    }

    public void StopShowPopupHintTask() {
        if (_onShowPopupHint != null)
        {
            _onShowPopupHint.Kill();
        }
        _isShowPopupHint = false;
        _onShowPopupHint = null;
    }

    private void OnShowBtnHint()
    {
        if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
            btnHint.gameObject.SetActive(false);
        else
            btnHint.gameObject.SetActive(true);
    }
    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        UIManager.Instance.EnablePuzzleBorder(false);
        UpdateHintUI();
        
        btnBack.gameObject.SetActive(GameData.Instance.SavedPack.SaveData.IsTutorialCompleted);
        base.Open();
        _isPlayingPuzzle = true;

        ActiveAnimationHint(true);
        OnShowBtnHint();
        //Check if device ios xsmax
#if UNITY_IOS || UNITY_ANDROID
        if (IsSpecialResolution())
        {
            var defineSpace = 70.0f;
            btnBack.GetComponent<RectTransform>().offsetMax = new Vector2(btnBack.GetComponent<RectTransform>().offsetMax.x, -defineSpace);
            btnBack.GetComponent<RectTransform>().offsetMin = new Vector2(btnBack.GetComponent<RectTransform>().offsetMin.x, -defineSpace);
            btnHint.GetComponent<RectTransform>().offsetMax = new Vector2(btnHint.GetComponent<RectTransform>().offsetMax.x, -defineSpace);
            btnHint.GetComponent<RectTransform>().offsetMin = new Vector2(btnHint.GetComponent<RectTransform>().offsetMin.x, -defineSpace);
            progressBar.GetComponent<RectTransform>().offsetMax = new Vector2(progressBar.GetComponent<RectTransform>().offsetMax.x, -defineSpace);
            progressBar.GetComponent<RectTransform>().offsetMin = new Vector2(progressBar.GetComponent<RectTransform>().offsetMin.x, -defineSpace);
        }
#endif
    }

    private bool _isShowPopupHint = false;
    private Tween _onShowPopupHint;
    public void OnShowPopupHint()
    {
        if (GameData.Instance.IsVipIap()) return;
        if (_onShowPopupHint != null) return;
        if (GameManager.Instance.GetStepGame() < StepGameConstants.StepComplete) return;
        if (_isShowPopupHint) return;
        DataCore.Debug.Log($"OnShowPopupHint", false);
        _onShowPopupHint = DOVirtual.DelayedCall(ConfigManager.TimeShowPopupHint, () =>
        {
            if (!_isShowPopupHint && !UIManager.Instance.UIHomepage.isActiveAndEnabled
            && !UIManager.Instance.UILoading.isActiveAndEnabled && !UIManager.Instance.UIIntro.isActiveAndEnabled
            && AdsService.Instance.IsRewardedAdLoaded())
            {
                UIManager.Instance.ShowPopupHint();
                _isShowPopupHint = true;
            }
        });
    }

    public bool IsSpecialResolution()
    {
        return ((float)Screen.height / (float)Screen.width) >= 1.8f;
    }
    public void UpdateHintUI()
    {
        if (GameData.Instance.IsVipIap()) {
            iconAds.gameObject.SetActive(false);
            txtHintNumber.gameObject.SetActive(true);
            txtHintNumber.text = ConfigManager.keyVipNumberUntimitedHint.ToString();
        } else {
            iconAds.gameObject.SetActive(GameData.Instance.SavedPack.SaveData.Hint == 0 ? true : false);
            txtHintNumber.gameObject.SetActive(GameData.Instance.SavedPack.SaveData.Hint > 0 ? true : false);
            if (GameData.Instance.SavedPack.SaveData.Hint > 0)
                txtHintNumber.text = GameData.Instance.SavedPack.SaveData.Hint.ToString();
        }
    }

    public void ActiveAnimationHint(bool isActive)
    {
        lightHintAnimation.gameObject.SetActive(isActive);
        if (lightHintAnimation.skeletonDataAsset != null)
        {
            lightHintAnimation.Clear();
            lightHintAnimation.Initialize(true);
            lightHintAnimation.AnimationState.SetAnimation(0, "Active", true);

        }
    }

    public override void Close()
    {
        ActiveAnimationHint(false);
        AdsService.Instance.HideBanner();
        base.Close();
        borderPuzzle.enabled = false;
        ShowZoomUI(false);
        _isPlayingPuzzle = false;
        _isShowPopupHint = false;
        this.indexCount = 0;
        this.timeDelayShowhand = 0.6f;
        GameManager.Instance.RemoveObjList(this);
        UIManager.Instance.PopupHint.Close();
    }

    public override void OnCloseManual()
    {
        base.OnCloseManual();
        UIManager.Instance.ShowUiQuit(() => {
            OnButtonBackPress();
            OnBackButtonClicked();
        },PopupQuit.TypeExit.InGame);
    }

    public void SetProgressBar(int totalObj)
    {
        progressBar.SetProgress(totalObj);
    }

    public void UpdateProgress(object data)
    {
        int numComplete = (int)data;

        progressBar.UpdateProgress(numComplete);
        if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted) {
            try
            {
                var mappingTutPuzzle = new Dictionary<string, int>()
                {
                    { "Alice_6_Tut", 1 },
                    { "Peter_Pan_19_Tut", 2 },
                    { "The_Secret_Garden_8_Tut", 3 },
                };
                var puzzleName = GameManager.Instance.CurrentPuzzle.PuzzleName;
                if (mappingTutPuzzle.ContainsKey(puzzleName))
                {
                    var index = mappingTutPuzzle[puzzleName];
                    int indexCurrentLayer = GameManager.Instance.CurrentPuzzle.GetIndexLayer();

                    AnalyticManager.Instance.TrackCompletedPuzzleStepInTutorial(index, indexCurrentLayer, numComplete, (int)GameSession.Instance.SessionPlayedTime);
                }
            }
            catch 
            {

            }
        }


    }

    public void HideTutorial()
    {
        MCache.Instance.Hand.Stop();
    }

    private Sequence mySequenceShowHand;
    public void ShowTutorial()
    {
        if (isZoomState)
        {
            return;
        }

        if (indexCount < 2 && GameManager.Instance.CurrentTutorial == 1)
            this.timeDelayShowhand = 3.0f;

        mySequenceShowHand = DOTween.Sequence();
        mySequenceShowHand.SetDelay(this.timeDelayShowhand).OnComplete(() =>
        {
            if (this.timeDelayShowhand != 0)
                Tutorial();
        });

    }

    private bool isOpen = false;

    public void ShowZoomUI(bool isShow)
    {
        if (isShow)
        {
            EnableZoom(true, () =>
            {
                btnCloseZoom.Run(Status.In);
            });
        }
        else
        {
            btnCloseZoom.Run(Status.Out, () =>
            {
                EnableZoom(false);
            });
        }
    }

    private void EnableZoom(bool enable, Action onComplete = null)
    {
        isZoomState = enable;
        for (int i = 0; i < listUIAnimation.Count; i++)
        {
            listUIAnimation[i].Run(enable ? Status.Out : Status.In, onComplete);
        }
    }

    public void EnableTutorialIconLight(bool enable)
    {
        if (mySequenceShowHand != null) mySequenceShowHand.Kill();
        iconLightTutorial.enabled = enable;
    }

    public void NumberShowHintTutorial()
    {
        indexCount++;
        if (mySequenceShowHand != null) mySequenceShowHand.Kill();
        LogicShowHandTutorial(indexCount);
    }

    public void UpdateIconLightTutorial(Sprite img, Vector2 size, Vector3 position, Vector3 scale)
    {
        iconLightTutorial.rectTransform.sizeDelta = size;
        iconLightTutorial.transform.position = position;
        iconLightTutorial.sprite = img;
        iconLightTutorial.rectTransform.localScale = scale;
    }

    private Vector3 posBeginTutorial;
    void Tutorial()
    {
        var hand = MCache.Instance.Hand;
        if (hand.IsShowing || hand.IsShowingHorizontal)
        {
            return;
        }

        int len = 0;
        int indexCurrentPuzzle = 0;
        int indexCurrentLayer = GameManager.Instance.CurrentPuzzle.GetIndexLayer();
        var arrObjectInLayer = GameManager.Instance.CurrentPuzzle.Layers[indexCurrentLayer].TargetObject;

        TargetObject2D targetObject2D = null;
        string nameHint = "";
        List<TargetObject2D> TargetObject2DList = new List<TargetObject2D>();
        for (var j = 0;j < arrObjectInLayer.Length;j++)
        {
            if (!arrObjectInLayer[j].IsDoneTarget)
            {
                len++;
                TargetObject2DList.Add(arrObjectInLayer[j]);
            }
        }
       
        if (len >= 3)
        {
            indexCurrentPuzzle = UnityEngine.Random.Range(0, 3);
            targetObject2D = TargetObject2DList[indexCurrentPuzzle];
            nameHint = targetObject2D.name;
        }
        else if (len > 1)
        {
            indexCurrentPuzzle = UnityEngine.Random.Range(0, len);
            targetObject2D = TargetObject2DList[indexCurrentPuzzle];
            nameHint = targetObject2D.name;
        }
        else
        {
            targetObject2D = TargetObject2DList[indexCurrentPuzzle];
            nameHint = targetObject2D.name;
        }

        hand.IsShowing = true;

     
        string str_NameSource = GameManager.Instance.CurrentPuzzle.GetNameTarget(nameHint);
        int indexItemUI = -1;
        ObjectItem objectItemTempUI = null;
        for (int i = 0; i < scrollObject.ObjectItems.Length; i++)
        {
            if (scrollObject.ObjectItems[i].gameObject.activeSelf && scrollObject.ObjectItems[i].transform.childCount > 0)
            {
                ++indexItemUI;
                if (scrollObject.ObjectItems[i].name.ToString().Equals(str_NameSource))
                {
                    objectItemTempUI = scrollObject.ObjectItems[i];
                    posBeginTutorial = objectItemTempUI.transform.position;
                    break;
                }
            }
        }

        StartMoveHand(objectItemTempUI, targetObject2D, false, str_NameSource);
    }

    private void StartMoveHand(ObjectItem objectItemTempUI , TargetObject2D targetObject2D ,bool isCheckPos, string str_NameSource)
    {
        var hand = MCache.Instance.Hand;
        if (objectItemTempUI != null)
        {

            var dragObj = objectItemTempUI.GetComponentInChildren<DragObject>();
            dragObj.enabled = true;
            //DOVirtual.DelayedCall(0.2f, () =>
            //{
            iconLightTutorial.enabled = true;

            Vector3 posMoveBegin = objectItemTempUI.transform.position;
            if (GameManager.Instance.CurrentTutorial == 1 && isCheckPos)
            {
                var sizeList = 0;
                var indexItemUINew = -1;
                for (int i = 0; i < scrollObject.ObjectItems.Length; i++)
                {
                    if (scrollObject.ObjectItems[i].gameObject.activeSelf && scrollObject.ObjectItems[i].transform.childCount > 0)
                    {
                        var objectHint = objectItemTempUI.GetComponentInChildren<DragObject>();
                        if (objectHint != null)
                            sizeList++;
                    }
                }
                for (int i = 0; i < scrollObject.ObjectItems.Length; i++)
                {
                    if (scrollObject.ObjectItems[i].gameObject.activeSelf && scrollObject.ObjectItems[i].transform.childCount > 0)
                    {
                        ++indexItemUINew;
                        if (scrollObject.ObjectItems[i].name.ToString().Equals(str_NameSource))
                        {
                            objectItemTempUI = scrollObject.ObjectItems[i];
                            break;
                        }
                    }
                }
                posMoveBegin = objectItemTempUI.transform.position;

                if (indexItemUINew == 0 || indexItemUINew == sizeList - 1)
                    posMoveBegin = posBeginTutorial;
                if (posMoveBegin.x < 0) posMoveBegin = new Vector3(190f, posMoveBegin.y, posMoveBegin.z);
                if (posMoveBegin.x > Screen.width) posMoveBegin = new Vector3(950f, posMoveBegin.y, posMoveBegin.z);

            }

            // World -> Screen
            Vector3 v3_Screen = Camera.main.WorldToScreenPoint(targetObject2D.transform.position);

            // UI -> World
            Vector3 v3_objectItemTempUI = Camera.main.ScreenToWorldPoint(posMoveBegin);

            // World -> Screen
            v3_objectItemTempUI = Camera.main.WorldToScreenPoint(v3_objectItemTempUI);

            var imgItemChildren = objectItemTempUI.transform.GetChild(0).GetComponent<Image>();
            UpdateIconLightTutorial(imgItemChildren.sprite, imgItemChildren.rectTransform.sizeDelta, v3_objectItemTempUI, imgItemChildren.rectTransform.localScale);

            dragObjectHint = objectItemTempUI.GetComponentInChildren<DragObject>();
            if (dragObjectHint == null)
                HideTutorial();

            hand.SetPositionHint(v3_objectItemTempUI, v3_Screen, objectItemTempUI.name);

            LogicShowHandTutorial(indexCount);
            //});
        }
    }
    private int indexCount = 0;
    private float timeDelayShowhand = 0.1f;
    private void LogicShowHandTutorial(int count)
    {
        var timeDelay = 0.0f;
        var keyValue = FirebaseManager.GetValueRemote(ConfigManager.KeyTimeDelayShowHandTutorial);
        if (!string.IsNullOrEmpty(keyValue))
            timeDelay = float.Parse(keyValue, CultureInfo.InvariantCulture);

        switch (GameManager.Instance.CurrentTutorial)
        {
            case 0:
                this.timeDelayShowhand = 0.1f;
                break;
            case 1:
                {
                    if (count >= 2)
                        this.timeDelayShowhand = timeDelay;
                }
                break;
        }

    }

    bool didUsedHint = false;
    private DragObject dragObjectHint;
    private Vector3 posBegin;
    private ObjectItem objectItemTempUI = null;
    void Hint()
    {
        try
        {
            if (GameData.Instance.SavedPack == null)
                return;

            if (GameData.Instance.SavedPack.SaveData.Hint <= 0 && !GameData.Instance.IsVipIap())
                return;

            DOVirtual.DelayedCall(5.0f, () =>
            {
                didUsedHint = false;
            });

            GameData.Instance.DecreaseHint(1, ConfigManager.GameData.ResourceSpentSource.hint_puzzle);
            UpdateHintUI();
            var hand = MCache.Instance.Hand;
            if (hand.IsShowing || hand.IsShowingHorizontal)
            {
                MCache.Instance.Hand.Stop();
            }
            GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.USE_HINT, 1);
            hand.IsShowing = true;

            if (GameManager.Instance.CurrentPuzzle == null)
                return;

            int indexCurrentLayer = GameManager.Instance.CurrentPuzzle.GetIndexLayer();
            var arrObjectInLayer = GameManager.Instance.CurrentPuzzle.Layers[indexCurrentLayer].TargetObject;

            TargetObject2D targetObject2D = null;
            string nameHint = "";
            int indexTarget = 0;
            for (int i = 0; i < arrObjectInLayer.Length; i++)
            {
                if (arrObjectInLayer[i].IsDoneTarget == false)
                {
                    nameHint = arrObjectInLayer[i].name;
                    targetObject2D = arrObjectInLayer[i];
                    indexTarget = i;
                    break;
                }
            }

            string str_NameSource = GameManager.Instance.CurrentPuzzle.GetNameTarget(nameHint);
            int indexItemUI = -1;

            for (int i = 0; i < scrollObject.ObjectItems.Length; i++)
            {
                if (scrollObject.ObjectItems[i].gameObject.activeSelf && scrollObject.ObjectItems[i].transform.childCount > 0)
                {
                    ++indexItemUI;
                    if (scrollObject.ObjectItems[i].name.ToString().Equals(str_NameSource))
                    {
                        objectItemTempUI = scrollObject.ObjectItems[i];
                        posBegin = objectItemTempUI.transform.position;
                        break;
                    }
                }
            }

            if (objectItemTempUI == null || targetObject2D == null)
            {
                HideTutorial();
                return;
            }

            UIManager.Instance.UIGameplay.ScrollObject.ScrollToIndex(indexItemUI, objectItemTempUI, targetObject2D, () =>
            {

                var sizeList = 0;
                var indexItemUINew = -1;
                for (int i = 0; i < scrollObject.ObjectItems.Length; i++)
                {
                    if (scrollObject.ObjectItems[i].gameObject.activeSelf && scrollObject.ObjectItems[i].transform.childCount > 0)
                    {
                        var objectHint = objectItemTempUI.GetComponentInChildren<DragObject>();
                        if (objectHint != null)
                            sizeList++;
                    }
                }
                for (int i = 0; i < scrollObject.ObjectItems.Length; i++)
                {
                    if (scrollObject.ObjectItems[i].gameObject.activeSelf && scrollObject.ObjectItems[i].transform.childCount > 0)
                    {
                        ++indexItemUINew;
                        if (scrollObject.ObjectItems[i].name.ToString().Equals(str_NameSource))
                        {
                            objectItemTempUI = scrollObject.ObjectItems[i];
                            break;
                        }
                    }
                }
                Vector3 posMoveBegin = objectItemTempUI.transform.position;

                if (indexItemUINew == 0 || indexItemUINew == sizeList - 1)
                    posMoveBegin = posBegin;
                if (posMoveBegin.x < 0) posMoveBegin = new Vector3(190f, posMoveBegin.y, posMoveBegin.z);
                if (posMoveBegin.x > Screen.width) posMoveBegin = new Vector3(950f, posMoveBegin.y, posMoveBegin.z);
                //DOVirtual.DelayedCall(0.8f, () =>
                //{
                // World -> Screen
                Vector3 v3_Screen = Camera.main.WorldToScreenPoint(targetObject2D.transform.position);

                // UI -> World
                Vector3 v3_objectItemTempUI = Camera.main.ScreenToWorldPoint(posMoveBegin);

                // World -> Screen
                v3_objectItemTempUI = Camera.main.WorldToScreenPoint(v3_objectItemTempUI);

                dragObjectHint = objectItemTempUI.GetComponentInChildren<DragObject>();
                if (dragObjectHint == null)
                    HideTutorial();

                hand.SetPositionHint(v3_objectItemTempUI, v3_Screen, objectItemTempUI.name);
                //});
            });

            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.UsedHint);
        }
        catch(Exception e)
        {
            DataCore.Debug.Log("UIGameplayController Hint" + e);
            return;
        }
       
    }
    public void ShowUI()
    {
        if (bol_AllObjUIHasFade == false)
        {
            DataCore.Debug.LogError("UI Not Fader");
            return;
        }
        DataCore.Debug.LogError("ShowUI");
        foreach (var item in ObjUI)
        {
            if (item == null)
            {
                continue;
            }

            if (item.GetComponent<DragObject>() != null && item.GetComponent<DragObject>().TargetObject.Layer.IndexLayer < 0)
            {
                continue;
            }

            FaderController fader = item.GetComponent<FaderController>();
            fader.transform.parent.gameObject.SetActive(true);
            fader.FadeHide();
        }
    }
    public void HideUI()
    {
        if (bol_AllObjUIHasFade == false)
        {
            DataCore.Debug.LogError("UI Not Fader");
            return;
        }

        DataCore.Debug.LogError("HideUI");

        foreach (var item in ObjUI)
        {
            if (item == null)
            {
                continue;
            }

            FaderController fader = item.GetComponent<FaderController>();

            fader.FadeShow(1.0f, () =>
            {
                fader.transform.parent.gameObject.SetActive(false);
            });
        }
    }
    private bool AddFadeControllerIntoObjUI()
    {
        foreach (var item in ObjUI)
        {
            if (item.GetComponent<FaderController>() == null)
            {
                item.AddComponent<FaderController>();
            }
        }
        return true;
    }
    public void AddFadeControllerIntoObjUI(ObjectItem[] ObjectItems)
    {
        foreach (var item in ObjectItems)
        {
            if (item.gameObject.transform.GetChild(0).gameObject.GetComponent<FaderController>() == null)
            {
                item.gameObject.transform.GetChild(0).gameObject.AddComponent<FaderController>();
                ObjUI.Add(item.gameObject.transform.GetChild(0).gameObject);
            }
        }
    }

    public void OnButtonBackPress()
    {
        try
        {
            //GameManager.Instance.FinishPuzzle();
            if (UIManager.Instance.UIGameplay == null || UIManager.Instance.UIGameplay.ScrollObject == null)
                return;
            if (UIManager.Instance.UIGameplay.ScrollObject != null) {
                UIManager.Instance.UIGameplay.ScrollObject.Clear();
            }
            

            if (ZoomInZoomOut.Instance.IsAllowMove)
            {
                ZoomInZoomOut.Instance.ResetToDefault();
            }
            if (GameManager.Instance.PuzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
            {
                this.PostEvent(EventID.BackHomeResume);
            }
            else
                this.PostEvent(EventID.BackGameDetail);
        }
        catch (Exception ex)
        {
            DataCore.Debug.Log($"Failed OnButtonBackPress. Error {ex.Message}");
        }

    }

    public void OnButtonTurnOffZoomPress()
    {
        ShowZoomUI(false);
        ZoomInZoomOut.Instance.ResetToDefault();
    }

    public void OnWatchAdsTap()
    {
        AdsService.Instance.ShowRewardedAd(AdsService.RwAdUnitIdHint,
            onRewardedAdClosedEvent: (complete) =>
            {
                SoundController.Instance.MuteBgMusic(false);
                if (!complete)
                {
                    UIManager.Instance.ShowPopupNoAds();
                }
            }, onRewardedAdReceivedReward: (adId, placement) =>
            {
                GameData.Instance.IncreaseHint(ConfigManager.NumberIncreaseHint, ConfigManager.GameData.ResourceEarnSource.puzzle_hint);
                UpdateHintUI();
            });
    }

}
