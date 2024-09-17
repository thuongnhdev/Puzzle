using DG.Tweening;
using System;
using EventDispatcher;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using com.F4A.MobileThird;
using DataCore;

public class ZoomInZoomOut : SingletonMonoAwake<ZoomInZoomOut>
{
    private const float Epsilon = 0.1f;
    private const float minimumScaleDownPerUpdate = 0.01f;
    private const float maximumScaleUpPerUpdate = 1.02f;

    public bool ClickedUI { get => clickedUI; set => clickedUI = value; }
    public bool DragDone { get => dragDone; set => dragDone = value; }
    public bool IsAllowMove { get => isAllowMove; set => isAllowMove = value; }
    public bool Bol_AllowZooom { get => bol_AllowZooom; set => bol_AllowZooom = value; }
    public bool Bol_Zooming { get; set; } = false;
    public bool IsUpdateCamera
    {
        get => isUpdateCamera;
    }

    public float CameraSizeOrigin
    {
        get => cameraSizeOrigin;
    }


    [SerializeField] private PuzzleController puzzleController;
    [SerializeField] private bool clickedUI = false;
    [SerializeField] private Text text;
    [SerializeField] private Vector3 posOrigin;
    [SerializeField] private bool isEndZ = false;
    [SerializeField] private bool isAllowMove = true;
    [SerializeField] private bool dragDone = true;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MatchCamera[] followers;
    [SerializeField] private BoxCollider2D touchSpace;

    [Header("Camera Config")]
    [SerializeField] private float smoothTime = 0.5f;
    [SerializeField] private float maxRatioZoomModified = 2;
    [SerializeField] private float moveSpeed = 150.0f;
    [SerializeField] private float vecSmoothSpeed = 2.45f;


    private float touchesPrevPosDifference, touchesCurPosDifference, zoomModifier;
    private Vector2 firstTouchPrevPos, secondTouchPrevPos;
    private float scaleXOrigin = 1;
    private Vector3 dragOrigin;
    private float cameraSizeOrigin;
    private bool isMoving = false;
    private bool bol_AllowZooom = true;
    private bool isZ = false;

    private float cameraZ;
    private float smoothOrthoVec;
    private Vector2? targetPos;
    private Vector2 smoothPosVec;
    private float wRatio;
    private float hRatio;
    private float picW;
    private float picH;
    private float targetOrthoSize;
    private Rect camMoveBound;
    private Vector2 lastMovePos;
    private Vector3 defaultPos;
    private bool isUpdateCamera = false;
    private float maxZoomIn = 4;
    private float maxZoomOut = 8;
    private float aspectRatio;


    public PuzzleController Puzzle
    {
        get => puzzleController;
        set
        {
            puzzleController = value;
            posOrigin = puzzleController.transform.GetChild(0).localPosition;
        }
    }

    public void Init()
    {
        cameraSizeOrigin = mainCamera.orthographicSize;
        cameraSizeOrigin *= GameManager.Instance.SizeRatio;
        mainCamera.orthographicSize = cameraSizeOrigin;
        transform.position *= GameManager.Instance.SizeRatio;
        maxZoomOut = cameraSizeOrigin;

        maxZoomIn = maxZoomOut / maxRatioZoomModified;
        //DataCore.Debug.Log(maxZoomOut + "-" + maxRatioZoomModified + "-" + maxZoomIn);
        defaultPos = transform.position;
        this.cameraZ = mainCamera.transform.position.z;

        this.RegisterListener(EventID.OnInitPuzzleCompleted, (o) => Active());

        for (int i = 0; i < followers.Length; i++)
        {
            followers[i].Init(mainCamera);
        }


        if (mainCamera.aspect > 0.5625f)
        {
            aspectRatio = 1;
        }
        else
        {
            aspectRatio = mainCamera.aspect / 0.5625f;
        }
    }
    void Update()
    {
        UpdateCamera();
        CheckInput();
    }

    public void SetToDefault(Action onCompelte)
    {
        if (puzzleController == null) return;
        if (puzzleController.transform.childCount == 0) return;
        if (puzzleController.transform.GetChild(0).localScale.x != scaleXOrigin)
        {
            DOVirtual.Float(puzzleController.transform.GetChild(0).localScale.x, scaleXOrigin, 0.2f, (x) =>
            {
                puzzleController.transform.GetChild(0).localScale = new Vector3(x, x, x);
            }).SetId(this);
        }

        if (puzzleController.transform.GetChild(0).position != posOrigin)
        {
            puzzleController.transform.GetChild(0).transform.DOLocalMove(posOrigin, 0.2f).SetId(this).OnComplete(() =>
            {
                onCompelte?.Invoke();
            });
        }

        onCompelte?.Invoke();
    }

