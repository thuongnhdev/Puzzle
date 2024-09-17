using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using DataCore;
using com.F4A.MobileThird;

[System.Serializable]
public class PlayerSavePack
{
    public long VERSION;

    public PlayerSaveData SaveData;
    public DailyChallengeSaveData DailyChallengeSavedData;
    public List<BookSaveData> BookSaveDatas;
    public List<LastPuzzlePlay> LastPuzzlePlays;
    public List<FreePuzzlePlaying> FreePuzzlePlayings;
    public List<BookSaveData> MyLibratyBookSaveDatas;
    public List<LastPuzzlePlay> MyLibraryPuzzlePlays;
    public List<CollectionPuzzleLastPlaying> LastPuzzlePlaysCollection;

    public LiveEventSaveData LiveEventSavedData;

    public List<CollectionPuzzlePlaying> CollectionPuzzlePlayings;
    public List<IndexPuzzleCollectionOpen> IndexPuzzleCollectionOpens;

    // new data collection v1.2.2
    public List<DataCollectionPuzzlePlaying> DataCollectionPuzzlePlayings;
    public List<DataCollectionPuzzleLastPlaying> DataLastPuzzlePlaysCollection;
    public List<DataIndexPuzzleCollectionOpen> DataIndexPuzzleCollectionOpens;

    public PlayerSavePack()
    {
        VERSION = 1;
        SaveData = new PlayerSaveData();
        DailyChallengeSavedData = new DailyChallengeSaveData();
        BookSaveDatas = new List<BookSaveData>();
        LastPuzzlePlays = new List<LastPuzzlePlay>();
        FreePuzzlePlayings = new List<FreePuzzlePlaying>();
        MyLibratyBookSaveDatas = new List<BookSaveData>();
        MyLibraryPuzzlePlays = new List<LastPuzzlePlay>();
        LastPuzzlePlaysCollection = new List<CollectionPuzzleLastPlaying>();
        CollectionPuzzlePlayings = new List<CollectionPuzzlePlaying>();
        IndexPuzzleCollectionOpens = new List<IndexPuzzleCollectionOpen>();

        // new data collection v1.2.2
        DataCollectionPuzzlePlayings = new List<DataCollectionPuzzlePlaying>();
        DataLastPuzzlePlaysCollection = new List<DataCollectionPuzzleLastPlaying>();
        DataIndexPuzzleCollectionOpens = new List<DataIndexPuzzleCollectionOpen>();
    }

    public LastPuzzlePlay GetCurrentPuzzleData()
    {
        if (LastPuzzlePlays.Count == 0)
        {
            return null;
        }

        return LastPuzzlePlays[LastPuzzlePlays.Count - 1];
    }

    public BookSaveData GetBookData(int id)
    {
        for (int i = 0; i < BookSaveDatas.Count; i++)
        {
            if (BookSaveDatas[i].Id == id)
            {
                return BookSaveDatas[i];
            }
        }

        return null;
    }


    public BookSaveData GetBookMyLibraryData(int id)
    {
        for (int i = 0; i < MyLibratyBookSaveDatas.Count; i++)
        {
            if (MyLibratyBookSaveDatas[i].Id == id)
            {
                return MyLibratyBookSaveDatas[i];
            }
        }

        return null;
    }

    public PuzzleStatus GetPuzzleStatus(int bookID, string partID, int puzzleID)
    {
        var bookUserData = GetBookData(bookID);

        if (bookUserData != null)
        {
            var partUserData = bookUserData.GetChapterSaveData(partID);
            if (partUserData != null)
            {
                var puzzleUserData = partUserData.GetPuzzleSaveData(puzzleID);
                if (puzzleUserData != null)
                {
                    return puzzleUserData.Stt;
                }
                else
                {
                    return PuzzleStatus.LOCK;
                }
            }
        }

        return PuzzleStatus.NONE;
    }

    public ChapterStatus GetPartStatus(int bookID, string partID)
    {
        var bookUserData = GetBookData(bookID);
        if (bookUserData != null)
        {
            var partUserData = bookUserData.GetChapterSaveData(partID);

            if (partUserData != null)
            {
                string[] partIdList = partID.Split('-');
                if (partIdList[1] == "1" || partIdList[1] == "2")
                {
                    partUserData.Stt = ChapterStatus.UNLOCK;
                    return ChapterStatus.UNLOCK;
                }
                else
                    return partUserData.Stt;
            }
        }

        return ChapterStatus.LOCK;
    }

    public void SaveUserPuzzleData(int bookID, string partID, int puzzleID, PuzzleStatus status)
    {
        var bookUserData = GetBookData(bookID);
        if (bookUserData != null)
        {
            var partUserData = bookUserData.GetChapterSaveData(partID);
            if (partUserData != null)
            {
                var puzzleUserData = partUserData.GetPuzzleSaveData(puzzleID);
                if (puzzleUserData != null)
                {
                    puzzleUserData.Stt = status;
                }
                else
                {
                    puzzleUserData = new PuzzleSaveData(bookID, partID, puzzleID, status);
                    partUserData.PuzzleSaveDatas.Add(puzzleUserData);
                }
            }
        }
        GameData.Instance.RequestSaveGame();
    }

