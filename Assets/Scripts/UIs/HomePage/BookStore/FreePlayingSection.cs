using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using DataCore;
using System;

public class FreePlayingSection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private FreePlayingTab tabPrefab;
    [SerializeField] private Transform footer;

    [SerializeField] private Transform tabContainer;

    private MasterDataStore _masterDataStore;

    private List<FreePlayingTab> _tabs;

    public void Init()
    {
        _masterDataStore = MasterDataStore.Instance;

        description.SetText(_masterDataStore.HomePageData.FreePlayingTodayDescription);

        _tabs = new List<FreePlayingTab>(MCache.Instance.Config.MAX_FREE_PUZZLE);

        //check today
        if (PlayerPrefs.GetInt(ConfigManager.KEY_FREE_PUZZLE_TODAY, 0) == 0 || PlayerPrefs.GetInt(ConfigManager.KEY_FREE_PUZZLE_TODAY, 0) != DateTime.Now.Day)
        {
            RandomPuzzle();
        }
        else
        {
            for (int i = 0; i < GameData.Instance.SavedPack.FreePuzzlePlayings.Count; i++)
            {
                FreePlayingTab newTab;
                newTab = Instantiate(tabPrefab, Vector3.zero, Quaternion.identity, tabContainer);
                var chapterData = _masterDataStore.GetPartById(GameData.Instance.SavedPack.FreePuzzlePlayings[i].BookId, GameData.Instance.SavedPack.FreePuzzlePlayings[i].PartId);
                var puzzleData = _masterDataStore.GetPuzzleById(GameData.Instance.SavedPack.FreePuzzlePlayings[i].BookId, chapterData.ID, GameData.Instance.SavedPack.FreePuzzlePlayings[i].PuzzleId);
                newTab.SetData(GameData.Instance.SavedPack.FreePuzzlePlayings[i].BookId, chapterData, puzzleData);
                _tabs.Add(newTab);
            }
        }


        DOVirtual.DelayedCall(0.0f, () =>
        {
            footer.SetAsLastSibling();
        }).Play();

    }

    private void RandomPuzzle()
    {
        GameData.Instance.SavedPack.FreePuzzlePlayings.Clear();

        var sizeFreePlay = MCache.Instance.Config.MAX_FREE_PUZZLE;
        for (int i = 0; i < sizeFreePlay; i++)
        {
            var indexBook = UnityEngine.Random.Range(1, MCache.Instance.Config.MAX_BOOK_SIZE);
            var bookData = _masterDataStore.GetBookByID(indexBook);
            if (bookData != null)
            {
                var indexChapter = UnityEngine.Random.Range(0, bookData.ListChapters.Count - 1);
                var chapterData = _masterDataStore.GetPartById(bookData.ID, bookData.ListChapters[indexChapter].ID);
                if (chapterData != null)
                {
                    var indexPuzzle = UnityEngine.Random.Range(0, chapterData.PuzzleLevels.Count - 1);

                    FreePuzzlePlaying freePuzzlePlaying = new FreePuzzlePlaying(bookData.ID, chapterData.ID, chapterData.PuzzleLevels[indexPuzzle].ID, 0);

                    var itemPuzzle = GameData.Instance.SavedPack.FreePuzzlePlayings.Find(it => it.PuzzleId == chapterData.PuzzleLevels[indexPuzzle].ID);
                    if (itemPuzzle == null && !GameData.Instance.SavedPack.FreePuzzlePlayings.Contains(freePuzzlePlaying))
                    {

                        FreePlayingTab newTab;

                        GameData.Instance.SavedPack.FreePuzzlePlayings.Add(freePuzzlePlaying);

                        var puzzleData = _masterDataStore.GetPuzzleById(bookData.ID, chapterData.ID, chapterData.PuzzleLevels[indexPuzzle].ID);
                        newTab = Instantiate(tabPrefab, Vector3.zero, Quaternion.identity, tabContainer);
                        newTab.SetData(bookData.ID, chapterData, puzzleData);
                        _tabs.Add(newTab);
                    }
                    else
                        sizeFreePlay++;
                }
            }

        }
        GameData.Instance.RequestSaveGame();
        PlayerPrefs.SetInt(ConfigManager.KEY_FREE_PUZZLE_TODAY, DateTime.Now.Day);
        PlayerPrefs.Save();
    }
}
