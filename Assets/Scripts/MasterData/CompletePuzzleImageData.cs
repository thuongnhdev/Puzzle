using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "CompletePuzzleImageData", menuName = "CompletePuzzleImageData", order = 1)]
[PreferBinarySerialization]
public class CompletePuzzleImageData : ScriptableObject
{
    public string CompleteImage;
}