    public void SaveUserChapterData(int bookID, string partID, ChapterStatus status)
    {
        var userDataBook = GetBookData(bookID);
        if (userDataBook == null)
        {
            userDataBook = new BookSaveData(bookID);
            userDataBook.PartSaveDatas.Add(new PartSaveData(bookID, partID, status));
            GameData.Instance.SavedPack.BookSaveDatas.Add(userDataBook);
            GameData.Instance.RequestSaveGame();
            if (status == ChapterStatus.UNLOCK)
            {
                ChapterMasterData chapter = MasterDataStore.Instance.GetPartById(bookID, partID);
                var book = MasterDataStore.Instance.GetBookByID(bookID);
                if (book != null && chapter != null)
                    AnalyticManager.Instance.TrackUnlockChapter(chapter.PartName, book.BookName);
            }
        }
        else
        {
            var userDataPart = userDataBook.GetChapterSaveData(partID);
            if (userDataPart == null)
            {
                userDataPart = new PartSaveData(bookID, partID, status);
                userDataBook.PartSaveDatas.Add(userDataPart);
            }
            else
            {
                userDataPart.Stt = status;
            }

            GameData.Instance.RequestSaveGame();
        }
    }

    public void SaveMyLibraryBookData(int bookID)
    {
        var userDataBook = GetBookMyLibraryData(bookID);
        if (userDataBook == null)
        {
            userDataBook = new BookSaveData(bookID);
            GameData.Instance.SavedPack.MyLibratyBookSaveDatas.Add(userDataBook);
        }
    }

    public PartSaveData GetPartUserData(int bookID, string partID)
    {
        var userDataBook = GetBookData(bookID);
        if (userDataBook != null)
        {
            return userDataBook.GetChapterSaveData(partID);
        }

        return null;
    }

    public int GetLastCompletePuzzleData(int bookID, string partID)
    {
        var partData = GetPartUserData(bookID, partID);
        int puzzleID = -1;
        if (partData != null)
        {
            for (int i = 0; i < partData.PuzzleSaveDatas.Count; i++)
            {
                if (partData.PuzzleSaveDatas[i].Stt == PuzzleStatus.COMPLETE)
                {
                    puzzleID = partData.PuzzleSaveDatas[i].Id;
                }
            }

        }

        return puzzleID;
    }

    public bool AddLastPlayedPuzzle(int bookId, string partId, int puzzleId)
    {
        if (bookId == 0)
        {
            //Tutorial
            return false;
        }

        int puzzleIndex = -1;

        for (int i = 0; i < LastPuzzlePlays.Count; i++)
        {
            if (LastPuzzlePlays[i].BookId == bookId && LastPuzzlePlays[i].PartId == partId &&
                LastPuzzlePlays[i].PuzzleId == puzzleId)
            {
                puzzleIndex = i;
            }
        }

        if (puzzleIndex < 0)
        {
            if (LastPuzzlePlays.Count == MCache.Instance.Config.MAX_RESUME_PLAYING_PUZZLE)
            {
                LastPuzzlePlays.RemoveAt(0);
            }

            LastPuzzlePlays.Add(new LastPuzzlePlay(bookId, partId, puzzleId));
        }
        else
        {
            LastPuzzlePlay item = LastPuzzlePlays[puzzleIndex];
            item.UpdateTime();
            LastPuzzlePlays.RemoveAt(puzzleIndex);
            LastPuzzlePlays.Add(item);
        }
        return true;
    }

    public void AddMyLibraryPlayedPuzzle(int bookId, string partId, int puzzleId)
    {
        if (bookId == 0 || string.Compare(partId, "0") == 0 || puzzleId <= 0)
        {
            return;
        }


        int puzzleIndex = -1;

        for (int i = 0; i < MyLibraryPuzzlePlays.Count; i++)
        {
            if (MyLibraryPuzzlePlays[i].BookId == bookId && MyLibraryPuzzlePlays[i].PartId == partId &&
                MyLibraryPuzzlePlays[i].PuzzleId == puzzleId)
            {
                puzzleIndex = i;
            }
        }

        if (puzzleIndex < 0)
        {
            MyLibraryPuzzlePlays.Add(new LastPuzzlePlay(bookId, partId, puzzleId));
        }
        else
        {
            LastPuzzlePlay item = MyLibraryPuzzlePlays[puzzleIndex];
            item.UpdateTime();
            MyLibraryPuzzlePlays.RemoveAt(puzzleIndex);
            MyLibraryPuzzlePlays.Add(item);
        }

    }

    public FreePuzzlePlaying GetFreePuzzlePlaying(int bookID, string partId, int puzzleID)
    {
        for (int i = 0; i < FreePuzzlePlayings.Count; i++)
        {
            if (FreePuzzlePlayings[i].BookId == bookID && FreePuzzlePlayings[i].PartId == partId && FreePuzzlePlayings[i].PuzzleId == puzzleID)
            {
                return FreePuzzlePlayings[i];
            }
        }

        return null;
    }

    public void SetFreePuzzlePlaying(int bookID, string partId, int puzzleID, int isDone)
    {
        for (int i = 0; i < FreePuzzlePlayings.Count; i++)
        {
            if (FreePuzzlePlayings[i].BookId == bookID && FreePuzzlePlayings[i].PartId == partId && FreePuzzlePlayings[i].PuzzleId == puzzleID)
            {
                FreePuzzlePlayings[i].isFinish = isDone;
            }
        }
    }


    //==========new data collection v1.2.2================

