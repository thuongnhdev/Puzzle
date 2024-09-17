using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectItem : MonoBehaviour
{
    [SerializeField] int indexItem;    

    public int IndexItem { get => indexItem; set => indexItem = value; }    

    public void AutoDestry()
    {
        Destroy(gameObject);
        //Camera.main.transform.GetComponent<ZoomInZoomOut>().ClickedUI = false;
    }

   
}
