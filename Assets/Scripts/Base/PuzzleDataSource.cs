using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using com.F4A.MobileThird;
using UnityEditor;
using UnityEngine.AddressableAssets;
using System;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.ResourceManagement.AsyncOperations;
using Spine.Unity;
using System.Linq;

public class PuzzleDataSource : SingletonMono<PuzzleDataSource>
{
    private List<BookMasterData> bookMasterDatas = new List<BookMasterData>();

    private List<ChapterMasterData> chapterMasterDatas = new List<ChapterMasterData>();

    private List<PuzzleLevelData> puzzleMasterDatas = new List<PuzzleLevelData>();

    private long timeRelease = 0;

    [SerializeField]
    private string pathFolder = "Assets/ScriptableObjects/";

    [SerializeField]
    private string pathFolderCollectionAsset = "Assets/ScriptableObjects/Collection/";

    [SerializeField]
    private string pathFolderBookAsset = "Assets/ScriptableObjects/Books/Books/";

    [SerializeField]
    private string pathFolderBookThumbnailsAsset = "Assets/ScriptableObjects/Books/Thumbnails/";

    [SerializeField]
    private string pathFolderChapterAsset = "Assets/ScriptableObjects/Chapters/Chapters/";

    [SerializeField]
    private string pathFolderChapterThumbnailsAsset = "Assets/ScriptableObjects/Chapters/Thumbnails/";

    [SerializeField]
    private string pathFolderPuzzleAsset = "Assets/ScriptableObjects/Puzzles/Puzzles/";

    [SerializeField]
    private string pathFolderPuzzleMusicAsset = "Assets/ScriptableObjects/Puzzles/BackgroundMusic/";

    [SerializeField]
    private string pathFolderPuzzleThumbnailsAsset = "Assets/ScriptableObjects/Puzzles/Thumbnails/";

    [SerializeField]
    private string pathFolderPuzzleCompletePhotosAsset = "Assets/ScriptableObjects/Puzzles/Photos/";

    [SerializeField]
    private string pathFolderPuzzleSpineAsset = "Assets/ScriptableObjects/Puzzles/Spines/";

    [SerializeField]
    private string pathThumbnailBookAsset = "Assets/Bundles/Thumbnails/Local/Book/";

    [SerializeField]
    private string pathThumbnailChapterAsset = "Assets/Bundles/Thumbnails/Puzzle/Small/";

    [SerializeField]
    private string pathThumbnailPuzzleAsset = "Assets/Bundles/Thumbnails/Puzzle/Small/";

    [SerializeField]
    private string pathThumbnailCompletePuzzleAsset = "Assets/Bundles/Thumbnails/Puzzle/Large/";

    [SerializeField]
    private string pathBackgroundMusic = "Assets/Bundles/Music/";

    [SerializeField]
    private string pathPrefabPuzzle = "Assets/Bundles/Puzzles/";

    [SerializeField]
    private string pathAnimationPuzzleAsset = "Assets/Bundles/Spines/";

    [SerializeField]
    private string pathFolderPuzzleCollectionAsset = "Assets/ScriptableObjects/Puzzles/Puzzles/";

    private string FilePng = ".png";
    private string FileAsset = ".asset";
    private string FileAudioClip = ".mp3";
    private string FileSkeleton = "SkeletonData";
    private string FilePrefab = ".prefab";
    private void Start()
    {
        this.timeRelease = DateTime.Now.Ticks;

        InitDataBookSource();
    }
    public void InitDataBookSource()
    {
        StartCoroutine(FirebaseManager.Instance.DownloadBytes(FirebaseManager.Instance.bookFilename, onComplete =>
        {
            CreateBookAsset(onComplete);
        }));
    }

    public void InitDataChapterSource()
    {
        StartCoroutine(FirebaseManager.Instance.DownloadBytes(FirebaseManager.Instance.partFilename, onComplete =>
        {
            CreateChapterAsset(onComplete);
        }));
    }

    public void InitDataPuzzleSource()
    {
        StartCoroutine(FirebaseManager.Instance.DownloadBytes(FirebaseManager.Instance.puzzleFilename, onComplete =>
        {
            CreatePuzzleAsset(onComplete);
        }));
    }