    public void ConvertDataUpdateCollection()
    {
        // convert old data move new data collection save with id, index

        var updateData = LastPuzzlePlaysCollection.Count > 0 || CollectionPuzzlePlayings.Count > 0 || IndexPuzzleCollectionOpens.Count > 0;
        DataCore.Debug.Log($"ConvertDataUpdateCollection: {updateData}", false);
        if (DataCollectionPuzzlePlayings == null)
        {
            DataCollectionPuzzlePlayings = new List<DataCollectionPuzzlePlaying>();
        }
        if (DataLastPuzzlePlaysCollection == null)
        {
            DataLastPuzzlePlaysCollection = new List<DataCollectionPuzzleLastPlaying>();
        }
        if (DataIndexPuzzleCollectionOpens == null)
        {
            DataIndexPuzzleCollectionOpens = new List<DataIndexPuzzleCollectionOpen>();
        }
        if (updateData)
        {

            if (GameData.Instance.SavedPack.CollectionPuzzlePlayings.Count > 0)
                GameData.Instance.SavedPack.ConvertCollectionPuzzlePlayings();
            if (GameData.Instance.SavedPack.LastPuzzlePlaysCollection.Count > 0)
                GameData.Instance.SavedPack.ConvertLastPuzzlePlaysCollection();
            if (GameData.Instance.SavedPack.IndexPuzzleCollectionOpens.Count > 0)
                GameData.Instance.SavedPack.ConvertIndexPuzzleCollectionOpens();
            GameData.Instance.RequestSaveGame();
        }
    }

    public void ConvertLastPuzzlePlaysCollection()
    {
        //Update old data version before 1.2.2 move to version after 1.2.2
        var oldData = MasterDataStore.Instance.OldCollectionData;
        DataCore.Debug.Log($"ConvertLastPuzzlePlaysCollection: {LastPuzzlePlaysCollection.Count}");

        foreach (var puzzle in LastPuzzlePlaysCollection)
        {
            var collectionId = puzzle.CollectionId;
            var key = $"{collectionId}_{puzzle.PuzzleName}";
            if (oldData != null && oldData.collectionPuzzlesKeyAndIndex.ContainsKey(key))
            {               
                var index = oldData.collectionPuzzlesKeyAndIndex[key];
                if (collectionId == 0) {
                    index += 1;
                }
                var oldPuzzleIndex = new DataCollectionPuzzleLastPlaying(collectionId, index)
                {
                    savedData = puzzle.savedData
                };
                if (DataLastPuzzlePlaysCollection.Find((x) => x.CollectionId == collectionId && x.CollectionIndex == index) == null)
                {
                    DataCore.Debug.Log($"ConvertLastPuzzlePlaysCollection {collectionId} {index}", false);
                    DataLastPuzzlePlaysCollection.Add(oldPuzzleIndex);
                }
            }
        }
        LastPuzzlePlaysCollection.Clear();
    }

    public void ConvertCollectionPuzzlePlayings()
    {
        var oldData = MasterDataStore.Instance.OldCollectionData;
        foreach (var puzzle in CollectionPuzzlePlayings)
        {
            var collectionId = puzzle.CollectionId;
            var key = $"{collectionId}_{puzzle.PuzzleName}";
            if (oldData != null && oldData.collectionPuzzlesKeyAndIndex.ContainsKey(key))
            {
                var index = oldData.collectionPuzzlesKeyAndIndex[key];
                if (collectionId == 0)
                {
                    index += 1;
                }
                var oldPuzzleIndex = new DataCollectionPuzzlePlaying(collectionId, index, puzzle.PuzzleStatus);
                if (DataCollectionPuzzlePlayings.Find((x) => x.CollectionId == collectionId && x.CollectionIndex == index) == null)
                {
                    DataCore.Debug.Log($"ConvertCollectionPuzzlePlayings {key} {collectionId} {index}", false);
                    DataCollectionPuzzlePlayings.Add(oldPuzzleIndex);
                }
            }
        }
        
        CollectionPuzzlePlayings.Clear();
    }

    public void ConvertIndexPuzzleCollectionOpens()
    {
        var oldData = MasterDataStore.Instance.OldCollectionData;

        foreach (var puzzle in IndexPuzzleCollectionOpens)
        {
            var collectionId = puzzle.CollectionId;
            var key = $"{collectionId}_{puzzle.PuzzleName}";
            if (oldData != null && oldData.collectionPuzzlesKeyAndIndex.ContainsKey(key))
            {
                var index = oldData.collectionPuzzlesKeyAndIndex[key];
                if (collectionId == 0)
                {
                    index += 1;
                }
                var oldPuzzleIndex = new DataIndexPuzzleCollectionOpen(collectionId, index);
                if (DataIndexPuzzleCollectionOpens.Find((x) => x.CollectionId == collectionId && x.CollectionIndex == index) == null)
                {
                    DataCore.Debug.Log($"ConvertIndexPuzzleCollectionOpens {collectionId} {index}", false);
                    DataIndexPuzzleCollectionOpens.Add(oldPuzzleIndex);
                }
            }
        }
        IndexPuzzleCollectionOpens.Clear();
    }

    public void DataCleanCollectionPuzzlePlayings()
    {
        var cleanDataCollectionPuzzlePlayings = new List<DataCollectionPuzzlePlaying>();

        for (int i = 0; i < DataCollectionPuzzlePlayings.Count; i++)
        {
            var puzzle = DataCollectionPuzzlePlayings[i];
            if (puzzle.PuzzleStatus == PuzzleStatus.COMPLETE || puzzle.PuzzleStatus == PuzzleStatus.UNLOCK)
            {
                cleanDataCollectionPuzzlePlayings.Add(puzzle);
            }
        }
        DataCollectionPuzzlePlayings = cleanDataCollectionPuzzlePlayings;
    }

    public DataCollectionPuzzlePlaying DataGetCollectionPuzzlePlaying(int collectionId, int collectionIndex)
    {
        var puzzleData = DataCollectionPuzzlePlayings.Find(puzzle => puzzle.CollectionId == collectionId && puzzle.CollectionIndex == collectionIndex);
        return puzzleData;
    }

