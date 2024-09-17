using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "SpineData", menuName = "SpineData", order = 1)]
[PreferBinarySerialization]
public class SpineData : ScriptableObject
{
    public string animation;
}