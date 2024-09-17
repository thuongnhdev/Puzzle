using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DataCore;

public class NewChapterTab : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI authorTxt;
    [SerializeField] private TextMeshProUGUI chapterTxt;
    [SerializeField] private TextMeshProUGUI lastUpdateTxt;

    [SerializeField] private Transform container;

    private int _bookId;
    private int _chapterId;
    private long _lastUpdate;
    private int _newChapterId;

    private Vector3 _mouseStartPos;
    private Vector3 _containerStartPos;

    private float _horizontalDragMinDistance = 30f;
    private float _verticalDragMinDistance = 30f;

    private bool _isHorizontalDrag = false;
    private bool _isVerticleDrag = false;

    private bool _lock;

    public void SetData(int newChapterId, int bookId, string partId, string name, string author, long lastUpdate, Sprite sprite)
    {
        this._bookId = bookId;
        this.nameTxt.SetText(name);
        this.authorTxt.SetText("by " + author);
        this.chapterTxt.SetText("Chapter " + partId);

        this._lastUpdate = lastUpdate;
        this.lastUpdateTxt.SetText(UIManager.ConvertReleaseDay(lastUpdate, "updated"));

        thumbnail.sprite = sprite;
        this._newChapterId = newChapterId;
    }

    public void OnClick()
    {
        if (_isHorizontalDrag || _lock)
        {
            return;
        }

        UIManager.Instance.CloseUIHome();
        UIManager.Instance.ShowUIBookDetail(_bookId, "news");

    }

    public long GetLastUpdate()
    {
        return _lastUpdate;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_lock)
        {
            return;
        }

        _mouseStartPos = GetMousePosition();
        _containerStartPos = container.localPosition;

        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.beginDragHandler);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_lock)
        {
            return;
        }

        Vector3 distance = GetMousePosition() - _mouseStartPos;

        if (!_isHorizontalDrag && !_isVerticleDrag)
        {
            if (distance.x > _horizontalDragMinDistance)
            {
                _isHorizontalDrag = true;
            } else if (Mathf.Abs(distance.y) > _verticalDragMinDistance)
            {
                _isVerticleDrag = true;
                return;
            }
        }

        if (_isHorizontalDrag)
        {
            float newPosX = _containerStartPos.x + Mathf.Max(0f, distance.x);
            container.localPosition = new Vector3(newPosX, container.localPosition.y, container.localPosition.z);
            
            return;
        }
        
        if (_isVerticleDrag)
        {
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.dragHandler);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_lock)
        {
            return;
        }

        if (_isHorizontalDrag)
        {
            Vector3 distance = GetMousePosition() - _mouseStartPos;

            if (distance.x > 100f)
            {
                //if()

                if (container.localPosition.x < 1100)
                {
                    GameData.Instance.AddRemovedNewChapter(this._newChapterId);
                    _lock = true;
                    container.DOLocalMoveX(1100, 0.3f).OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                    }).Play();
                }      
            }
            else
            {
                container.localPosition = _containerStartPos;
            }
        }

        _isHorizontalDrag = false;
        _isVerticleDrag = false;
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.endDragHandler);
    }

    private Vector3 GetMousePosition()
    {
        
#if UNITY_EDITOR
        return Input.mousePosition;
#else
        return Input.mousePosition;
#endif
    }
}
