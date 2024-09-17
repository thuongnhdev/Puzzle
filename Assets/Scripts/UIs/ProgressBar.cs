using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private RectTransform rectContainer;
    [SerializeField] private Transform dotContainer;
    [SerializeField] private RectTransform dotPrefab;

    [SerializeField] public Image barImg;

    private List<RectTransform> dots;
    private int _totalObj;
    private float _layerWidth;

    private float _progresBarWidth;
    private float _dotWidth;
    private Tween _updateTween;

    public void Init()
    {
        _progresBarWidth = rectContainer.rect.width;
        _dotWidth = dotPrefab.rect.width;

        if (dots == null)
        {
            dots = new List<RectTransform>();
        }
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void SetProgress(int totalObj)
    {
        DataCore.Debug.Log("Set Progress: " + totalObj, false); 

        this._totalObj = totalObj;
        this._layerWidth = this._progresBarWidth / totalObj;

        Reset();
        InitDots(totalObj - 1);

        Vector2 tempPos;
        for (int i = 0; i < totalObj - 1; i++)
        {
            tempPos = dots[i].anchoredPosition;
            tempPos.x = (i + 1) * this._layerWidth - _dotWidth/ 2;

            dots[i].anchoredPosition = tempPos;
            dots[i].gameObject.SetActive(true);
        }

        SetActive(true);
    }

    public void UpdateProgress(int layer)
    {
        if (_updateTween != null)
        {
            _updateTween.Kill();
            _updateTween = null;
        }

        DataCore.Debug.Log(layer + "-" + this._totalObj, false);
        float targetAmount = (float)layer / this._totalObj;
        DataCore.Debug.Log("Target Amount: " + targetAmount, false);
        _updateTween = barImg.DOFillAmount(targetAmount, MCache.Instance.Config.TIME_UPDATE_PROGRESS_BAR).Play();
    }

    private void InitDots(int dot)
    {
        if (dots.Count < dot)
        {
            int remain = dot - dots.Count;
            RectTransform newDot;
            for (int i = 0; i < remain; i++)
            {
                newDot = CreateNewDot();
                newDot.transform.SetParent(dotContainer);
                newDot.transform.localPosition = Vector3.zero;
                newDot.offsetMin = new Vector2(newDot.offsetMin.x, 0);
                newDot.offsetMax = new Vector2(newDot.offsetMax.x, 0);
                newDot.gameObject.SetActive(false);
                dots.Add(newDot);
            }
        }
    }

    public void SetProgressArtBlits(float progress)
    {
        if (_updateTween != null)
        {
            _updateTween.Kill();
            _updateTween = null;
        }

        DataCore.Debug.Log("SetProgressArtBlits: " + progress, false);
        _updateTween = barImg.DOFillAmount(progress, MCache.Instance.Config.TIME_UPDATE_PROGRESS_BAR).Play();
    }
    private RectTransform CreateNewDot()
    {
        return Instantiate(dotPrefab);
    }

    private void Reset()
    {
        if (_updateTween != null)
        {
            _updateTween.Kill();
            _updateTween = null;
        }

        for (int i = 0; i < dots.Count; i++)
        {
            dots[i].gameObject.SetActive(false);
        }

        barImg.fillAmount = 0;
    }
}
