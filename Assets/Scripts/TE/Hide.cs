using DG.Tweening;
using UnityEngine;

public class Hide : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        HideObj();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void HideObj()
    {
        SpriteRenderer img = transform.GetComponent<SpriteRenderer>();
        var tempColor = img.color;        
        DOVirtual.Float(0.0f, 1.0f, 2, (x) =>
         {
             tempColor.a = x;
             img.color = tempColor;
         }).OnComplete(() =>
         {
             //layer.UpdateStageLayer();
             //gameObject.SetActive(false);
         }).SetId(this);
    }
}
