using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DataCore;

public class YourCollectionTab : MonoBehaviour
{
    [SerializeField] private GameObject containter;
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI authorTxt;
    [SerializeField] private TextMeshProUGUI authorTxtLine2;
    [SerializeField] private TextMeshProUGUI chapterComplete;

    private int _bookId;
    private BookMasterData _data;

    public void SetActive(bool isActive)
    {
        containter.SetActive(isActive);
    }

    public void SetData(int bookId, BookMasterData data)
    {
        this._bookId = bookId;
        this._data = data;

        AssetManager.Instance.LoadPathAsync<Sprite>(data.Thumbnail.Thumbnail, (thumb) =>
        {
            if (thumb != null && thumbnail != null)
            {
                thumbnail.sprite = thumb;
            }
        });

        nameTxt.SetText(data.BookName);
        var authorMsg = "by " + data.Author;

        authorTxt.gameObject.SetActive(true);
        authorTxtLine2.gameObject.SetActive(false);
        authorTxt.SetText(authorMsg);
        authorTxtLine2.SetText(authorMsg);

        var chapterComp = GetPuzzleComplete() + "/" + GetPuzzleTotal();
        chapterComplete.SetText(chapterComp);

        var tile = (float)Screen.height / Screen.width;
        if (tile > 1.5f && authorMsg.Length > 25)
        {
            authorTxt.gameObject.SetActive(false);
            authorTxtLine2.gameObject.SetActive(true);
        }
        else if (authorMsg.Length > 30)
        {
            authorTxt.gameObject.SetActive(false);
            authorTxtLine2.gameObject.SetActive(true);
        }
    }

    private int GetPuzzleTotal()
    {
        var indexPuzzle = 0;

        for (int i = 0; i < _data.ListChapters.Count; i++)
        {
            var partMasterData = _data.ListChapters[i];
            for(int j = 0;j< partMasterData.PuzzleLevels.Count;j++)
            {
                indexPuzzle++;
            }    
        }

        return indexPuzzle;
    }

    private int GetPuzzleComplete()
    {
        var indexComplete = 0;

        List<LastPuzzlePlay> itemlist = GameData.Instance.SavedPack.MyLibraryPuzzlePlays;
        foreach (var item in itemlist)
        {
            if (item.BookId == this._bookId)
            {
                indexComplete++;
            }
        }

        return indexComplete;
    }

    public void OnClick()
    {
        UIManager.Instance.ShowUIBookDetail(_bookId, "your_collection", UIManager.Instance.UIHomepage);
    }
}
