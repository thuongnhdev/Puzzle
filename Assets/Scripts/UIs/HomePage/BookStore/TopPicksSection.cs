using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using DataCore;

public class TopPicksSection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TopPickTab tabPrefab;
    [SerializeField] private Transform footer;

    [SerializeField] private Transform tabContainer;

    private MasterDataStore _masterDataStore;

    private List<TopPickTab> _tabs;

    public void Init()
    {
        _masterDataStore = MasterDataStore.Instance;

        description.SetText(_masterDataStore.HomePageData.TopPickDescrtion);

        int amount = _masterDataStore.HomePageData.TopPickData.Count;
        _tabs = new List<TopPickTab>(amount);
        Dictionary<BookMasterData, int> listThumbnail = new Dictionary<BookMasterData, int>();
        BookMasterData book;

        for (int i = 0; i < amount; i++)
        {
            book = _masterDataStore.GetBookByID(_masterDataStore.HomePageData.TopPickData[i]);
            if (book != null)
            {
                var newTab = Instantiate(tabPrefab, Vector3.zero, Quaternion.identity, tabContainer);
                listThumbnail.Add(book,i);
                AssetManager.Instance.LoadPathAsync<Sprite>(book.Thumbnail.Thumbnail, (thumb) =>
                {
                    if (thumb != null && listThumbnail != null)
                    {
                        foreach (var item in listThumbnail)
                        {
                            if (item.Key.Thumbnail.Thumbnail.Contains(thumb.name))
                            {
                                newTab.SetData(item.Key.ID, thumb);
                            }
                        }
                    }
                });
                _tabs.Add(newTab);
            }
            else
            {
                DataCore.Debug.LogError("[TOP PICK] Cant find book " + _masterDataStore.HomePageData.TopPickData[i]);
            }
        }

        DOVirtual.DelayedCall(0.0f, () =>
        {
            footer.SetAsLastSibling();
        }).Play();
    }
}
