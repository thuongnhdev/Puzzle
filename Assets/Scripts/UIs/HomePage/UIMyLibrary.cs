using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DataCore;
using EventDispatcher;
using System;

public class UIMyLibrary : HomepageTab
{
    [SerializeField] private ResumePlayingSection resumePlayingSection;
    [SerializeField] private Transform footer;

    [SerializeField] private Transform container;
    [SerializeField] private GameObject collectGroupPrefab_2;

    private List<YourCollectionTab> _items;
    private MasterDataStore _masterDataStore;

    [SerializeField]
    private GameObject loginObject;

    public override void Init()
    {
        base.Init();
        if (!_didInit) {
            _masterDataStore = MasterDataStore.Instance;
            _items = new List<YourCollectionTab>();

            resumePlayingSection.Init();

            this.RegisterListener(EventID.OnUpdateSubcribeState, (o) => OnEventSubsciption());
            this.RegisterListener(EventID.OnUpdateResumePlaying, (o) => OnEventSubsciption());
        }
        _didInit = true;
    }


    protected override void AfterShowed()
    {
        base.AfterShowed();
        DataCore.Debug.Log("UIMyLibrary AfterShowed", false);
        loginObject.SetActive(true);
        var login = PlayerPrefs.GetInt(ConfigManager.LoginSuccess, 0);
        if (login == 1)
            loginObject.SetActive(false);

        scrollRect.verticalNormalizedPosition = 1;
        resumePlayingSection.UpdateData();
        UpdateYourCollection();
    }

    public void OnClickLogin()
    {
        UIManager.Instance.ShowUILogin();
    }
    private void OnEventSubsciption()
    {
        UpdateYourCollection();
    }
    private void UpdateYourCollection()
    {
        DataCore.Debug.Log("UIMyLibrary UpdateYourCollection", false);

        if (GameData.Instance.SavedPack == null || GameData.Instance.SavedPack.BookSaveDatas == null)
            return;

        int len = GameData.Instance.SavedPack.BookSaveDatas.Count;

        BookMasterData book;
        YourCollectionTab item;
        GameObject group_2;
        int remainTab = _items.Count;
        for (int i = 0; i < len; i++)
        {
            book = _masterDataStore.GetBookByID(GameData.Instance.SavedPack.BookSaveDatas[i].Id);
            if (i >= remainTab)
            {
                group_2 = Instantiate(collectGroupPrefab_2, Vector3.zero, Quaternion.identity, container);
                group_2.name = "CollectionGroupCreate";
                YourCollectionTab[] newTabs = group_2.transform.GetComponentsInChildren<YourCollectionTab>();
                for (int j = 0; j < newTabs.Length; j++)
                {
                    newTabs[j].SetActive(false);
                }
                _items.AddRange(newTabs);
                remainTab = _items.Count;
            }
            item = _items[i];
            item.SetData(GameData.Instance.SavedPack.BookSaveDatas[i].Id, book);
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
}
