using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DataCore;
using EventDispatcher;
using System;
using TMPro;
using ArtStory.Home.Puzzle.Grids;
using System.Linq;

public class PuzzleCollectionTab : HomepageTab
{
    [SerializeField] private Transform footer;

    [SerializeField] private Transform[] container;
    [SerializeField] private GameObject puzzlePrefab;
    [SerializeField] private Transform[] puzzleParent;

    private List<MyAllCollectionTab> _items;
    //private MasterDataStore _masterDataStore;

    [SerializeField] private GameObject[] imageChoiceTab;
    [SerializeField] private BasicGridAdapter[] panelChoiceTab;

    [SerializeField] private Color[] colorsTmpChoice;
    [SerializeField] private TextMeshProUGUI[] tmpChoice;

    [SerializeField] private Sprite sprDefaulThumbanil;

    [SerializeField] private RectTransform rectTransformTab;

    [SerializeField] private RectTransform rectButtonTab;


    public override void Init()
    {
        base.Init();

        //_masterDataStore = MasterDataStore.Instance;
        if (!_didInit) {
            //init status puzzle collection
            InitStatusPuzzle();

            SetStatusTab();
            UpdateYourCollection();

            this.RegisterListener(EventID.OnUpdateSubcribeState, (o) => OnEventSubsciption());

            var tile = (float)Screen.height / Screen.width;

            if (tile > 1.7f)
            {
                rectButtonTab.offsetMax = new Vector2(rectButtonTab.offsetMax.x, 30);
            }
            else
            {
                rectButtonTab.offsetMax = new Vector2(rectButtonTab.offsetMax.x, 0);
            }
            SetPositionIndexTab(1.0f);
        }
        _didInit = true;
    }

    private void SetPositionIndexTab(float timeDelay = 0.5f)
    {
        DOVirtual.DelayedCall(timeDelay, () =>
        {
            try
            {
                if (!panelChoiceTab[(int)currentTab].gameObject.activeInHierarchy)
                    return;

                var itemCacheIndex = GameData.Instance.SavedPack.DataGetIndexPuzzleCollectionComplete((int)currentTab);
                if (itemCacheIndex != null)
                {
                    var listItemData = MasterDataStore.Instance.GetCollectionData((int)currentTab);
                    if (listItemData == null || listItemData.Count == 0) return;
                    //var index = listItemData.FindIndex(x => x.Name == itemCacheIndex.PuzzleName && x.ID == itemCacheIndex.PuzzleId);
                    var index = itemCacheIndex.CollectionIndex;
                    if (index > 4)
                    {
                        DataCore.Debug.Log($"currentTab: {currentTab} SmoothScrollTo {index}");
                        var tab = panelChoiceTab[(int)currentTab];                        
                        tab.SmoothScrollTo(index, 0.5f, onDone: () => {
                            DataCore.Debug.Log($"completed SmoothScrollTo {index} in currentTab: {currentTab}");
                        });
                    }
                }
            }
            catch (Exception e)
            {
                DataCore.Debug.Log($"Failed to SetPositionIndexTab. Error: {e.Message}");
            }
        }).Play();
    }

    private void OnEnable()
    {
        var index = (int)currentTab;
        if (puzzleParent[index].transform.childCount == 0)
            return;
        OnClickChangeTab((int)currentTab);
        panelChoiceTab[index].OnRefesh();
    }

