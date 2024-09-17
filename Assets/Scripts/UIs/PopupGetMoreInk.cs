using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataCore;

public class PopupGetMoreInk : BasePanel
{
    private BasePanel previousPanel;

    [SerializeField]
    private GameObject loginTransform;

    public override void SetData(object[] data)
    {
        base.SetData(data);
        if (data.Length > 0)
        {
            previousPanel = (BasePanel)data[0];
        }


    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        ShareUIManager.Instance.ShowSharedUI(SceneSharedEle.COIN, false);
        var login = PlayerPrefs.GetInt(ConfigManager.LoginSuccess, 0);
        loginTransform.SetActive(login != 1);

        base.Open();
    }

    public void OnMoreDealClick()
    {

        CloseWithoutShowPreviousPanel();
        if (previousPanel != null)
        {
            previousPanel.Close();
        }

        UIManager.Instance.ShowShop(previousPanel);
    }

    public void OnClickLogin()
    {
        CloseWithoutShowPreviousPanel();
        UIManager.Instance.ShowUILogin();
    }

    public void CloseWithoutShowPreviousPanel()
    {
        base.Close();
    }

    public override void Close()
    {
        GameManager.Instance.RemoveObjList(this);
        base.Close();
        if (previousPanel != null)
        {
            previousPanel.Open();
            previousPanel = null;
        }
    }

    public override void OnSwipeLeft()
    {
        Close();
    }
}