    public void InitDataPuzzleCollection()
    {
        StartCoroutine(FirebaseManager.Instance.DownloadBytes(FirebaseManager.Instance.puzzleCollectionFilename, onComplete =>
        {
            CreatePuzzleCollection(onComplete);
        }));
    }

    private Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();

    public void CreateBookAsset(string data)
    {
        bookMasterDatas.Clear();
        GetBookData myObject = JsonUtility.FromJson<GetBookData>(data);
        for (var i = 0; i < myObject.baseAbilities.Count; i++)
        {
            string bookName = myObject.baseAbilities[i].BookName;
            bookName = bookName.Replace(" ", "_");
            var nameAsset = bookName + FileAsset;
            string path = this.pathFolderBookAsset + nameAsset;
            var nameThumbnailsAsset = myObject.baseAbilities[i].Thumbnails + FileAsset;
            string pathThumbnails = this.pathFolderBookThumbnailsAsset + nameThumbnailsAsset;
            coroutineQueue.Enqueue(OnCreateAssetBook(path, pathThumbnails, myObject.baseAbilities[i]));
        }
        StartCoroutine(CoroutineCoordinator(() =>
        {
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
            InitDataChapterSource();
        }));

    }

    IEnumerator CoroutineCoordinator(Action completed)
    {
        while (coroutineQueue.Count > 0)
            yield return StartCoroutine(coroutineQueue.Dequeue());

        completed?.Invoke();
        yield return null;
        
    }


    private IEnumerator OnCreateAssetBook(string path, string pathThumbnail, BookDataItem baseAbility)
    {
        Debug.Log(path);
#if UNITY_EDITOR
        ThumbnailsData assetThumbnail = (ThumbnailsData) AssetDatabase.LoadAssetAtPath(pathThumbnail, typeof(ThumbnailsData));
        if (assetThumbnail == null) {
            assetThumbnail = ScriptableObject.CreateInstance<ThumbnailsData>();
            AssetDatabase.CreateAsset(assetThumbnail, pathThumbnail);
        }
        string pathSprite = this.pathThumbnailBookAsset + baseAbility.Thumbnails + FilePng;
        assetThumbnail.Thumbnail = pathSprite;

        Selection.activeObject = assetThumbnail;
        EditorUtility.SetDirty(assetThumbnail);
        AssetDatabase.SaveAssets();

        BookMasterData asset = (BookMasterData) AssetDatabase.LoadAssetAtPath(pathThumbnail, typeof(BookMasterData));
        if (asset == null) {
            asset = (BookMasterData) ScriptableObject.CreateInstance<BookMasterData>();
            AssetDatabase.CreateAsset(asset, path);
        }
        asset.BookName = baseAbility.BookName;
        asset.Author = baseAbility.Author;
        asset.ID = int.Parse(baseAbility.BookId);
        asset.Description = baseAbility.Description;
        asset.Illustration = baseAbility.Illustrator;
        asset.Status = BookStatus.OnGoing;
        asset.Release = this.timeRelease;
        asset.Version = baseAbility.Version;
        asset.Thumbnail = assetThumbnail;
        asset.ListChapters.Clear();
        //AssetDatabase.Refresh();
        this.bookMasterDatas.Add(asset);
        Selection.activeObject = asset;
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

#endif
        yield return new WaitForEndOfFrame();
    }

    private void OnUpdateAssetBook(BookMasterData book, BookDataItem baseAbility)
    {
        book.BookName = baseAbility.BookName;
        book.Author = baseAbility.Author;
        book.ID = int.Parse(baseAbility.BookId);
        book.Description = baseAbility.Description;
        book.Illustration = baseAbility.Illustrator;
        book.Status = BookStatus.OnGoing;
        book.Release = this.timeRelease;
        string pathSprite = baseAbility.Thumbnails;
        book.Version = baseAbility.Version;

#if UNITY_EDITOR
        Selection.activeObject = book;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
#elif UNITY_IOS
          
#else
    
#endif

        this.bookMasterDatas.Add(book);
    }


    private static T isCheckAsset<T>(string pathAsset) where T : ScriptableObject
    {
#if UNITY_EDITOR
        var oldItem = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(pathAsset);
        string path = AssetDatabase.GetAssetPath(oldItem);

        T asset = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;

        return asset;
#elif UNITY_IOS
          return null;
#else
    return null;
#endif
    }

