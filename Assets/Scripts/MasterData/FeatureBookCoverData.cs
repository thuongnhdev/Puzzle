using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "FeatureBookCoverData", menuName = "FeatureBookCoverData", order = 1)]
[PreferBinarySerialization]
public class FeatureBookCoverData : ScriptableObject
{
    public string Label;
    public string IphoneThumbnail;
    public string IPadThumbnail;
}