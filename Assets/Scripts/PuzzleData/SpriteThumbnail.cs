using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteThumbnail", menuName = "SpriteThumbnail", order = 1)]
public class SpriteThumbnail : ScriptableObject
{
    public string ID;
    public Sprite Thumbnail;
}
