using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PostCardData", menuName = "PostCardData", order = 1)]
[PreferBinarySerialization]
public class PostCardData : ScriptableObject
{
    public int LiveEventSeasonId;
    public string LiveEventSeasonName;
    public int LiveEventPostCardId;
    public string LiveEventPostCardName;
    public string Cover;
    public List<PuzzleLiveEventData> puzzleLiveEventDatas = new List<PuzzleLiveEventData>();
}