    public DataCollectionPuzzlePlaying DataGetLastPlayedPuzzleInCollection(int collectionId)
    {
        DataCollectionPuzzlePlaying result = null;
        var maxIndex = 0;
        for (int i = 0; i < DataCollectionPuzzlePlayings.Count; i++)
        {
            var puzzle = DataCollectionPuzzlePlayings[i];
            if (puzzle.CollectionId == collectionId && (puzzle.PuzzleStatus == PuzzleStatus.COMPLETE))
            {
                if (puzzle.CollectionIndex > maxIndex) {
                    maxIndex = puzzle.CollectionIndex;
                    result = puzzle;
                }                
            }
        }

        return result;
    }

    public void DataSetCollectionPuzzlePlaying(int collectionId, int collectionIndex, PuzzleStatus puzzleStatus)
    {
        var index = DataCollectionPuzzlePlayings.FindIndex(puzzle => puzzle.CollectionId == collectionId && puzzle.CollectionIndex == collectionIndex);
        if (index < 0)
        {
            var newPuzzleData = new DataCollectionPuzzlePlaying(collectionId, collectionIndex, puzzleStatus);
            DataCollectionPuzzlePlayings.Add(newPuzzleData);
        }
        else
        {
            var puzzleData = DataCollectionPuzzlePlayings[index];
            puzzleData.PuzzleStatus = puzzleStatus;
            DataCollectionPuzzlePlayings[index] = puzzleData;
        }
        GameData.Instance.RequestSaveGame();
    }

    public void DataClearPuzzleCollectionData()
    {
        DataLastPuzzlePlaysCollection.Clear();
    }

    public DataCollectionPuzzleLastPlaying DataGetCurrentPuzzleCollectionData(int collectionId, int collectionIndex)
    {
        if (DataLastPuzzlePlaysCollection.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < DataLastPuzzlePlaysCollection.Count; i++)
        {
            if (DataLastPuzzlePlaysCollection[i].CollectionId == collectionId
                && DataLastPuzzlePlaysCollection[i].CollectionIndex == collectionIndex)
            {
                return DataLastPuzzlePlaysCollection[i];
            }
        }
        return null;
    }

    public bool DataRemoveLastPlayedPuzzleCollection(int collectionId, int collectionIndex)
    {
        int puzzleIndex = -1;

        for (int i = 0; i < DataLastPuzzlePlaysCollection.Count; i++)
        {
            if (DataLastPuzzlePlaysCollection[i].CollectionId == collectionId &&
                DataLastPuzzlePlaysCollection[i].CollectionIndex == collectionIndex)
            {
                puzzleIndex = i;
            }
        }
        DataCore.Debug.Log("Remove:" + puzzleIndex + "-" + collectionId + "-" + collectionIndex);

        if (puzzleIndex >= 0 && DataLastPuzzlePlaysCollection.Count > 0)
        {
            DataLastPuzzlePlaysCollection.RemoveAt(puzzleIndex);
            return true;
        }

        return false;
    }


    public bool DataAddLastPlayedCollection(int collection, int collectionIndex)
    {
        if (collection < 0)
        {
            //Tutorial
            return false;
        }

        int puzzleIndex = -1;

        for (int i = 0; i < DataLastPuzzlePlaysCollection.Count; i++)
        {
            if (DataLastPuzzlePlaysCollection[i].CollectionId == collection &&
                DataLastPuzzlePlaysCollection[i].CollectionIndex == collectionIndex)
            {
                puzzleIndex = i;
            }
        }

        if (puzzleIndex < 0)
        {
            if (DataLastPuzzlePlaysCollection.Count == MCache.Instance.Config.MAX_RESUME_PLAYING_PUZZLE)
            {
                DataLastPuzzlePlaysCollection.RemoveAt(0);
            }

            DataLastPuzzlePlaysCollection.Add(new DataCollectionPuzzleLastPlaying(collection, collectionIndex));
        }
        else
        {
            DataCollectionPuzzleLastPlaying item = DataLastPuzzlePlaysCollection[puzzleIndex];
            item.UpdateTime();
            DataLastPuzzlePlaysCollection.RemoveAt(puzzleIndex);
            DataLastPuzzlePlaysCollection.Add(item);
        }
        return true;
    }

    public DataIndexPuzzleCollectionOpen DataGetIndexPuzzleCollectionComplete(int collectionId)
    {
        for (int i = 0; i < DataIndexPuzzleCollectionOpens.Count; i++)
        {
            if (DataIndexPuzzleCollectionOpens[i].CollectionId == collectionId)
            {
                return DataIndexPuzzleCollectionOpens[i];
            }
        }

        return null;
    }

    public bool DataAddIndexPuzzleCollectionComplete(int collectionId, int collectionIndex)
    {
        if (collectionId < 0 || collectionIndex < 0)
        {
            //Tutorial
            return false;
        }
        int puzzleIndex = -1;

        for (int i = 0; i < DataIndexPuzzleCollectionOpens.Count; i++)
        {
            if (DataIndexPuzzleCollectionOpens[i].CollectionId == collectionId)
            {
                puzzleIndex = i;
            }
        }

        if (puzzleIndex < 0)
        {
            if (DataIndexPuzzleCollectionOpens.Count == MCache.Instance.Config.MAX_RESUME_PLAYING_PUZZLE)
            {
                DataIndexPuzzleCollectionOpens.RemoveAt(0);
            }
            DataIndexPuzzleCollectionOpens.Add(new DataIndexPuzzleCollectionOpen(collectionId, collectionIndex));
        }
        else
        {
            DataIndexPuzzleCollectionOpen item = new DataIndexPuzzleCollectionOpen(collectionId, collectionIndex);
            DataIndexPuzzleCollectionOpens.RemoveAt(puzzleIndex);
            DataIndexPuzzleCollectionOpens.Add(item);
        }
        return true;
    }

    //===================================================

