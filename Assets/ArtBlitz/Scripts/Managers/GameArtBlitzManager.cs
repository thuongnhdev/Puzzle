using System;
using System.Collections;
using System.Collections.Generic;
using com.F4A.MobileThird;
using DataCore;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LeoScript.ArtBlitz
{
    public class GameArtBlitzManager : SingletonMono<GameArtBlitzManager>
    {
        [SerializeField] private GridGenerator gridGenerator;
        [SerializeField] private Vector2Int layout;
        [SerializeField] private Texture2D originalTexture;
        [SerializeField] private bool isBossLevel;

        [SerializeField] private List<GameGrid> createdGameGrids;

        [SerializeField] private float originalScale;
        [SerializeField] private float zoomScale;

        [SerializeField] private RectTransform scaleTrans;

        [SerializeField] private ProgressBar progressBar;
        [SerializeField] Button btnBack;
        [SerializeField] Button btnHint;
        [SerializeField] private TextMeshProUGUI txtHintNumber;
        [SerializeField] private GameObject iconAds;

        [SerializeField] int currentGridIndex = 0;

        private GameObject objCurrent;
        private float localScaleX = 0.425f;

        private float progressCurrent = 0f;

        private PuzzleLevelData puzzleLevelData;
        bool didUsedHint = false;
        public void Init(PuzzleLevelData data)
        {
            UIManager.Instance.UILiveEvent.Close();
            this.puzzleLevelData = data;
            originalScale = scaleTrans.localScale.x;
            localScaleX = originalScale;
            zoomScale = originalScale * 0.5f;
            if (DataHolder.texture != null)
            {
                layout = DataHolder.layout;
                originalTexture = DataHolder.texture;
                isBossLevel = DataHolder.isBossLevel;
            }
            btnHint.gameObject.SetActive(false);
            btnHint.onClick.AddListener(() =>
            {
                SoundController.Instance.PlaySfxClick();

                if (GameData.Instance.SavedPack.SaveData.Hint == 0 && !GameData.Instance.IsVipIap())
                {
                    OnWatchAdsTap();
                }
                else
                {
                    if (didUsedHint) return;
                    didUsedHint = true;
                    Hint();
                }
            });

            progressCurrent = 0;
            UpdateHintUI();
            StartGame();
            //OnShowPopupHint();
            //Check if device ios xsmax
#if UNITY_IOS || UNITY_ANDROID
            if (IsSpecialResolution())
            {
                var defineSpace = 70.0f;
                btnBack.GetComponent<RectTransform>().offsetMax = new Vector2(btnBack.GetComponent<RectTransform>().offsetMax.x, -defineSpace);
                btnBack.GetComponent<RectTransform>().offsetMin = new Vector2(btnBack.GetComponent<RectTransform>().offsetMin.x, -defineSpace);
                progressBar.GetComponent<RectTransform>().offsetMax = new Vector2(progressBar.GetComponent<RectTransform>().offsetMax.x, -defineSpace);
                progressBar.GetComponent<RectTransform>().offsetMin = new Vector2(progressBar.GetComponent<RectTransform>().offsetMin.x, -defineSpace);
            }
#endif
        }

        private bool _isShowPopupHint = false;
        private Tween _onShowPopupHint;
        public void OnShowPopupHint()
        {
            if (GameData.Instance.IsVipIap()) return;
            if (_onShowPopupHint != null) return;
            if (_isShowPopupHint) return;
            DataCore.Debug.Log($"OnShowPopupHint", false);
            _onShowPopupHint = DOVirtual.DelayedCall(ConfigManager.TimeShowPopupHint, () =>
            {
                if (!_isShowPopupHint)
                {
                    UIManager.Instance.ShowPopupHintArtBlitz();
                    _isShowPopupHint = true;
                }
            });
        }

        public void StopShowPopupHintTask()
        {
            if (_onShowPopupHint != null)
            {
                _onShowPopupHint.Kill();
            }
            _isShowPopupHint = false;
            _onShowPopupHint = null;
        }

        public void OnWatchAdsTap()
        {
            AdsService.Instance.ShowRewardedAd(AdsService.RwAdUnitIdHint,
                onRewardedAdClosedEvent: (complete) =>
                {
                    SoundController.Instance.MuteBgMusic(false);
                    if (!complete)
                    {
                        UIManager.Instance.ShowPopupNoAds();
                    }
                }, onRewardedAdReceivedReward: (adId, placement) =>
                {
                    GameData.Instance.IncreaseHint(ConfigManager.NumberIncreaseHint, ConfigManager.GameData.ResourceEarnSource.puzzle_hint);
                    UpdateHintUI();
                });
        }

        public void UpdateHintUI()
        {
            if (GameData.Instance.IsVipIap())
            {
                iconAds.gameObject.SetActive(false);
                txtHintNumber.gameObject.SetActive(true);
                txtHintNumber.text = ConfigManager.keyVipNumberUntimitedHint.ToString();
            }
            else
            {
                iconAds.gameObject.SetActive(GameData.Instance.SavedPack.SaveData.Hint == 0 ? true : false);
                txtHintNumber.gameObject.SetActive(GameData.Instance.SavedPack.SaveData.Hint > 0 ? true : false);
                if (GameData.Instance.SavedPack.SaveData.Hint > 0)
                    txtHintNumber.text = GameData.Instance.SavedPack.SaveData.Hint.ToString();
            }
        }

        void Hint()
        {
            try
            {
                if (GameData.Instance.SavedPack == null)
                    return;

                if (GameData.Instance.SavedPack.SaveData.Hint <= 0 && !GameData.Instance.IsVipIap())
                    return;

                DOVirtual.DelayedCall(5.0f, () =>
                {
                    didUsedHint = false;
                });
                GameData.Instance.DecreaseHint(1, ConfigManager.GameData.ResourceSpentSource.hint_puzzle);
                UpdateHintUI();
                GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.USE_HINT, 1);
               
                //EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.UsedHint);
            }
            catch (Exception e)
            {
                DataCore.Debug.Log("UIGameplayController Hint" + e);
                return;
            }

        }

        public bool IsSpecialResolution()
        {
            return ((float)Screen.height / (float)Screen.width) >= 1.8f;
        }

        public void OnHomeButtonClicked()
        {
            AdsService.Instance.SetAutoShowBanner(false);
            AdsService.Instance.HideBanner();
            StopShowPopupHintTask();
            scaleTrans.localScale = Vector3.one * localScaleX;
            if(createdGameGrids.Count > 0)
                Destroy(objCurrent);

            createdGameGrids.Clear();
            UIManager.Instance.ShowUIArtBlitzGame(false);
            UIManager.Instance.ShowUILiveEvent();
        }

        private void StartGame()
        {
            if (AdsLogic.IsBannerAds())
            {
                AdsService.Instance.SetAutoShowBanner(true);
                AdsService.Instance.ShowBanner();
            }
            if (isBossLevel)
            {
                for (int i = 0; i < 4; i++)
                {
                    Rect rect = GetRectForGridInBossLevel(i);
                    GameGrid grid = CreateOneGrid(rect);
                    SetGridLocalPosition(i, grid);
                }
            }
            else
            {
                CreateOneGrid(new Rect(0, 0, 1, 1));
            }

            GetCurrentGrid().Unlock();
        }
        private Rect GetRectForGridInBossLevel(int i)
        {
            if (i == 0)
            {
                return new Rect(0f, 0.5f, 0.5f, 0.5f);
            }
            else if (i == 1)
            {
                return new Rect(0.5f, 0.5f, 0.5f, 0.5f);
            }
            else if (i == 2)
            {
                return new Rect(0f, 0f, 0.5f, 0.5f);
            }
            else if (i == 3)
            {
                return new Rect(0.5f, 0f, 0.5f, 0.5f);
            }

            return Rect.zero;
        }

        private void SetGridLocalPosition(int i, GameGrid grid)
        {
            if (i == 0)
            {
                grid.transform.localPosition = Vector2.zero;
            }
            else if (i == 1)
            {
                grid.transform.localPosition = new Vector2(originalTexture.width, 0);
            }
            else if (i == 2)
            {
                grid.transform.localPosition = new Vector2(0, -originalTexture.height);
            }
            else if (i == 3)
            {
                grid.transform.localPosition = new Vector2(originalTexture.width, -originalTexture.height);
            }
        }

        private GameGrid CreateOneGrid(Rect sampleRect)
        {
            GameGrid grid = gridGenerator.CreateNewGrid(layout, originalTexture, sampleRect);
            objCurrent = grid.gameObject;
            grid.Shuffle();
            grid.Lock();
            grid.OnProgressUpdatedEventHandler += GameGrid_OnProgressUpdated;
            createdGameGrids.Add(grid);
            return grid;
        }

        private void GameGrid_OnProgressUpdated(float progress, List<ContainerBase> holdingContainerList)
        {
            if (progress > progressBar.barImg.fillAmount)
            {
                if(holdingContainerList != null){
                    foreach (ContainerBase container in holdingContainerList)
                    {
                        if (progress > progressCurrent)
                        {
                            container.EndMoveCells();
                            SoundController.Instance.PlaySfxClickTing();
                        }
                    }
                }
            }

            progressBar.SetProgressArtBlits(progress);
            if (progress > progressCurrent) progressCurrent = progress;
            if (progress == 1.0f)
            {
                GetCurrentGrid().Complete();

                GameGrid nextGrid = GetNextGrid();

                if (nextGrid != null)
                {
                    currentGridIndex++;
                    MoveToNextGrid(nextGrid, () =>
                    {
                        nextGrid.Unlock();
                        progressBar.SetProgressArtBlits(0.0f);
                    });
                }
                else
                {
                    MoveToWhole(() =>
                    {
                        GameData.Instance.SavedPack.SaveData.CountCompletedPuzzle++;
                        GameData.Instance.SavedPack.SaveData.CountCompletedPuzzleLuckyDraw++;
                        GameData.Instance.RequestSaveGame();
                        AdsService.Instance.SetAutoShowBanner(false);
                        AdsService.Instance.HideBanner();
                        ShowCompleteUI(() => {
                            OnHomeButtonClicked();
                        });
                    });
                }
            }
        }

        private GameGrid GetCurrentGrid()
        {
            return createdGameGrids[currentGridIndex];
        }

        private GameGrid GetNextGrid()
        {
            if (currentGridIndex == (createdGameGrids.Count) - 1)
            {
                return null;
            }
            else
            {
                return createdGameGrids[currentGridIndex + 1];
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                currentGridIndex++;
                MoveToNextGrid(createdGameGrids[currentGridIndex], null);
            }

            else if (Input.GetKeyDown(KeyCode.N))
            {
                MoveToWhole(null);
            }

        }

        private void ShowCompleteUI(Action onComplete = null)
        {
            UIManager.Instance.ShowArtBlitsCompleted(puzzleLevelData, onComplete);
        }
      
        public void MoveToNextGrid(GameGrid grid, System.Action OnFinish)
        {
            Vector2 targetPosition = -grid.GetComponent<RectTransform>().localPosition * originalScale;
            StartCoroutine(MoveCoroutine(targetPosition, originalScale, OnFinish));
        }

        public void MoveToWhole(System.Action OnFinish)
        {
            Vector3 targetPosition = Vector3.zero;

            foreach (GameGrid grid in createdGameGrids)
            {
                targetPosition += grid.GetComponent<RectTransform>().localPosition;
            }

            Vector2 targetPosition2D = -targetPosition / 4 * zoomScale;

            StartCoroutine(MoveCoroutine(targetPosition2D, zoomScale, OnFinish));
        }

        private IEnumerator MoveCoroutine(Vector2 targetPosition, float targetScale, System.Action OnFinish)
        {
            yield return new WaitForSeconds(1.0f);

            float timePassed = 0.0f;
            float currentScale = scaleTrans.localScale.x;
            Vector2 currentPosition = scaleTrans.localPosition;

            while (timePassed < 1.0f)
            {
                timePassed += Time.deltaTime;

                Vector2 positionThisFrame = Vector2.Lerp(currentPosition, targetPosition, timePassed);

                scaleTrans.localPosition = positionThisFrame;

                float scaleThisFrame = Mathf.Lerp(currentScale, targetScale, timePassed);

                scaleTrans.localScale = Vector3.one * scaleThisFrame;

                yield return new WaitForEndOfFrame();
            }

            scaleTrans.localPosition = targetPosition;
            scaleTrans.localScale = Vector3.one * targetScale;
            OnFinish?.Invoke();
        }

        
    }
}
