using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleLiveEventData", menuName = "PuzzleLiveEventData", order = 1)]
[PreferBinarySerialization]
public class PuzzleLiveEventData : ScriptableObject
{
    public int LiveEventSeasonId;
    public string LiveEventSeasonName;
    public int LiveEventPostCardId;
    public string PostCardName;
    public string Difficulty;
    public int PlayMode;
    public PuzzleLevelData PuzzleLevelData;
}
