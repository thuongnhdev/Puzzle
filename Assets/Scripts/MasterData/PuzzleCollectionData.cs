using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PuzzleCollectionData", menuName = "PuzzleCollectionData", order = 1)]
[PreferBinarySerialization]
public class PuzzleCollectionData : ScriptableObject
{
    public List<PuzzleLevelData> PuzzleLevels = new List<PuzzleLevelData>();
}