    public void CreateChapterAsset(string data)
    {
        chapterMasterDatas.Clear();
        DataCore.Debug.Log(data);
        GetChapterData myObject = JsonUtility.FromJson<GetChapterData>(data); ;
        for (var i = 0; i < myObject.baseAbilities.Count; i++)
        {
            string bookName = myObject.baseAbilities[i].BookName;
            bookName = bookName.Replace(" ", "_");
            string chapterName = myObject.baseAbilities[i].ChapterName;
            chapterName = chapterName.Replace(" ", "_");
            var nameAsset = bookName + "_" + chapterName + FileAsset;
            string path = this.pathFolderChapterAsset + nameAsset;
            var nameAssetThumbnail = bookName + "_" + chapterName + FileAsset;
            string pathThumbnail = this.pathFolderChapterThumbnailsAsset + nameAssetThumbnail;
            coroutineQueue.Enqueue(OnCreateChapterAsset(path, pathThumbnail, myObject.baseAbilities[i]));
        }

        StartCoroutine(CoroutineCoordinator(() =>
        {
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif
            InitDataPuzzleSource();
        }));

    }

    private IEnumerator OnCreateChapterAsset(string path, string pathThumbnail, ChapterDataItem chapterDataItem)
    {
#if UNITY_EDITOR
            
        ThumbnailsData assetThumbnail = (ThumbnailsData) AssetDatabase.LoadAssetAtPath(pathThumbnail, typeof(ThumbnailsData));
        if (assetThumbnail == null) {
            assetThumbnail = ScriptableObject.CreateInstance<ThumbnailsData>();
            AssetDatabase.CreateAsset(assetThumbnail, pathThumbnail);
        }
  
        string pathSprite = this.pathThumbnailChapterAsset + chapterDataItem.Thumbnails + FilePng;
        assetThumbnail.Thumbnail = pathSprite;
        Selection.activeObject = assetThumbnail;
        EditorUtility.SetDirty(assetThumbnail);
        AssetDatabase.SaveAssets();

        ChapterMasterData asset = (ChapterMasterData) AssetDatabase.LoadAssetAtPath(pathThumbnail, typeof(ChapterMasterData));
        if (asset == null) {
            asset = ScriptableObject.CreateInstance<ChapterMasterData>();
            AssetDatabase.CreateAsset(asset, path);
        }
        asset.PuzzleLevels.Clear();
        var chapterId = chapterDataItem.BookId + "-" + chapterDataItem.ChapterId;
        asset.ID = chapterId;
        Debug.Log($"Chapter: {chapterId}");
        asset.Author = chapterDataItem.Author;
        asset.PartName = chapterDataItem.ChapterName;
        asset.Description = chapterDataItem.Description;
        asset.Price = Int32.Parse(chapterDataItem.Price);
        asset.Thumbnail = assetThumbnail;
        asset.Version = chapterDataItem.Version;
        this.chapterMasterDatas.Add(asset);
        Selection.activeObject = asset;
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

        // Add part into list book
        var iTemBook = this.bookMasterDatas.Find(b => b.ID == Int32.Parse(chapterDataItem.BookId));
        if (iTemBook != null)
        {
            iTemBook.ListChapters.Add(asset);
            Selection.activeObject = iTemBook;
            EditorUtility.SetDirty(iTemBook);
            AssetDatabase.SaveAssets();
        }

#endif

        yield return new WaitForEndOfFrame();
    }

    public void CreatePuzzleAsset(string data)
    {
        this.puzzleMasterDatas.Clear();
        GetPuzzleData myObject = JsonUtility.FromJson<GetPuzzleData>(data);
        for (var i = 0; i < myObject.baseAbilities.Count; i++)
        {
            var nameAsset = myObject.baseAbilities[i].PuzzleName + FileAsset;
            string path = this.pathFolderPuzzleAsset + nameAsset;
            var nameAssetThumbnail = myObject.baseAbilities[i].PuzzleName + FileAsset;
            string pathThumbnail = this.pathFolderPuzzleThumbnailsAsset + nameAssetThumbnail;
            var nameAssetThumbnailComplete = myObject.baseAbilities[i].PuzzleName + FileAsset;
            string pathThumbnailComplete = this.pathFolderPuzzleCompletePhotosAsset + nameAssetThumbnailComplete;
            var nameAssetBgMusic = myObject.baseAbilities[i].BackgroundMusic ;
            string pathBgMusic = this.pathFolderPuzzleMusicAsset + nameAssetBgMusic + FileAsset;
            var nameAssetSpine = myObject.baseAbilities[i].PuzzleName + "_" + FileSkeleton + FileAsset;
            string pathSpine = this.pathFolderPuzzleSpineAsset + nameAssetSpine;
            coroutineQueue.Enqueue(OnCreateAssetPuzzle(path, pathThumbnail, pathThumbnailComplete, pathBgMusic, nameAssetBgMusic, pathSpine, myObject.baseAbilities[i]));            
        }
        StartCoroutine(CoroutineCoordinator(() =>
        {
#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

            InitDataPuzzleCollection();
        }));
    }

