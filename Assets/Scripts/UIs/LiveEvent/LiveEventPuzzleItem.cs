using System;
using System.Collections;
using System.Collections.Generic;
using com.F4A.MobileThird;
using DataCore;
using DG.Tweening;
using LeoScript.ArtBlitz;
using UnityEngine;
using UnityEngine.UI;
using Debug = DataCore.Debug;
using Random = UnityEngine.Random;

public enum LiveEventPuzzleItemDifficult
{
    Easy,
    Normal,
    Hard
}

public enum LiveEventPuzzleItemState
{
    Lock,
    Unlock,
    Selected
}

public class LiveEventPuzzleItem : MonoBehaviour
{

    [SerializeField] private GameObject rootItem;
    [SerializeField] private Image background;
    [SerializeField] private Image lockImg;

    [SerializeField] private Color selectedColor;
    [SerializeField] private Color easyColor;
    [SerializeField] private Color mediumColor;
    [SerializeField] private Color hardColor;

    private int _id;
    private int _postCardId;
    private LiveEventPuzzleData _data;
    private LiveEventPuzzleItemState puzzleItemState;

    private Action<int> _onClick;
    private Sequence lockAnimSequence;

    private ItemPanel itemPanel = null;
    public Toggle isBossLevel;
    public void OnClick()
    {
        if (puzzleItemState == LiveEventPuzzleItemState.Selected)
        {
            return;
        }

        if (puzzleItemState != LiveEventPuzzleItemState.Unlock )
        {
            puzzleItemState = LiveEventPuzzleItemState.Selected;
        }

        PlayAnimSelect();
        _onClick?.Invoke(_id);
        
        UpdateItemState();
    }


    
    public void InitData(int postCardId, int id, LiveEventPuzzleData data, LiveEventPuzzleItemState stt, Action<int> onClick)
    {
        this._data = data;
        this._id = id;
        this._postCardId = postCardId;
        this._onClick = onClick;
        
        puzzleItemState = stt;
        PuzzleLevelData levelData = MasterDataStore.Instance.GetPuzzleById(data.BookId, data.ChapterId, data.PuzzleId);
        AssetManager.Instance.DownloadResource(levelData.ThumbnailLabel(), completed: (size) =>
        {
            AssetManager.Instance.LoadPathAsync<Sprite>(levelData.CompletePuzzleImage.CompleteImage, (thumb) =>
            {
                if (thumb != null)
                {
                    ItemPanel item = new ItemPanel();
                    item.texture = thumb.texture;
                    item.isBossLevel = isBossLevel;
                    item.selectedLayout = GetLayoutItem(data);
                    itemPanel = item;
                }
            });
        });
      
       
        UpdateUI();
    }

    private Vector2Int GetLayoutItem(LiveEventPuzzleData data)
    {
        Vector2Int vector2Int = new Vector2Int(3,4);
        switch(data.Diffucult)
        {
            case LiveEventPuzzleItemDifficult.Easy:
                vector2Int = new Vector2Int(3, 4);
                break;
            case LiveEventPuzzleItemDifficult.Normal:
                vector2Int = new Vector2Int(4, 7);
                break;
            case LiveEventPuzzleItemDifficult.Hard:
                vector2Int = new Vector2Int(5, 9);
                break;
        }
        return vector2Int;
    }

    public void Play()
    {
        if (!SocialManager.Instance.isConnectionNetwork())
        {
            if(itemPanel == null)
            {
                UIManager.Instance.ShowPopupNoInternet(() =>
                {
                });
            }else
                GameManager.Instance.StartEventLevel(_postCardId, _id, _data, ConfigManager.GameData.PlayType.new_puzzle, itemPanel);

        }
        else
        {
            GameManager.Instance.StartEventLevel(_postCardId, _id, _data, ConfigManager.GameData.PlayType.new_puzzle, itemPanel);
        }
       
    }


    public void Select()
    {
        Debug.Log("Select");
        if (puzzleItemState == LiveEventPuzzleItemState.Selected)
        {
            return;
        }
        puzzleItemState = LiveEventPuzzleItemState.Selected;
        UpdateItemState();
        PlayAnimSelect();
    }


    public void UnSelect()
    {
        Debug.Log("Unselect");
        puzzleItemState = LiveEventPuzzleItemState.Lock;
        UpdateItemState();
        StopAnimSelect();
    }

    private void StopAnimSelect()
    {
        if (lockAnimSequence != null)
        {
            lockAnimSequence.Kill();
        }
        lockImg.transform.localScale = Vector3.one;
    }

    private void PlayAnimSelect()
    {
        lockAnimSequence = DOTween.Sequence();
        lockAnimSequence.Append(lockImg.transform.DOScale(1.25f, 0.5f));
        lockAnimSequence.Append(lockImg.transform.DOScale(1.0f, 0.5f));
        lockAnimSequence.Append(lockImg.transform.DOScale(1.25f, 0.5f));
        lockAnimSequence.Append(lockImg.transform.DOScale(1.0f, 0.5f));
    }

    private void UpdateUI()
    {

        UpdateItemState();
    }

    private void UpdateItemState()
    {
        rootItem.SetActive(true);
        switch (puzzleItemState)
        {
            case LiveEventPuzzleItemState.Lock:
                UpdateItemColorByLevel(_data.Diffucult);
                break;
            case LiveEventPuzzleItemState.Unlock:
                rootItem.SetActive(false);
                break;
            case LiveEventPuzzleItemState.Selected:
                background.color = selectedColor;
                break;
           
        }
    }

    private void UpdateItemColorByLevel(LiveEventPuzzleItemDifficult level)
    {
        switch (level)
        {
            case LiveEventPuzzleItemDifficult.Easy:
                background.color = easyColor;
                break;
            case LiveEventPuzzleItemDifficult.Normal:
                background.color = mediumColor;
                break;
            case LiveEventPuzzleItemDifficult.Hard:
                background.color = hardColor;
                break;
            
        }
    }

}