    //========== data collection before v1.2.2================
    //public void CleanCollectionPuzzlePlayings()
    //{
    //    var cleanCollectionPuzzlePlayings = new List<CollectionPuzzlePlaying>();

    //    for (int i = 0; i < CollectionPuzzlePlayings.Count; i++)
    //    {
    //        var puzzle = CollectionPuzzlePlayings[i];
    //        if (puzzle.PuzzleStatus == PuzzleStatus.COMPLETE || puzzle.PuzzleStatus == PuzzleStatus.UNLOCK)
    //        {
    //            cleanCollectionPuzzlePlayings.Add(puzzle);
    //        }
    //    }
    //    CollectionPuzzlePlayings = cleanCollectionPuzzlePlayings;
    //}

    //public CollectionPuzzlePlaying GetCollectionPuzzlePlaying(int collectionId, string puzzleName, int puzzleID)
    //{
    //    var puzzleData = CollectionPuzzlePlayings.Find(puzzle => puzzle.CollectionId == collectionId && puzzle.PuzzleName == puzzleName && puzzle.PuzzleId == puzzleID);
    //    return puzzleData;
    //}

    //public CollectionPuzzlePlaying GetLastPlayedPuzzleInCollection(int collectionId)
    //{
    //    CollectionPuzzlePlaying result = null;
    //    for (int i = 0; i < CollectionPuzzlePlayings.Count; i++)
    //    {
    //        var puzzle = CollectionPuzzlePlayings[i];
    //        if (puzzle.CollectionId == collectionId && (puzzle.PuzzleStatus == PuzzleStatus.COMPLETE || puzzle.PuzzleStatus == PuzzleStatus.UNLOCK))
    //        {
    //            result = puzzle;
    //        }
    //    }
    //    return result;
    //}

    //public void SetCollectionPuzzlePlaying(int collectionId, string puzzleName, int puzzleID, PuzzleStatus puzzleStatus)
    //{
    //    var index = CollectionPuzzlePlayings.FindIndex(puzzle => puzzle.CollectionId == collectionId && puzzle.PuzzleName == puzzleName && puzzle.PuzzleId == puzzleID);
    //    if (index < 0)
    //    {
    //        var newPuzzleData = new CollectionPuzzlePlaying(collectionId, puzzleName, puzzleID, puzzleStatus);
    //        CollectionPuzzlePlayings.Add(newPuzzleData);
    //    }
    //    else
    //    {
    //        var puzzleData = CollectionPuzzlePlayings[index];
    //        puzzleData.PuzzleStatus = puzzleStatus;
    //        CollectionPuzzlePlayings[index] = puzzleData;
    //    }
    //    GameData.Instance.RequestSaveGame();
    //}

    //public void ClearPuzzleCollectionData()
    //{
    //    LastPuzzlePlaysCollection.Clear();
    //}

    //public CollectionPuzzleLastPlaying GetCurrentPuzzleCollectionData(int collectionId, int collectionIndex)
    //{
    //    if (LastPuzzlePlaysCollection.Count == 0)
    //    {
    //        return null;
    //    }

    //    var puzzle = MasterDataStore.Instance.GetCollectionPuzzleByIndex(collectionId, collectionIndex);

    //    for (int i = 0; i < LastPuzzlePlaysCollection.Count; i++)
    //    {
    //        if (LastPuzzlePlaysCollection[i].CollectionId == collectionId
    //            && LastPuzzlePlaysCollection[i].PuzzleName == puzzle.Name
    //            && LastPuzzlePlaysCollection[i].PuzzleId == puzzle.ID)
    //        {
    //            return LastPuzzlePlaysCollection[i];
    //        }
    //    }
    //    return null;
    //}

    //public bool RemoveLastPlayedPuzzleCollection(int collectionId, string puzzleName, int puzzleId)
    //{
    //    int puzzleIndex = -1;

    //    for (int i = 0; i < LastPuzzlePlaysCollection.Count; i++)
    //    {
    //        if (LastPuzzlePlaysCollection[i].CollectionId == collectionId && LastPuzzlePlaysCollection[i].PuzzleName == puzzleName &&
    //            LastPuzzlePlaysCollection[i].PuzzleId == puzzleId)
    //        {
    //            puzzleIndex = i;
    //        }
    //    }
    //    DataCore.Debug.Log("Remove:" + puzzleIndex + "-" + collectionId + "-" + puzzleName + "-" + puzzleId);

    //    if (puzzleIndex >= 0 && LastPuzzlePlaysCollection.Count > 0)
    //    {
    //        LastPuzzlePlaysCollection.RemoveAt(puzzleIndex);
    //        return true;
    //    }

    //    return false;
    //}

    //public bool AddLastPlayedCollection(int collection, string puzzleName, int puzzleId)
    //{
    //    if (collection < 0)
    //    {
    //        //Tutorial
    //        return false;
    //    }

    //    int puzzleIndex = -1;

    //    for (int i = 0; i < LastPuzzlePlaysCollection.Count; i++)
    //    {
    //        if (LastPuzzlePlaysCollection[i].CollectionId == collection && LastPuzzlePlaysCollection[i].PuzzleName == puzzleName &&
    //            LastPuzzlePlaysCollection[i].PuzzleId == puzzleId)
    //        {
    //            puzzleIndex = i;
    //        }
    //    }

    //    if (puzzleIndex < 0)
    //    {
    //        if (LastPuzzlePlaysCollection.Count == MCache.Instance.Config.MAX_RESUME_PLAYING_PUZZLE)
    //        {
    //            LastPuzzlePlaysCollection.RemoveAt(0);
    //        }

