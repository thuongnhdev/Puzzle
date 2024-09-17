using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class NumberCounter : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI txt;
    //[SerializeField] private float timeComplete = 1;

    private int currentData = 0;

    public void Reset()
    {
        currentData = 0;
    }

    public void PlayAnim(int to, float duration, Action onComplete = null)
    {
        ShowText(true);
        DOTween.To(() => currentData, x => currentData = x, to, duration).OnUpdate(() =>
        {
            txt.text = currentData.ToString();

        }).OnComplete(() =>
        {
            currentData = to;
            onComplete?.Invoke();
        });
    }

    public void ShowText(bool enable, Action onComplete = null)
    {
        if (enable)
        {
            txt.DOFade(1, 0f).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
        else
        {
            txt.DOFade(0, 1.0f).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
    }

}
