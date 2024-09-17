using System.Collections;
using System.Collections.Generic;
using EventDispatcher;
using UnityEngine;
using TMPro;
using DataCore;

public class AllCollectionSection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private AllCollectionTab tabPrefab;

    [SerializeField] private Transform tabContainer;

    private MasterDataStore _masterDataStore;

    private List<AllCollectionTab> _tabs;

    public void Init()
    {
        _masterDataStore = MasterDataStore.Instance;

        description.SetText(_masterDataStore.HomePageData.AllCollectionDescription);

        int amount = _masterDataStore.BookDatas.Count;
        _tabs = new List<AllCollectionTab>(amount);
        Dictionary<BookMasterData, int> listThumbnail = new Dictionary<BookMasterData, int>();

        List<BookMasterData> sortedBook = new List<BookMasterData>(amount);
        sortedBook.AddRange(_masterDataStore.BookDatas);
        sortedBook.Sort((t1, t2) => t1.Release.CompareTo(t2.Release));

        BookMasterData book;
        
        for (int i = amount - 1; i >= 0; i--)
        {

            book = sortedBook[i];

            AllCollectionTab newTab = Instantiate(tabPrefab, Vector3.zero, Quaternion.identity, tabContainer);
            listThumbnail.Add(book, i);
            AssetManager.Instance.LoadPathAsync<Sprite>(book.Thumbnail.Thumbnail, (thumb) =>
            {
                if (thumb != null)
                {
                    foreach (var item in listThumbnail)
                    {
                        if (item.Key.Thumbnail.Thumbnail.Contains(thumb.name))
                        {
                            newTab.SetData(item.Key.ID, item.Key.BookName, item.Key.Author, item.Key.Status.ToString(), item.Key.Release, thumb);
                            _tabs.Add(newTab);
                        }
                    }
                }
            });

        }

        this.RegisterListener(EventID.OnUpdateSubcribeState, (o) => UpdateSubcribe((int)o));
    }

    public void UpdateSubcribe(int bookId)
    {
        //DataCore.Debug.Log("Update " + bookId);
        for (int i = 0; i < _tabs.Count; i++)
        {
            _tabs[i].UpdateData(bookId);
        }
    }
}
