using System;
using System.Collections;
using System.Collections.Generic;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICredits : BasePanel
{
    [SerializeField] private RectTransform[] container;

    [SerializeField] private GameObject[] objPanelShow;

    [SerializeField] private Image[] imageBtnChoice;

    [SerializeField] private Sprite sprBtnChoice;

    [SerializeField] private Sprite sprBtnNoChoice;
    public enum typePane
    {
        FAQ = 0,
        Credit = 1,
    }

    private typePane typePaneCurrent = typePane.FAQ;
    public override void Init()
    {

    }

    public override void SetData(object[] data)
    {
        base.SetData(data);

    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
        container[0].transform.localPosition = new Vector3(container[0].transform.localPosition.x, 0, container[0].transform.localPosition.z);
        container[1].transform.localPosition = new Vector3(container[1].transform.localPosition.x, 0, container[1].transform.localPosition.z);
        typePaneCurrent = typePane.FAQ;
        OpenPanel();
    }

    private void OpenPanel()
    {
        for (var i = 0; i < objPanelShow.Length; i++)
        {
            objPanelShow[i].SetActive(false);
            imageBtnChoice[i].sprite = sprBtnNoChoice;
        }
        var index = (int)typePaneCurrent;
        objPanelShow[index].SetActive(true);
        imageBtnChoice[index].sprite = sprBtnChoice;
        container[index].transform.localPosition = new Vector3(container[index].transform.localPosition.x, 0, container[index].transform.localPosition.z);


    }
    public void BtnCredit()
    {
        SoundController.Instance.PlaySfxClick();
        this.typePaneCurrent = typePane.Credit;
        OpenPanel();
    }

    public void BtnFAQ()
    {
        SoundController.Instance.PlaySfxClick();
        this.typePaneCurrent = typePane.FAQ;
        OpenPanel();
    }

    public void BtnContinuePress()
    {
        Close();
        UIManager.Instance.Fader.Hide();
    }

    public override void Close()
    {
        GameManager.Instance.RemoveObjList(this);
        base.Close();
    }

    public override void OnSwipeLeft()
    {
        Close();
    }
}