    private void InitStatusPuzzle()
    {
        for (int i = 0; i < MasterDataStore.Instance.HomePageData.puzzleCollectionDatas.Length; i++)
        {
            if (i == (int)MasterDataStore.TypeCollection.ALL)
            {
                if (GameData.Instance.SavedPack.DataGetCollectionPuzzlePlaying(0, 0) == null)
                {
                    GameData.Instance.SavedPack.DataSetCollectionPuzzlePlaying(0, 0, PuzzleStatus.COMPLETE);
                }

                if (GameData.Instance.SavedPack.DataGetCollectionPuzzlePlaying(0, 1) == null)
                {
                    GameData.Instance.SavedPack.DataSetCollectionPuzzlePlaying(0, 1, PuzzleStatus.UNLOCK);
                }
            }
            else
            {
                if (GameData.Instance.SavedPack.DataGetCollectionPuzzlePlaying(i, 0) == null)
                {
                    GameData.Instance.SavedPack.DataSetCollectionPuzzlePlaying(i, 0, PuzzleStatus.UNLOCK);
                }
            }

        }
        try
        {
            if (!GameData.Instance.IsVipIap())
            {
                for (int i = 0; i < MasterDataStore.Instance.HomePageData.puzzleCollectionDatas.Length; i++)
                {
                    var listItemData = MasterDataStore.Instance.GetCollectionData(i);
                    var lastPuzzle = GameData.Instance.SavedPack.DataGetLastPlayedPuzzleInCollection(i);
                    if (lastPuzzle != null && lastPuzzle.PuzzleStatus == PuzzleStatus.COMPLETE && lastPuzzle.CollectionIndex > 0)
                    {
                        //var index = listItemData.FindIndex(puzzle => puzzle.Name == lastPuzzle.PuzzleName && puzzle.ID == lastPuzzle.PuzzleId);
                        if (lastPuzzle.CollectionIndex + 1 < listItemData.Count)
                        {
                            var nextPuzzle = listItemData[lastPuzzle.CollectionIndex + 1];
                            if (nextPuzzle != null && nextPuzzle.Name != "FakeData") {
                                var playedNextPuzzle = GameData.Instance.SavedPack.DataGetCollectionPuzzlePlaying(i, (lastPuzzle.CollectionIndex + 1));
                                if (playedNextPuzzle == null || playedNextPuzzle.PuzzleStatus != PuzzleStatus.COMPLETE)
                                {
                                    GameData.Instance.SavedPack.DataSetCollectionPuzzlePlaying(i, (lastPuzzle.CollectionIndex + 1), PuzzleStatus.UNLOCK);
                                }
                            }        
                        }
                    }

                }
            }
        }
        catch (Exception ex)
        {
            DataCore.Debug.Log($"Failed to unlock puzzle. Error: {ex.Message}");
        }
      
    }

    private void SetStatusTab()
    {
        for (int i = 0; i < imageChoiceTab.Length; i++)
        {
            imageChoiceTab[i].SetActive(false);
            panelChoiceTab[i].gameObject.SetActive(false);
            tmpChoice[i].color = colorsTmpChoice[0];
        }
        var index = (int)currentTab;
        imageChoiceTab[index].SetActive(true);
        panelChoiceTab[index].gameObject.SetActive(true);
        tmpChoice[index].color = colorsTmpChoice[1];
    }

    protected override void AfterShowed()
    {
        base.AfterShowed();

        scrollRect.verticalNormalizedPosition = 1;
        UpdateYourCollection();
    }

    private void OnEventSubsciption()
    {
        UpdateYourCollection();
    }

    public void OnClickChangeTab(int typeCollection)
    {
        DataCore.Debug.Log($"OnClickChangeTab {typeCollection}");
        currentTab = (MasterDataStore.TypeCollection)typeCollection;
        SetStatusTab();
        UpdateYourCollection();
        SetPositionIndexTab();
    }


    private MasterDataStore.TypeCollection currentTab = MasterDataStore.TypeCollection.ALL;
    private void UpdateYourCollection()
    {
        if (puzzleParent[(int)currentTab].transform.childCount > 0)
            return;

        var puzzleList = MasterDataStore.Instance.GetCollectionData((int)currentTab);

        int amount = puzzleList.Count;

        _items = new List<MyAllCollectionTab>();
        panelChoiceTab[(int)currentTab].InitData(((int)currentTab), amount, puzzleList, sprDefaulThumbanil);

        DOVirtual.DelayedCall(0.0f, () =>
        {
            try
            {
                footer.SetAsLastSibling();
            }
            catch
            {

            }
        }).Play();
    }

    //    private void PuzzlePlayAgainCallback(PuzzleLevelData puzzleData)
    //{
    //    for (int i = 0; i < masterData.PuzzleLevels.Count; i++)
    //    {
    //        if (masterData.PuzzleLevels[i].ID == puzzleData.ID)
    //        {
    //            UIManager.Instance.PopupPlayAgain.SetData(new object[] { masterData.PuzzleLevels[i], bookID, masterData.ID });
    //            UIManager.Instance.PopupPlayAgain.Open();
    //        }
    //    }

    //}

    public static List<T> GetRandomItemsFromList<T>(List<T> list, int number)
    {
        // this is the list we're going to remove picked items from
        List<T> tmpList = new List<T>(list);
        // this is the list we're going to move items to
        List<T> newList = new List<T>();

        // make sure tmpList isn't already empty
        while (newList.Count < number && tmpList.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, tmpList.Count);
            newList.Add(tmpList[index]);
            tmpList.RemoveAt(index);
        }

        return newList;
    }
}
