using DG.Tweening;
using System;
using System.Collections;
using EventDispatcher;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DataCore;

public class DragObject : MonoBehaviour, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler, IBeginDragHandler, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler
{
    Canvas canvas;
    [SerializeField] RectTransform rectDrag;
    [SerializeField] GameObject objSource;
    [SerializeField] GameObject objTarget;
    [SerializeField] Transform trsNew;
    [SerializeField] Transform trstOld;
    [SerializeField] TargetObject2D targetObject;
    [SerializeField] public string nameParent;
    [SerializeField] public Image imgObjSource;
    [SerializeField] private UIHepler uiHelper;

    [SerializeField] public Sprite sprHandUATutorial;
    [SerializeField] private Image imgHandUATutorial;

    ScrollRect scrollRect;

    bool isDone = false;
    Vector2 posDefault;
    Vector2 posRectDefault;
    Vector3 scaleDefault;
    ObjectItem objectItem;
    PuzzleController puzzle;
    public bool IsDone { get => isDone; set => isDone = value; }
    public ObjectItem ObjectItem { get => objectItem; set => objectItem = value; }
    public GameObject ObjSource { get => objSource; set => objSource = value; }
    public GameObject ObjTarget { get => objTarget; set => objTarget = value; }
    public TargetObject2D TargetObject { get => targetObject; set => targetObject = value; }

    private Tween _checkFillTargetTween;

    private readonly int _pixelDefaulSmall = 100;

    private SpriteRenderer sprHighLightTutorial = null;

    void InitAwake()
    {
        rectDrag = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        scrollRect = GetComponentInParent<ScrollRect>();
        trstOld = transform.parent;
        sizeXOld = trstOld.GetComponent<LayoutElement>().preferredWidth;
        // imgObjSource = GetComponent<Image>();
        objectItem = GetComponentInParent<ObjectItem>();

    }
 
