using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DataCore;
using EventDispatcher;
using System;
using TMPro;

public class MyAllCollection : HomepageTab
{
    [SerializeField] private Transform footer;

    [SerializeField] private Transform container;
    [SerializeField] private GameObject collectGroupPrefab_2;

    private List<MyAllCollectionTab> _items;
    [SerializeField] private TextMeshProUGUI description;
    private MasterDataStore _masterDataStore;


    public override void Init()
    {
        base.Init();

        _masterDataStore = MasterDataStore.Instance;
        if (!_didInit) {
            UpdateYourCollection();
        }

        this.RegisterListener(EventID.OnUpdateSubcribeState, (o) => OnEventSubsciption());
        _didInit = true;
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
    private void UpdateYourCollection()
    {

        description.SetText(_masterDataStore.HomePageData.AllCollectionDescription);

        int amount = _masterDataStore.BookDatas.Count;
        _items = new List<MyAllCollectionTab>();
        Dictionary<BookMasterData, int> listThumbnail = new Dictionary<BookMasterData, int>();

        List<BookMasterData> sortedBook = new List<BookMasterData>();
        sortedBook = GetRandomItemsFromList<BookMasterData>(_masterDataStore.BookDatas, _masterDataStore.BookDatas.Count);

        BookMasterData book;

        for (int i = 0; i < amount; i++)
        {
            book = sortedBook[i];
            MyAllCollectionTab item;
            GameObject group_2;
            int remainTab = _items.Count;

            if (i >= remainTab)
            {
                group_2 = Instantiate(collectGroupPrefab_2, Vector3.zero, Quaternion.identity, container);
                group_2.name = "CollectionGroupCreate";
                MyAllCollectionTab[] newTabs = group_2.transform.GetComponentsInChildren<MyAllCollectionTab>();
                for (int j = 0; j < newTabs.Length; j++)
                {
                    newTabs[j].SetActive(false);
                }
                _items.AddRange(newTabs);
                remainTab = _items.Count;
            }
            item = _items[i];
            item.SetData(book.ID, book);
            item.SetActive(true);
        }
    
        DOVirtual.DelayedCall(0.0f, () =>
        {
            try
            {
                footer.SetAsLastSibling();
            }
            catch (Exception e)
            {

            }
        }).Play();
    }

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
