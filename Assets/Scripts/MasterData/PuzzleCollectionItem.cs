using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PuzzleCollectionItem", menuName = "PuzzleCollectionItem", order = 1)]
[PreferBinarySerialization]
public class PuzzleCollectionItem : ScriptableObject
{
    public int CollectionId;
    public string CollectionName;
    public string PuzzleName;
    public PuzzleLevelData PuzzleLevels;
}
