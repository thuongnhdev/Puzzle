using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DataCore;

public class MyAllCollectionTab : MonoBehaviour
{
    [SerializeField] private GameObject containter;
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI authorTxt;
    [SerializeField] private TextMeshProUGUI authorTxtLine2;

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

    private int GetChapterComplete()
    {
        var indexComplete = 0;

        for (int i = 0; i < _data.ListChapters.Count; i++)
        {
            var partMasterData = _data.ListChapters[i];
            var puzzleMasterData = partMasterData.PuzzleLevels[partMasterData.PuzzleLevels.Count - 1];
            var puzzleStatus =
                GameData.Instance.SavedPack.GetPuzzleStatus(this._bookId, partMasterData.ID,
                    puzzleMasterData.ID);

            if (puzzleStatus == PuzzleStatus.COMPLETE)
            {
                indexComplete++;
            }
        }

        return indexComplete;
    }

    public void OnClick()
    {
        UIManager.Instance.ShowUIBookDetail(_bookId, "all_collections", UIManager.Instance.UIHomepage);
    }
}
