using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "BackgroundMusicData", menuName = "BackgroundMusicData", order = 1)]
[PreferBinarySerialization]
public class BackgroundMusicData : ScriptableObject
{
    public string AddressPath;
    public string Name;
    public string GetLabel() {
        if (!string.IsNullOrEmpty(Name)) {
            return $"Music_{Name}";
        }
        return string.Empty;
    }
}