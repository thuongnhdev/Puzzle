using DataCore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using com.F4A.MobileThird;

public class ResumePlayingTab : MonoBehaviour
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI partTxt;
    [SerializeField] private TextMeshProUGUI lastPlayTxt;

    private int _bookId;
    private string _partId;
    private int _puzzleId;

    public void SetData(LastPuzzlePlay lastData, PuzzleLevelData data)
    {
        this._bookId = lastData.BookId;
        this._partId = lastData.PartId;
        this._puzzleId = lastData.PuzzleId;
        AssetManager.Instance.DownloadResource(data.ThumbnailLabel(), completed: (size) =>
        {
            AssetManager.Instance.LoadPathAsync<Sprite>(data.Thumbnail.Thumbnail, (thumb) =>
            {
                if (thumb != null && thumbnail != null)
                {
                    thumbnail.sprite = thumb;
                }
            });
        });

        var book = MasterDataStore.Instance.GetBookByID(this._bookId);
        var name = data.Name.Replace("_", " ");
        nameTxt.SetText(book.BookName);

        string[] x = _partId.Split('-');
        partTxt.SetText("Chapter " + x[1] + " - puzzle " + _puzzleId);
        lastPlayTxt.SetText(UIManager.ConvertReleaseDay(lastData.lastTimePlay, "play"));
    }

    public void OnClick()
    {
        SoundController.Instance.PlaySfxClick();
        // Play Puzzle
        GameManager.Instance.StartLevel(_bookId, _partId, _puzzleId, ConfigManager.GameData.PlayType.resume_puzzle);
        //  Show intertial ads
        //AdsService.Instance.ShowInterstitial(AdsService.IntAdPlacementResume, (complete) =>
        //{
        //    SoundController.Instance.MuteBgMusic(false);
        //});
    }
}