    //        LastPuzzlePlaysCollection.Add(new CollectionPuzzleLastPlaying(collection, puzzleName, puzzleId));
    //    }
    //    else
    //    {
    //        CollectionPuzzleLastPlaying item = LastPuzzlePlaysCollection[puzzleIndex];
    //        item.UpdateTime();
    //        LastPuzzlePlaysCollection.RemoveAt(puzzleIndex);
    //        LastPuzzlePlaysCollection.Add(item);
    //    }
    //    return true;
    //}

    public IndexPuzzleCollectionOpen GetIndexPuzzleCollectionComplete(int collectionId)
    {
        for (int i = 0; i < IndexPuzzleCollectionOpens.Count; i++)
        {
            if (IndexPuzzleCollectionOpens[i].CollectionId == collectionId)
            {
                return IndexPuzzleCollectionOpens[i];
            }
        }

        return null;
    }

    public bool AddIndexPuzzleCollectionComplete(int collectionId, string puzzleName, int puzzleId)
    {
        if (collectionId < 0 || string.IsNullOrEmpty(puzzleName) || puzzleId < 0)
        {
            //Tutorial
            return false;
        }
        int puzzleIndex = -1;

        for (int i = 0; i < IndexPuzzleCollectionOpens.Count; i++)
        {
            if (IndexPuzzleCollectionOpens[i].CollectionId == collectionId)
            {
                puzzleIndex = i;
            }
        }

        if (puzzleIndex < 0)
        {
            if (IndexPuzzleCollectionOpens.Count == MCache.Instance.Config.MAX_RESUME_PLAYING_PUZZLE)
            {
                IndexPuzzleCollectionOpens.RemoveAt(0);
            }
            IndexPuzzleCollectionOpens.Add(new IndexPuzzleCollectionOpen(collectionId, puzzleName, puzzleId));
        }
        else
        {
            IndexPuzzleCollectionOpen item = new IndexPuzzleCollectionOpen(collectionId, puzzleName, puzzleId);
            IndexPuzzleCollectionOpens.RemoveAt(puzzleIndex);
            IndexPuzzleCollectionOpens.Add(item);
        }
        return true;
    }

    //===================================================

    public bool RemoveLastPlayedPuzzle(int bookId, string partId, int puzzleId)
    {
        int puzzleIndex = -1;

        for (int i = 0; i < LastPuzzlePlays.Count; i++)
        {
            if (LastPuzzlePlays[i].BookId == bookId && LastPuzzlePlays[i].PartId == partId &&
                LastPuzzlePlays[i].PuzzleId == puzzleId)
            {
                puzzleIndex = i;
            }
        }
        DataCore.Debug.Log("Remove:" + puzzleIndex + "-" + bookId + "-" + partId + "-" + puzzleId);

        if (puzzleIndex >= 0 && LastPuzzlePlays.Count > 0)
        {
            LastPuzzlePlays.RemoveAt(puzzleIndex);
            return true;
        }

        return false;
    }

    public bool GetSubcribeState(int bookId)
    {
        for (int i = 0; i < BookSaveDatas.Count; i++)
        {
            if (BookSaveDatas[i].Id == bookId)
            {
                return BookSaveDatas[i].IsSubcribed;
            }
        }

        return false;
    }

    public bool SwitchIsSubcribeState(int bookId)
    {
        for (int i = 0; i < BookSaveDatas.Count; i++)
        {
            if (BookSaveDatas[i].Id == bookId)
            {
                BookSaveDatas[i].IsSubcribed = !BookSaveDatas[i].IsSubcribed;
                return BookSaveDatas[i].IsSubcribed;
            }
        }

        BookSaveData newBook = new BookSaveData(bookId);
        newBook.IsSubcribed = true;
        BookSaveDatas.Add(newBook);
        GameData.Instance.RequestSaveGame();

        return newBook.IsSubcribed;
    }

    public void UpdateChallenge(ChallengeType type, int value)
    {
        try
        {
            if (DailyChallengeSavedData == null) {
                DailyChallengeSavedData = new DailyChallengeSaveData();
            }
            for (int i = 0; i < DailyChallengeSavedData.ChallengeSaveDatas.Length; i++)
            {
                if (DailyChallengeSavedData.ChallengeSaveDatas[i].Type == type)
                {
                    if (type != ChallengeType.PLAY_DIFFERENCE_BOOK)
                    {
                        DailyChallengeSavedData.ChallengeSaveDatas[i].ReachedValue += value;
                    }
                    else
                    {
                        if (!DailyChallengeSavedData.ChallengeSaveDatas[i].BookIds.Contains(value))
                        {
                            DailyChallengeSavedData.ChallengeSaveDatas[i].ReachedValue++;
                            DailyChallengeSavedData.ChallengeSaveDatas[i].BookIds.Add(value);
                        }
                    }
                }
            }
            GameData.Instance.RequestSaveGame();
        }
        catch (Exception ex)
        {
            DataCore.Debug.Log($"Failed UpdateChallenge: {ex.Message}");
        }

    }
}

[System.Serializable]
public class PlayerSaveData
{
    public int Coin;
    public int Hint;

    public bool IsTutorialCompleted;
    public bool IsShowChooseFirstBook;
    public bool IsShowPushNotiPermission;

    public bool IsMusicActive;
    public bool IsSoundActive;
    public bool IsEnableDebugMode;
    public bool IsHapticActive;
    public bool IsDescriptionActive;

    public int LastDayLuckyDraw;
    public int DailyLuckyDrawRemain;
    public int DailyLuckyDrawShowFree;
    public int LastDayPromotionBanner;
    public int PromotionBannerRemain;
    public int CountCompletedPuzzlePromotionBanner;
    public int LastTimeShowPromotion;
    public int CountCompletedPuzzleLuckyDraw;
    public int LastDaySubscription;
    public int SubscriptionRemain;
    public int CountCompletedPuzzle;

