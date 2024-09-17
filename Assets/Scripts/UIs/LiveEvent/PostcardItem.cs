using System;
using System.Collections;
using System.Collections.Generic;
using DataCore;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Debug = DataCore.Debug;

[System.Serializable]
public enum LiveEventPostCardState
{
    Lock,
    Unlock,
    Completed,
    Claimed,
}


public class PostcardItem : MonoBehaviour
{
    public LiveEventPostCardState Status
    {
        get { return _status; }
    }

    public int TotalPuzzleComplete
    {
        get {
            if (_id < GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas.Count)
                return GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas[_id].PuzzleIdCompleted.Count;
            else
                return 0;
        }
    }

    public int TotalPuzzle
    {
        get { return listItem.Count; }
    }

    public int InkReward
    {
        get { return _inkReward; }
    }

    [SerializeField] private GameObject lockGroup;
    [SerializeField] private GameObject puzzleGroup;
    [SerializeField] private Image postcardImg;
    [SerializeField] private Transform puzzleItemPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private RectTransform bgRectTransform;

    private int _id;
    private int _selectedPuzzleId;
    private int _inkReward;
    private LiveEventPostCardState _status;
    private LiveEventPostCardData _liveEventPostCardData;
    private List<LiveEventPuzzleItem> listItem = new List<LiveEventPuzzleItem>();

    [SerializeField] private Sprite[] sprPostCard;
 
    public void InitData(int id, LiveEventPostCardData postCardData)
    {
        ClearData();
        this._id = id;
        this._status = LiveEventPostCardState.Lock;
        if (id < GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas.Count)
            this._status = GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas[_id].Status;

        if (GameData.Instance.IsVipIap())
        {
            this._status = LiveEventPostCardState.Unlock;
            GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas[_id].Status = LiveEventPostCardState.Unlock;
        }
        this._inkReward = postCardData.InkReward;
        _liveEventPostCardData = postCardData;
        postcardImg.sprite = sprPostCard[id];
        //DownloadPostcardImg();
        UpdateUI();
    }

    private void DownloadPostcardImg()
    {
        AssetManager.Instance.DownloadResource(_liveEventPostCardData.ThumbnailLabel(), completed: (size) =>
        {
            AssetManager.Instance.LoadPathAsync<Sprite>(_liveEventPostCardData.Thumbnail.Thumbnail, (thumb) =>
            {
                if (thumb != null && postcardImg != null)
                {
                    postcardImg.sprite = thumb;
                }
            });
        });
    }

    public void CheatUnlock24Item()
    {
        int amount = _liveEventPostCardData.PuzzleData.Length;
        for (int i = 0; i < amount-1; i++)
        {
            GameData.Instance.SavedPack.LiveEventSavedData.UpdateCompletePuzzle(this._id, i);
        }
        GameData.Instance.saveGameData();
    }

    

    public void UpdateUI()
    {
        if (_status == LiveEventPostCardState.Lock)
        {
            lockGroup.SetActive(true);
            puzzleGroup.SetActive(false);
            return;
        }
        else
        {
            lockGroup.SetActive(false);
            puzzleGroup.SetActive(true);
        }

        int amount = _liveEventPostCardData.PuzzleData.Length;
        if (_id < GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas.Count)
            _selectedPuzzleId = GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas[_id].SelectedPuzzleId;


        LiveEventPuzzleItemState state;

        int checkCompletePostCard = -1;
        for (int i = 0; i < amount; i++)
        {
            var puzzleItem = Instantiate(puzzleItemPrefab, content).GetComponent<LiveEventPuzzleItem>();
            state = GameData.Instance.SavedPack.LiveEventSavedData.PostCardDatas[_id].GetPuzzleStatus(i);
            puzzleItem.InitData(_id, i, _liveEventPostCardData.PuzzleData[i], state, OnPuzzleSelected);
            listItem.Add(puzzleItem);
            if (state == LiveEventPuzzleItemState.Lock && checkCompletePostCard == -1)
            {
                checkCompletePostCard = i;
            }

            if (state == LiveEventPuzzleItemState.Unlock && i == _selectedPuzzleId)
            {
                _selectedPuzzleId = -1;
            }
        }

        if (_selectedPuzzleId == -1)
        {
            _selectedPuzzleId = checkCompletePostCard;
        }

        if (this.Status == LiveEventPostCardState.Unlock)
        {
            if (checkCompletePostCard == -1)
            {
                this._status = LiveEventPostCardState.Completed;
                GameData.Instance.SavedPack.LiveEventSavedData.UpdatePostCardStatus(_id, LiveEventPostCardState.Completed);
                GameData.Instance.RequestSaveGame();
                AnimationShowPostcardImg();
            }
            else
            {
                listItem[_selectedPuzzleId].Select();
            }
        }
    }

    public void PlayNow()
    {
        listItem[_selectedPuzzleId].Play();
    }

    public void OnPuzzleSelected(int selectedId)
    {
        listItem[_selectedPuzzleId].UnSelect();
        this._selectedPuzzleId = selectedId;
    }

    public void UpdateClaimed()
    {
        this._status = LiveEventPostCardState.Claimed;
        GameData.Instance.SavedPack.LiveEventSavedData.AddPostCardItem(_id);
        GameData.Instance.SavedPack.LiveEventSavedData.UpdatePostCardStatus(_id, LiveEventPostCardState.Claimed);
        GameData.Instance.RequestSaveGame();
    }
	
    private void ClearData()
    {
        for (int i = 0; i < listItem.Count; i++)
        {
            Destroy(listItem[i].gameObject);
        }
        listItem.Clear();
    }

    [ContextMenu("Anim")]
    public void AnimationShowPostcardImg()
    {
      //  transform.DOPunchScale(Vector3.one * 0.55f, 1, 10, 1);
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 1.0f).SetDelay(0.1f).SetEase(Ease.OutBack).Play();
    }

    public void AnimScaleBackGround(Action onComplete = null)
    {
        bgRectTransform.localScale = Vector3.one;
        bgRectTransform.DOScale(new Vector3(0.3f,0.3f,0.3f), 0.4f).SetDelay(0.1f).SetEase(Ease.Linear).Play().OnComplete(()=> {
            bgRectTransform.DOScale(Vector3.one, 0.4f).SetEase(Ease.Linear).Play().OnComplete(()=> {
                onComplete?.Invoke();
            });
        });
    }
}
