using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PuzzleCollectionItem", menuName = "PuzzleCollectionItem", order = 1)]
[PreferBinarySerialization]
public class OldCollectionData : ScriptableObject
{
    [SerializeField()]
    public SerializableDictionaryBase<string, int> collectionPuzzlesKeyAndIndex;
}
