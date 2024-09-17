using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = System.Random;
using TMPro;
using DataCore;

public class ScrollObjectManager : MonoBehaviour, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
{
    [SerializeField] private Slider sldProgression;//sld_Progression
    [SerializeField] private TextMeshProUGUI txtProgression;
    [SerializeField] private Transform trsContent;
    [SerializeField] private ObjectItem[] m_objectItems;
    [SerializeField] private RectTransform r_ViewPort;

    private ScrollRect scrollView;
    private float m_TempNumber = 0.01f;
    public Transform TrsContent { get => trsContent; set => trsContent = value; }
    public ObjectItem[] ObjectItems { get => m_objectItems; set => m_objectItems = value; }
    public float TempNumber { get => m_TempNumber; set => m_TempNumber = value; }

    private void Awake()
    {
        scrollView = GetComponent<ScrollRect>();

    }

    public void UpdateObjItem()
    {
        m_objectItems = new ObjectItem[trsContent.childCount];
        for (int indexItem = 0; indexItem < trsContent.childCount; indexItem++)
        {
            m_objectItems[indexItem] = trsContent.GetChild(indexItem).GetComponent<ObjectItem>();
            m_objectItems[indexItem].IndexItem = indexItem;
        }
    }
    public void Clear()
    {
        if (m_objectItems == null) return;
        if (m_objectItems.Length == 0) return;
        for (int indexItem = 0; indexItem < m_objectItems.Length; indexItem++)
        {
            if (m_objectItems[indexItem].gameObject != null)
                Destroy(m_objectItems[indexItem].gameObject);
        }
    }
    public void CreateObjectsItemUI(PuzzleController puzzle, SpriteRenderer[] arrSpriteSource, SpriteRenderer[] arrSpriteTarget)
    {
        ObjectItem demo = trsContent.GetChild(0).GetComponent<ObjectItem>();
        m_objectItems = new ObjectItem[arrSpriteSource.Length];

        List<int> randomIndexs = RandomIndexList(arrSpriteSource.Length);
        for (int index = 0; index < randomIndexs.Count; index++)
        {
            ObjectItem objectItem = Instantiate(demo, trsContent);
            objectItem.gameObject.SetActive(true);
            objectItem.GetComponentInChildren<DragObject>().ObjSource = arrSpriteSource[randomIndexs[index]].gameObject;
            objectItem.GetComponentInChildren<DragObject>().ObjTarget = arrSpriteTarget[randomIndexs[index]].gameObject;
            objectItem.IndexItem = randomIndexs[index];
            objectItem.name = arrSpriteSource[randomIndexs[index]].sprite.name;
            objectItem.GetComponentInChildren<DragObject>().Init(puzzle);

            m_objectItems[index] = objectItem;
        }

        SetSizeViewPort(m_objectItems.Length);


        UIManager.Instance.UIGameplay.AddFadeControllerIntoObjUI(m_objectItems);

    }

    public void CreateObjectsItemUITutorial(PuzzleController puzzle, SpriteRenderer[] arrSpriteSource, SpriteRenderer[] arrSpriteTarget)
    {
        ObjectItem demo = trsContent.GetChild(0).GetComponent<ObjectItem>();
        m_objectItems = new ObjectItem[arrSpriteSource.Length];

        List<int> randomIndexs = new List<int>();
        for (int i = 0; i < arrSpriteSource.Length; i++)
        {
            randomIndexs.Add(i);
        }
     
        for (int index = 0; index < randomIndexs.Count; index++)
        {
            ObjectItem objectItem = Instantiate(demo, trsContent);
            objectItem.gameObject.SetActive(true);
            objectItem.GetComponentInChildren<DragObject>().ObjSource = arrSpriteSource[randomIndexs[index]].gameObject;
            objectItem.GetComponentInChildren<DragObject>().ObjTarget = arrSpriteTarget[randomIndexs[index]].gameObject;
            objectItem.IndexItem = randomIndexs[index];
            objectItem.name = arrSpriteSource[randomIndexs[index]].sprite.name;
            objectItem.GetComponentInChildren<DragObject>().Init(puzzle);

            m_objectItems[index] = objectItem;
        }

        SetSizeViewPort(m_objectItems.Length);


        UIManager.Instance.UIGameplay.AddFadeControllerIntoObjUI(m_objectItems);

    }