    public void CreatePuzzleCollection(string data)
    {
        Debug.Log($"CreatePuzzleCollection: {data}");
//#if UNITY_EDITOR
        GetCollectionData myObject = JsonUtility.FromJson<GetCollectionData>(data);

        //list collection
        List<string> nameCollection = new List<string>();
        for(int i = 0;i<myObject.baseAbilities.Count;i++)
        {
            if (!nameCollection.Contains(myObject.baseAbilities[i].CollectionName))
                nameCollection.Add(myObject.baseAbilities[i].CollectionName);
        }

        for(int i = 0; i< nameCollection.Count;i++)
        {
            var name = nameCollection[i] + FileAsset;
            var pathCollection = this.pathFolderCollectionAsset + name;
            coroutineQueue.Enqueue(OnCreatePuzzleCollectionAsset(pathCollection, nameCollection[i], myObject.baseAbilities));
          
        }

//#endif
        StartCoroutine(CoroutineCoordinator(() =>
        {
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
                }));

    }

    private int DefinePuzzleEmpty = 4;
    private IEnumerator OnCreatePuzzleCollectionAsset(string path,string nameCollection,  List<CollectionDataItem> itemList)
    {
#if UNITY_EDITOR
        AssetDatabase.DeleteAsset(path);

        PuzzleCollectionData assetCollection = ScriptableObject.CreateInstance<PuzzleCollectionData>();
        AssetDatabase.CreateAsset(assetCollection, path);
        

        for (var j = 0; j < itemList.Count; j++)
        {
            if (nameCollection == itemList[j].CollectionName)
            {
                var itemAsset = this.puzzleMasterDatas.Find(name => name.Name.Equals(itemList[j].PuzzleName));
                if (itemAsset != null)
                {
                    assetCollection.PuzzleLevels.Add(itemAsset);
                }
            }
        }

        Selection.activeObject = assetCollection;
        EditorUtility.SetDirty(assetCollection);
        AssetDatabase.SaveAssets();
#endif

        yield return new WaitForEndOfFrame();
    }

    private void isUpdatePuzzleAsset(string path, string pathThumbnail, string pathThumbnailComplete, string pathBgMusic, string nameAssetBgMusic, string pathSpine, PuzzleDataItem puzzleDataItem)
    {
        OnCreateAssetPuzzle(path, pathThumbnail, pathThumbnailComplete, pathBgMusic, nameAssetBgMusic, pathSpine, puzzleDataItem);
        //PuzzleLevelData puzzle = isCheckAsset<PuzzleLevelData>(path);
        //if (puzzle == null)
        //    OnCreateAssetPuzzle(path, puzzleDataItem);
        //else if (puzzle.Version != puzzleDataItem.Version)
        //    OnUpdateAssetPuzzle(puzzle, puzzleDataItem);
    }

