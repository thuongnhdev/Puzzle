using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DailyChallengeData", menuName = "DailyChallengeData")]
public class DailyChallengeData : ScriptableObject
{
    public int TimeReset;
    public ChallengeData[] Data;
}

[System.Serializable]
public struct ChallengeData
{
    public ChallengeType Type;
    public int[] Amount;
    public int[] InkDrops;
}

[System.Serializable]
public enum ChallengeType{
    COMPLETE_PUZZLE, PLAY_DIFFERENCE_BOOK, USE_INK_DROPS, UNLOCK_NEW_CHAPTER, USE_HINT, SHARE_PUZZLE, SPIN_LUCKY_DRAW
}