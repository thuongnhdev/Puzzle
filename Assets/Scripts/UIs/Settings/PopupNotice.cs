using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupNotice : BasePopup
{
    [SerializeField] private TextMeshProUGUI msgTxt;

    public override void Init()
    {
        base.Init();

      
    }

    public override void SetData(object[] data)
    {
        base.SetData(data);

        if (data.Length > 0)
        {
            msgTxt.SetText(data[0].ToString());
        }
    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
    }

    public override void Close()
    {
        GameManager.Instance.RemoveObjList(this);
        base.Close();
    }
}
