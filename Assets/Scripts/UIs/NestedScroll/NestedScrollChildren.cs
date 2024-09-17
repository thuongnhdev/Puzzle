using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NestedScrollChildren : ScrollRect
{
    //private const float SCROLL_SPEED = 0.05f;
    //[SerializeField] private ScrollRect scrollRect;
    //[SerializeField] ScrollRect parentScrollRect;

    //public void OnDrag(PointerEventData eventData)
    //{
    //    if (Mathf.Abs(eventData.delta.x) >= Mathf.Abs(eventData.delta.y))
    //    {
    //        scrollRect.horizontalNormalizedPosition -= eventData.delta.x * SCROLL_SPEED * Time.deltaTime;
    //    }
    //    else
    //    {
    //        parentScrollRect.verticalNormalizedPosition -= eventData.delta.y * SCROLL_SPEED * Time.deltaTime;
    //    }
    //}

    private ScrollRect _parentScroll;
    private bool _draggingParent = false;

    protected override void Awake()
    {
        base.Awake();
    }

    public void SetParent(ScrollRect parent)
    {
        _parentScroll = parent;
    }

    bool IsPotentialParentDrag(Vector2 inputDelta)
    {
        if (_parentScroll != null)
        {
            if (_parentScroll.horizontal && !_parentScroll.vertical)
            {
                return Mathf.Abs(inputDelta.x) > Mathf.Abs(inputDelta.y);
            }
            if (!_parentScroll.horizontal && _parentScroll.vertical)
            {
                return Mathf.Abs(inputDelta.x) < Mathf.Abs(inputDelta.y);
            }
            else return true;
        }
        return false;
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        base.OnInitializePotentialDrag(eventData);
        _parentScroll?.OnInitializePotentialDrag(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (IsPotentialParentDrag(eventData.delta))
        {
            _parentScroll.OnBeginDrag(eventData);
            _draggingParent = true;
        }
        else
        {
            base.OnBeginDrag(eventData);
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (_draggingParent)
        {
            _parentScroll.OnDrag(eventData);
        }
        else
        {
            base.OnDrag(eventData);
        }
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (_parentScroll != null && _draggingParent)
        {
            _draggingParent = false;
            _parentScroll.OnEndDrag(eventData);
        }
    }
}
