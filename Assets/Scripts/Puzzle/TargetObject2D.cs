using System;
using DG.Tweening;
using UnityEngine;

public class TargetObject2D : MonoBehaviour
{
    [SerializeField] LayerInfo layer;
    [SerializeField] string nameTargetObject;
    [SerializeField] bool isDoneTarget = false;

    public LayerInfo Layer { get => layer; set => layer = value; }
    public bool IsDoneTarget { get => isDoneTarget; set => isDoneTarget = value; }
    public string NameTargetObject { get => nameTargetObject; set => nameTargetObject = value; }
    public void UpdateName()
    {
        nameTargetObject = GetComponent<SpriteRenderer>().name;
    }
    public void Hide()
    {
        SpriteRenderer img = transform.GetComponent<SpriteRenderer>();

        var tempColor = img.color;
        DOVirtual.Float(tempColor.a, 0, MCache.Instance.Config.TIME_HIDE_OBJ_GUIDLINE, (x) =>
        {
            tempColor.a = x;
            img.color = tempColor;
        }).OnComplete(() =>
        {
            gameObject.SetActive(false);
        }).SetId(this);

        IsDoneTarget = true;
        layer.UpdateStageLayer();
    }
    public void Show()
    {
        try
        {
            if (transform == null) return;
            SpriteRenderer img = transform.GetComponent<SpriteRenderer>();

            if (img == null) return;

            var tempColor = img.color;
            tempColor.a = 0;
            img.color = tempColor;

            img.gameObject.SetActive(true);
            DOVirtual.Float(0.0f, 1.0f, MCache.Instance.Config.TIME_SHOW_OBJ_GUIDLINE, (x) =>
            {
                tempColor.a = x;
                if (img != null)
                    img.color = tempColor;
            }).SetId(this);
        }
        catch(Exception e)
        {
            DataCore.Debug.Log("TargetObject2D" + e);
        }
     
    }   
}