    public int playedPuzzle;
    public int completedRewardedAd;
    public int shownInterstitial;

    public int LastDayCompleteRemain;
    public int PuzzleCompleteRemain;
    // Daily Reward
    public int DailyRewardIndex;
    public int LastDayDailyReward;

    public List<int> NewChapterRemoved;

    // save puzzle resume
    public int ResumeBookId;
    public string ResumePartId;
    public int ResumePuzzleId;
    public bool IsResumeComplete;
    public int StepGameOnBoard;

    public bool DidRemoveAd;
    public bool DidVIP;

    public PlayerSaveData()
    {
        Coin = 0;
        Hint = 1;
        IsTutorialCompleted = false;
        IsShowChooseFirstBook = false;
        IsShowPushNotiPermission = false;
        CountCompletedPuzzlePromotionBanner = 0;
        IsMusicActive = true;
        IsSoundActive = true;
        IsHapticActive = true;
        IsDescriptionActive = true;
        IsEnableDebugMode = false;
        NewChapterRemoved = new List<int>();
        playedPuzzle = 0;
        completedRewardedAd = 0;
        shownInterstitial = 0;
        CountCompletedPuzzle = 0;
        ResumeBookId = 0;
        ResumePartId = "0";
        ResumePuzzleId = 0;
        IsResumeComplete = false;
        PuzzleCompleteRemain = 5;
        StepGameOnBoard = 0;
        DidRemoveAd = false;
        DidVIP = false;
    }
}

[System.Serializable]
public class BookSaveData
{
    public int Id;
    public bool IsSubcribed;
    public List<PartSaveData> PartSaveDatas;

    public BookSaveData(int bookId)
    {
        this.Id = bookId;
        this.IsSubcribed = false;
        this.PartSaveDatas = new List<PartSaveData>();
    }

    public PartSaveData GetChapterSaveData(string partId)
    {
        for (int i = 0; i < PartSaveDatas.Count; i++)
        {
            if (PartSaveDatas[i].Id == partId)
            {
                return PartSaveDatas[i];
            }
        }

        return null;
    }
}

[System.Serializable]
public class PartSaveData
{
    // Parent
    public int BookID;

    public string Id;
    public ChapterStatus Stt;
    public List<PuzzleSaveData> PuzzleSaveDatas;

    public PartSaveData(int bookId, string partId)
    {
        this.BookID = bookId;
        this.Id = partId;
        this.PuzzleSaveDatas = new List<PuzzleSaveData>();
    }

    public PartSaveData(int bookId, string partId, ChapterStatus stt)
    {

        this.BookID = bookId;
        this.Id = partId;
        this.Stt = stt;
        this.PuzzleSaveDatas = new List<PuzzleSaveData>();
    }

    public PuzzleSaveData GetPuzzleSaveData(int puzzleID)
    {
        for (int i = 0; i < PuzzleSaveDatas.Count; i++)
        {
            if (PuzzleSaveDatas[i].Id == puzzleID)
            {
                return PuzzleSaveDatas[i];
            }
        }

        return null;
    }

}

[System.Serializable]
public class PuzzleSaveData
{
    // Parent
    public int BookID;
    public string PartID;

    public int Id;
    public PuzzleStatus Stt;

    public PuzzleSaveData(int bookId, string partId, int puzzleId)
    {
        this.BookID = bookId;
        this.PartID = partId;
        this.Id = puzzleId;
    }

    public PuzzleSaveData(int bookId, string partId, int puzzleId, PuzzleStatus stt)
    {
        this.BookID = bookId;
        this.PartID = partId;
        this.Id = puzzleId;
        this.Stt = stt;
    }
}


[System.Serializable]
public class LastPuzzlePlay
{
    public int BookId;
    public string PartId;
    public int PuzzleId;
    public long lastTimePlay;
    public LevelInfo savedData;

    public bool IsEventPuzzle;
    public int LiveEventPostCardId;
    public int LiveEventPuzzleId;

    public LastPuzzlePlay(int bookId, string partId, int puzzleId)
    {
        lastTimePlay = DateTime.Now.Ticks;
        this.BookId = bookId;
        this.PartId = partId;
        this.PuzzleId = puzzleId;

        savedData = new LevelInfo();
    }

    public LastPuzzlePlay(int bookId, string partId, int puzzleId, int postCardId, int eventPuzzleId, bool isEventPuzzle)
    {
        lastTimePlay = DateTime.Now.Ticks;
        this.BookId = bookId;
        this.PartId = partId;
        this.PuzzleId = puzzleId;

        this.LiveEventPostCardId = postCardId;
        this.LiveEventPuzzleId = eventPuzzleId;
        this.IsEventPuzzle = isEventPuzzle;

        savedData = new LevelInfo();
    }
    public void UpdateTime()
    {
        lastTimePlay = DateTime.Now.Ticks;
    }
}

[System.Serializable]
public class IndexPuzzleCollectionOpen
{
    public int CollectionId;
    public string PuzzleName;
    public int PuzzleId;

    public IndexPuzzleCollectionOpen(int collectionId, string puzzleName, int puzzleId)
    {
        this.CollectionId = collectionId;
        this.PuzzleName = puzzleName;
        this.PuzzleId = puzzleId;

    }
}

[System.Serializable]
public class DataIndexPuzzleCollectionOpen
{
    public int CollectionId;
    public int CollectionIndex;

    public DataIndexPuzzleCollectionOpen(int collectionId, int collectionIndex)
    {
        this.CollectionId = collectionId;
        this.CollectionIndex = collectionIndex;
    }
}


[System.Serializable]
public class FreePuzzlePlaying
{
    public int BookId;
    public string PartId;
    public int PuzzleId;
    public int isFinish;
    public LevelInfo savedData;

