using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum BookStatus
{
    OnGoing,
    Cancelled,
    Completed,
}

[CreateAssetMenu(fileName = "BookMasterData", menuName = "BookMasterData", order = 1)]
[PreferBinarySerialization]
public class BookMasterData : ScriptableObject
{
    public int ID;
    public string BookName;
    public string Description;
    public string Author;
    public string Illustration;
    public BookStatus Status;
    public long Release;
    public ThumbnailsData Thumbnail;
    //public Sprite Avatar;
    public List<ChapterMasterData> ListChapters = new List<ChapterMasterData>();
    public string Version;

    public void UpdateCurrentTime()
    {
        Release = DateTime.Now.Ticks;
    }
}
