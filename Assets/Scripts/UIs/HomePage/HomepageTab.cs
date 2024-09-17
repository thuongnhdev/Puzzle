using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomepageTab : MonoBehaviour
{
    [SerializeField] protected ScrollRect scrollRect;

    protected bool _isOpen;
    protected bool _didInit = false;
    public bool DidInit { get { return _didInit; } }
    public virtual void Init()
    {
    }

    public virtual void Show(bool forceRefresh = false)
    {
        if (_isOpen && !forceRefresh)
        {
            return;
        }

        _isOpen = true;
        gameObject.SetActive(true);

        AfterShowed();
    }

    protected virtual void AfterShowed()
    {

    }

    public virtual void Hide()
    {
        if (!_isOpen)
        {
            return;
        }

        _isOpen = false;
        gameObject.SetActive(false);
        UIReset();
    }

    protected virtual void UIReset()
    {

    }
}