    private IEnumerator OnCreateAssetPuzzle(string path, string pathThumbnail, string pathThumbnailComplete, string pathBgMusic, string nameAssetBgMusic,  string pathSpine, PuzzleDataItem puzzleDataItem)
    {
        DataCore.Debug.Log($"{puzzleDataItem.PuzzleName}");
#if UNITY_EDITOR
        ThumbnailsData assetThumbnail = (ThumbnailsData) AssetDatabase.LoadAssetAtPath(pathThumbnail, typeof(ThumbnailsData));
        if (assetThumbnail == null) {
            assetThumbnail = ScriptableObject.CreateInstance<ThumbnailsData>();
            AssetDatabase.CreateAsset(assetThumbnail, pathThumbnail);
        }
       
        string pathSprite = this.pathThumbnailPuzzleAsset + puzzleDataItem.PuzzleName + FilePng;
        assetThumbnail.Thumbnail = pathSprite;
        Selection.activeObject = assetThumbnail;
        EditorUtility.SetDirty(assetThumbnail);
        AssetDatabase.SaveAssets();

        CompletePuzzleImageData assetThumbnailComplete = (CompletePuzzleImageData) AssetDatabase.LoadAssetAtPath(pathThumbnail, typeof(CompletePuzzleImageData));
        if (assetThumbnailComplete == null) {
            assetThumbnailComplete = ScriptableObject.CreateInstance<CompletePuzzleImageData>();
            AssetDatabase.CreateAsset(assetThumbnailComplete, pathThumbnailComplete);
        }
        
        string pathCompleteSprite = $"{this.pathThumbnailCompletePuzzleAsset}{puzzleDataItem.PuzzleName}{FilePng}";
        assetThumbnailComplete.CompleteImage = pathCompleteSprite;
        Selection.activeObject = assetThumbnailComplete;
        EditorUtility.SetDirty(assetThumbnailComplete);
        AssetDatabase.SaveAssets();

        BackgroundMusicData assetBgMusic = (BackgroundMusicData) AssetDatabase.LoadAssetAtPath(pathThumbnail, typeof(BackgroundMusicData));
        if (assetBgMusic == null) {
            assetBgMusic = ScriptableObject.CreateInstance<BackgroundMusicData>();
            AssetDatabase.CreateAsset(assetBgMusic, pathBgMusic);
        }        
        
        string pathMusic = this.pathBackgroundMusic + puzzleDataItem.BackgroundMusic + FileAudioClip;
        assetBgMusic.AddressPath = pathMusic;
        assetBgMusic.Name = nameAssetBgMusic;
        Selection.activeObject = assetBgMusic;
        EditorUtility.SetDirty(assetBgMusic);
        AssetDatabase.SaveAssets();

        SpineData assetSpine = (SpineData) AssetDatabase.LoadAssetAtPath(pathThumbnail, typeof(SpineData));
        if (assetSpine == null) {
            assetSpine = ScriptableObject.CreateInstance<SpineData>();
            AssetDatabase.CreateAsset(assetSpine, pathSpine);
        }


        string pathSpineData = $"{this.pathAnimationPuzzleAsset}Spine_{puzzleDataItem.PuzzleName}/{puzzleDataItem.PuzzleName}_{FileSkeleton}{FileAsset}";
        assetSpine.animation = pathSpineData;
        Selection.activeObject = assetSpine;
        EditorUtility.SetDirty(assetSpine);
        AssetDatabase.SaveAssets();

        PuzzleLevelData asset = (PuzzleLevelData) AssetDatabase.LoadAssetAtPath(pathThumbnail, typeof(PuzzleLevelData));
        if (asset == null) {
            asset = ScriptableObject.CreateInstance<PuzzleLevelData>();
            AssetDatabase.CreateAsset(asset, path);
        }


        asset.ID = Int32.Parse(puzzleDataItem.PuzzleId);
        asset.Level = Int32.Parse(puzzleDataItem.ChapterId);
        asset.Name = puzzleDataItem.PuzzleName;
        asset.Desc = puzzleDataItem.Description;
        asset.Ink = Int32.Parse(puzzleDataItem.RewardInks);
        asset.Thumbnail = assetThumbnail;
        asset.CompletePuzzleImage = assetThumbnailComplete;
        asset.BGMusic = assetBgMusic;
        asset.Animation = assetSpine;
        asset.Version = puzzleDataItem.Version;

        //ThumbnailsData thumbnailNew = isCheckAsset<ThumbnailsData>(pathThumbnail);
        //if (thumbnailNew == null && asset.Thumbnail == null)
        //{
        //    ThumbnailsData assetThumbnailNew = ScriptableObject.CreateInstance<ThumbnailsData>();
        //    AssetDatabase.CreateAsset(assetThumbnailNew, pathThumbnail);
        //    string pathSpriteNew = this.pathThumbnailPuzzleAsset + puzzleDataItem.PuzzleName;
        //    UnityEngine.Object sprTNew = AssetDatabase.LoadAssetAtPath(pathSpriteNew + FilePng, typeof(Sprite));

        //    if (sprTNew != null)
        //    {
        //        assetThumbnail.Thumbnail = pathThumbnail;
        //        asset.Thumbnail = assetThumbnail;
        //        assetThumbnailComplete.CompleteImage = pathSpriteNew;
        //        asset.CompletePuzzleImage = assetThumbnailComplete;
        //    }
        //}

        string pathPrefab = $"{this.pathPrefabPuzzle}{puzzleDataItem.PuzzleName}/{puzzleDataItem.PuzzleName}{FilePrefab}";
        asset.PuzzlePrefabAddress = pathPrefab;
        Selection.activeObject = asset;
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

        var IdCompare = puzzleDataItem.BookId + "-" + puzzleDataItem.ChapterId;
        // Add part into list book
        //Debug.Log($"IdCompare {IdCompare}");
        var iTemPart = this.chapterMasterDatas.Find(p => p.ID == IdCompare);
        if (iTemPart != null)
        {
            //Debug.Log($"Update {iTemPart.ID}");
            Selection.activeObject = iTemPart;
            iTemPart.PuzzleLevels.Add(asset);
            EditorUtility.SetDirty(iTemPart);
            AssetDatabase.SaveAssets();
        }

        puzzleMasterDatas.Add(asset);

#endif
        yield return new WaitForEndOfFrame();
    }
    void OnApplicationQuit()
    {
#if UNITY_EDITOR
        AssetDatabase.SaveAssets();
#elif UNITY_IOS

#else

#endif
    }

