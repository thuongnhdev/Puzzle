using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using LogicUI.FancyTextRendering;


public class PopupAppUpdate : BasePopup
{
    private const float MAX_HEIGHT_BY_SCREEN = 0.8f;

    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform scrollContainer;

    [SerializeField] private TextMeshProUGUI versionTxt;
    [SerializeField] private MarkdownRenderer versionDetailTxt;

    [SerializeField] private MarkdownRenderer scrollVersionDetailTxt;


    public override void SetData(object[] data)
    {
        base.SetData(data);

        if (data.Length >= 2)
        {
            versionTxt.SetText(data[0].ToString());
            versionDetailTxt.Source = data[1].ToString();
            scrollVersionDetailTxt.Source = data[1].ToString();           
        }

        DOVirtual.DelayedCall(2.0f, () =>
        {
            float maxHeight = 1920 * 0.8f;
            float heightContainer = 100 + versionTxt.GetPreferredValues().y + versionDetailTxt.TextMesh.GetPreferredValues(852, 0).y + 150;
            versionDetailTxt.TextMesh.ForceMeshUpdate(true, true);
          
            if (heightContainer > maxHeight)
            {

                heightContainer = maxHeight;
                scrollContainer.gameObject.SetActive(true);
                versionDetailTxt.TextMesh.enabled = false;
                scrollContainer.sizeDelta = new Vector2(scrollContainer.sizeDelta.x, scrollVersionDetailTxt.TextMesh.GetPreferredValues(852, 0).y + 100);
            }
            else
            {

                scrollContainer.gameObject.SetActive(false);
                versionDetailTxt.TextMesh.enabled = true;
            }
            container.sizeDelta = new Vector2(container.sizeDelta.x, heightContainer);
        }).Play();
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
