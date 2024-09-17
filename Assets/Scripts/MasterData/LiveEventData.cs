using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "LiveEventData", menuName = "LiveEventData")]
public class LiveEventData : ScriptableObject
{
    public TimeData StartTime;
    public TimeData EndTime;
    public LiveEventPostCardData[] PostCardDatas;
}

[System.Serializable]
public struct LiveEventPostCardData
{
    public ThumbnailsData Thumbnail;
    public LiveEventPuzzleData[] PuzzleData;
    public int InkReward;

    public string ThumbnailLabel()
    {
        if (Thumbnail != null && !string.IsNullOrEmpty(Thumbnail.Thumbnail))
        {
            var fileName = Thumbnail.Thumbnail.Split('/').Last();
            if (!string.IsNullOrEmpty(fileName))
            {
                var thumbnail = fileName.Split('.').First();
                return $"Thumbnail_Small_{thumbnail}";
            }
        }

        return string.Empty;
    }
}

[System.Serializable]
public struct LiveEventPuzzleData
{
    public int BookId;
    public string ChapterId;
    public int PuzzleId;
    public LiveEventPuzzleItemDifficult Diffucult;
}

[System.Serializable]
public struct TimeData
{
    public int Year;
    public int Month;
    public int Day;
}