    private void OnUpdateAssetPuzzle(PuzzleLevelData asset, PuzzleDataItem puzzleDataItem)
    {
        asset.ID = Int32.Parse(puzzleDataItem.PuzzleId);
        asset.Level = Int32.Parse(puzzleDataItem.ChapterId);
        asset.Name = puzzleDataItem.PuzzleName;
        asset.Desc = puzzleDataItem.Description;
        asset.Ink = Int32.Parse(puzzleDataItem.RewardInks);

        asset.Version = puzzleDataItem.Version;
#if UNITY_EDITOR
        Selection.activeObject = asset;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
#elif UNITY_IOS
          
#else
    
#endif
        var IdCompare = puzzleDataItem.BookId + "-" + puzzleDataItem.ChapterId;
        // Add part into list book
        var iTemPart = this.chapterMasterDatas.Find(p => p.ID == IdCompare);
        if (iTemPart != null)
            iTemPart.PuzzleLevels.Add(asset);
    }

}

[System.Serializable]
public class GetBookData
{
    public List<BookDataItem> baseAbilities;
    public GetBookData()
    {
        baseAbilities = new List<BookDataItem>();
    }
}
[System.Serializable]
public class BookDataItem
{
    public string BookId;
    public string BookName;
    public string Description;
    public string Author;
    public string Illustrator;
    public string Thumbnails;
    public string Version;

}

[System.Serializable]
public class GetChapterData
{
    public List<ChapterDataItem> baseAbilities;
    public GetChapterData()
    {
        baseAbilities = new List<ChapterDataItem>();
    }
}

[System.Serializable]
public class ChapterDataItem
{
    public string BookId;
    public string BookName;
    public string ChapterId;
    public string ChapterName;
    public string Author;
    public string Description;
    public string Price;
    public string Thumbnails;
    public string Version;
}

[System.Serializable]
public class GetPuzzleData
{
    public List<PuzzleDataItem> baseAbilities;
    public GetPuzzleData()
    {
        baseAbilities = new List<PuzzleDataItem>();
    }
}

[System.Serializable]
public class PuzzleDataItem
{
    public string BookId;
    public string BookName;
    public string ChapterId;
    public string ChapterName;
    public string PuzzleId;
    public string PuzzleName;
    public string Description;
    public string Objects;
    public string RewardInks;
    public string Price;
    public string Thumbnails;
    public string BackgroundMusic;
    public string Version;
}

[System.Serializable]
public class GetCollectionData
{
    public List<CollectionDataItem> baseAbilities;
    public GetCollectionData()
    {
        baseAbilities = new List<CollectionDataItem>();
    }
}

[System.Serializable]
public class CollectionDataItem
{
    public int CollectionId;
    public string CollectionName;
    public string PuzzleName;
}