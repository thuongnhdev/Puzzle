using UnityEngine;
using UnityEngine.UI;

public class CreateAndAutoLoad : MonoBehaviour
{
    [SerializeField] GameObject objDemo;
    [SerializeField] Sprite[] arrSpriteObject;
    [SerializeField] Sprite[] arrSpriteGuideLine;
    [ContextMenu("Create and Auto Load")]
    public void CreateAndLoadImage()
    {
        for (int i = 0; i < arrSpriteObject.Length; i++)
        {
            Image imgObj = Instantiate(objDemo, transform).GetComponent<Image>();
            imgObj.sprite = arrSpriteObject[i];
            imgObj.name = imgObj.sprite.name;
            imgObj.SetNativeSize();
        }
    }
    [ContextMenu("Load GuideLine")]
    public void LoadImage()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Image img = transform.GetChild(i).GetComponent<Image>();
            img.sprite = arrSpriteGuideLine[i];
            img.SetNativeSize();
        }
    }
    [ContextMenu("Load Image Scroll")]
    public void LoadImageScroll()
    {
        for (int i = 0; i < arrSpriteObject.Length; i++)
        {
            Image imgObj = Instantiate(objDemo, transform).GetComponent<Image>();
            imgObj.transform.GetChild(0).GetComponent<Image>().sprite = arrSpriteObject[i];
            imgObj.name = imgObj.transform.GetChild(0).GetComponent<Image>().sprite.name;
            imgObj.transform.GetChild(0).GetComponent<Image>().SetNativeSize();
        }
        objDemo.gameObject.SetActive(false);
    }
    [ContextMenu("Set Image Scroll")]
    public void SetScroll()
    {
        transform.GetChild(0).localScale = new Vector3(0.2f, 0.2f, 1);        
    }
}
