using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BasePopup : BasePanel
{
    [SerializeField] private Transform popup;

    public override void Open()
    {
        if (_isOpen)
        {
            return;
        }

        GameManager.Instance.AddObjList(this);
        base.Open();
        DataCore.Debug.Log("Open");
        Vector3 defaultScale = Vector3.one;
        popup.localScale = Vector3.zero;

        popup.DOScale(defaultScale, MCache.Instance.Config.TIME_FADDING_POPUP).SetEase(Ease.OutBack).SetUpdate(true).Play();
    }

    public override void OnSwipeLeft()
    {
        Close();
    }

    public override void Close()
    {
        base.Close();
        GameManager.Instance.RemoveObjList(this);
    }
}
