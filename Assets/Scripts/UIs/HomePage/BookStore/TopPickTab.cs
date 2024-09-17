using UnityEngine;
using UnityEngine.UI;

public class TopPickTab : MonoBehaviour
{
    [SerializeField] private Image thumbnail;

    public int _bookId;

    public void SetData(int bookId, Sprite sprite)
    {
        this._bookId = bookId;
        thumbnail.sprite = sprite;
    }

    public void OnClick()
    {
        DataCore.Debug.Log($"TopPickTab: {_bookId}");
        SoundController.Instance.PlaySfxClick();
        UIManager.Instance.CloseUIHome();
        UIManager.Instance.ShowUIBookDetail(_bookId, "top_pick");

    }
}
