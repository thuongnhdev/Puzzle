using System;
using System.Collections;
using System.Collections.Generic;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UILoading : BasePanel
{
    [SerializeField] private bool showHintText;
    [SerializeField] private Image imgLoadingProgress;
    [SerializeField] private TextMeshProUGUI txtHintText;

    private Action onLoadingComplete;
    private List<string> hintTextData = new List<string>();
    private float timeToNextHint = 0;
    private float minLoadingTime = 2.0f;

    private float waittingProgress = 0;
    private float mainProgress = 0;
    private float currentWaitingTime = 0;
    private bool startWaitingForMinLoading = false;
    private IEnumerator showHint;
    //private IEnumerator waitting;

    public override void Init()
    {
        hintTextData.Clear();
        hintTextData.AddRange(MCache.Instance.Config.HINT_TEXT);
        showHint = ShowHintText();
        //waitting = WaitingForMinLoadingTime();
    }

    public override void SetData(object[] data)
    {
        base.SetData(data);
        minLoadingTime = (float)data[0];
        onLoadingComplete = (Action)data[1];
    }

    private void Update()
    {
        if (!startWaitingForMinLoading)
        {
            return;
        }

        currentWaitingTime += Time.deltaTime;
        waittingProgress = Math.Min(0.5f, currentWaitingTime / minLoadingTime * 0.5f);
        float total = waittingProgress + mainProgress;
        //DataCore.Debug.Log("Waitting: " + waittingProgress + ", currentWaitingTime: " + currentWaitingTime);
        UpdateProgressBar(total);
        if (waittingProgress >= 0.5f)
        {
            startWaitingForMinLoading = false;
        }

    }

    /// <summary>
    /// This value must be in range [0-1]
    /// </summary>
    /// <param name="value">[0-1]</param>
    public void UpdateProgress(float value)
    {
        mainProgress = value * 0.5f;  // this UpdateProgress max is 50% of total progress, 50% left is minLoadingTime.
        float total = waittingProgress + mainProgress;
        //DataCore.Debug.Log("UpdateProgress: " + mainProgress);
        UpdateProgressBar(total);

    }

    private IEnumerator ShowHintText()
    {
        if (!showHintText)
        {
            txtHintText.text = "";
            yield break;
        }
        List<string> txtLoadingList = hintTextData;
        while (true)
        {
            timeToNextHint -= Time.deltaTime;
            if (timeToNextHint <= 0)
            {
                timeToNextHint = 1;
                if (txtLoadingList.Count > 0)
                {
                    var index = UnityEngine.Random.Range(0, txtLoadingList.Count);
                    txtHintText.text = txtLoadingList[index];
                    txtLoadingList.RemoveAt(index);
                }
            }

            yield return null;
        }

    }

    private IEnumerator WaitingForMinLoadingTime()
    {

        while (true)
        {
            currentWaitingTime += Time.deltaTime;
            waittingProgress = currentWaitingTime / minLoadingTime * 0.5f;
            float total = waittingProgress + mainProgress;
            //DataCore.Debug.Log("Waitting: " + waittingProgress + ", currentWaitingTime: " + currentWaitingTime);
            UpdateProgressBar(total);
            if (waittingProgress >= 0.5f)
            {
                yield break;
            }

            yield return null;
        }
    }

    private void UpdateProgressBar(float value)
    {
        imgLoadingProgress.fillAmount = value;
        if (value >= 1)
        {
            startWaitingForMinLoading = false;
            onLoadingComplete?.Invoke();            
            Close();
        }
    }

    public override void Open()
    {
        base.Open();

        waittingProgress = 0;
        mainProgress = 0;
        currentWaitingTime = 0;
        startWaitingForMinLoading = true;
        UpdateProgress(0);

        StartCoroutine(showHint);
        Application.backgroundLoadingPriority = ThreadPriority.High;
        //StartCoroutine(waitting);
    }

    public override void Close()
    {
        DataCore.Debug.Log("UILoading Close");

        Application.backgroundLoadingPriority = ThreadPriority.Normal;
        StopCoroutine(showHint);
        //StopCoroutine(waitting);
        waittingProgress = 0;
        mainProgress = 0;
        currentWaitingTime = 0;
        base.Close();

    }


}