    public void SetSizeViewPort(int sizeItem)
    {
        r_ViewPort.offsetMin = new Vector2(0, r_ViewPort.offsetMin.y);
        r_ViewPort.offsetMax = new Vector2(0, r_ViewPort.offsetMax.y);
        if (sizeItem == 1)
        {
            var tile = (float)Screen.height / Screen.width;
            if (tile > 1.5f)
            {
                r_ViewPort.offsetMin = new Vector2(380, r_ViewPort.offsetMin.y);
                r_ViewPort.offsetMax = new Vector2(-380 ,r_ViewPort.offsetMax.y);
            }
            else
            {
                r_ViewPort.offsetMin = new Vector2(460, r_ViewPort.offsetMin.y);
                r_ViewPort.offsetMax = new Vector2(-460, r_ViewPort.offsetMax.y);
            }


        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        UIManager.Instance.UIGameplay.HideTutorial();
        Camera.main.transform.GetComponent<ZoomInZoomOut>().ClickedUI = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
      
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        UIManager.Instance.UIGameplay.HideTutorial();
        Invoke("UnClickUI", 0.25f);
    }
    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        Camera.main.transform.GetComponent<ZoomInZoomOut>().ClickedUI = true;
    }
    void UnClickUI()
    {
        Camera.main.transform.GetComponent<ZoomInZoomOut>().ClickedUI = false;
    }
    public void UpdateProgression(int current, int total)
    {
        txtProgression.text = current.ToString() + " / " + total.ToString();
        DOVirtual.Float(sldProgression.value, current * 1.0f / total, 0.2f, (x) =>
        {
            sldProgression.value = x;
        });
    }
    public void ScrollToIndex(int indexHint, ObjectItem begin, TargetObject2D end, Action OnComplete)
    {
        //DataCore.Debug.Log("Scroll to: " + indexHint);
        float size = trsContent.GetComponent<RectTransform>().sizeDelta.x;
        int indexPos = indexHint;
        float xTo = ((indexPos - 1) * 350) / size;
        // DataCore.Debug.Log(scrollView.horizontalNormalizedPosition + "-------" + xTo);
        // DataCore.Debug.Log(0.032f + m_TempNumber);
        //DataCore.Debug.Log(scrollView.horizontalNormalizedPosition + "-" + xTo);
        //if (Mathf.Abs(scrollView.horizontalNormalizedPosition - xTo) <= 0.000f + m_TempNumber) // 0.032
        //{
        //    OnComplete?.Invoke();
        //    return;
        //}

        DOVirtual.Float(scrollView.horizontalNormalizedPosition, xTo, 0.5f, (x) =>
         {
             scrollView.horizontalNormalizedPosition = x;
         }).OnComplete(() =>
         {
             OnComplete?.Invoke();
         }).SetEase(Ease.OutQuint);
    }
    public void ShowObject()
    {
    }
    public void HideObject()
    {
    }
    public void Stop()
    {
        //DataCore.Debug.Log("Stop scrollView");
        scrollView.enabled = false;
    }

    public void EnableScrollWhenTutorial(bool enable)
    {
        if (scrollView != null)
        {
            scrollView.horizontal = enable;
        }
    }

    public void AllowScroll()
    {
        //DataCore.Debug.Log("AllowScroll scrollView");
        scrollView.enabled = true;
    }

    private List<int> RandomIndexList(int length)
    {
        List<int> result = new List<int>(length);
        List<int> tmp = new List<int>(length);

        for (int i = 0; i < length; i++)
        {
            tmp.Add(i);
        }

        int random;
        while (tmp.Count != 0)
        {
            random = UnityEngine.Random.Range(0, tmp.Count);
            result.Add(tmp[random]);
            tmp.RemoveAt(random);
        }

        return result;
    }
}
