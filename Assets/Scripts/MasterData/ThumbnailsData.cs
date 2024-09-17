using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "ThumbnailsData", menuName = "ThumbnailsData", order = 1)]
[PreferBinarySerialization]
public class ThumbnailsData : ScriptableObject
{
    public string Thumbnail;
}
