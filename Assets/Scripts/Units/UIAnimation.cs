using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum Status
{
    In,
    Out
}

public class UIAnimation : MonoBehaviour
{

    [SerializeField] private RectTransform rect;
    [SerializeField] private Status status;
    [SerializeField] private Vector2 offest;

    private Tween currentTween;

    public void Run(Status type, Action onComplete = null)
    {
        if (status != type)
        {
            if (rect != null && currentTween == null)
            {
                status = type;
                Vector2 tempOffset = status == Status.In ? -offest : offest;
                currentTween = rect.DOAnchorPos(rect.anchoredPosition + tempOffset, 0.25f).OnComplete(() =>
                {
                    onComplete?.Invoke();
                    currentTween = null;
                });
            }
        }

    }

    public void ResetPositionToInitStatus(Status type)
    {
        if (status != type)
        {
            if (currentTween != null)
            {
                currentTween.Kill();
                currentTween = null;
            }

            if (rect != null)
            {
                status = type;
                Vector2 tempOffset = status == Status.In ? -offest : offest;
                rect.anchoredPosition = rect.anchoredPosition + tempOffset;
            }

        }
    }
}
