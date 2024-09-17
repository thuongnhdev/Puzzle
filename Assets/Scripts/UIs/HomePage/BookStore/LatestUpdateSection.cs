using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class LatestUpdateSection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private LatestUpdateTab tabPrefab;
    [SerializeField] private Transform footer;

    [SerializeField] private Transform tabContainer;

    private MasterDataStore _masterDataStore;

    private List<LatestUpdateTab> _tabs;

    public void Init()
    {
        _masterDataStore = MasterDataStore.Instance;

        description.SetText(_masterDataStore.HomePageData.LatestUpdateDescrtion);

        int amount = _masterDataStore.HomePageData.LastestUpdateItems.Count;
        if (amount > MCache.Instance.Config.MAX_LATEST_UPDATE_PUZZLE)
        {
            amount = MCache.Instance.Config.MAX_LATEST_UPDATE_PUZZLE;
        }


        _tabs = new List<LatestUpdateTab>(amount);
        ChapterMasterData part;
        LastestUpdateItem item;
        LatestUpdateTab newTab;

        for (int i = 0; i < amount; i++)
        {
            item = _masterDataStore.HomePageData.LastestUpdateItems[i];
            part = _masterDataStore.GetPartById(item.BookID, item.PartID);
            if (part != null)
            {
                newTab = Instantiate(tabPrefab, Vector3.zero, Quaternion.identity, tabContainer);
                newTab.SetData(item.BookID, part);
                _tabs.Add(newTab);
            }
            else
            {
                DataCore.Debug.LogError("[LATESTUPDATE] Cant find Part " + _masterDataStore.HomePageData.TopPickData[i]);
            }
        }

        DOVirtual.DelayedCall(0.0f, () =>
        {
            footer.SetAsLastSibling();
        }).Play();
    }

}
