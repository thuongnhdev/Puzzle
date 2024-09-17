using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupPurchaseStatus : BasePopup
{
    private const string PURCHASE_SUCCESS_MESSAGE = "Purchase successfully";
    private const string PURCHASE_FAILED_MESSAGE = "Purchase unsuccessfully";

    [SerializeField] private TextMeshProUGUI msgTxt;
    [SerializeField] private Image img;

    [SerializeField] private Sprite[] sprites;

    private Action _onComplete;
    private bool _isBuySuccessful;

    public override void Init()
    {
        base.Init();
    }

    public override void SetData(object[] data)
    {
        base.SetData(data);

        if (data.Length >= 2)
        {
            _isBuySuccessful = (bool) data[0];
            _onComplete = (Action) data[1];
        }

        if (_isBuySuccessful)
        {
            msgTxt.SetText(PURCHASE_SUCCESS_MESSAGE);
            img.sprite = sprites[0];
        }
        else
        {
            msgTxt.SetText(PURCHASE_FAILED_MESSAGE);
            img.sprite = sprites[1];
        }
    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
    }

    public override void Close()
    {
        if (!_isOpen)
        {
            return;
        }
        GameManager.Instance.RemoveObjList(this);
        base.Close();
        _onComplete?.Invoke();
    }

   
}
