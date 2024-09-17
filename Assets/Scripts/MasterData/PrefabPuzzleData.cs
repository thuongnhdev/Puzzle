using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "PrefabPuzzleData", menuName = "PrefabPuzzleData", order = 1)]
public class PrefabPuzzleData : ScriptableObject
{
    public PuzzleLevelInitData PrefabPuzzleLevelData;
}