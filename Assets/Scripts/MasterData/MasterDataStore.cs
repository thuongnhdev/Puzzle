using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.F4A.MobileThird;
public class MasterDataStore : SingletonMonoAwake<MasterDataStore>
{
    public enum TypeCollection
    {
        ALL = 0,
        Fantasy = 1,
        Dark = 2,
        Romance = 3,
        Adventure = 4,
    }
    [SerializeField]
    private List<BookMasterData> bookDatas = new List<BookMasterData>();
    
    [SerializeField] private PuzzleLevelData[] TutorialPuzzle;
    [SerializeField] private OldCollectionData oldCollectionData;
    [SerializeField]
    private HomePageStructData homePageData;

    public OldCollectionData OldCollectionData {
        get { return oldCollectionData; }
    }

    public HomePageStructData HomePageData
    {
        get { return homePageData; }
    }

    public List<BookMasterData> BookDatas
    {
        get { return bookDatas; }
    }


    private Dictionary<int, BookMasterData> _books;
    private Dictionary<string, ChapterMasterData> _parts;
    private Dictionary<string, PuzzleLevelData> _puzzleFromBookDatas;
    private Dictionary<int, List<PuzzleLevelData>> _cachedCollectionData;

    [SerializeField] private LiveEventData liveEventData;

    public LiveEventData LiveEventData
    {
        get { return liveEventData; }
    }

    public void Init()
    {
        try
        {
            _books = new Dictionary<int, BookMasterData>();
            _parts = new Dictionary<string, ChapterMasterData>();
            _puzzleFromBookDatas = new Dictionary<string, PuzzleLevelData>();
            _cachedCollectionData = new Dictionary<int, List<PuzzleLevelData>>();
            for (int i = 0; i < bookDatas.Count; i++)
            {
                _books.Add(bookDatas[i].ID, bookDatas[i]);
                for (int j = 0; j < bookDatas[i].ListChapters.Count; j++)
                {
                    _parts.Add(bookDatas[i].ListChapters[j].ID, bookDatas[i].ListChapters[j]);
                    for (int k = 0; k < bookDatas[i].ListChapters[j].PuzzleLevels.Count; k++)
                    {
                        var keyPuzzle = bookDatas[i].ListChapters[j].ID + "-" + bookDatas[i].ListChapters[j].PuzzleLevels[k].ID;

                        if (!_puzzleFromBookDatas.ContainsKey(keyPuzzle))
                            _puzzleFromBookDatas.Add(keyPuzzle, bookDatas[i].ListChapters[j].PuzzleLevels[k]);
                    }
                }
            }          
        }
        catch (System.Exception ex)
        {
            DataCore.Debug.Log($"MasterDataStore Init. Error: {ex.Message}");
        }

    }

    public List<PuzzleLevelData> GetCollectionData(int tab)
    {
        int indexTab = (int)tab;        

        if (indexTab >= HomePageData.puzzleCollectionDatas.Length) return null;
        if (_cachedCollectionData.ContainsKey(indexTab))
        {
            return _cachedCollectionData[indexTab];
        }
        else {
            var listItemData = new List<PuzzleLevelData>();
            if (tab == (int)TypeCollection.ALL)
            {
                PuzzleLevelData[] tutorials = GetPuzzleTutorials();
                if (tutorials.Length > 0)
                {
                    var puzzleLevelData = tutorials[1];
                    puzzleLevelData.ID = 1;
                    listItemData.Add(puzzleLevelData);
                }
            }

            listItemData.AddRange(HomePageData.puzzleCollectionDatas[indexTab].PuzzleLevels);

            for (int i = 0; i < 2; i++)
            {
                var name = "FakeData";
                var fakeData = ScriptableObject.CreateInstance<PuzzleLevelData>();
                fakeData.Name = name;
                listItemData.Add(fakeData);
            }

            _cachedCollectionData[indexTab] = listItemData;
            return listItemData;
        }

    }


    public PuzzleLevelData GetCollectionPuzzleByIndex(int collectionId, int collectionIndex) {
        var listPuzzle = GetCollectionData(collectionId);
        if (collectionIndex < listPuzzle.Count) {
            return listPuzzle[collectionIndex];
        }
        return null;

    }

    public BookMasterData GetBookByID(int id)
    {
        BookMasterData book = null;
        _books.TryGetValue(id, out book);

        return book;
    }

    public ChapterMasterData GetPartById(int bookId, string partId)
    {
        ChapterMasterData part = null;
        string key = bookId + "-" + partId;
        if (partId.Contains("-"))
            key = partId;

        _parts.TryGetValue(key, out part);
        return part;
    }

    public PuzzleLevelData GetPuzzleById(int bookId, string partId, int puzzleId)
    {
        PuzzleLevelData puzzle = null;
        string key = partId + "-" + puzzleId;
        _puzzleFromBookDatas.TryGetValue(key, out puzzle);

        return puzzle;
    }

    public PuzzleLevelData GetPuzzleTutorial()
    {
        return TutorialPuzzle[GameManager.Instance.CurrentTutorial];
    }

    public PuzzleLevelData[] GetPuzzleTutorials() {
        return TutorialPuzzle;

    }


}
