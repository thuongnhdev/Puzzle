using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class FaderController : MonoBehaviour
{
    Image img;

    public void Init()
    {
        img = GetComponent<Image>();
    }

    public void FadeShow(float duration = 1.0f, Action onComplete = null)
    {
        DataCore.Debug.Log($"Fader FadeShow: {duration}", false);
        try
        {
            var tempColor = img.color;
            DOVirtual.Float(1.0f, 0.0f, duration, (x) =>
            {
                tempColor.a = x;
                img.color = tempColor;
            }).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
        catch (Exception)
        {
            DataCore.Debug.LogError("img " + gameObject.name + "//" + gameObject.transform.parent.name);
        }
       
    }
    public void FadeHide(float duration = 1.0f, Action onComplete = null)
    {
        DataCore.Debug.Log($"Fader FadeHide: {duration}", false);
        var tempColor = img.color;
        DOVirtual.Float(0.0f, 1.0f, duration, (x) =>
        {
            tempColor.a = x;
            img.color = tempColor;
        }).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }
    public void Hide()
    {
        DataCore.Debug.Log("Fader Hide", false);
        gameObject.SetActive(false);
    }
    public void Show()
    {
        DataCore.Debug.Log("Fader Show", false);
        gameObject.SetActive(true);
    }
}
