using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DataCore;

public class PopupLanguage : BasePopup
{
    [SerializeField] private Image[] buttomImgs;
    [SerializeField] private TextMeshProUGUI[] buttomTexts;

    [SerializeField] private Sprite[] buttonSprites;
    [SerializeField] private Color32[] textColors;

    public override void Init()
    {
        base.Init();

        OnSelectLanguage(0);
    }

    public void OnSelectLanguage(int id)
    {
        if (!ConfigManager.IdLanguege.Contains(id))
            return;

        for (int i = 0; i < buttomImgs.Length; i++)
        {
            buttomImgs[i].sprite = buttonSprites[0];
            buttomTexts[i].color = textColors[0];
        }

        buttomImgs[id].sprite = buttonSprites[1];
        buttomTexts[id].color = textColors[1];
    }

    public override void Open()
    {
        GameManager.Instance.AddObjList(this);
        base.Open();
    }

    public override void Close()
    {
        base.Close();
        GameManager.Instance.RemoveObjList(this);
    }
}
