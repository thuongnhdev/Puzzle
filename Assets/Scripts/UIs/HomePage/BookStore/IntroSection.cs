using System.Collections;
using System.Collections.Generic;
using DanielLochner.Assets.SimpleScrollSnap;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DataCore;
using DG.Tweening;

public class IntroSection : MonoBehaviour
{
    private const float DRAG_OFFSET = 0;
    private const float DELAY_DRAG = 1;

    [SerializeField] private RectTransform pagePrefab;
    [SerializeField] private Vector2 sizeDots;

    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private Transform pageContainer;
    [SerializeField] private SimpleScrollSnap simpleScrollSnap;
    [SerializeField] private Image[] pics;

    [SerializeField] private Sprite sprEventComming;
    [SerializeField] private Sprite sprEventLiving;
    [SerializeField] private Image imageThumbnailEvent;
    [SerializeField] private GameObject panelEventComming;
    [SerializeField] private GameObject panelEventLiving;

    private MasterDataStore _masterDataStore;

    private List<RectTransform> _pageDots;
    private List<PageData> _pageDatas;
    private bool _didInit = false;
    [SerializeField] private TextMeshProUGUI timeEventLive;

    private bool isLiveEvent = true;

    private struct PageData
    {
        public int bookId;
        public Color32 nameColor;
        public string bookName;
    }

    private int _curDot;
    private float _timeCount;
    private bool _draggingParent = false;

    private bool _didLoadCover = false;

    public void Init()
    {
        if (_didInit) return;
        try
        {
            _masterDataStore = MasterDataStore.Instance;

            int amount = _masterDataStore.HomePageData.IntroData.Count;
            _pageDatas = new List<PageData>(amount);

            if (!simpleScrollSnap.enabled)
            {
                DOVirtual.DelayedCall(0.2f, () =>
                {
                    simpleScrollSnap.enabled = true;
                });
            }
            var tile = (float)Screen.height / Screen.width;
            if (tile < 1.5f)
                simpleScrollSnap.sizeControl = SimpleScrollSnap.SizeControl.FitWidth;
            else
                simpleScrollSnap.sizeControl = SimpleScrollSnap.SizeControl.Fit;
            DataCore.Debug.Log("Page: " + amount, false);
            PageData data;
            BookMasterData bookData;
            Dictionary<BookMasterData, int> listThumbnail = new Dictionary<BookMasterData, int>();
            for (int i = 0; i < amount; i++)
            {
                data = new PageData();
                var introData = _masterDataStore.HomePageData.IntroData[i];
                bookData = _masterDataStore.GetBookByID(introData.BookID);
                if (bookData == null)
                {
                    DataCore.Debug.LogError("Book Id isn't exist");
                    amount--;
                    continue;
                }
                data.bookId = bookData.ID;
                data.bookName = bookData.BookName;
                listThumbnail.Add(bookData, i);
                _pageDatas.Add(data);
            
            }
            _didInit = true;


            CreatePageDots(amount);

            if (isLiveEvent)
                ActiveEvent(0);
        }
        catch (System.Exception ex)
        {
            DataCore.Debug.LogError($"IntroSection Init: {ex.Message}");
        }

    }
    

    private IEnumerator LoadCovers() {
        if (!_didInit) yield return null;
        _didLoadCover = true;
        yield return new WaitForEndOfFrame();
        int amount = _masterDataStore.HomePageData.IntroData.Count;

        var i = 0;
        bool loading = false;

        while (i < amount) {
            var introData = _masterDataStore.HomePageData.IntroData[i];
#if UNITY_ANDROID
                    AssetManager.Instance.LoadPathAsync<Sprite>(introData.Cover.IphoneThumbnail, (coverPicture) =>
                    {
                        if (coverPicture != null)
                        {
                            DataCore.Debug.Log($"{i} {introData.Cover.IphoneThumbnail}", false);
                            pics[i].sprite = coverPicture;
                            pics[i].rectTransform.localScale = new Vector3(1f, 1f, 1);
                        }
                        loading = true;
                    });
#elif UNITY_IOS
            var identifier = SystemInfo.deviceModel;

            if (identifier.StartsWith("iPad"))
            {
                // Do something for iPad
                AssetManager.Instance.DownloadResource(introData.Cover.Label, completed: (size) =>
                {
                    AssetManager.Instance.LoadPathAsync<Sprite>(introData.Cover.IPadThumbnail, (coverPicture) =>
                    {
                        if (coverPicture != null)
                        {
                            DataCore.Debug.Log($"{i} {introData.Cover.IPadThumbnail}", false);
                            pics[i].sprite = coverPicture;
                            pics[i].rectTransform.localScale = new Vector3(0.94f, 0.7f, 1);
                        }
                        loading = true;
                    });
                });

            }
            else
            {
                AssetManager.Instance.DownloadResource(introData.Cover.Label, completed: (size) =>
                {
                    AssetManager.Instance.LoadPathAsync<Sprite>(introData.Cover.IphoneThumbnail, (coverPicture) =>
                    {
                        if (coverPicture != null)
                        {
                            DataCore.Debug.Log($"{i} {introData.Cover.IphoneThumbnail}", false);
                            pics[i].sprite = coverPicture;
                            pics[i].rectTransform.localScale = new Vector3(1f, 1f, 1);
                        }
                        loading = true;
                    });
                });

            }
#endif
            yield return new WaitUntil(() => loading);

            i++;
            loading = false;
        }
        yield return new WaitForEndOfFrame();
    }

