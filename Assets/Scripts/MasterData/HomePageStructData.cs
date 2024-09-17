using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LastestUpdateItem
{
    public int BookID;
    public string PartID;
}

[System.Serializable]
public struct IntroItem
{
    public int BookID;
    public FeatureBookCoverData Cover;
}

[System.Serializable]
public struct NewChapterUpdate
{
    public int Id;
    public int BookId;
    public string PartID;
    public long UpdateTime; // long
}

[CreateAssetMenu(fileName = "HomePageStructData", menuName = "HomePage", order = 1)]
public class HomePageStructData : ScriptableObject
{
    public List<IntroItem> IntroData = new List<IntroItem>();

    [Space]
    public List<int> TopPickData = new List<int>();
    public string TopPickDescrtion;

    [Space]
    public List<LastestUpdateItem> LastestUpdateItems = new List<LastestUpdateItem>();
    public string LatestUpdateDescrtion;

    [Space]
    public string AllCollectionDescription;

    [Space]
    [Header("News")]
    public string VersionName;
    public TextAsset VersionUpdate;
    public NewChapterUpdate[] NewChapterUpdates;

    [Space]
    public string FreePlayingTodayDescription;

    [Space]
    [Header("Collection")]
    public PuzzleCollectionData[] puzzleCollectionDatas;
}