    public void Active()
    {

        picW = puzzleController.GetResolutionOfBG().x / 100.0f; // pixel per unit: 100
        picH = puzzleController.GetResolutionOfBG().y / 100.0f;

        mainCamera.transform.position = defaultPos;

        targetPos = null;
        UpdateOrthoSize(maxZoomOut);
        IsAllowMove = false;
    }

    private void UpdateCamera()
    {
        if (mainCamera.orthographicSize != targetOrthoSize && targetOrthoSize > 0)
        {
            mainCamera.orthographicSize = Mathf.SmoothDamp(mainCamera.orthographicSize,
                targetOrthoSize, ref smoothOrthoVec, smoothTime);

            if (Mathf.Abs(targetOrthoSize - mainCamera.orthographicSize) <= 0.1f)
            {
                UpdateOrthoSize(targetOrthoSize);
                isUpdateCamera = false;
            }
            else
            {
                isUpdateCamera = true;
            }
        }

        //position
        if (targetPos != null)
        {
            Vector3 newPos;
            float smoothTime = 1.0f / (vecSmoothSpeed);
            newPos.x = Mathf.SmoothDamp(transform.position.x, targetPos.Value.x, ref smoothPosVec.x, smoothTime);
            newPos.y = Mathf.SmoothDamp(transform.position.y, targetPos.Value.y, ref smoothPosVec.y, smoothTime);
            newPos.z = cameraZ;

            float dif = (targetPos.Value - (Vector2)newPos).magnitude;

            if (Mathf.Abs(dif) < 0.02f)
            {
                newPos.x = targetPos.Value.x;
                newPos.y = targetPos.Value.y;
                targetPos = null;
            }

            transform.position = newPos;
        }
    }

    private void CheckInput()
    {
        try
        {
            if (EventSystem.current.IsPointerOverGameObject(0) || EventSystem.current.IsPointerOverGameObject())
                return;

            if (GameManager.Instance == null)
                return;

            if (string.IsNullOrEmpty(GameManager.Instance.CurrentState))
                return;

            if (GameManager.Instance.CurrentState != GameConstants.PLAYING)
                return;

            if (ClickedUI || puzzleController == null || Bol_AllowZooom == false)
            {
                return;
            }

#if UNITY_EDITOR

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0)
            {
                OnInputScale(GetScale(1.1f), Input.mousePosition);
            }
            else if (scroll < 0)
            {
                OnInputScale(GetScale(0.9f), Input.mousePosition);
            }
#else
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.touches[0];
            Touch touch1 = Input.touches[1];

            if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended) return;

            float previousDistance = Vector2.Distance(touch0.position - touch0.deltaPosition,
                touch1.position - touch1.deltaPosition);

            float currentDistance = Vector2.Distance(touch0.position, touch1.position);

            if (previousDistance != currentDistance)
            {
                OnInputScale(GetScale(currentDistance / previousDistance), (touch0.position + touch1.position) / 2);
  
            }
        }
