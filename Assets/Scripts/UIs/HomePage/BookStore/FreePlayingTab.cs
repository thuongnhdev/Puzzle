using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DataCore;

public class FreePlayingTab : MonoBehaviour
{
    [SerializeField] private Image thumbnail;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI partTxt;

    private int _bookId;
    private ChapterMasterData _data;
    private PuzzleLevelData _puzzleLevelData;

    public void SetData(int bookId, ChapterMasterData data , PuzzleLevelData levelData)
    {
        this._bookId = bookId;
        this._data = data;
        this._puzzleLevelData = levelData;

        if (!string.IsNullOrEmpty(data.ChapterThumbnailLabel()))
        {
            AssetManager.Instance.DownloadResource(data.ChapterThumbnailLabel(), completed: (size) =>
            {
                AssetManager.Instance.LoadPathAsync<Sprite>(this._puzzleLevelData.Thumbnail.Thumbnail, (thumb) =>
                {
                    if (thumb != null && thumbnail != null)
                    {
                        thumbnail.sprite = thumb;

                        var freePuzzleData = GameData.Instance.SavedPack.GetFreePuzzlePlaying(this._bookId, this._data.ID, this._puzzleLevelData.ID);
                        if (freePuzzleData != null)
                        {
                            if (freePuzzleData.isFinish == 0)
                                EnableGrayScaleThumbnail(true);
                            else
                                EnableGrayScaleThumbnail(false);
                        }
                     
                    }
                });
            });
        }


        var book = MasterDataStore.Instance.GetBookByID(this._bookId);
        nameTxt.SetText(book.BookName);
        var msgTextId = $"Puzzle {GetPuzzleTotal()}";
        partTxt.SetText(msgTextId);
    }

    
    private int GetPuzzleTotal()
    {
        var indexPuzzle = 0;
        var book = MasterDataStore.Instance.GetBookByID(this._bookId);
        for (int i = 0; i < book.ListChapters.Count; i++)
        {
            var partMasterData = book.ListChapters[i];
            for (int j = 0; j < partMasterData.PuzzleLevels.Count; j++)
            {
                indexPuzzle++;
                if (string.Compare(partMasterData.ID, this._data.ID) == 0 && partMasterData.PuzzleLevels[j].ID == this._puzzleLevelData.ID)
                    return indexPuzzle;
            }
        }
        return indexPuzzle;
    }

    public void OnEnable()
    {
        var freePuzzleData = GameData.Instance.SavedPack.GetFreePuzzlePlaying(this._bookId, this._data.ID, this._puzzleLevelData.ID);
        
        if (freePuzzleData == null)
            return;

        if (freePuzzleData.isFinish == 0)
            EnableGrayScaleThumbnail(true);
        else
            EnableGrayScaleThumbnail(false);
    }
    public void OnClick()
    {
        SoundController.Instance.PlaySfxClick();

        GameManager.Instance.StartLevel(_bookId, this._data.ID, this._puzzleLevelData.ID, ConfigManager.GameData.PlayType.free_puzzle);
    }

    private void EnableGrayScaleThumbnail(bool enable)
    {
        Material mat = new Material(thumbnail.material);
        mat.SetFloat("_EffectAmount", enable ? 1 : 0);
        thumbnail.material = mat;
    }
}