    private void ActiveEvent(int dots)
    {
        panelEventLiving.SetActive(false);
        panelEventComming.SetActive(false);
        if (dots == 0)
        {
            if (LiveEventTimer.Instance.IsEventActive)
            {
                imageThumbnailEvent.sprite = sprEventLiving;
                panelEventLiving.SetActive(true);
                timeEventLive.text = LiveEventTimer.Instance.GetDay() + "d " + LiveEventTimer.Instance.GetHour() + "h";
            }
            else
            {
                imageThumbnailEvent.sprite = sprEventComming;
                panelEventComming.SetActive(true);
            }
        }

    }
    public void CreatePageDots(int dots)
    {
        _pageDots = new List<RectTransform>(dots);

        RectTransform newDot;
        if (isLiveEvent) dots = dots + 1;
        for (int i = 0; i < dots; i++)
        {
            newDot = Instantiate(pagePrefab, Vector3.zero, Quaternion.identity, pageContainer);
            _pageDots.Add(newDot);
        }

        _curDot = 0;
        UpdatePages();
    }

    public void OnPagetap()
    {
        SoundController.Instance.PlaySfxClick();
        if (isLiveEvent)
        {
            if (_curDot == 0)
            {
                UIManager.Instance.ShowUILiveEvent();
            }
            else
            {
                UIManager.Instance.CloseUIHome();
                UIManager.Instance.ShowUIBookDetail(_pageDatas[_curDot].bookId, "featured_book");
            }
        }
        else
        {
            UIManager.Instance.CloseUIHome();
            UIManager.Instance.ShowUIBookDetail(_pageDatas[_curDot].bookId, "featured_book");
        }

    }

    public void OnLiveEvent()
    {
        SoundController.Instance.PlaySfxClick();
        UIManager.Instance.ShowUILiveEvent();
    }

    public void PanelChange()
    {
        if (isLiveEvent)
        {
            panelEventLiving.SetActive(false);
            panelEventComming.SetActive(false);
            _curDot = simpleScrollSnap.CurrentPanel;
            ActiveEvent(_curDot);
            UpdatePages();
        }
        else
        {
            _curDot = simpleScrollSnap.CurrentPanel;
            UpdatePages();
        }
    }

    private void Update()
    {
        if (_timeCount > 0)
        {
            _timeCount -= Time.deltaTime;
        }
        if (!_didLoadCover && this.isActiveAndEnabled) {
            StartCoroutine(LoadCovers());
        }
    }

    private void PreviousPage()
    {
        _curDot--;
        if (_curDot < 0)
        {
            _curDot = _pageDots.Count - 1;
        }

        UpdatePages();
    }

    private void NextPage()
    {
        _curDot++;
        if (_curDot >= _pageDots.Count)
        {
            _curDot = 0;
        }
        UpdatePages();
    }

    private void UpdatePages()
    {
        for (int i = 0; i < _pageDots.Count; i++)
        {
            _pageDots[i].sizeDelta = new Vector2(i == _curDot ? sizeDots.y : sizeDots.x, _pageDots[i].sizeDelta.y);
        }
        if (isLiveEvent)
        {
            if (_curDot != 0 && _curDot < _pageDatas.Count)
            {
                nameTxt.SetText(_pageDatas[_curDot].bookName);
                nameTxt.color = _pageDatas[_curDot].nameColor;
            }
        }
        else if (_curDot < _pageDatas.Count)
        {
            nameTxt.SetText(_pageDatas[_curDot].bookName);
            nameTxt.color = _pageDatas[_curDot].nameColor;
        }
    }
}