#endif
            if (!IsAllowMove)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                lastMovePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                if (!IsTouchOnTouchSpace(lastMovePos))
                {
                    isMoving = true;
                }
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 newMovePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                OnInputMove(newMovePos - lastMovePos);
            }

            if (Input.GetMouseButtonUp(0))
            {
                isMoving = false;
            }
        }
        catch (Exception ex)
        {

        }

    }

    private float GetScale(float rawScale)
    {
        rawScale = ClampScale(rawScale);
        if (rawScale < 1.0f)
        {
            rawScale -= ((1.0f - rawScale) * MCache.Instance.Config.ZOOM_SPEED);
        }
        else if (rawScale > 1.0f)
        {
            rawScale += ((rawScale - 1.0f) * MCache.Instance.Config.ZOOM_SPEED);
        }

        return ClampScale(rawScale);
    }
    private float ClampScale(float rawScale)
    {
        return (rawScale > maximumScaleUpPerUpdate ? maximumScaleUpPerUpdate : (rawScale < minimumScaleDownPerUpdate ? minimumScaleDownPerUpdate : rawScale));
    }

    private float _newOrthoSizePre = 0;
    private void OnInputScale(float scale, Vector3 focus)
    {
        
        if (!IsTouchOnTouchSpace(mainCamera.ScreenToWorldPoint(focus)))
        {
            return;
        }
      
        if (!IsAllowMove)
        {
            IsAllowMove = true;
            //UIManager.Instance.UIGameplay.ShowZoomUI(true);
        }
      
        if (targetPos == null)
        {
            scale = -(scale - 1) + 1;
            float newOrthoSize = Mathf.Clamp(mainCamera.orthographicSize * scale, maxZoomIn, maxZoomOut);
            if (newOrthoSize != mainCamera.orthographicSize)
            {
                UpdateOrthoSize(newOrthoSize, true);
            }

            //pos
            Vector3 offsetToMid;

            offsetToMid = mainCamera.ScreenToWorldPoint(focus);

            offsetToMid.x = (mainCamera.transform.position.x - offsetToMid.x);
            offsetToMid.y = (mainCamera.transform.position.y - offsetToMid.y);
            offsetToMid.z = 0;
            offsetToMid *= scale - 1;
            _newOrthoSizePre = newOrthoSize;
            mainCamera.transform.position = ClaimPosition(mainCamera.transform.position + offsetToMid);
            if (newOrthoSize >= maxZoomOut) {
                UIManager.Instance.UIGameplay.OnButtonTurnOffZoomPress();
            }else
            {
                UIManager.Instance.UIGameplay.ShowZoomUI(true);
            }
        }
    }

    private void OnInputMove(Vector2 vec)
    {
        Vector3 worldMove;
        worldMove.x = vec.x * wRatio;
        worldMove.y = vec.y * hRatio;
        worldMove.z = cameraZ;

        worldMove = mainCamera.transform.position - new Vector3(worldMove.x, worldMove.y, 0);
        mainCamera.transform.position = ClaimPosition(worldMove);

        if (targetPos != null)
        {
            targetPos = null;
        }
    }

    private void UpdateOrthoSize(float size, bool force = false)
    {
        targetOrthoSize = size;
        float percent = size / maxZoomOut;
        Vector2 res = new Vector2(picW, picH) * aspectRatio;

        res -= res * percent;
        res /= 2;

        camMoveBound.xMin = -res.x;
        camMoveBound.xMax = res.x;
        camMoveBound.yMin = -res.y + defaultPos.y * percent;
        camMoveBound.yMax = res.y + defaultPos.y * percent;


        //camMoveBound.yMin = camMoveBound.yMin * PercentSizeY;
        //camMoveBound.yMax = camMoveBound.yMax * PercentSizeY

        camMoveBound.xMin = NearZero(camMoveBound.xMin);
        camMoveBound.xMax = NearZero(camMoveBound.xMax);
        camMoveBound.yMin = NearZero(camMoveBound.yMin);
        camMoveBound.yMax = NearZero(camMoveBound.yMax);


        if (force)
        {
            mainCamera.orthographicSize = targetOrthoSize;
        }

        ReCalculateScreenRatio();
    }

    private void ReCalculateScreenRatio()
    {
        wRatio = (mainCamera.aspect * mainCamera.orthographicSize * moveSpeed) / Screen.width;
        hRatio = (mainCamera.orthographicSize * moveSpeed) / Screen.height;
    }

    private float NearZero(float value)
    {
        if (Mathf.Abs(value) < Epsilon)
        {
            return 0;
        }

        return value;
    }

    private Vector3 ClaimPosition(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, camMoveBound.xMin, camMoveBound.xMax);
        pos.y = Mathf.Clamp(pos.y, camMoveBound.yMin, camMoveBound.yMax);
        return pos;
    }

    private bool IsTouchOnTouchSpace(Vector2 point)
    {
        return touchSpace.OverlapPoint(point);
    }

    [ContextMenu("Default")]
    public void ResetToDefault(bool force = false)
    {
        DataCore.Debug.Log("Reset");
        IsAllowMove = false;
        SetTargetCameraSize(maxZoomOut, force);
        SetTargetPos(defaultPos);
    }
    public void SetTargetCameraSize(float size, bool force = false)
    {
        UpdateOrthoSize(size, force);
    }

    public void SetTargetPos(Vector2 pos)
    {
        targetPos = pos;
    }
}