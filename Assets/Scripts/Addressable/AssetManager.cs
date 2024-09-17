using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;
using DataCore;
using System.Collections;
using UnityEngine.AddressableAssets.ResourceLocators;
using com.F4A.MobileThird;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace DataCore
{
    public class AddressableLabels
    {
        public const string Default = "default";
        public const string AutoLoad = "autoload";
        public const string Instantiate = "instantiate";
    }
    public class AssetManager : SingletonMono<AssetManager>
    {
        [SerializeField] List<string> LocalLabels;
        private bool m_IsInit = false;

        public bool IsInit { get => m_IsInit; set => m_IsInit = value; }

        public void Init(Action<float> progress, Action completed)
        {

            StartCoroutine(_Init(progress, completed));
        }

        private IEnumerator _Init(Action<float> progress, Action completed)
        {
            Application.backgroundLoadingPriority = ThreadPriority.High;
            var task = Addressables.InitializeAsync();
            while (!task.IsDone)
            {
                float _progress = task.GetDownloadStatus().Percent;
                progress?.Invoke(_progress);
                yield return new WaitForEndOfFrame();
            }

            m_IsInit = true;
            _downloadedSize = 0;
            Addressables.ResourceManager.InternalIdTransformFunc = InternalIdTransform;
            completed?.Invoke();
            Application.backgroundLoadingPriority = ThreadPriority.Normal;
            StartCoroutine(CheckCatalogs());

        }

        IEnumerator CheckCatalogs()
        {
            var internetConnection = SocialManager.Instance.isConnectionNetwork();
            DataCore.Debug.Log($"CheckCatalogs in internetConnection: {internetConnection}");
            if (!internetConnection)
            {
                yield break;
            }
            List<string> catalogsToUpdate = new List<string>();
            AsyncOperationHandle<List<string>> checkForUpdateHandle
                = Addressables.CheckForCatalogUpdates(false);
            checkForUpdateHandle.Completed += op =>
            {
                catalogsToUpdate.AddRange(op.Result);
            };

            yield return checkForUpdateHandle;

            if (catalogsToUpdate.Count > 0)
            {
                AsyncOperationHandle<List<IResourceLocator>> updateHandle
                    = Addressables.UpdateCatalogs(catalogsToUpdate, false);
                yield return updateHandle;
                Addressables.Release(updateHandle);
            }

            Addressables.Release(checkForUpdateHandle);
        }

        private long _downloadedSize = 0;
        string InternalIdTransform(IResourceLocation location)
        {
            if (location.ResourceType == typeof(IAssetBundleResource) && location.InternalId.StartsWith("http") && EnableDebugMode())
            {
                var newUrl = location.InternalId.Replace("https://yomistudiostorage.sgp1.cdn.digitaloceanspaces.com/art_story_puzzle/v3/", "https://yomistudiostorage.sgp1.cdn.digitaloceanspaces.com/art_story_puzzle/development/");
                //UnityEngine.Debug.Log($"newUrl {newUrl}");
                return newUrl;
            }

            return location.InternalId;
        }

        private static bool EnableDebugMode()
        {
            return DataCore.Debug.EnableDebugMode();
        }


        #region Load Assets
        private Dictionary<string, AsyncOperationHandle> KeyOperationPairs = new Dictionary<string, AsyncOperationHandle>();
        public void ReleasePath(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                if (KeyOperationPairs.ContainsKey(path))
                {
                    DataCore.Debug.Log($"Released path: {path}", false);
                    var operation = KeyOperationPairs[path];
                    KeyOperationPairs.Remove(path);
                    Addressables.Release(operation);
                }
            }
            catch (Exception ex)
            {
                DataCore.Debug.Log($"Failed ReleasePath {path}. Error: {ex.Message}");
            }

        }
        public void LoadPathAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object
        {

            if (string.IsNullOrEmpty(path))
            {
                onComplete(null);
                return;
            }
            //DataCore.Debug.Log($"Load path: {path}");
            try
            {
                //DataCore.Debug.Log($"Load asset path: {path}");                
                AsyncOperationHandle<T> download = Addressables.LoadAssetAsync<T>(path);
                download.Completed += (AsyncOperationHandle<T> operation) =>
                {
                    if (operation.Status == AsyncOperationStatus.Succeeded)
                    {
                        if (KeyOperationPairs.ContainsKey(path))
                        {
                            var preOperator = KeyOperationPairs[path];
                            KeyOperationPairs.Remove(path);
                        }
                        KeyOperationPairs.Add(path, download);
                        onComplete(operation.Result as T);
                    }
                    else
                    {
                        onComplete(null);
                        DataCore.Debug.LogError("Failed loading asset : " + operation.Status + " $: " + path);
                    }
                };

            }
            catch (Exception ex)
            {
                DataCore.Debug.LogError("Can not find asset : " + path + ex.ToString());
            }
        }

        public void LoadPuzzleAsync<T>(AssetReference reference, Action<float> onProgressLoading, Action<T> onComplete) where T : UnityEngine.Object
        {
            try
            {
                var op = reference.OperationHandle;

                if (op.IsValid())
                {
                    var handle = op.Convert<T>();

                    if (!op.IsDone)
                    {
                        op.Completed += (loadRequest) =>
                        {
                            if (loadRequest.IsValid() && loadRequest.Result != null)
                            {
                                onComplete(op.Result as T);
                            }
                        };

                    }
                    else
                    {
                        onComplete(op.Result as T);
                    }
                }
                else
                {
                    AsyncOperationHandle<T> download = reference.LoadAssetAsync<T>();
                    var progress = download.PercentComplete;
                    onProgressLoading?.Invoke(progress);
                    download.Completed += (AsyncOperationHandle<T> operation) =>
                    {
                        if (operation.Status == AsyncOperationStatus.Succeeded)
                            onComplete(operation.Result as T);
                        else DataCore.Debug.LogError("Failed loading asset : " + operation.Status);
                    };
                }
            }
            catch (Exception ex)
            {
                DataCore.Debug.LogError("Can not find asset : " + ex.ToString());
            }

        }

        private Action<bool> onCompletePart;
        private List<string> partReferences = new List<string>();
        public static event System.Action<string, UnityEngine.Object> OnLoadPuzzlePart = delegate { };
        //public void LoadChapterAsync<T>(List<string> reference, Action<bool> onComplete = null) where T : UnityEngine.Object
        //{
        //    Addressables.InitializeAsync().Completed += OnAddressablesInitialized;
        //    partReferences = reference;
        //    onCompletePart = onComplete;
        //    StartCoroutine(RoutineLoadItems());
        //}

        //private void OnAddressablesInitialized(AsyncOperationHandle<IResourceLocator> locator)
        //{
        //    StartCoroutine(RoutineLoadItems());
        //}
        //private IEnumerator RoutineLoadItems()
        //{
        //    List<object> keys = new List<object>(partReferences.Count);
        //    partReferences.ForEach(r => keys.Add(r));

        //    // You can add a typeof() at the end to filter on type as well. Take not of the merge mode
        //    AsyncOperationHandle<IList<IResourceLocation>> locationsHandle = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union);
        //    yield return locationsHandle;

        //    // Using UnityEngine.Object here because I don't know what you'll be loading
        //    AsyncOperationHandle<IList<UnityEngine.Object>> objectsHandle = Addressables.LoadAssetsAsync<UnityEngine.Object>(locationsHandle.Result, null);
        //    yield return objectsHandle;
        //    if (objectsHandle.Result.Count == partReferences.Count)
        //    {
        //        var index = 0;
        //        foreach (var itemPuzzle in objectsHandle.Result)
        //        {
        //            // save object into map
        //            OnLoadPuzzlePart?.Invoke(partReferences[index], itemPuzzle);
        //            index++;
        //        }
        //        onCompletePart?.Invoke(true);
        //    }
        //    else
        //    {
        //        onCompletePart?.Invoke(false);
        //    }
        //}
        #endregion

        #region Download Assets

        public void DownloadResource(string bundlename, System.Action<float> progress = null, Action<long> completed = null, bool forced = false, bool shoudTrack = true)
        {
            var bundleNames = new List<string>() { bundlename };
            StartCoroutine(_DownloadResources(bundleNames, progress, completed, forced, shoudTrack));
        }

        public void DownloadResources(List<string> bundlenames, System.Action<float> progress = null, Action<long> completed = null, bool forced = false, bool shoudTrack = true)
        {
            StartCoroutine(_DownloadResources(bundlenames, progress, completed, forced, shoudTrack));
        }

        private IEnumerator _DownloadResources(List<string> bundlenames, System.Action<float> progress = null, Action<long> completed = null, bool forced = false, bool shoudTrack = true)
        {
            if (!m_IsInit)
            {
                DataCore.Debug.LogError("Not init yet");
                yield break;
            }
            int _totalProgress = bundlenames.Count;

            float _progressSum = 0;
            long totalDownloadSize = 0;
            foreach (var bundle in bundlenames)
            {
                DataCore.Debug.Log("Start Load: " + bundle, false);
                var downloadSizeOp = Addressables.GetDownloadSizeAsync(bundle);
                while (!downloadSizeOp.IsDone)
                {
                    yield return new WaitForEndOfFrame();
                }
                var downloadSize = downloadSizeOp.Result;
                bool IsDownloadedBundle = CPlayerPrefs.GetInt(bundle, 0) != 0 && downloadSize == 0 && !forced;
                Debug.Log($"Check download size {bundle}: {downloadSize} - {IsDownloadedBundle}", false);

                if (!IsDownloadedBundle)
                {
                    var download = Addressables.DownloadDependenciesAsync(bundle);
                    while (!download.IsDone)
                    {
                        float _progress = download.GetDownloadStatus().Percent;
                        var progressValue = (_progressSum + _progress) / _totalProgress;
                        progress?.Invoke(progressValue);
                        yield return new WaitForEndOfFrame();
                    }
                    _progressSum += 1;
                    yield return new WaitForEndOfFrame();
                    CPlayerPrefs.SetInt(bundle, 1);
                    DataCore.Debug.Log("bundleLoaded: " + bundle + " $ " + download.Status, false);
                    if (shoudTrack)
                    {
                        _downloadedSize += downloadSize;
                    }
                    totalDownloadSize += downloadSize;
                    Addressables.Release(download);
                }
                else
                {
                    _progressSum += 1;
                }
                Addressables.Release(downloadSizeOp);
            }

            yield return new WaitForEndOfFrame();

            _progressSum = 1;
            progress?.Invoke(_progressSum);
            yield return new WaitForEndOfFrame();
            completed?.Invoke(totalDownloadSize);
        }

        #endregion


        #region Update label Asset
        IEnumerator UpdateLabelAsset(string label)
        {
            long updateLabelSize = 0;
            var async = Addressables.GetDownloadSizeAsync(label);
            yield return async;
            if (async.Status == AsyncOperationStatus.Succeeded)
                updateLabelSize = async.Result;
            Addressables.Release(async);
            if (updateLabelSize == 0)
            {
                DataCore.Debug.Log($"{label} last version", false);
                yield break;
            }
            yield return DownloadLabelAsset(label);
        }

        IEnumerator DownloadLabelAsset(string label)
        {
            var downloadAsync = Addressables.DownloadDependenciesAsync(label, true);

            while (!downloadAsync.IsDone)
            {
                float percent = downloadAsync.PercentComplete;
                DataCore.Debug.Log($"{label}: {percent * 100} %", false);
                yield return new WaitForEndOfFrame();
            }
            Addressables.Release(downloadAsync);

        }
        #endregion

        #region Clear Asset
        IEnumerator ClearAllAssetCoro()
        {
            foreach (var locats in Addressables.ResourceLocators)
            {
                var async = Addressables.ClearDependencyCacheAsync(locats.Keys, true);
                yield return async;
                Addressables.Release(async);
            }
            Caching.ClearCache();
        }

        IEnumerator ClearAssetCoro(string label)
        {
            var async = Addressables.LoadResourceLocationsAsync(label);
            yield return async;
            var locats = async.Result;
            foreach (var locat in locats)
                Addressables.ClearDependencyCacheAsync(locat.PrimaryKey);
        }
        #endregion

#if UNITY_ANDROID
        private void OnApplicationPause(bool pause)
        {
            if (pause) {
                if (_downloadedSize > 0) {
                    AnalyticManager.Instance.TrackDownloadedBundle(_downloadedSize);
                }                
            }
        }
#else
        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                if (_downloadedSize > 0)
                {
                    AnalyticManager.Instance.TrackDownloadedBundle(_downloadedSize);
                }
            }
        }
#endif
        private void OnApplicationQuit()
        {
            if (_downloadedSize > 0)
            {
                AnalyticManager.Instance.TrackDownloadedBundle(_downloadedSize);
            }
        }
    }
}