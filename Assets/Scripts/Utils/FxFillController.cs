using UnityEngine;

public class FxFillController : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Animator anim;
    ZoomInZoomOut m_Zoom;
    private void Awake()
    {
        m_Zoom = Camera.main.GetComponent<ZoomInZoomOut>();
    }
    public void ShowFX(Sprite sprite, int orderLayer)
    {
        gameObject.SetActive(true);
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = orderLayer;
        if (m_Zoom != null)
        {
            m_Zoom.Bol_AllowZooom = false;
        }
        anim.SetTrigger("run");
    }
    public void Hide()
    {
        if (m_Zoom != null)
        {
            m_Zoom.Bol_AllowZooom = true;
        }
        gameObject.SetActive(false);
    }
}
