/*
 * * * * This bare-bones script was auto-generated * * * *
 * The code commented with "/ * * /" demonstrates how data is retrieved and passed to the adapter, plus other common commands. You can remove/replace it once you've got the idea
 * Complete it according to your specific use-case
 * Consult the Example scripts if you get stuck, as they provide solutions to most common scenarios
 * 
 * Main terms to understand:
 *		Model = class that contains the data associated with an item (title, content, icon etc.)
 *		Views Holder = class that contains references to your views (Text, Image, MonoBehavior, etc.)
 * 
 * Default expected UI hiererchy:
 *	  ...
 *		-Canvas
 *		  ...
 *			-MyScrollViewAdapter
 *				-Viewport
 *					-Content
 *				-Scrollbar (Optional)
 *				-ItemPrefab (Optional)
 * 
 * Note: If using Visual Studio and opening generated scripts for the first time, sometimes Intellisense (autocompletion)
 * won't work. This is a well-known bug and the solution is here: https://developercommunity.visualstudio.com/content/problem/130597/unity-intellisense-not-working-after-creating-new-1.html (or google "unity intellisense not working new script")
 * 
 * 
 * Please read the manual under "/Docs", as it contains everything you need to know in order to get started, including FAQ
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using frame8.Logic.Misc.Other.Extensions;
using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using Com.TheFallenGames.OSA.DataHelpers;
using EventDispatcher;
using DataCore;
using DG.Tweening;
using Spine.Unity;
using TMPro;
// You should modify the namespace to your own or - if you're sure there won't ever be conflicts - remove it altogether
namespace ArtStory.Home.Puzzle.Grids
{
    // There is 1 important callback you need to implement, apart from Start(): UpdateCellViewsHolder()
    // See explanations below
    public class BasicGridAdapter : GridAdapter<GridParams, HomePuzzleItemViewsHolder>
    {
        // Helper that stores data and notifies the adapter when items count changes
        // Can be iterated and can also have its elements accessed by the [] operator
        public SimpleDataHelper<PuzzleLevelData> Data { get; private set; }
        #region GridAdapter implementation

        private int _collectionId;
        private Sprite _sprDefaul;
        public void InitData(int collectionId, int count, List<PuzzleLevelData> puzzleList, Sprite sprDefaul)
        {
            _sprDefaul = sprDefaul;
            _collectionId = collectionId;
            if (Data == null) {
                Data = new SimpleDataHelper<PuzzleLevelData>(this);
            }                        
            Data.List.Clear();
            Data.List.AddRange(puzzleList);
            _CellsCount = count;
        }
        public void OnRefesh()
        {
            Refresh();
        }

        public bool freezeContentEndEdgeOnCountChange;
        /// <param name="contentPanelEndEdgeStationary">ignored because we override this via <see cref="freezeContentEndEdgeOnCountChange"/></param>
        /// <seealso cref="GridAdapter{TParams, TCellVH}.Refresh(bool, bool)"/>
        public override void Refresh(bool contentPanelEndEdgeStationary = false, bool keepVelocity = false)
        {
            if (Data == null) return;
            _CellsCount = Data.List.Count;
            base.Refresh(freezeContentEndEdgeOnCountChange, keepVelocity);
        }

        protected override void Start()
        {
            // Calling this initializes internal data and prepares the adapter to handle item count changes
            base.Start();
        }

        // This is called anytime a previously invisible item become visible, or after it's created, 
        // or when anything that requires a refresh happens
        // Here you bind the data from the model to the item's views
        // *For the method's full description check the base implementation
        protected override void UpdateCellViewsHolder(HomePuzzleItemViewsHolder newOrRecycled)
        {
            // In this callback, "newOrRecycled.ItemIndex" is guaranteed to always reflect the
            // index of item that should be represented by this views holder. You'll use this index
            // to retrieve the model from your data set


            //newOrRecycled.backgroundImage.color = model.color;
            //newOrRecycled.titleText.text = model.title + " #" + newOrRecycled.ItemIndex;
            PuzzleLevelData model = Data[newOrRecycled.ItemIndex];
            newOrRecycled.SetData(_collectionId, _sprDefaul, newOrRecycled.ItemIndex, model, PuzzlePlayAgainCallback, () => { });
        }

        private void PuzzlePlayAgainCallback(int collectionId, int collectionIndex)
        {
            var puzzle = MasterDataStore.Instance.GetCollectionPuzzleByIndex(collectionId, collectionIndex);
            UIManager.Instance.PopupPlayAgainCollection.SetData(new object[] { collectionId, collectionIndex, puzzle });
            UIManager.Instance.PopupPlayAgainCollection.Open();
        }

        // This is the best place to clear an item's views in order to prepare it from being recycled, but this is not always needed, 
        // especially if the views' values are being overwritten anyway. Instead, this can be used to, for example, cancel an image 
        // download request, if it's still in progress when the item goes out of the viewport.
        // <newItemIndex> will be non-negative if this item will be recycled as opposed to just being disabled
        // *For the method's full description check the base implementation
        /*
		protected override void OnBeforeRecycleOrDisableCellViewsHolder(MyGridItemViewsHolder inRecycleBinOrVisible, int newItemIndex)
		{
			base.OnBeforeRecycleOrDisableCellViewsHolder(inRecycleBinOrVisible, newItemIndex);
		}
		*/
        #endregion

        // These are common data manipulation methods
        // The list containing the models is managed by you. The adapter only manages the items' sizes and the count
        // The adapter needs to be notified of any change that occurs in the data list. 
        // For GridAdapters, only Refresh and ResetItems work for now
        #region data manipulation
        public void AddItemsAt(int index, IList<PuzzleLevelData> items)
        {
            //Commented: this only works with Lists. ATM, Insert for Grids only works by manually changing the list and calling NotifyListChangedExternally() after
            //Data.InsertItems(index, items);
            Data.List.InsertRange(index, items);
            Data.NotifyListChangedExternally();
        }

        public void RemoveItemsFrom(int index, int count)
        {
            //Commented: this only works with Lists. ATM, Remove for Grids only works by manually changing the list and calling NotifyListChangedExternally() after
            //Data.RemoveRange(index, count);
            Data.List.RemoveRange(index, count);
            Data.NotifyListChangedExternally();
        }

        public void SetItems(IList<PuzzleLevelData> items)
        {
            Data.ResetItems(items);
        }
        #endregion


        // Here, we're requesting <count> items from the data source
        void RetrieveDataAndUpdate(int count)
        {
            StartCoroutine(FetchMoreItemsFromDataSourceAndUpdate(count));
        }

        // Retrieving <count> models from the data source and calling OnDataRetrieved after.
        // In a real case scenario, you'd query your server, your database or whatever is your data source and call OnDataRetrieved after
        IEnumerator FetchMoreItemsFromDataSourceAndUpdate(int count)
        {
            // Simulating data retrieving delay
            yield return new WaitForSeconds(.5f);

            var newItems = new PuzzleLevelData[count];

            OnDataRetrieved(newItems);
        }

        void OnDataRetrieved(IList<PuzzleLevelData> newItems)
        {
            //Commented: this only works with Lists. ATM, Insert for Grids only works by manually changing the list and calling NotifyListChangedExternally() after
            // Data.InsertItemsAtEnd(newItems);
            Data.List.Clear();
            Data.List.AddRange(newItems);
            Data.NotifyListChangedExternally();
        }
    }

    // This class keeps references to an item's views.
    // Your views holder should extend BaseItemViewsHolder for ListViews and CellViewsHolder for GridViews
    // The cell views holder should have a single child (usually named "Views"), which contains the actual 
    // UI elements. A cell's root is never disabled - when a cell is removed, only its "views" GameObject will be disabled
    public class HomePuzzleItemViewsHolder : CellViewsHolder
    {
        // Retrieving the views from the item's root GameObject
        public override void CollectViews()
        {
            base.CollectViews();
            // GetComponentAtPath is a handy extension method from frame8.Logic.Misc.Other.Extensions
            // which infers the variable's component from its type, so you won't need to specify it yourself

            views.GetComponentAtPath("Thumbnail", out thumbnail);
            views.GetComponentAtPath("IconStatus", out iconStatus);
            views.GetComponentAtPath("Download", out parentDownloading);
            views.GetComponentAtPath("LockAnim", out unLockCoverAnimation);
            views.GetComponentAtPath("ThumbnailUnlock", out lockObject);
            views.GetComponentAtPath("TmpIndex", out tmpIndex);
            views.GetComponentAtPath("Border", out border);
            btnPlay = views.GetComponent<Button>();
        }
        public Image thumbnail;
        private Image iconStatus;
        private Sprite sprIconPlay;
        private Sprite sprIconLock;
        private Sprite _sprDefaul;
        private RectTransform parentDownloading;
        private RectTransform border;

        private SkeletonGraphic unLockCoverAnimation;
        private Image lockObject;

        private int _collectionId;
        private int _collectionIndex;
        private PuzzleStatus status;
        private PuzzleLevelData puzzleData;

        private Action<int, int> onPuzzlePlayAgain;
        private Action onPuzzleLock;

        [SerializeField] Button btnPlay;
        [SerializeField] TextMeshProUGUI tmpIndex;

        private int collectionIndex;
        public void SetData(int collectionId, Sprite sprDefaul, int index, PuzzleLevelData puzzleMasterData, Action<int, int> puzzlePlayAgain, Action puzzleLock)
        {
            if (puzzleData != null)
            {
                try
                {
                    OnRelease();
                }
                catch (Exception ex)
                {
                    DataCore.Debug.Log($"Failed to release thumbnail. Error: {ex.Message}");
                }

            }

            if (puzzleMasterData.Name == "FakeData")
            {
                thumbnail.gameObject.SetActive(false);
                iconStatus.gameObject.SetActive(false);
                parentDownloading.gameObject.SetActive(false);
                unLockCoverAnimation.gameObject.SetActive(false);
                lockObject.gameObject.SetActive(false);
                tmpIndex.gameObject.SetActive(false);
                border.gameObject.SetActive(false);
            }
            else
            {
                thumbnail.sprite = sprDefaul;
                _collectionId = collectionId;
                _sprDefaul = sprDefaul;
                puzzleData = puzzleMasterData;
                onPuzzlePlayAgain = puzzlePlayAgain;
                onPuzzleLock = puzzleLock;
                collectionIndex = index;

                if (unLockCoverAnimation != null)
                {
                    unLockCoverAnimation.gameObject.SetActive(true);
                }
                if (lockObject != null)
                {
                    lockObject.gameObject.SetActive(true);
                }
                if (parentDownloading != null)
                    parentDownloading.gameObject.SetActive(false);

                UpdateStatus();
                UpdateUI();

                btnPlay.onClick.AddListener(() =>
                {
                    SoundController.Instance.PlaySfxClick();
                    OnPuzzlePlayClick();
                });
                _collectionIndex = index;
                if (status == PuzzleStatus.COMPLETE)
                {
                    tmpIndex.gameObject.SetActive(false);
                }
                else
                {
                    tmpIndex.gameObject.SetActive(true);
                    index++;
                    string indexMsg = index.ToString();
                    if (index < 10)
                        indexMsg = "0" + index.ToString();

                    tmpIndex.SetText(indexMsg);
                }
            }

        }

        public void OnRelease()
        {
            try
            {
                if (puzzleData == null) return;

                if (_loadThumbnail != null)
                {
                    _loadThumbnail.Kill();
                    _loadThumbnail = null;
                }
                thumbnail.sprite = _sprDefaul;

                AssetManager.Instance.ReleasePath(puzzleData.Thumbnail.Thumbnail);
            }
            catch (Exception ex)
            {
                DataCore.Debug.Log($"Fail HomePuzzleItemViewsHolder OnRelease. Error: {ex.Message}", false);
            }

        }

        Tween _loadThumbnail;

        private void UpdateUI()
        {
            if (parentDownloading != null)
                parentDownloading.gameObject.SetActive(true);
            if (puzzleData == null)
                return;
            if (thumbnail == null || puzzleData.Thumbnail == null)
                return;
            if (string.IsNullOrEmpty(puzzleData.Thumbnail.Thumbnail) || string.IsNullOrEmpty(puzzleData.ThumbnailLabel()))
                return;

            _loadThumbnail = DOVirtual.DelayedCall(1f, () =>
            {
                AssetManager.Instance.DownloadResource(puzzleData.ThumbnailLabel(), completed: (size) =>
                {
                    AssetManager.Instance.LoadPathAsync<Sprite>(puzzleData.Thumbnail.Thumbnail, (thumb) =>
                    {
                        if (thumb != null && thumbnail != null)
                        {
                            if (parentDownloading != null)
                                parentDownloading.gameObject.SetActive(false);
                            thumbnail.sprite = thumb;
                        }
                    });
                });
            });
        }




        public void ActiveAnimationHint(string nameAnim)
        {
            if (unLockCoverAnimation == null)
                return;
            if (lockObject != null)
            {
                lockObject.gameObject.SetActive(true);
            }
            unLockCoverAnimation.gameObject.SetActive(true);
            if (unLockCoverAnimation.skeletonDataAsset != null)
            {
                try
                {
                    unLockCoverAnimation.AnimationState.SetAnimation(1, nameAnim, false);
                }
                catch
                {
                    DataCore.Debug.Log("Animation not found: animation");
                }
                
                DOVirtual.DelayedCall(0.8f, () =>
                {
                    unLockCoverAnimation.gameObject.SetActive(false);
                    lockObject.gameObject.SetActive(false);
                    //StartCoroutine(FadeOutText(1.0f, lockObject));
                    GameData.Instance.SavedPack.DataSetCollectionPuzzlePlaying(_collectionId, collectionIndex, PuzzleStatus.UNLOCK);
                    status = PuzzleStatus.UNLOCK;
                    GameData.Instance.RequestSaveGame();

                    StartDownload();
                    DOVirtual.DelayedCall(0.8f, () =>
                    {
                        StopDownload();
                    });

                });
            }
        }

        private IEnumerator FadeOutText(float timeSpeed, Image image)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            while (image.color.a > 0.0f)
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - (Time.deltaTime * timeSpeed));
                yield return null;
            }
        }
        private void UpdateStatus()
        {
            if (puzzleData == null)
                return;

            var collectionPuzzleData = GameData.Instance.SavedPack.DataGetCollectionPuzzlePlaying(_collectionId, collectionIndex);
            if (collectionPuzzleData != null)
            {
                status = collectionPuzzleData.PuzzleStatus;
            }
            else
            {
                status = PuzzleStatus.LOCK;
            }

            if (GameData.Instance.IsVipIap())
            {
                if (status != PuzzleStatus.COMPLETE)
                {
                    status = PuzzleStatus.UNLOCK;
                }
            }

            if (status == PuzzleStatus.COMPLETE)
            {
                thumbnail.gameObject.SetActive(true);
                border.gameObject.SetActive(true);
            }
            else if (status == PuzzleStatus.UNLOCK)
            {
                thumbnail.gameObject.SetActive(true);
                tmpIndex.gameObject.SetActive(true);
                border.gameObject.SetActive(true);
            }
            else
            {
                thumbnail.gameObject.SetActive(true);
                unLockCoverAnimation.gameObject.SetActive(true);
                lockObject.gameObject.SetActive(true);
                tmpIndex.gameObject.SetActive(true);
                border.gameObject.SetActive(true);
            }

            tmpIndex.gameObject.SetActive(status != PuzzleStatus.COMPLETE);

            if (status != PuzzleStatus.COMPLETE)
                EnableGrayScaleThumbnail(true);
            else
                EnableGrayScaleThumbnail(false);

            UpdateIconLock();
        }


        public void StartDownload()
        {
            if (unLockCoverAnimation != null)
                unLockCoverAnimation.gameObject.SetActive(false);
            if (lockObject != null)
                lockObject.gameObject.SetActive(false);
            parentDownloading.gameObject.SetActive(true);
        }

        public void StopDownload()
        {
            if (unLockCoverAnimation != null)
                unLockCoverAnimation.gameObject.SetActive(false);
            if (lockObject != null)
                lockObject.gameObject.SetActive(false);
            iconStatus.gameObject.SetActive(false);
            parentDownloading.gameObject.SetActive(false);
        }

        public void UpdateStatusVip()
        {
            if (lockObject != null) lockObject.gameObject.SetActive(false);
            if (unLockCoverAnimation != null) unLockCoverAnimation.gameObject.SetActive(false);
            iconStatus.gameObject.SetActive(false);
            iconStatus.sprite = sprIconPlay;
        }

        private void UpdateIconLock()
        {
            switch (status)
            {
                case PuzzleStatus.NONE:
                    iconStatus.gameObject.SetActive(false);
                    iconStatus.sprite = sprIconLock;
                    if (lockObject != null) lockObject.gameObject.SetActive(true);
                    if (unLockCoverAnimation != null) unLockCoverAnimation.gameObject.SetActive(true);
                    break;
                case PuzzleStatus.LOCK:
                    iconStatus.gameObject.SetActive(false);
                    iconStatus.sprite = sprIconLock;
                    if (lockObject != null) lockObject.gameObject.SetActive(true);
                    if (unLockCoverAnimation != null) unLockCoverAnimation.gameObject.SetActive(true);
                    break;
                case PuzzleStatus.UNLOCK:
                    if (lockObject != null) lockObject.gameObject.SetActive(false);
                    if (unLockCoverAnimation != null) unLockCoverAnimation.gameObject.SetActive(false);
                    iconStatus.gameObject.SetActive(false);
                    iconStatus.sprite = sprIconPlay;
                    break;
                case PuzzleStatus.COMPLETE:
                    if (lockObject != null) lockObject.gameObject.SetActive(false);
                    if (unLockCoverAnimation != null) unLockCoverAnimation.gameObject.SetActive(false);
                    if (tmpIndex != null) tmpIndex.gameObject.SetActive(false);
                    iconStatus.gameObject.SetActive(false);
                    EnableGrayScaleThumbnail(false);
                    break;
            }
        }

        private void EnableGrayScaleThumbnail(bool enable)
        {
            Material mat = new Material(thumbnail.material);
            mat.SetFloat("_EffectAmount", enable ? 1 : 0);
            thumbnail.material = mat;
        }

        bool _disablePuzzlePlayClick = false;
        public void OnPuzzlePlayClick()
        {
            if (_disablePuzzlePlayClick) return;
            _disablePuzzlePlayClick = true;
            if (status == PuzzleStatus.UNLOCK)
            {
                GameManager.Instance.StartLevelCollection(_collectionId, _collectionIndex, puzzleData.ID, ConfigManager.GameData.PlayType.collection_play_puzzle);
            }
            else if (status == PuzzleStatus.COMPLETE)
            {
                onPuzzlePlayAgain?.Invoke(_collectionId, _collectionIndex);
            }
            else
            {
                PuzzleLockCallback();
            }
            DOVirtual.DelayedCall(2, () =>
            {
                _disablePuzzlePlayClick = false;
            });
        }

        public void PuzzleLockCallback()
        {
            UIManager.Instance.ShowPopupCompleteThePrevious();
        }

        public void OnPuzzlePlayAgainClick()
        {
            GameManager.Instance.StartLevelCollection(_collectionId, _collectionIndex, puzzleData.ID, ConfigManager.GameData.PlayType.collection_play_puzzle);
        }

        private void StepGame()
        {
            var playGame = GameManager.Instance.GetStepGame();
            if (playGame == StepGameConstants.Tutorial)
                GameManager.Instance.SetStepGame(StepGameConstants.PlayPuzzleOne);
            else if (playGame == StepGameConstants.HomePage)
                GameManager.Instance.SetStepGame(StepGameConstants.PlayPuzzleTwo);
            else if (playGame == StepGameConstants.Rating)
                GameManager.Instance.SetStepGame(StepGameConstants.PlayPuzzleThree);
            else if (playGame == StepGameConstants.PlayPuzzleThree)
                GameManager.Instance.SetStepGame(StepGameConstants.LuckyDrawOne);
        }
    }
}