    public FreePuzzlePlaying(int bookId, string partId, int puzzleId, int isDone)
    {
        this.BookId = bookId;
        this.PartId = partId;
        this.PuzzleId = puzzleId;
        this.isFinish = isDone;
        savedData = new LevelInfo();
    }
}

[System.Serializable]
public class CollectionPuzzlePlaying
{
    public int CollectionId;
    public string PuzzleName;
    public int PuzzleId;
    public PuzzleStatus PuzzleStatus;
    public long lastTimePlay;

    public CollectionPuzzlePlaying(int collectionId, string puzzleName, int puzzleId, PuzzleStatus puzzleStatus)
    {
        lastTimePlay = DateTime.Now.Ticks;
        this.CollectionId = collectionId;
        this.PuzzleName = puzzleName;
        this.PuzzleId = puzzleId;
        this.PuzzleStatus = puzzleStatus;
    }

    public void UpdateTime()
    {
        lastTimePlay = DateTime.Now.Ticks;
    }
}

[System.Serializable]
public class DataCollectionPuzzlePlaying
{
    public int CollectionId;
    public int CollectionIndex;
    public PuzzleStatus PuzzleStatus;
    public long lastTimePlay;

    public DataCollectionPuzzlePlaying(int collectionId, int collectionIndex, PuzzleStatus puzzleStatus)
    {
        lastTimePlay = DateTime.Now.Ticks;
        this.CollectionId = collectionId;
        this.CollectionIndex = collectionIndex;
        this.PuzzleStatus = puzzleStatus;
    }

    public void UpdateTime()
    {
        lastTimePlay = DateTime.Now.Ticks;
    }
}

[System.Serializable]
public class CollectionPuzzleLastPlaying
{
    public int CollectionId;
    public string PuzzleName;
    public int PuzzleId;
    public LevelInfo savedData;
    public long lastTimePlay;

    public CollectionPuzzleLastPlaying(int collectionId, string puzzleName, int puzzleId)
    {
        lastTimePlay = DateTime.Now.Ticks;
        this.CollectionId = collectionId;
        this.PuzzleName = puzzleName;
        this.PuzzleId = puzzleId;
        savedData = new LevelInfo();
    }

    public void UpdateTime()
    {
        lastTimePlay = DateTime.Now.Ticks;
    }
}

[System.Serializable]
public class DataCollectionPuzzleLastPlaying
{
    public int CollectionId;
    public int CollectionIndex;
    public LevelInfo savedData;
    public long lastTimePlay;

    public DataCollectionPuzzleLastPlaying(int collectionId, int collectionIndex)
    {
        lastTimePlay = DateTime.Now.Ticks;
        this.CollectionId = collectionId;
        this.CollectionIndex = collectionIndex;
        savedData = new LevelInfo();
    }

    public void UpdateTime()
    {
        lastTimePlay = DateTime.Now.Ticks;
    }
}

[System.Serializable]
public class DailyChallengeSaveData
{
    public int LastDay;
    public ChallengeSaveData[] ChallengeSaveDatas;

    public DailyChallengeSaveData()
    {
        ChallengeSaveDatas = new ChallengeSaveData[GameConstants.AMOUNT_CHALLENGE_PER_DAY];
    }
}

[System.Serializable]
public class ChallengeSaveData
{
    public ChallengeType Type;
    public bool IsClaimed;
    public int ChallengeTargetId;
    public int ReachedValue;
    public List<int> BookIds;
}

[System.Serializable]
public class LiveEventSaveData
{
    public List<PostCardSaveData> PostCardDatas;
    public int ShowTutorial;
    public LiveEventSaveData(int amountPostCard)
    {
        PostCardDatas = new List<PostCardSaveData>();
        PostCardSaveData postCardSaveData;

        for (int i = 0; i < amountPostCard; i++)
        {
            postCardSaveData = new PostCardSaveData();
            PostCardDatas.Add(postCardSaveData);
        }
        ShowTutorial = 0;
        PostCardDatas[0].Status = LiveEventPostCardState.Unlock;
    }

    public void UpdatePostCardStatus(int id, LiveEventPostCardState stt)
    {
        PostCardDatas[id].Status = stt;
        if (stt == LiveEventPostCardState.Completed && id < PostCardDatas.Count - 1)
        {
            PostCardDatas[id + 1].Status = LiveEventPostCardState.Unlock;
        }
    }

    public void UpdateCompletePuzzle(int postCardId,int index)
    {
        this.PostCardDatas[postCardId].PuzzleIdCompleted.Add(index);
    }

    public void AddPostCardItem(int id)
    {
        int amount = LiveEventTimer.Instance.LiveEventData.PostCardDatas.Length;
        if (id < amount)
        {
            PostCardSaveData postCardSaveData;
            postCardSaveData = new PostCardSaveData();
            PostCardDatas.Add(postCardSaveData);
        }
    }
}

[System.Serializable]
public class PostCardSaveData
{
    public LiveEventPostCardState Status;
    public int SelectedPuzzleId;
    public List<int> PuzzleIdCompleted;
    public int ClaimReward;
    
    public PostCardSaveData()
    {
        Status = LiveEventPostCardState.Lock;
        SelectedPuzzleId = 0;
        PuzzleIdCompleted = new List<int>();
        ClaimReward = 0;
    }

    public LiveEventPuzzleItemState GetPuzzleStatus(int index)
    {
        return this.PuzzleIdCompleted.Contains(index) ? LiveEventPuzzleItemState.Unlock : LiveEventPuzzleItemState.Lock;
    }
}

[System.Serializable]
public enum ChapterStatus
{
    LOCK, UNLOCK
}