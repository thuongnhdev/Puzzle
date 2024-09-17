using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DataCore;

public class LatestUpdateTab : MonoBehaviour
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI partTxt;

    private int _bookId;
    private ChapterMasterData _data;


    public void SetData(int bookId, ChapterMasterData data)
    {
        this._bookId = bookId;
        this._data = data;
        if (!string.IsNullOrEmpty(data.ChapterThumbnailLabel())) {
            AssetManager.Instance.DownloadResource(data.ChapterThumbnailLabel(), completed: (size) =>
            {
                AssetManager.Instance.LoadPathAsync<Sprite>(data.Thumbnail.Thumbnail, (thumb) =>
                {
                    if (thumb != null && thumbnail != null)
                    {
                        thumbnail.sprite = thumb;
                    }
                });
            });
        }

        
        var book = MasterDataStore.Instance.GetBookByID(this._bookId);
        nameTxt.SetText(book.BookName);
        partTxt.SetText(data.PartName);
    }

    public void OnClick()
    {
        SoundController.Instance.PlaySfxClick();

        UIManager.Instance.ShowUIPartDetail(_data, _bookId, UIManager.Instance.UIHomepage, "latest_update");
    }
}
