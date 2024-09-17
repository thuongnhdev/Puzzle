using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class BasePanel : MonoBehaviour
{
    [SerializeField] private PanelType panelType;
    [SerializeField] private bool useShareUI = true;
    [SerializeField] private SceneSharedEle sharedEle;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool deactiveWhenDisable = true;
    [SerializeField] private bool useAnimFade = true;

    private SceneSharedEle _cacheSceneSharedEle;
    public bool IsOpen
    {
        get { return _isOpen; }
    }

    protected bool _isOpen;

    public virtual void Init()
    {

    }

    public virtual void SetData(object[] data)
    {

    }

    public virtual void Open()
    {
        if (_isOpen)
        {
            return;
        }

        if (useShareUI)
        {
            //_cacheSceneSharedEle = ShareUIManager.Instance.CurSceneSharedEle;
            //ShareUIManager.Instance.ShowSharedUI(sharedEle);
        }
        _isOpen = true;
        FadeIn();
    }

    public virtual void Close()
    {
        if (!_isOpen)
        {
            return;
        }

        _isOpen = false;

        if (useShareUI)
        {
            //ShareUIManager.Instance.ShowSharedUI(_cacheSceneSharedEle);
        }

        UIReset();
        FadeOut();
    }

    public virtual void OnCloseManual()
    {
    }

    protected virtual void UIReset()
    {

    }

    private void FadeIn()
    {
        gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            if (useAnimFade)
            {
                canvasGroup.alpha = 0;
                canvasGroup.DOFade(1, MCache.Instance.Config.TIME_FADDING_POPUP).SetUpdate(true).OnComplete(() =>
                {
                    canvasGroup.interactable = true;
                }).Play();
            }
            else
            {
                canvasGroup.alpha = 1;
            }
        }
    }

    private void FadeOut()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            canvasGroup.DOFade(0, MCache.Instance.Config.TIME_FADDING_POPUP).SetUpdate(true).OnComplete(() =>
            {
                canvasGroup.interactable = true;
                if (deactiveWhenDisable)
                {
                    gameObject.SetActive(false);
                }

            }).Play();
        }
        else
        {
            canvasGroup.interactable = false;
            gameObject.SetActive(false);
        }
    }

    public virtual void ManualRefeshData()
    {

    }

    public virtual void OnUpdateInk(float animDuration, Action onComplete = null)
    {

    }

    public virtual void OnPlayAnimInkIcon()
    {

    }

    private void OnEnable()
    {
        ShareUIManager.OnUpdateInk += OnUpdateInk;
        ShareUIManager.OnPlayAnimInkIcon += OnPlayAnimInkIcon;
    }

    private void OnDisable()
    {
        ShareUIManager.OnUpdateInk -= OnUpdateInk;
        ShareUIManager.OnPlayAnimInkIcon -= OnPlayAnimInkIcon;
    }

    private Vector2 fingerDown;
    private Vector2 fingerUp;
    public bool detectSwipeOnlyAfterRelease = false;

    public float SWIPE_THRESHOLD = 20f;

    // Update is called once per frame
    void Update()
    {

        //foreach (Touch touch in Input.touches)
        //{
        //    if (touch.phase == TouchPhase.Began)
        //    {
        //        fingerUp = touch.position;
        //        fingerDown = touch.position;
        //    }

        //    //Detects Swipe while finger is still moving
        //    if (touch.phase == TouchPhase.Moved)
        //    {
        //        if (!detectSwipeOnlyAfterRelease)
        //        {
        //            fingerDown = touch.position;
        //            checkSwipe();
        //        }
        //    }

        //    //Detects swipe after finger is released
        //    if (touch.phase == TouchPhase.Ended)
        //    {
        //        fingerDown = touch.position;
        //        checkSwipe();
        //    }
        //}
#if UNITY_ANDROID || UNITY_EDITOR
        if (Input.GetKey(KeyCode.Escape))
        {
            if (GameManager.Instance == null) return;
            if (!GameManager.Instance.IsCloseEscape && GameManager.Instance.GetStepGame() >= StepGameConstants.PlayPuzzleOne)
            {
                if (GameManager.Instance.objListOpen == null || UIManager.Instance.UiDownloadAndShare == null) return;
                if (GameManager.Instance.objListOpen.Count > 0 && !UIManager.Instance.UiDownloadAndShare.isActiveAndEnabled)
                {
                    var index = GameManager.Instance.objListOpen.Count - 1;
                    GameManager.Instance.IsCloseEscape = true;
                    if (GameManager.Instance.objListOpen[index].name.CompareTo("UIBookContent") == 0 || GameManager.Instance.objListOpen[index].name.CompareTo("UIPartDetail") == 0 ||
                        GameManager.Instance.objListOpen[index].name.CompareTo("UIGameplay") == 0 || GameManager.Instance.objListOpen[index].name.CompareTo("UIHomepage") == 0)
                    {
                        GameManager.Instance.objListOpen[index].OnCloseManual();
                    }
                    else
                    {
                        GameManager.Instance.objListOpen[index].Close();
                    }
                }
                return;
            }
        }
#endif
    }

    void checkSwipe()
    {
        //Check if Vertical swipe
        if (verticalMove() > SWIPE_THRESHOLD && verticalMove() > horizontalValMove())
        {
            //Debug.Log("Vertical");
            if (fingerDown.y - fingerUp.y > 0)//up swipe
            {
                OnSwipeUp();
            }
            else if (fingerDown.y - fingerUp.y < 0)//Down swipe
            {
                OnSwipeDown();
            }
            fingerUp = fingerDown;
        }

        //Check if Horizontal swipe
        else if (horizontalValMove() > SWIPE_THRESHOLD && horizontalValMove() > verticalMove())
        {
            //Debug.Log("Horizontal");
            if (fingerDown.x - fingerUp.x > 0)//Right swipe
            {
                OnSwipeRight();
            }
            else if (fingerDown.x - fingerUp.x < 0)//Left swipe
            {
                OnSwipeLeft();
            }
            fingerUp = fingerDown;
        }

        //No Movement at-all
        else
        {
            //Debug.Log("No Swipe!");
        }
    }

    float verticalMove()
    {
        return Mathf.Abs(fingerDown.y - fingerUp.y);
    }

    float horizontalValMove()
    {
        return Mathf.Abs(fingerDown.x - fingerUp.x);
    }

    //////////////////////////////////CALLBACK FUNCTIONS/////////////////////////////
    void OnSwipeUp()
    {
        Debug.Log("Swipe UP");
    }

    void OnSwipeDown()
    {
        Debug.Log("Swipe Down");
    }

    public virtual void OnSwipeLeft()
    {
        Debug.Log("Swipe Left");
    }

    void OnSwipeRight()
    {
        Debug.Log("Swipe Right");
    }

}
