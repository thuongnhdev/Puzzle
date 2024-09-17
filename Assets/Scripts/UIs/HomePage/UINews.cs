using System.Collections;
using System.Collections.Generic;
using DataCore;
using DG.Tweening;
using TMPro;
using com.F4A.MobileThird;

#if UNITY_IOS
using Unity.Notifications.iOS;
#endif
using LogicUI.FancyTextRendering;

using UnityEngine;

public class UINews : HomepageTab
{
    [SerializeField] private Transform footer;
    [SerializeField] private Transform container;

    [SerializeField] private NewChapterTab tabPrefab;

    [SerializeField] private Transform versionContainer;
    [SerializeField] private TextMeshProUGUI versionTxt;
    [SerializeField] private MarkdownRenderer versionDetailTxt;

    [SerializeField] private PopupAppUpdate popupAppUpdate;
    [SerializeField] private GameObject btnNotification;

    private List<NewChapterTab> _tabs;
    private MasterDataStore _masterDataStore;

    public override void Init()
    {
        base.Init();
        if (!_didInit) {
            _masterDataStore = MasterDataStore.Instance;

            if (GameData.Instance.SavedPack.SaveData.NewChapterRemoved == null)
            {
                GameData.Instance.SavedPack.SaveData.NewChapterRemoved = new List<int>();
            }

            CheckNotificationPermission();

            versionTxt.SetText(_masterDataStore.HomePageData.VersionName);
            string versionDetail = _masterDataStore.HomePageData.VersionUpdate.text;
            versionDetailTxt.Source = versionDetail;

            popupAppUpdate.SetData(new object[] { _masterDataStore.HomePageData.VersionName, versionDetail });

            int amount = _masterDataStore.HomePageData.NewChapterUpdates.Length;
            _tabs = new List<NewChapterTab>(amount);
            DataCore.Debug.Log("New Chapter Update: " + amount);
            try
            {
                ChapterMasterData part;
                NewChapterUpdate item;
                NewChapterTab newTab;

                for (int i = 0; i < amount; i++)
                {
                    item = _masterDataStore.HomePageData.NewChapterUpdates[i];
                    if (GameData.Instance.SavedPack.SaveData.NewChapterRemoved.Contains(item.Id))
                    {
                        continue;
                    }

                    part = _masterDataStore.GetPartById(item.BookId, item.PartID);

                    if (part != null)
                    {
                        newTab = Instantiate(tabPrefab, Vector3.zero, Quaternion.identity, container);
                        AssetManager.Instance.DownloadResource(part.ChapterThumbnailLabel(), completed: (size) =>
                        {
                            AssetManager.Instance.LoadPathAsync<Sprite>(part.Thumbnail.Thumbnail, (thumb) =>
                            {
                                if (thumb != null)
                                {
                                    newTab.SetData(item.Id, item.BookId, part.ID, part.PartName, part.Author, item.UpdateTime, thumb);
                                    _tabs.Add(newTab);
                                }
                            });
                        });

                    }
                    else
                    {
                        DataCore.Debug.LogError("[New Chapter Tab] Cant find data" + item.BookId + "-" + item.PartID);
                    }
                }

                DOVirtual.DelayedCall(0.0f, () =>
                {
                    versionContainer.SetAsLastSibling();
                    footer.SetAsLastSibling();
                }).Play();
            }
            catch (System.Exception ex)
            {
                DataCore.Debug.LogError($"Failed UINews Init: {ex.Message}");
            }
        }
        _didInit = true;

    }

    private void OnEnable()
    {
        CheckNotificationPermission();
    }

    private void CheckNotificationPermission()
    {
#if UNITY_IOS
        var tokenSent = PlayerPrefs.GetInt(ConfigManager.TokenSent, 0) != 0;
        if (tokenSent)
        {
            var notificationSetting = iOSNotificationCenter.GetNotificationSettings();
            btnNotification.SetActive(notificationSetting.AuthorizationStatus != AuthorizationStatus.Authorized);
        }
        else
        {
            btnNotification.SetActive(true);
        }
#elif UNITY_ANDROID
        btnNotification.SetActive(false);
#elif UNITY_EDITOR
        btnNotification.SetActive(true);
#endif
    }

    protected override void AfterShowed()
    {
        base.AfterShowed();
        //scrollRect.verticalNormalizedPosition = 1;
        ShareUIManager.Instance.ShowSharedUI(SceneSharedEle.NONE);

        UpdateData();
    }

    public void ShowAppUpdatePopup()
    {
        popupAppUpdate.Open();
    }

    private void UpdateData()
    {
        int amount = _tabs.Count;

        NewChapterUpdate item;

        for (int i = 0; i < amount; i++)
        {
            item = _masterDataStore.HomePageData.NewChapterUpdates[i];
            if (GameData.Instance.SavedPack.SaveData.NewChapterRemoved.Contains(item.Id) || !GameData.Instance.SavedPack.GetSubcribeState(item.BookId))
            {
                _tabs[i].gameObject.SetActive(false);
            }
            else
            {
                _tabs[i].gameObject.SetActive(true);

            }
        }
    }

    public void OnNotificationClick()
    {
        DataCore.Debug.Log("OnNotificationClick");
        var tokenSent = PlayerPrefs.GetInt(ConfigManager.TokenSent, 0) != 0;
        if (tokenSent)
        {
            // Show Settings
            DataCore.Debug.Log("OpenSettings");
            if (NativeGallery.CanOpenSettings()) {
                NativeGallery.OpenSettings();
            }            
        }
        else
        {
            FirebaseManager.Instance.RequestPermission(() =>
            {
                PlayerPrefs.SetInt(ConfigManager.TokenSent, 1);
                PlayerPrefs.Save();
            });
        }
    }
}
