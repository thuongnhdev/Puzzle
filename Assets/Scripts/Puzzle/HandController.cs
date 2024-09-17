using DG.Tweening;
using UnityEngine;
using DataCore;
using System.Collections.Generic;

public class HandController : MonoBehaviour
{
    [SerializeField] Vector3 v3_PosOrigin;
    [SerializeField] RectTransform rect_Parent;
    [SerializeField] Canvas m_parent;
    [SerializeField] Camera m_Camera;
    RectTransform rect_Hand;
    [SerializeField] FaderController fader;
    [SerializeField] FaderController faderLeft;
    bool isShowing = false;
    bool isShowingHorizontal = false;

    public bool IsShowing { get => isShowing; set => isShowing = value; }
    public bool IsShowingHorizontal { get => isShowingHorizontal; set => isShowingHorizontal = value; }

    [SerializeField] private Animator handAnimator;

    private string statusClick = "StatusClick";
    private string statusOut = "StatusOut";

    private void Awake()
    {
        rect_Hand = GetComponent<RectTransform>();
        v3_PosOrigin = rect_Hand.anchoredPosition;
        m_parent = GetComponentInParent<Canvas>();

        fader.Init();
        faderLeft.Init();
        _posHandObject.Clear();
    }

    private Vector3 v3_BeginHint , v3_TargetHint;
    private string nameObjHint;
    public void SetPositionHint(Vector3 v3_Begin, Vector3 v3_Target, string nameObj)
    {
        v3_BeginHint = v3_Begin;
        v3_TargetHint = v3_Target;
        nameObjHint = nameObj;
        StartMove();
    }
    private Dictionary<string,Vector3>  _posHandObject = new Dictionary<string, Vector3>();
    public void StartMove()
    {
        rect_Hand.gameObject.SetActive(true);
        fader.Show();
        faderLeft.Hide();
        Vector2 v2_anchoredPos;

        if(!Tutorial.IsCompleted)
        {
            if (!_posHandObject.ContainsKey(nameObjHint))
            {
                _posHandObject.Clear();
                _posHandObject.Add(nameObjHint, v3_BeginHint);
            }
            else
            {
                foreach (var posItem in _posHandObject)
                    v3_BeginHint = posItem.Value;
            }
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect_Parent, v3_BeginHint, m_parent.renderMode == RenderMode.ScreenSpaceOverlay ? null : m_Camera, out v2_anchoredPos);

        rect_Hand.anchoredPosition = v2_anchoredPos;
       
        fader.FadeHide(0.2f, () =>
            {
                DOVirtual.DelayedCall(1.0f, () =>
                {
                    if (IsShowing)
                    {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect_Parent, v3_TargetHint, m_parent.renderMode == RenderMode.ScreenSpaceOverlay ? null : m_Camera, out v2_anchoredPos);
                        if (v3_BeginHint == Vector3.zero || v3_TargetHint == Vector3.zero)
                            return;
                        rect_Hand.DOAnchorPos(v2_anchoredPos, 1).OnComplete(() =>
                        {
                            DOVirtual.DelayedCall(0.5f, () =>
                            {
                                fader.FadeShow(1.0f, () =>
                                {
                                    StartMove();
                                });
                            }).SetId(this);
                        }).SetEase(Ease.Linear).SetId(this);
                    }
                    else
                    {
                        Stop(fader);
                    }
                });
               
            });
        
    }


    private int indexShowHorizontal = 0;
    public int IndexShowHorizontal { get => indexShowHorizontal; set => indexShowHorizontal = value; }
    public void StartMoveHorizontal(Vector3 v3_Begin)
    {
        IsShowingHorizontal = true;
        fader.Hide();
        faderLeft.Show();
        rect_Hand.gameObject.SetActive(true);

        var tile = (float)Screen.height / Screen.width;
        float durationMove = 200.0f;
        if (tile > 1.8f) durationMove = 500.0f;

        if (v3_Begin.x < durationMove)
            v3_Begin = new Vector3(v3_Begin.x + durationMove, v3_Begin.y, v3_Begin.z);
        Vector3 v3_Target = new Vector3(v3_Begin.x - durationMove, v3_Begin.y, v3_Begin.z);

        Vector2 v2_anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect_Parent, v3_Begin, m_parent.renderMode == RenderMode.ScreenSpaceOverlay ? null : m_Camera, out v2_anchoredPos);

        rect_Hand.anchoredPosition = v2_anchoredPos;
        

        faderLeft.FadeHide(0.2f, () =>
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect_Parent, v3_Target, m_parent.renderMode == RenderMode.ScreenSpaceOverlay ? null : m_Camera, out v2_anchoredPos);
            rect_Hand.DOAnchorPos(v2_anchoredPos, 1).OnComplete(() =>

            {
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    faderLeft.FadeShow(0.2f, () =>
                    {
                        IndexShowHorizontal++;
                        if (IndexShowHorizontal < 4)
                            StartMoveHorizontal(v3_Begin);
                        else
                        {
                            IsShowingHorizontal = false;
                            rect_Hand.gameObject.SetActive(false);
                            IsShowing = false;
                            rect_Hand.DOKill();
                            DOTween.Kill(this);
                        }
                    });
                }).SetId(this);
            }).SetEase(Ease.Linear).SetId(this);
          
        });



    }

    
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (IsShowing)
            {
                //DataCore.Debug.Log("Update Stop");    
                if (Tutorial.IsCompleted)
                {
                    Stop(fader);
                }
            }
        }
    }
 
    public void Stop(FaderController faderController = null)
    {
        v3_BeginHint = Vector3.zero;
        v3_TargetHint = Vector3.zero;
        if (faderController == null) faderController = fader;
        IndexShowHorizontal = 5;
        rect_Hand.gameObject.SetActive(false);
     
        if (IsShowing == false) return;
        IsShowing = false;
        rect_Hand.DOKill();
        DOTween.Kill(this);
        fader.FadeShow(0.2f, null);
    }

}