    public void Init(PuzzleController puzzle)
    {
        InitAwake();
        uiHelper.FixedSize();
        if (rectDrag == null)
        {
            //DataCore.Debug.Log("rectDrag null: " + rectDrag);
            rectDrag = GetComponent<RectTransform>();
        }
        posDefault = rectDrag.anchoredPosition;

        nameParent = transform.parent.name;
        targetObject = objTarget.GetComponent<TargetObject2D>();
        imgObjSource.sprite = objSource.GetComponent<SpriteRenderer>().sprite;

        imgObjSource.SetNativeSize();
        Vector3 vScale;
        vScale.z = 1;
        float detalX = imgObjSource.GetComponent<RectTransform>().sizeDelta.x;
        float detalY = imgObjSource.GetComponent<RectTransform>().sizeDelta.y;

        vScale.x = 280 / detalX;
        vScale.y = 160 / detalY; // 160

        if (vScale.x < vScale.y)
        {
            vScale.y = vScale.x;
        }
        else
        {
            vScale.x = vScale.y;
        }

        float ratio = 0.31f + (GameManager.Instance.SizeRatio == 1 ? 0 : 1) * (0.31f + 0.31f * (1 - GameManager.Instance.SizeRatio));

        if (vScale.x > ratio * (Camera.main.aspect / 0.5625f)) // out of range
        {
            vScale.x = ratio * (Camera.main.aspect / 0.5625f);
            vScale.y = ratio * (Camera.main.aspect / 0.5625f);
        }
       
        rectDrag.transform.localScale = Vector3.one;
        imgObjSource.transform.localScale = vScale;

        if (ObjTarget.GetComponent<TargetObject2D>().Layer.IndexLayer == -1)
        {
            transform.parent.gameObject.SetActive(false);
        }
        //scale object small width < 100 pixel
        var isSmall = (detalY <= this._pixelDefaulSmall && detalX <= this._pixelDefaulSmall) ? true : false;
        if (isSmall)
        {
            imgObjSource.transform.localScale = new Vector2(imgObjSource.transform.localScale.x * 2.5f, imgObjSource.transform.localScale.y * 2.5f);
        }
        scaleDefault = imgObjSource.transform.localScale;
        this.puzzle = puzzle;

        //disable highlight
        if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
        {
            var objHL = objTarget.transform.Find("HL");
            if (objHL != null)
                sprHighLightTutorial = objHL.GetComponent<SpriteRenderer>();
           
            if (sprHighLightTutorial != null)
            {
                sprHighLightTutorial.enabled = false;
                objTarget.GetComponent<SpriteRenderer>().enabled = true;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsDone || isDragComeBack || ZoomInZoomOut.Instance.Bol_Zooming) return;

        if (_checkFillTargetTween != null)
        {
            _checkFillTargetTween.Kill();
            _checkFillTargetTween = null;
        }

        Vector3 vPosCurrent = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        var posDis = vPosCurrent.y - vBeginDrag.y;
        if (posDis <= 0 && isDragObj == false)
        {
            isDragObj = false;
            scrollRect.OnDrag(eventData);
            return;
        }

        var distanceToDrag = MCache.Instance.Config.DISTANCE_TO_DRAG_OBJECT;
        if ((Mathf.Abs(vBeginDrag.y - vPosCurrent.y) <= distanceToDrag) && isDragObj == false)
        {
            isDragObj = false;
            scrollRect.OnDrag(eventData);
            return;
        }
        isDragObj = true;
        scrollRect.OnEndDrag(eventData);
        scrollRect.StopMovement();

        //enable highlight
        if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
        {
            if (sprHighLightTutorial != null)
            {
                sprHighLightTutorial.enabled = true;
                objTarget.GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        #region Follow Mouse
        if (rectDrag.transform.parent != trsNew)
        {
            SoundController.Instance.PlayVibrate();

            rectDrag.transform.parent = trsNew;

            float scale = MCache.Instance.Config.SIZE_DRAG_WITH_TARGET_OBJ;
            Vector3 vec = new Vector3(scale, scale, 1);
            imgObjSource.transform.DOScale(vec, 0.25f).OnComplete(() =>
            {

            }).SetEase(Ease.Linear).SetId(this);

            RectTransform rect = trstOld.GetComponent<RectTransform>();
            ScaleInOld(null);
            var distanceFinger = MCache.Instance.Config.DISTANCE_FINGER;
            var tile = (float)Screen.height / Screen.width;
            if (tile < 1.8f)
            {
                distanceFinger = distanceFinger / 2;
            }
            tg = rectDrag.anchoredPosition + ((Vector2.up * distanceFinger) / canvas.scaleFactor / (Camera.main.aspect / 0.5625f));
        }

        Vector2 vTarget = (eventData.delta / canvas.scaleFactor);

        vTarget += tg;
        rectDrag.anchoredPosition = vTarget;

        tg = rectDrag.anchoredPosition;

        #endregion
        if (targetObject.Layer.IndexLayer != puzzle.GetIndexLayer()) return;

        var delayFillTarget = MCache.Instance.Config.DELAY_FILL_TARGET;
        if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
        {
            delayFillTarget = 0.015f;
        }

        _checkFillTargetTween = DOVirtual.DelayedCall(delayFillTarget, () =>
        {
            CheckFillTarget();
        }).Play();
        
    }
    Vector2 tg;
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_checkFillTargetTween != null)
        {
            _checkFillTargetTween.Kill();
            _checkFillTargetTween = null;
        }
        //disable highlight
        if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
        {
            if (sprHighLightTutorial != null)
            {
                sprHighLightTutorial.enabled = false;
                objTarget.GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        Camera.main.transform.GetComponent<ZoomInZoomOut>().ClickedUI = false;
        scrollRect.OnEndDrag(eventData);
        Camera.main.transform.GetComponent<ZoomInZoomOut>().DragDone = true;
        if (IsDone || isDragObj == false || isDragComeBack) return;
        StartCoroutine("ActiveOut");
    }

    IEnumerator ActiveOut()
    {
        yield return new WaitUntil(() => isScaleInOld == false);
        isDragComeBack = true;
        isDragObj = false;
        rectDrag.transform.DOLocalMove(posDefault, 0.45f).OnComplete(() => { isDragComeBack = false; }).SetEase(Ease.Linear).SetId(this);
        imgObjSource.transform.DOScale(scaleDefault, 0.3f).SetEase(Ease.Linear).SetId(this);
        rectDrag.transform.parent = trstOld;

        ScaleOutOld(null);
    }

    private void CheckFillTarget()
    {
        Vector2 vecConvert = Camera.main.WorldToScreenPoint(objSource.transform.position);

        float objDis = Vector2.Distance(rectDrag.position, vecConvert);
        float minHeight =
            objDis / (rectDrag.rect.height > rectDrag.rect.width ? rectDrag.rect.height : rectDrag.rect.width);

        var percentFillTarget = MCache.Instance.Config.PERCENT_FILL_TARGET;
        //if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
        //    percentFillTarget = percentFillTarget * 2.0f;

        if (minHeight < percentFillTarget)
        {
            if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
                Time.timeScale = 4;

            SoundController.Instance.PlayVibrate();

            UIManager.Instance.UIGameplay.ScrollObject.TempNumber += 0.01f;
            SpriteRenderer spriteRenderer = objSource.GetComponent<SpriteRenderer>();
            GameObject temp = Instantiate(objSource);
            temp.transform.position = vecConvert;
            temp.gameObject.SetActive(false);
            Camera.main.transform.GetComponent<ZoomInZoomOut>().ClickedUI = false;


            puzzle.CurrentIndexObject++;
            puzzle.UpdateProgression();
            temp.transform.DOMove(objSource.transform.position, 0).OnComplete(() =>
            {
                Camera.main.transform.GetComponent<ZoomInZoomOut>().DragDone = true;
                //Taptic.Vibrate();
                DOVirtual.DelayedCall(0, () =>
                {
                    temp.gameObject.SetActive(false);
                });
            }).SetId(this);

            #region hide img drag
            rectDrag.gameObject.SetActive(false);
            objTarget.GetComponent<TargetObject2D>().Hide();

            objSource.gameObject.SetActive(true);

            MCache.Instance.FxFill.transform.position = objSource.transform.position;
            MCache.Instance.FxFill.ShowFX(objSource.GetComponent<SpriteRenderer>().sprite, spriteRenderer.sortingOrder);
            this.PostEvent(EventID.PauseTimeTutorial, true);
            
            var timeFillColor = .1f;
            var timeDelayTutorial = 0.0f;
            var durationScale = MCache.Instance.Config.DURATION_SCALE_OBJECT;
            var distanceFillTarget = MCache.Instance.Config.DISTANCE_FILL_TARGET - .2f;
        

            objSource.transform.DOScale(MCache.Instance.Config.SCALE_OBJECT_TO, durationScale).OnComplete(() =>
            {
                objSource.transform.DOScale(MCache.Instance.Config.SCALE_OBJECT_BACK, durationScale).OnComplete(() =>
                {
                    Color tmp = objSource.GetComponent<SpriteRenderer>().color;
                    tmp.a = 0f;

                    DOVirtual.Float(1.0f, 0.0f, timeFillColor, (x) =>
                    {
                        tmp.a = x;
                        objSource.GetComponent<SpriteRenderer>().color = tmp;
                    }).OnComplete(() =>
                    {
                        if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
                        {
                            Time.timeScale = 1;
                            UIManager.Instance.UIGameplay.NumberShowHintTutorial();
                        }
                        objSource.gameObject.SetActive(false);
                        this.PostEvent(EventID.PauseTimeTutorial, false);
                        DOVirtual.DelayedCall(distanceFillTarget, () =>
                        {
                            tmp.a = 1.0f;
                            objSource.GetComponent<SpriteRenderer>().color = tmp;
                            DOVirtual.DelayedCall(timeDelayTutorial, () =>
                            { objSource.gameObject.SetActive(true); });

                        });
                    }).SetEase(Ease.Linear).SetId(this);
                }).SetEase(Ease.Linear).SetId(this);
            }).SetEase(Ease.Linear).SetId(this);

            IsDone = true;
            #endregion

            if (PlayerData.Instance.UserProfile.m_LevelOldInfo.m_LsObjectDone.Contains(nameParent) == false)
            {
                PlayerData.Instance.UserProfile.m_LevelOldInfo.m_LsObjectDone.Add(nameParent);
            }

            if (GameManager.Instance.PuzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
            {

                var data = GameData.Instance.SavedPack.DataGetCurrentPuzzleCollectionData(GameManager.Instance._collectionId, GameManager.Instance._collectionIndex);
                if (data != null && data.savedData.m_LsObjectDone.Contains(nameParent) == false)
                {
                    data.savedData.m_LsObjectDone.Add(nameParent);                    
                }
            }
            else
            {
                if (GameData.Instance.SavedPack.GetCurrentPuzzleData() != null && GameData.Instance.SavedPack.GetCurrentPuzzleData().savedData.m_LsObjectDone.Contains(nameParent) == false)
                {
                    GameData.Instance.SavedPack.GetCurrentPuzzleData().savedData.m_LsObjectDone.Add(nameParent);
                }
            }
        }
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        Camera.main.transform.GetComponent<ZoomInZoomOut>().ClickedUI = true;
        vBeginDrag = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        Camera.main.transform.GetComponent<ZoomInZoomOut>().DragDone = false;
        //Camera.main.transform.GetComponent<ZoomInZoomOut>().IsAllowMove = true;

        MCache.Instance.Hand.Stop();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        scrollRect.OnBeginDrag(eventData);

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isSelected = true;
        scrollRect.StopMovement();
        //DataCore.Debug.Log("OnPointerDown");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelected == false) return;
        isAllowFllowMouse = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isAllowFllowMouse = false;
        isSelected = false;
    }
    [SerializeField] bool isAllowFllowMouse = false;
    [SerializeField] bool isSelected = false;
    Vector3 vBeginDrag;
    float sizeXOld;
    bool isScaleInOld = false;
    bool isScaleOutOld = false;
    bool isDragObj = false;
    bool isDragComeBack = false;
    void ScaleInOld(Action onComplete)
    {
        if (trstOld == null) return;

        LayoutElement layoutElement = trstOld.GetComponent<LayoutElement>();

        if (layoutElement == null) return;

        if (isScaleInOld || layoutElement.preferredWidth == 0) return;
        if (!Tutorial.IsCompleted)
        {
            UIManager.Instance.UIGameplay.EnableTutorialIconLight(false);
        }

        isScaleInOld = true;
        DOVirtual.Float(sizeXOld, 0, 0.5f, (x) =>
        {
            layoutElement.preferredWidth = x;
        }).OnComplete(() =>
        {
            isScaleInOld = false;
            onComplete?.Invoke();
        }).SetEase(Ease.Linear);
    }
    void ScaleOutOld(Action onComplete)
    {
        if (trstOld == null) return;

        LayoutElement layoutElement = trstOld.GetComponent<LayoutElement>();
        if (layoutElement == null) return;
        if (isScaleOutOld) return;

        isScaleOutOld = true;
        DOVirtual.Float(0, sizeXOld, 0.5f, (x) =>
        {
            layoutElement.preferredWidth = x;
        }).OnComplete(() =>
        {
            if (!Tutorial.IsCompleted)
            {
                UIManager.Instance.UIGameplay.EnableTutorialIconLight(true);
            }
            isScaleOutOld = false;
            onComplete?.Invoke();
        }).SetEase(Ease.Linear);
    }

}
