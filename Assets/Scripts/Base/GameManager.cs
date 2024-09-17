using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using EventDispatcher;
using Spine.Unity;
using DataCore;
using com.F4A.MobileThird;
using System.Globalization;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using UnityEngine.SceneManagement;
using LeoScript.ArtBlitz;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    [Range(0.0f, 1.0f)]
    [SerializeField] float m_SizeRatio;
    [SerializeField] private Transform spawnPoint;
    //[SerializeField] PuzzleController[] m_Puzzles;
    //[SerializeField] Animator[] m_PuzzlesAnim;
    [SerializeField] PuzzleController m_CurrentPuzzle;
    [SerializeField] PuzzleController m_OldPuzzle;
    [SerializeField] private SkeletonAnimation puzzleLightAnimation;
    [SerializeField] private SkeletonAnimation puzzleSkeletonAnimation;
    [SerializeField] private MatchCamera matchCameraComponent;
    [SerializeField] private Transform anchorTop;
    [SerializeField] private SpriteMask mask;
    [SerializeField] private CaptureImage capturePuzzle;
    [SerializeField] private SpriteRenderer puzzleCompleteImg;
    [SerializeField] private SpriteRenderer puzzleAnimationHolder;
    [SerializeField] private GameObject watermask;
    [SerializeField] private GameObject effectVictoryTutorial;

    private int _currentTutorial = 0;

    public int CurrentTutorial
    {
        get { return this._currentTutorial; }
        set { this._currentTutorial = value; }
    }

    private float _minLoadingIntro = 2.0f;

    public float MinLoadingIntro
    {
        get { return this._minLoadingIntro; }
        set { this._minLoadingIntro = value; }
    }

    private IRateService rateService;
    public int Play3TimesInARow = 0;

    public string CurrentState
    {
        get
        {
            if (_stateMachine == null)
                return "";

            return _stateMachine.CurrentState.name;
        }
    }

    public PuzzleController CurrentPuzzle { get => m_CurrentPuzzle; set => m_CurrentPuzzle = value; }
    public float SizeRatio { get => m_SizeRatio; set => m_SizeRatio = value; }
    public CaptureImage CapturePuzzle => capturePuzzle;
    public SpriteRenderer PuzzleCompleteImg => puzzleCompleteImg;

    public float ScaleRatio
    {
        get => _scaleRatio;
    }

    [HideInInspector] public StateMachine _stateMachine;

    private PuzzleDataSave _puzzleDataSave;

    public Tutorial _tutorial;
    private bool loadPuzzleDone = false;
    private float cacheSpawnPointPosY = 0;

    public string _puzzleName;
    private int _curBookId;
    private string _curPartId;
    private string _puzzleOpenPlacement;
    public int _curPuzzleId;
    private int _curBookIdOpening;

    public int _collectionId;
    public int _collectionIndex;

    private float _scaleRatio;

    // Live Event
    private bool _isEventLevel;
    private int _liveEventPostCardId;
    private int _liveEventPuzzleId;

    public int CurPostCardId
    {
        get { return _liveEventPostCardId; }
    }

    public int CurEventPuzzleId
    {
        get { return _liveEventPuzzleId; }
    }


    public int CurBookId
    {
        get { return _curBookId; }
    }

    public int CurBookIdOpening
    {
        get { return this._curBookIdOpening; }
        set { this._curBookIdOpening = value; }
    }

    public string CurPartId
    {
        get { return _curPartId; }
    }

    public string PuzzleOpenPlacement
    {
        get { return _puzzleOpenPlacement; }
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    private bool didInitial = false;
    private void Start()
    {
        Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreen);

        PlayerData.Instance.Init(() =>
        {
            Initialized();
            _stateMachine.SetCurrentState(GameConstants.MENU);
            OnMenuIn();
            if (rateService == null)
                InitServiceRateUs();

            if (GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
            {
                // check show daily subscription gift
                if (CPlayerPrefs.GetInt(ConfigManager.KeyBuySubscriptionSuccess, 0) == 1)
                {
                    var isGiftToday = CPlayerPrefs.GetInt(ConfigManager.DailySubscriptionDone, 0);
                    var todayGift = IAPManager.Instance.GetDaySubscription();
                    if (isGiftToday != todayGift)
                        UIManager.Instance.ShowUISubscriptionDaily();
                }

                if (AdsService.Instance.IsRemoteIntertitial(AdsService.IntAdPlacementSplash))
                {
                    AdsService.Instance.ShowInterstitial(AdsService.IntAdPlacementSplash, (complete) =>
                    {
                        SoundController.Instance.MuteBgMusic(false);
                        ShowDailyRewardIfNeeded();
                    });
                }
                else {
                    ShowDailyRewardIfNeeded();
                }
            }

            if (GetStepGame() > StepGameConstants.Tutorial)
                SoundController.Instance.PlayMainBackgroundMusic();
        });

        InitServiceRateUs();
    }

    private void ShowDailyRewardIfNeeded() {
        DOVirtual.DelayedCall(2.5f, () =>
        {
            if (UIManager.Instance.CanShowPopupDailyReward())
            {
                UIManager.Instance.ShowDailyReward();
            }
            else
            {
                UIManager.Instance.ShowUISubscription(true);
            }
        });                        
    }

    private void Initialized()
    {
        //try
        //{
            MinLoadingIntro = MCache.Instance.Config.MIN_LOADING_WAITING_TIME;

            InitstateMachine();
            cacheSpawnPointPosY = spawnPoint.transform.position.y;

            MasterDataStore.Instance.Init();
            _puzzleDataSave = new PuzzleDataSave(false);
            _puzzleDataSave.LoadGameData();
            capturePuzzle.Init();
            watermask.SetActive(!GameData.Instance.IsVipIap());
            UIManager.Instance.Init();
            //DailyPopupManager.Instance.Init();

            this.RegisterListener(EventID.BackHome, (o) => BackHome((ChooseFirstBookData)o));
            this.RegisterListener(EventID.BackBookDetail, (o) => BackBookDetail());
            this.RegisterListener(EventID.BackGameDetail, (o) => BackGameDetail());
            this.RegisterListener(EventID.PlayPuzzle, (o) => Play());
            this.RegisterListener(EventID.DestroyPuzzle, (o) => DestroyOldPuzzle());
            this.RegisterListener(EventID.UsedHint, (o) => UsedHint());
            this.RegisterListener(EventID.BackHomeResume, (o) => BackHomeResume());
            this.RegisterListener(EventID.BackHomeNextChapter, (o) => BackHomeNextChapter());
            this.RegisterListener(EventID.BackHomeStore, (o) => BackHomeStore());
            this.RegisterListener(EventID.BackPuzzleTab, (o) => BackPuzzleTab());
            this.RegisterListener(EventID.OnInitPuzzleCompleted, (o) =>
            {
                mask.sprite = m_CurrentPuzzle.GetSprite("BG");
                loadPuzzleDone = true;
                _scaleRatio = 0.25f * CurrentPuzzle.GetResolutionOfBG().x / 750.0f / m_SizeRatio; // Base size 750 -> size ratio 0.25f
            });

            this.RegisterListener(EventID.NextLayer, (o) =>
            {
                UIManager.Instance.UIGameplay.SetProgressBar(m_CurrentPuzzle.GetTotalObjInCurrentLayer());
            });

            if (!Tutorial.IsCompleted)
            {
                _tutorial = new Tutorial();
                DataCore.Debug.Log("init tutorial");
                //UIManager.Instance.UIGameplay.EnableUITutorial(true);
                AnalyticManager.Instance.LogFirstOpenMainGame((int)GameSession.Instance.SessionPlayedTime);

                this.RegisterListener(EventID.PauseTimeTutorial, (o) =>
                {
                    _tutorial.Pause((bool)o);
                });

            }

            ZoomInZoomOut.Instance.Init();
            didInitial = true;
        //}
        //catch (Exception ex)
        //{
        //    DataCore.Debug.LogError($"Failed GameManager Initialized. {ex.Message}");
        //}

    }

    private void InitstateMachine()
    {
        _stateMachine = new StateMachine();

        _stateMachine.CreateState(GameConstants.MENU, OnMenu);
        _stateMachine.CreateState(GameConstants.INTRO, OnIntro);
        _stateMachine.CreateState(GameConstants.PLAYING, OnPlaying);
        _stateMachine.CreateState(GameConstants.ENDING, OnEnding);

        _stateMachine.CreateTransition(GameConstants.MENU, GameConstants.INTRO, GameConstants.MENU_TO_INTRO,
            OnIntroIn);
        _stateMachine.CreateTransition(GameConstants.INTRO, GameConstants.PLAYING, GameConstants.INTRO_TO_PLAYING,
            OnPlayingIn);
        _stateMachine.CreateTransition(GameConstants.PLAYING, GameConstants.ENDING, GameConstants.PLAYING_TO_ENDING,
            OnEndingIn);
        _stateMachine.CreateTransition(GameConstants.ENDING, GameConstants.MENU, GameConstants.ENDING_TO_MENU,
            OnMenuIn);
        _stateMachine.CreateTransition(GameConstants.ENDING, GameConstants.INTRO, GameConstants.ENDING_TO_INTRO,
            OnIntroIn);
        _stateMachine.CreateTransition(GameConstants.PLAYING, GameConstants.MENU, GameConstants.PLAYING_TO_MENU,
            OnPlayingToMenuIn);
    }

    private void InitServiceRateUs()
    {
#if UNITY_ANDROID
        rateService = new GoogleRateService();
        rateService.Initialize();
#elif UNITY_IOS
        rateService = new AppleRateService();
        rateService.Initialize();
#endif
    }
    public float ComputeCameraOthorsize()
    {
        float size = anchorTop.localPosition.y * anchorTop.parent.localScale.x;
        return size;
    }

    public float GetPuzzleScale()
    {
        return spawnPoint.localScale.x;
    }

    void Update()
    {
        if (!didInitial) return;
        if (_stateMachine != null)
        {
            _stateMachine.Update();
        }

        if (UIManager.Instance != null && UIManager.Instance.UIGameplay.IsPlayingPuzzle)
        {
            _playingTime += Time.deltaTime;
        }
    }

    //void LateUpdate()
    //{
    //    GameData.Instance.OnLateUpdate();
    //}

    public void LoadingDone(bool value)
    {
        DataCore.Debug.Log("Load puzzle: " + value, false);
        loadPuzzleDone = value;
    }

    public void OnMenuIn()
    {
        DataCore.Debug.Log("On Menu In");
        this.Play3TimesInARow = 0;
        if (GameData.Instance == null)
            return;
        if (GameData.Instance.SavedPack == null)
            return;

        if (GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
        {
            DataCore.Debug.Log("On Home Page");
            UIManager.Instance.UIHomepage.OpenBackHome();
            GameData.Instance.SavedPack.SaveData.IsShowChooseFirstBook = true;
            GameData.Instance.RequestSaveGame();

        }
        else
        {
            DataCore.Debug.Log("On tutorial Page");
            StartLevel(0, "0", 0, ConfigManager.GameData.PlayType.new_puzzle); // 0 - 0 - 0 => tutorial
        }
    }

    public void OnIntroIn()
    {
        ResetSpawnPoint();
        var levelData = MasterDataStore.Instance.GetPuzzleTutorial();
        if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
        {
            UIManager.Instance.CloseUIHome();
            levelData = MasterDataStore.Instance.GetPuzzleTutorial();
            UIManager.Instance.ShowChapterIntro(levelData, false);
        }
        else
        {
            bool shouldShowIntro = false;
            if (_puzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
            {
                levelData = MasterDataStore.Instance.GetCollectionPuzzleByIndex(_collectionId, _collectionIndex);
            }
            else
            {
                levelData = MasterDataStore.Instance.GetPuzzleById(_curBookId, _curPartId, _curPuzzleId);
                shouldShowIntro = shouldShowIntro = GameData.Instance.SavedPack.SaveData.IsDescriptionActive && !(_puzzleOpenPlacement == ConfigManager.GameData.PlayType.free_puzzle || _puzzleOpenPlacement == ConfigManager.GameData.PlayType.resume_puzzle);
                UIManager.Instance.ShowChapterIntro(levelData, shouldShowIntro);
            }

            DataCore.Debug.Log($"Start Show Banner shouldShowIntro {shouldShowIntro} IsBannerAds {AdsLogic.IsBannerAds()}");
            if (!shouldShowIntro && AdsLogic.IsBannerAds())
            {
                AdsService.Instance.ShowBanner();
            }
        }

        if (levelData == null)
        {
            if (_puzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
            {
                DataCore.Debug.Log($"Level Data is null. _puzzleOpenPlacement: {_puzzleOpenPlacement} _collectionId: {_collectionId} _collectionIndex: {_collectionIndex}");
            }
            else
            {
                DataCore.Debug.Log($"Level Data is null. _puzzleOpenPlacement: {_puzzleOpenPlacement} _curBookId: {_curBookId} _curPartId: {_curPartId} _curPuzzleId {_curPuzzleId}");
            }
            OnClosePopupLoadPuzzleFail();
        }
        else
        {

            if (_puzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
            {
                UIManager.Instance.ShowChapterIntro(levelData, false);
                if (GameData.Instance.SavedPack.DataAddLastPlayedCollection(_collectionId, _collectionIndex))
                {
                    this.PostEvent(EventID.OnUpdateResumeCollectionPlaying);
                }
            }
            else
            {
                if (GameData.Instance.SavedPack.AddLastPlayedPuzzle(_curBookId, _curPartId, _curPuzzleId))
                {
                    this.PostEvent(EventID.OnUpdateResumePlaying);
                }

                if (!_isEventLevel)
                {
                    GameData.Instance.SavedPack.SaveData.ResumeBookId = this._curBookId;
                    GameData.Instance.SavedPack.SaveData.ResumePartId = this._curPartId;
                    GameData.Instance.SavedPack.SaveData.ResumePuzzleId = this._curPuzzleId;
                    GameData.Instance.SavedPack.SaveData.IsResumeComplete = true;
                    GameData.Instance.RequestSaveGame();
                }
                else
                {
                    GameData.Instance.SavedPack.SaveData.IsResumeComplete = false;
                    GameData.Instance.RequestSaveGame();
                }
            }
            AssetManager.Instance.DownloadResource(levelData.BGMusic.GetLabel(), completed: (size) =>
            {
                AssetManager.Instance.LoadPathAsync<AudioClip>(levelData.BGMusic.AddressPath, (bgMusic) =>
                {
                    if (bgMusic != null)
                    {
                        SoundController.Instance.PlayBGMusic(bgMusic, levelData.BGMusic.AddressPath);
                    }
                    else
                    {
                        AssetManager.Instance.ReleasePath(levelData.BGMusic.AddressPath);
                    }
                });
            });

            LoadPuzzleIndex(levelData, _puzzleOpenPlacement);
        }
    }

   

    public void OnPlayingToMenuIn()
    {
        DataCore.Debug.Log("OnPlayingToMenuIn");
    }

    private float _playingTime;
    public void OnPlayingIn()
    {
        DataCore.Debug.Log("OnPlayingIn");
        _usedHint = 0;
        _playingTime = 0;
    }

    public void OnEndingIn()
    {
        if (GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
        {
            UIManager.Instance.ShowPuzzleCompleted();
            GameData.Instance.IncreasePlayedGame();

            try
            {
                UIManager.Instance.UIGameplay.StopShowPopupHintTask();
                var puzzleData = MasterDataStore.Instance.GetPuzzleById(_curBookId, _curPartId, _curPuzzleId);
                if (puzzleData.Name != null && _puzzleOpenPlacement != null && _playingTime > 0)
                {
                    AnalyticManager.Instance.TrackCompletePuzzle(puzzleData.Name, _puzzleOpenPlacement, (int)_playingTime, _usedHint);
                    var step = GameManager.Instance.GetStepGame();
                }
            }
            catch
            {
            }

        }
        else
        {
            UIManager.Instance.EnablePuzzleBorder(true);
            UIManager.Instance.UIGameplay.Close();
            var index = CurrentTutorial + 2;
            AnalyticManager.Instance.TrackCompletedPuzzleInTutorial(index, (int)GameSession.Instance.SessionPlayedTime);
            if (this.CurrentTutorial == ConfigManager.TIME_COUNT_SHOW_TUTORIAL - 1)
            {
                UIManager.Instance.ShowPuzzleCompleted();
                CompleteTutorial();
            }
            else
            {
                this.CurrentTutorial++;
                OnIntroIn();
                _stateMachine.SetCurrentState(GameConstants.ENDING_TO_INTRO);
            }
        }
    }

    public void ActiveBorderPuzzleAnimation(bool isActive)
    {
        puzzleAnimationHolder.enabled = isActive;
    }
    public void ActivePuzzleAnimation(bool isActive)
    {
        DOVirtual.DelayedCall(0.2f, () =>
        {
            if (puzzleSkeletonAnimation.skeletonDataAsset != null)
            {
                puzzleLightAnimation.gameObject.SetActive(isActive);
                puzzleLightAnimation.ClearState();
                puzzleLightAnimation.Initialize(true);
                try
                {
                    puzzleLightAnimation.AnimationState.SetAnimation(0, "animation", false);
                }
                catch
                {
                    DataCore.Debug.Log("Animation not found: animation");
                }

                puzzleSkeletonAnimation.gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1f);
                puzzleSkeletonAnimation.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                puzzleSkeletonAnimation.gameObject.SetActive(isActive && _IsLoadedAnimation);
                DOVirtual.Float(1.0f, 1.02f, 5f, (value) =>
            {
                puzzleSkeletonAnimation.gameObject.transform.localScale = new Vector3(value, value, value);
            });

                DOVirtual.DelayedCall(3f, () =>
                {
                    puzzleLightAnimation.gameObject.SetActive(false);
                });
            }
        });
    }


    public void CompleteTutorial()
    {
        if (!Tutorial.IsCompleted)
        {
            _tutorial.ForceComplete();
            AnalyticManager.Instance.TrackCompleteTutorial((int)GameSession.Instance.SessionPlayedTime);
        }
    }
    public void StartEventLevel(int postCardId, int eventPuzzleId, LiveEventPuzzleData data, string placement , ItemPanel itemPanel)
    {
        this._liveEventPostCardId = postCardId;
        this._liveEventPuzzleId = eventPuzzleId;

        this._curBookId = data.BookId;
        this._curPartId = data.ChapterId;
        this._curPuzzleId = data.PuzzleId;
        this._puzzleOpenPlacement = placement;
        this._isEventLevel = true;

        DataHolder.texture = itemPanel.texture;
        DataHolder.isBossLevel = itemPanel.isBossLevel;
        DataHolder.layout = itemPanel.selectedLayout;

        //_stateMachine.ProcessTriggerEvent(GameConstants.MENU_TO_INTRO);
        PuzzleLevelData levelData = MasterDataStore.Instance.GetPuzzleById(data.BookId, data.ChapterId, data.PuzzleId);
        StartCoroutine(LoadScene(levelData));
    }

    public void StartLevel(int bookId, string partId, int puzzleId, string placement)
    {
        DataCore.Debug.Log($"Start Puzzle: {bookId}_{partId}_{puzzleId}_{placement}");
        this._curBookId = bookId;
        this._curPartId = partId;
        this._curPuzzleId = puzzleId;
        this._puzzleOpenPlacement = placement;
        this._isEventLevel = false;
        _stateMachine.ProcessTriggerEvent(GameConstants.MENU_TO_INTRO);
    }

    public void StartLevelCollection(int collectionId, int collectionIndex,int puzzleId, string placement)
    {
        DataCore.Debug.Log($"Start Puzzle from collection: {collectionId}_{collectionIndex}_{placement}");
        this._collectionId = collectionId;
        this._collectionIndex = collectionIndex;
        this._puzzleOpenPlacement = placement;
        this._curPuzzleId = puzzleId;
        this._isEventLevel = false;
        _stateMachine.ProcessTriggerEvent(GameConstants.MENU_TO_INTRO);
    }

    public void LoadPuzzleIndex(PuzzleLevelData puzzleData, string placement)
    {
        LoadingDone(false);
        LoadAssetPuzzle(puzzleData, placement);
    }

    private void LoadAssetPuzzle(PuzzleLevelData puzzleData, string placement)
    {
        DataCore.Debug.Log($"LoadAssetPuzzle. _puzzleOpenPlacement: {placement} puzzleName: {puzzleData.Name}");

        StartCoroutine(CleanPuzzleAssets(() =>
        {
            MCache.Instance.FxFill.Hide();

            UIManager.Instance.UpdateProgressLoading(0.0f);
            var startTime = GameSession.Instance.SessionPlayedTime;

            AssetManager.Instance.DownloadResources(puzzleData.Labels(), progress: (progressValue) =>
            {
                UIManager.Instance.UpdateProgressLoading(0.8f * progressValue);
            }, completed: (downloadedSize) =>
            {
                var duration = GameSession.Instance.SessionPlayedTime - startTime;
                if (Tutorial.IsCompleted && downloadedSize >= 0)
                {
                    AnalyticManager.Instance.TrackDownloadedPuzzle(puzzleData.Name, downloadedSize, duration);
                }

                AssetManager.Instance.LoadPathAsync<GameObject>(puzzleData.PuzzlePrefabAddress, (prefab) =>
                {
                    if (prefab != null)
                    {
                        UIManager.Instance.UpdateProgressLoading(0.95f);
                        InitPuzzle(prefab, puzzleData, placement);
                    }
                    else
                    {
                        if (!SocialManager.Instance.isConnectionNetwork())
                        {
                            OnClosePopupNoInternet();
                        }
                        else
                        {
                            OnClosePopupLoadPuzzleFail();
                        }
                    }
                });
            }, shoudTrack: false);
        }));
    }

    private void OnClosePopupNoInternet()
    {
        UIManager.Instance.UILoading.Close();
        UIManager.Instance.ShowPopupNoInternet(() =>
        {
            UIManager.Instance.UIIntro.Close();
            UIManager.Instance.UIGameplay.Close();
            if (ZoomInZoomOut.Instance.IsAllowMove)
            {
                ZoomInZoomOut.Instance.ResetToDefault();
            }
            _stateMachine.SetCurrentState(GameConstants.MENU);
            OnMenuIn();
        });
    }

    private void OnClosePopupLoadPuzzleFail()
    {
        UIManager.Instance.UILoading.Close();
        string bookName = "";
        string chapterName = "";
        string puzzleName = "";
        if (_puzzleOpenPlacement != ConfigManager.GameData.PlayType.collection_play_puzzle)
        {
            var book = MasterDataStore.Instance.GetBookByID(this._curBookId);
            if (book != null)
                bookName = book.BookName;

            var chapter = MasterDataStore.Instance.GetPartById(_curBookId, _curPartId);
            if (chapter != null)
                chapterName = chapter.PartName;

            var puzzle = MasterDataStore.Instance.GetPuzzleById(this._curBookId, this._curPartId, this._curPuzzleId);
            if (puzzle != null)
                puzzleName = puzzle.Name;

            AnalyticManager.Instance.TrackFailedToLoadPuzzleEvent(puzzleName);

            UIManager.Instance.ShowPopupLoadPuzzleFail(() =>
            {
                UIManager.Instance.UIIntro.Close();
                UIManager.Instance.UIGameplay.Close();
                if (ZoomInZoomOut.Instance.IsAllowMove)
                {
                    ZoomInZoomOut.Instance.ResetToDefault();
                }
                _stateMachine.SetCurrentState(GameConstants.MENU);
                OnMenuIn();
            }, bookName, chapterName, puzzleName);
        }
        else
        {
            UIManager.Instance.ShowPopupLoadPuzzleFail(() =>
            {
                UIManager.Instance.UIIntro.Close();
                UIManager.Instance.UIGameplay.Close();
                if (ZoomInZoomOut.Instance.IsAllowMove)
                {
                    ZoomInZoomOut.Instance.ResetToDefault();
                }
                _stateMachine.SetCurrentState(GameConstants.MENU);
                OnMenuIn();
            });
        }

    }

    public bool _IsLoadedAnimation = false;
    private PuzzleLevelData _puzzleLevelData = null;
    private void InitPuzzle(GameObject prefab, PuzzleLevelData puzzleData, string placement)
    {
        var data = prefab.GetComponent<PuzzleLevelInitData>();
        if (data != null)
        {
            _puzzleLevelData = puzzleData;
            m_CurrentPuzzle.PreInit(data, placement);

            bool isResumePlay = false;
            if (placement == ConfigManager.GameData.PlayType.collection_play_puzzle)
            {
                var lastPlayPuzzle = GameData.Instance.SavedPack.DataGetCurrentPuzzleCollectionData(_collectionId, _collectionIndex);
                isResumePlay = lastPlayPuzzle != null && lastPlayPuzzle.savedData.m_LsObjectDone.Count > 0;
            }
            else
            {
                isResumePlay = placement == ConfigManager.GameData.PlayType.resume_puzzle ||
                                                            (GameData.Instance.SavedPack.GetCurrentPuzzleData() != null && GameData.Instance.SavedPack.GetCurrentPuzzleData().savedData.m_LsObjectDone.Count > 0);
            }

            m_CurrentPuzzle.Init(isResumePlay, () =>
            {
                DataCore.Debug.Log("Completed loading puzzle");
                UIManager.Instance.UpdateProgressLoading(1f);
            });



            Camera.main.transform.GetComponent<ZoomInZoomOut>().Puzzle = m_CurrentPuzzle;

            if (placement != ConfigManager.GameData.PlayType.resume_puzzle && placement != ConfigManager.GameData.PlayType.collection_play_puzzle)
            {
                GameData.Instance.SavedPack.AddMyLibraryPlayedPuzzle(this._curBookId, this._curPartId, this._curPuzzleId);
            }
            if (!UIManager.Instance.UIIntro.IsOpen)
            {
                if (GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
                {
                    UIManager.Instance.UIGameplay.OnShowPopupHint();
                }
            }
        }
    }

    //private void SavePuzzlePartOnComplete(string assetGUID, UnityEngine.Object prefab)
    //{
    //    if (_puzzleDataSave.PuzzleAssetResource.PuzzleItemList.Count >= ConfigManager.Size_Cache_puzzle)
    //        _puzzleDataSave.PuzzleAssetResource.PuzzleItemList.Clear();

    //    bool isCache = true;

    //    for (int i = 0; i < _puzzleDataSave.PuzzleAssetResource.PuzzleItemList.Count; i++)
    //    {
    //        if (_puzzleDataSave.PuzzleAssetResource.PuzzleItemList[i].AssetGUID == assetGUID)
    //            isCache = false;
    //    }

    //    if (isCache)
    //    {
    //        _puzzleDataSave.SaveData(assetGUID, (GameObject)prefab);
    //        GameData.Instance.RequestSaveGame();
    //    }

    //}

    //private PuzzleItem loadPrefabSaved(string assetGUID)
    //{
    //    for (int i = 0; i < _puzzleDataSave.PuzzleAssetResource.PuzzleItemList.Count;i++)
    //    {
    //        if (_puzzleDataSave.PuzzleAssetResource.PuzzleItemList[i].AssetGUID.CompareTo(assetGUID) == 0)
    //            return _puzzleDataSave.PuzzleAssetResource.PuzzleItemList[i];
    //    }

    //    return null;
    //}

    public void ResetScaleSpawnPoint()
    {
        ResetSpawnPoint();
        if (Camera.main.aspect < 0.5625f)
        {
            spawnPoint.localScale = Vector3.one * (Camera.main.aspect / 0.5625f);
        }
        else
        {
            spawnPoint.localScale = Vector3.one;
        }
    }

    public void OnMenu()
    {

    }

    public void OnIntro()
    {

    }

    public void OnResetTimeShowTutorial()
    {
        if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
        {
            if (_tutorial != null) {
                _tutorial.CurrentTime = 0;
            }
            
            MCache.Instance.Hand.IsShowing = false;
        }
    }

    public void OnPlaying()
    {
        if (!Tutorial.IsCompleted && loadPuzzleDone)
        {
            if (_tutorial != null) {
                _tutorial.OnUpdate(Time.deltaTime);
            }            
        }
    }

    public void OnPausing()
    {

    }

    public void OnEnding()
    {

    }

    public void Play()
    {
        DataCore.Debug.Log(GameConstants.INTRO_TO_PLAYING);
        _stateMachine.ProcessTriggerEvent(GameConstants.INTRO_TO_PLAYING);
    }

    public void EnableMatchCamera(bool enable)
    {
        matchCameraComponent.enabled = enable;
    }

    //private IEnumerator RunAfter(float time, Action action)
    //{
    //    yield return new WaitForSeconds(time);
    //    action?.Invoke();
    //}

    //private IEnumerator RunAfterOneFrame(Action action)
    //{
    //    yield return new WaitForEndOfFrame();
    //    action?.Invoke();
    //}

    public void GetOld()
    {
        m_OldPuzzle = Instantiate(m_CurrentPuzzle, transform);
        m_OldPuzzle.gameObject.SetActive(false);
    }

    public void ClearPuzzleAsset()
    {
        StartCoroutine(CleanPuzzleAssets());
    }

    private IEnumerator CleanPuzzleAssets(Action onComplete = null)
    {
        yield return new WaitUntil(() => AssetManager.Instance.IsInit == true);
        if (m_CurrentPuzzle != null)
        {
            m_CurrentPuzzle.DestroyData();
        }
        yield return new WaitForSeconds(0.1f); // wait a frame

        if (puzzleSkeletonAnimation.skeletonDataAsset != null)
        {
            var anim = puzzleSkeletonAnimation.skeletonDataAsset;
            puzzleSkeletonAnimation.skeletonDataAsset = null;
            puzzleSkeletonAnimation.gameObject.SetActive(false);
            puzzleSkeletonAnimation.AnimationName = null;
            puzzleSkeletonAnimation.ClearState();

            AssetManager.Instance.ReleasePath(_puzzleLevelData.Animation.animation);
        }
        yield return new WaitForSeconds(0.1f); // wait a frame


        if (_puzzleLevelData != null)
        {
            AssetManager.Instance.ReleasePath(_puzzleLevelData.PuzzlePrefabAddress);
            _puzzleLevelData = null;
        }

        Resources.UnloadUnusedAssets();
        GC.Collect();
        yield return new WaitForSeconds(0.1f); // wait a frame
        onComplete?.Invoke();
    }

    public void BackPartDetail()
    {
        MCache.Instance.FxFill.Hide();
        UIManager.Instance.Fader.Show();
        UIManager.Instance.Fader.FadeHide(0.5f, () =>
        {
            if (m_CurrentPuzzle != null)
            {
                m_CurrentPuzzle.DestroyData();
            }
            UIManager.Instance.UIGameplay.Close();
            UIManager.Instance.UIPuzzleCompleted.Close();
            DOVirtual.DelayedCall(0.5f, () =>
            {
                DataCore.Debug.Log("ProcessTriggerEvent ENDING_TO_MENU");
                _stateMachine.ProcessTriggerEvent(GameConstants.ENDING_TO_MENU);
                //UIManager.Instance.ShowUIPartDetail(this._curPartId, this._curBookId, UIManager.Instance.UIHomepage);
                UIManager.Instance.Fader.FadeShow(1.0f, () =>
                {
                    UIManager.Instance.Fader.Hide();
                    EnableMatchCamera(true);
                });

            });
        });
    }

    public void BackGameDetail()
    {
        MCache.Instance.FxFill.Hide();
        UIManager.Instance.Fader.Show();
        UIManager.Instance.Fader.FadeHide(1.0f, () =>
        {
            if (m_CurrentPuzzle != null)
            {
                m_CurrentPuzzle.DestroyData();
            }
            UIManager.Instance.UIGameplay.Close();
            UIManager.Instance.UIPuzzleCompleted.Close();
            DOVirtual.DelayedCall(0.5f, () =>
            {
                _stateMachine.ProcessTriggerEvent(GameConstants.PLAYING_TO_MENU);
                UIManager.Instance.ShowUIBookDetail(this._curBookId, "back_book_detail");
                UIManager.Instance.Fader.FadeShow(1.0f, () =>
                {
                    UIManager.Instance.Fader.Hide();
                    EnableMatchCamera(true);
                });
            });
        });
    }

    public void BackBookDetail(ChooseFirstBookData firstBookData = null)
    {
        try
        {
            PuzzleLevelData nextPuzzle = null;
            float fadeHideDuration = 1;

            if (firstBookData != null)
            {
                fadeHideDuration = 0;
                nextPuzzle = MasterDataStore.Instance.GetPuzzleById(firstBookData.BookID, firstBookData.PartID, firstBookData.PuzzleID);
                this._curBookId = firstBookData.BookID;
                this._curPartId = firstBookData.PartID;
            }
            else
                nextPuzzle = GetNextPuzzle();

            MCache.Instance.FxFill.Hide();
            UIManager.Instance.Fader.Show();
            UIManager.Instance.Fader.FadeHide(fadeHideDuration, () =>
            {
                DestroyOldPuzzle();
                UIManager.Instance.UIPuzzleCompleted.Close();
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    if (nextPuzzle == null)
                    {
                        SoundController.Instance.PlayMainBackgroundMusic();
                        _stateMachine.ProcessTriggerEvent(GameConstants.ENDING_TO_MENU);
                        UIManager.Instance.ShowUIBookDetail(this._curBookId, "back_book_detail");
                    }
                    else
                    {
                        this._curPuzzleId = nextPuzzle.ID;
                        _stateMachine.ProcessTriggerEvent(GameConstants.ENDING_TO_INTRO);
                    }

                    UIManager.Instance.Fader.FadeShow(1.0f, () =>
                    {

                        UIManager.Instance.Fader.Hide();
                        EnableMatchCamera(true);
                    });
                });
            });

            this.Play3TimesInARow++;
        }
        catch (Exception ex)
        {
            DataCore.Debug.LogError($"Failed BackHome {ex.Message}");
        }

    }


    public void BackHomeResume()
    {
        MCache.Instance.FxFill.Hide();
        UIManager.Instance.Fader.Show();

        UIManager.Instance.Fader.FadeHide(1.0f, () =>
        {
            if (m_CurrentPuzzle != null)
            {
                m_CurrentPuzzle.DestroyData();
            }
            UIManager.Instance.UIGameplay.Close();
            UIManager.Instance.UIPuzzleCompleted.Close();
            DOVirtual.DelayedCall(0.5f, () =>
            {
                _stateMachine.ProcessTriggerEvent(GameConstants.PLAYING_TO_MENU);
                UIManager.Instance.ShowHomePage();
                SoundController.Instance.PlayMainBackgroundMusic();
                UIManager.Instance.Fader.FadeShow(1.0f, () =>
                {
                    UIManager.Instance.Fader.Hide();
                    EnableMatchCamera(true);
                });

                UIManager.Instance.ShowUISubscription(true);
            });
        });
    }

    public void BackHome(ChooseFirstBookData firstBookData = null)
    {
        try
        {
            PuzzleLevelData nextPuzzle = null;
            float fadeHideDuration = 1;

            if (firstBookData != null)
            {
                fadeHideDuration = 0;
                nextPuzzle = MasterDataStore.Instance.GetPuzzleById(firstBookData.BookID, firstBookData.PartID, firstBookData.PuzzleID);
                this._curBookId = firstBookData.BookID;
                this._curPartId = firstBookData.PartID;
            }
            else
                nextPuzzle = GetNextPuzzle();

            MCache.Instance.FxFill.Hide();
            UIManager.Instance.Fader.Show();
            UIManager.Instance.Fader.FadeHide(fadeHideDuration, () =>
            {
                DestroyOldPuzzle();
                UIManager.Instance.UIPuzzleCompleted.Close();
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    if (nextPuzzle == null)
                    {
                        SoundController.Instance.PlayMainBackgroundMusic();
                        _stateMachine.ProcessTriggerEvent(GameConstants.ENDING_TO_MENU);
                        UIManager.Instance.UIHomepage.OpenBackHome();
                    }
                    else
                    {
                        this._curPuzzleId = nextPuzzle.ID;
                        _stateMachine.ProcessTriggerEvent(GameConstants.ENDING_TO_INTRO);
                    }

                    UIManager.Instance.Fader.FadeShow(1.0f, () =>
                    {

                        UIManager.Instance.Fader.Hide();
                        EnableMatchCamera(true);
                    });
                });
            });

            this.Play3TimesInARow++;
        }
        catch (Exception ex)
        {
            DataCore.Debug.LogError($"Failed BackHome {ex.Message}");
        }

    }
    public void BackHomeNextChapter(bool isChapterDetail = false)
    {
        try
        {
            PuzzleLevelData nextPuzzle;
            float fadeHideDuration = 1;
            if (isChapterDetail)
            {
                nextPuzzle = MasterDataStore.Instance.GetPuzzleById(this._curBookId, this.CurPartId, this._curPuzzleId);
            }
            MCache.Instance.FxFill.Hide();
            UIManager.Instance.Fader.Show();
            UIManager.Instance.Fader.FadeHide(fadeHideDuration, () =>
            {
                DestroyOldPuzzle();
                UIManager.Instance.UIPuzzleCompleted.Close();
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    _stateMachine.ProcessTriggerEvent(GameConstants.ENDING_TO_INTRO);
                    UIManager.Instance.Fader.FadeShow(1.0f, () =>
                    {

                        UIManager.Instance.Fader.Hide();
                        EnableMatchCamera(true);
                    });
                });
            });

            this.Play3TimesInARow++;
        }
        catch (Exception ex)
        {
            DataCore.Debug.LogError($"Failed BackHome {ex.Message}");
        }

    }

    public void ShowChapterCompleteMode(Action<bool, int> onComplete = null)
    {
        var book = MasterDataStore.Instance.GetBookByID(this._curBookId);
        var chapterCurrent = MasterDataStore.Instance.GetPartById(_curBookId, _curPartId);
        if (this._curPuzzleId != chapterCurrent.PuzzleLevels[chapterCurrent.PuzzleLevels.Count - 1].ID)
        {
            onComplete?.Invoke(false, 0);
            return;
        }

        string partId = "0";
        var indexChapter = 0;
        int indexNextChapter = 0;
        PuzzleLevelData nextPuzzleInBook;
        for (var i = 0; i < book.ListChapters.Count; i++)
        {
            if (book.ListChapters[i].ID == this._curPartId)
            {
                if (i == book.ListChapters.Count - 1 && this._curPuzzleId == chapterCurrent.PuzzleLevels[chapterCurrent.PuzzleLevels.Count - 1].ID)
                {
                    indexChapter = i;
                    break;
                }
                indexChapter = i;
                indexNextChapter = i + 1;
                partId = book.ListChapters[indexNextChapter].ID;
                break;
            }
        }

        if (indexChapter == book.ListChapters.Count - 1)
        {
            if (book.Status == BookStatus.Completed)
            {
                UIManager.Instance.ShowUiPopupChapterDetailComplete(PopupChapterDetailComplete.TypeChapterComplete.ChapterCompleteFinish, (isAction) =>
                {
                    if (isAction)
                        onComplete?.Invoke(false, 1);
                });
            }
            else
            {
                UIManager.Instance.ShowUiPopupChapterDetailComplete(PopupChapterDetailComplete.TypeChapterComplete.ChapterCompleteWait, (isAction) =>
                {
                    if (isAction)
                        onComplete?.Invoke(false, 2);
                });
            }
        }
        else
        {
            var ink = GameData.Instance.SavedPack.SaveData.Coin;
            var chapter = MasterDataStore.Instance.GetPartById(_curBookId, partId);
            var partStt = GameData.Instance.SavedPack.GetPartStatus(this._curBookId, partId);
            if (partStt == ChapterStatus.LOCK)
            {

                this._curPartId = book.ListChapters[indexNextChapter].ID;
                this._curPuzzleId = book.ListChapters[indexNextChapter].PuzzleLevels[0].ID;
                nextPuzzleInBook = MasterDataStore.Instance.GetPuzzleById(this._curBookId, this._curPartId, this._curPuzzleId);
                UIManager.Instance.ShowUiPopupChapterDetailComplete(PopupChapterDetailComplete.TypeChapterComplete.ChapterInk, (isAction) =>
                {
                    if (isAction)
                    {
                        onComplete?.Invoke(true, 3);
                        GameManager.Instance.StartLevel(this._curBookId, this._curPartId, this._curPuzzleId, ConfigManager.GameData.PlayType.new_puzzle);
                        GameData.Instance.SavedPack.SaveUserChapterData(this._curBookId, this._curPartId, ChapterStatus.UNLOCK);
                        var partData = GameData.Instance.SavedPack.GetPartUserData(this._curBookId, this._curPartId);
                        if (partData != null)
                        {
                            for (int i = 0; i < partData.PuzzleSaveDatas.Count; i++)
                            {
                                if (partData.PuzzleSaveDatas[i].Id == this._curPuzzleId && partData.PuzzleSaveDatas[i].Stt != PuzzleStatus.UNLOCK)
                                    GameData.Instance.SavedPack.SaveUserPuzzleData(this._curBookId, this._curPartId, this._curPuzzleId, PuzzleStatus.UNLOCK);
                            }
                        }
                        GameData.Instance.SavedPack.SaveMyLibraryBookData(this._curBookId);
                        GameData.Instance.RequestSaveGame();

                    }
                    else
                    {
                        BackHomeStore();
                    }

                }, chapter.Price.ToString());
            }
            else
            {
                onComplete?.Invoke(false, 4);
            }

        }
    }

    private void BackPuzzleTab()
    {
        MCache.Instance.FxFill.Hide();
        UIManager.Instance.Fader.Show();
        DestroyOldPuzzle();
        UIManager.Instance.UIPuzzleCompleted.Close();
        UIManager.Instance.Fader.FadeHide(1.0f, () =>
        {
            UIManager.Instance.Fader.Hide();
            SoundController.Instance.PlayMainBackgroundMusic();
            _stateMachine.ProcessTriggerEvent(GameConstants.ENDING_TO_MENU);
            UIManager.Instance.ShowHomePage();
        });
    }

    private void BackHomeStore()
    {
        MCache.Instance.FxFill.Hide();
        UIManager.Instance.Fader.Show();
        DestroyOldPuzzle();
        UIManager.Instance.UIPuzzleCompleted.Close();
        UIManager.Instance.Fader.FadeHide(1.0f, () =>
        {
            UIManager.Instance.Fader.Hide();
            SoundController.Instance.PlayMainBackgroundMusic();
            _stateMachine.ProcessTriggerEvent(GameConstants.ENDING_TO_MENU);
        });
    }

    public void DestroyOldPuzzle()
    {
        if (m_CurrentPuzzle != null)
        {
            m_CurrentPuzzle.DestroyData();
        }
    }

    private int _usedHint = 0;
    public void UsedHint()
    {
        _usedHint += 1;
    }

    public void OnRating()
    {
        SetStepGame(StepGameConstants.Rating);
        SetStepGame(StepGameConstants.LuckyDrawOne);
        if (rateService == null)
            InitServiceRateUs();
#if UNITY_ANDROID
        if (rateService != null) rateService.ShowReviewInApp(false);
#elif UNITY_IOS
        if (rateService != null) rateService.ShowReviewInApp(false);
#endif



    }

    //private void ShowNextChapter()
    //{
    //    this._curPuzzleId = 3;
    //    GameManager.Instance.StartLevel(this._curBookId, this._curPartId, this._curPuzzleId, ConfigManager.GameData.PlayType.new_puzzle);
    //}

    public void ResumeGameWhenOpen()
    {
        if (GameData.Instance.SavedPack.SaveData.IsResumeComplete)
        {
            MinLoadingIntro = 0;
            _curBookId = GameData.Instance.SavedPack.SaveData.ResumeBookId;
            _curPartId = GameData.Instance.SavedPack.SaveData.ResumePartId;
            _curPuzzleId = GameData.Instance.SavedPack.SaveData.ResumePuzzleId;
            GameManager.Instance.StartLevel(this._curBookId, this._curPartId, this._curPuzzleId, ConfigManager.GameData.PlayType.new_puzzle);

        }
        else
        {
            List<LastPuzzlePlay> lastPlays = GameData.Instance.SavedPack.LastPuzzlePlays;
            if (lastPlays.Count > 0)
            {
                MinLoadingIntro = 0;
                var index = lastPlays.Count - 1;
                this._curBookId = lastPlays[index].BookId;
                this._curPartId = lastPlays[index].PartId;
                this._curPuzzleId = lastPlays[index].PuzzleId;
                GameManager.Instance.StartLevel(this._curBookId, this._curPartId, this._curPuzzleId, ConfigManager.GameData.PlayType.new_puzzle);

            }
            else
            {
                PuzzleLevelData nextPuzzle;

                this._curBookId = GameData.Instance.SavedPack.SaveData.ResumeBookId;
                this._curPartId = GameData.Instance.SavedPack.SaveData.ResumePartId;
                this._curPuzzleId = GameData.Instance.SavedPack.SaveData.ResumePuzzleId;
                nextPuzzle = GetNextPuzzle();
                if (nextPuzzle != null)
                {
                    MinLoadingIntro = 0;
                    this._curBookId = GameData.Instance.SavedPack.SaveData.ResumeBookId;
                    this._curPartId = GameData.Instance.SavedPack.SaveData.ResumePartId;
                    this._curPuzzleId = nextPuzzle.ID;
                    GameManager.Instance.StartLevel(this._curBookId, this._curPartId, this._curPuzzleId, ConfigManager.GameData.PlayType.new_puzzle);

                }
                else
                {
                    var book = MasterDataStore.Instance.GetBookByID(this._curBookId);
                    for (var i = 0; i < book.ListChapters.Count; i++)
                    {
                        if (i == book.ListChapters.Count - 1)
                        {
                            break;
                        }
                        if (book.ListChapters[i].ID == this._curPartId)
                        {
                            var indexNextChapter = i + 1;
                            this._curPartId = book.ListChapters[indexNextChapter].ID;
                            this._curPuzzleId = book.ListChapters[indexNextChapter].PuzzleLevels[0].ID;
                            break;
                        }
                    }

                    var partStt = GameData.Instance.SavedPack.GetPartStatus(this._curBookId, this._curPartId);

                    if (partStt == ChapterStatus.UNLOCK)
                    {
                        ChapterMasterData nextChapter;
                        nextChapter = GetNextChapter();
                        if (nextChapter != null)
                        {
                            MinLoadingIntro = 0;
                            this._curPuzzleId = 1;
                            this._curPartId = nextChapter.ID;
                            GameManager.Instance.StartLevel(this._curBookId, this._curPartId, this._curPuzzleId, ConfigManager.GameData.PlayType.new_puzzle);

                        }
                        else
                        {
                            UIManager.Instance.UpdateProgressLoading(1f);
                        }
                    }
                    else
                    {
                        UIManager.Instance.UpdateProgressLoading(1f);
                    }

                }
            }
        }
    }

    private PuzzleLevelData GetNextPuzzle()
    {
        if (this._curBookId <= 0 || string.IsNullOrEmpty(this._curPartId))
            return null;

        var partMasterData = MasterDataStore.Instance.GetPartById(this._curBookId, this._curPartId);
        if (partMasterData == null)
            return null;

        string[] idPart = this._curPartId.Split('-');
        if (!string.IsNullOrEmpty(idPart[1]) && this._curPuzzleId == partMasterData.PuzzleLevels[partMasterData.PuzzleLevels.Count - 1].ID)
        {
            this._curPuzzleId = 1;
            partMasterData = GetNextChapter();
            if (partMasterData != null)
            {
                this._curPartId = partMasterData.ID;
                var partStt = GameData.Instance.SavedPack.GetPartStatus(this._curBookId, partMasterData.ID);
                if (partStt == ChapterStatus.UNLOCK)
                    return partMasterData.PuzzleLevels[0];
            }
            else
            {
                return null;
            }

        }
        else
        {
            for (int i = 0; i < partMasterData.PuzzleLevels.Count; i++)
            {
                if (partMasterData.PuzzleLevels[i].ID == this._curPuzzleId && i + 1 < partMasterData.PuzzleLevels.Count)
                {
                    return partMasterData.PuzzleLevels[i + 1];
                }
            }

        }
        return null;

    }

    public ChapterMasterData GetNextChapter()
    {
        var bookMasterData = MasterDataStore.Instance.GetBookByID(this._curBookId);
        for (var i = 0; i < bookMasterData.ListChapters.Count; i++)
        {
            if (bookMasterData.ListChapters[i].ID == this._curPartId && i + 1 < bookMasterData.ListChapters.Count)
            {
                return bookMasterData.ListChapters[i + 1];
            }
        }

        return null;

    }

    public void LoadNextPuzzleIndex()
    {
    }

    private void OnLoadAnimation()
    {
        if (_puzzleLevelData == null) return;

        puzzleLightAnimation.gameObject.SetActive(false);
        puzzleSkeletonAnimation.gameObject.SetActive(false);
        //puzzleAnimationHolder.SetActive(false);
        _IsLoadedAnimation = false;
        if (_puzzleLevelData != null && _puzzleLevelData.Animation != null && _puzzleLevelData.Animation.animation != null)
        {
            AssetManager.Instance.LoadPathAsync<SkeletonDataAsset>(_puzzleLevelData.Animation.animation, (anim) =>
            {
                if (anim != null && puzzleSkeletonAnimation != null)
                {
                    DataCore.Debug.Log($"Loaded {_puzzleLevelData.Animation.animation}");
                    puzzleSkeletonAnimation.ClearState();

                    puzzleSkeletonAnimation.skeletonDataAsset = anim;

                    puzzleSkeletonAnimation.Initialize(true);
                    try
                    {
                        puzzleSkeletonAnimation.AnimationState.SetAnimation(0, "animation", true);
                    }
                    catch
                    {
                        DataCore.Debug.Log("Animation not found: animation");
                    }
                    _IsLoadedAnimation = true;
                    //ActivePuzzleAnimation(true);
                }
            });
        }
    }
    public void FinishPuzzle()
    {
        OnLoadAnimation();
        MinLoadingIntro = MCache.Instance.Config.MIN_LOADING_WAITING_TIME;
        if (ZoomInZoomOut.Instance.IsAllowMove)
        {
            ZoomInZoomOut.Instance.ResetToDefault();
        }

        GameData.Instance.SavedPack.SaveData.CountCompletedPuzzle++;
        GameData.Instance.SavedPack.SaveData.CountCompletedPuzzleLuckyDraw++;
        GameData.Instance.SavedPack.SaveData.CountCompletedPuzzlePromotionBanner++;
        GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.COMPLETE_PUZZLE, 1);
        GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.PLAY_DIFFERENCE_BOOK, _curBookId);
        //GameData.Instance.IncreaseInks(MasterDataStore.Instance.GetPuzzleById(_curBookId, _curPartId, _curPuzzleId).Ink, ConfigManager.GameData.ResourceEarnSource.puzzle_reward);

        SetTotalPuzzleFinish();


        UIManager.Instance.UIGameplay.ScrollObject.Clear();

        if (_isEventLevel)
        {
            GameData.Instance.SavedPack.LiveEventSavedData.UpdateCompletePuzzle(_liveEventPostCardId, _liveEventPuzzleId);
        }
        else
        {
            GameData.Instance.SavedPack.SaveUserPuzzleData(_curBookId, _curPartId, _curPuzzleId, PuzzleStatus.COMPLETE);

        }

        if (GameManager.Instance.PuzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
        {
            var puzzle = MasterDataStore.Instance.GetCollectionPuzzleByIndex(_collectionId, _collectionIndex);
            GameData.Instance.SavedPack.DataRemoveLastPlayedPuzzleCollection(_collectionId, _collectionIndex);
            GameData.Instance.SavedPack.DataSetCollectionPuzzlePlaying(_collectionId, _collectionIndex, PuzzleStatus.COMPLETE);

            if (GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
            {
                GameData.Instance.SavedPack.DataAddIndexPuzzleCollectionComplete(_collectionId, _collectionIndex);
            }
        }
        else
        {
            GameData.Instance.SavedPack.SaveUserPuzzleData(_curBookId, _curPartId, _curPuzzleId, PuzzleStatus.COMPLETE);
            if (GameData.Instance.SavedPack.RemoveLastPlayedPuzzle(_curBookId, _curPartId, _curPuzzleId))
            {
                this.PostEvent(EventID.OnUpdateResumePlaying);
            }
        }


        if (GetStepGame() == StepGameConstants.StepComplete)
            GameData.Instance.SavedPack.SaveData.IsResumeComplete = false;

        GameData.Instance.RequestSaveGame();
        if (GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
        {
            _stateMachine.ProcessTriggerEvent(GameConstants.PLAYING_TO_ENDING);
        }
        else
        {
            effectVictoryTutorial.SetActive(true);
            var timeDelay = 5.0f;
            if (CurrentTutorial < 1) ActivePuzzleAnimation(true);
            else timeDelay = 0;
            DOVirtual.DelayedCall(timeDelay, () =>
            {
                ActivePuzzleAnimation(false);
                _stateMachine.ProcessTriggerEvent(GameConstants.PLAYING_TO_ENDING);
            });
            DOVirtual.DelayedCall(5.5f, () =>
            {
                effectVictoryTutorial.SetActive(false);
            });
        }

        // check if update
        var playCount = PlayerPrefs.GetInt(ConfigManager.KeyTotalPlayPuzzleFinish, 0);
        if (playCount > ConfigManager.keyMaxPuzzleUpdateShowAds)
        {
            GameManager.Instance.SetStepGame(StepGameConstants.StepComplete);
        }
    }

    private void SetTotalPuzzleFinish()
    {
        var playPuzzleCount = PlayerPrefs.GetInt(ConfigManager.KeyTotalPlayPuzzleFinish, 0);
        playPuzzleCount++;
        PlayerPrefs.SetInt(ConfigManager.KeyTotalPlayPuzzleFinish, playPuzzleCount);
        PlayerPrefs.Save();
    }
    public void PlayAnimMoveAndScaleCompletePuzzle(float posY, float scale, Action onComplete)
    {
        spawnPoint.transform.DOMoveY(posY, 0.5f);
        spawnPoint.transform.DOScale(scale, 0.5f).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    public void ResetSpawnPoint()
    {
        spawnPoint.transform.position = new Vector3(0, cacheSpawnPointPosY, 0);

    }

    //  Cheat

    public void ForceFinishPuzzle()
    {
        //if (!Tutorial.IsCompleted)
        //{
        //   _tutorial.ForceComplete();
        //}
        UIManager.Instance.UIGameplay.ScrollObject.Clear();
    }

    public void LoadReplayPuzzle()
    {
        //MCache.Instance.FxFill.Hide();
        //UIManager.Instance.Fader.Show();
        //UIManager.Instance.Fader.FadeHide(1.0f, () =>
        //{
        //    UIManager.Instance.UIGameplay.ScrollObject.Clear();

        //    if (m_CurrentPuzzle != null)
        //    {
        //        Destroy(m_CurrentPuzzle.gameObject);
        //    }
        //    m_CurrentPuzzle = Instantiate(m_OldPuzzle, transform);

        //    Camera.main.transform.GetComponent<ZoomInZoomOut>().Puzzle = m_CurrentPuzzle;

        //    m_CurrentPuzzle.gameObject.SetActive(true);

        //    UserProfileManager.Instance.UserProfile.m_LevelOldInfo.m_IndexCurrentLayerDone = 0;
        //    //m_CurrentPuzzle.PreInit();

        //    DOVirtual.DelayedCall(1.0f, () =>
        //    {
        //        UIManager.Instance.Fader.FadeShow(1.0f, () =>
        //        {
        //            Destroy(m_OldPuzzle.gameObject);
        //            var levelData = GetLevelDataByLevel(m_CurrentIndexLevel);
        //            m_CurrentPuzzle.Init(levelData.PuzzleAnimator);
        //            //m_CurrentPuzzle.Init(m_PuzzlesAnim[m_CurrentIndexLevel], true);

        //            UIManager.Instance.Fader.Hide();
        //        });
        //    });
        //});
    }

    public void SetStepGame(int step)
    {
        if (IsCompleteStepGame())
            return;
        GameData.Instance.SavedPack.SaveData.StepGameOnBoard = step;
    }

    public int GetStepGame()
    {
        var stepComplete = GameData.Instance.SavedPack.SaveData.StepGameOnBoard;
        if (GameData.Instance.SavedPack.SaveData.StepGameOnBoard > stepComplete)
            return GameData.Instance.SavedPack.SaveData.StepGameOnBoard;
        return stepComplete;
    }

    public bool IsCompleteStepGame()
    {
        var stepComplete = GameData.Instance.SavedPack.SaveData.StepGameOnBoard;
        return stepComplete == StepGameConstants.StepComplete;
    }

    //public void StepGame()
    //{
    //    UnlockAllPuzzle();
    //    if (IsCompleteStepGame())
    //        return;
    //    var playGame = GetStepGame();
    //    if (playGame == StepGameConstants.Tutorial)
    //        SetStepGame(StepGameConstants.PlayPuzzleOne);
    //    else if (playGame == StepGameConstants.Rating)
    //        SetStepGame(StepGameConstants.PlayPuzzleThree);
    //    else if (playGame == StepGameConstants.LuckyDrawOne)
    //        SetStepGame(StepGameConstants.PlayPuzzleFour);
    //    else if (playGame == StepGameConstants.LuckyDrawTwo)
    //        SetStepGame(StepGameConstants.PlayPuzzleFive);

    //}

    public void UnlockAllPuzzle()
    {
        if (PuzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
            return;

        if (this._curBookId == 0 || this._curPartId.Equals("0") || this._curPuzzleId == 0)
            return;

        var partStt = GameData.Instance.SavedPack.GetPartStatus(this._curBookId, this._curPartId);

        if (partStt != ChapterStatus.UNLOCK && PuzzleOpenPlacement != ConfigManager.GameData.PlayType.collection_play_puzzle)
            GameData.Instance.SavedPack.SaveUserChapterData(this._curBookId, this._curPartId, ChapterStatus.UNLOCK);

        var bookUserData = GameData.Instance.SavedPack.GetBookData(this._curBookId);

        if (bookUserData != null)
        {
            var partUserData = bookUserData.GetChapterSaveData(this._curPartId);
            if (partUserData != null)
            {
                var chapterData = MasterDataStore.Instance.GetPartById(this._curBookId, this._curPartId);
                var puzzleUserData = partUserData.GetPuzzleSaveData(chapterData.PuzzleLevels[0].ID);

                if (puzzleUserData == null)
                {
                    for (var i = 0; i < chapterData.PuzzleLevels.Count; i++)
                    {
                        GameData.Instance.SavedPack.SaveUserPuzzleData(this._curBookId, this._curPartId, chapterData.PuzzleLevels[i].ID, PuzzleStatus.UNLOCK);

                    }
                }
            }
        }


        GameData.Instance.RequestSaveGame();
    }

    IEnumerator LoadScene(PuzzleLevelData data)
    {
        yield return new WaitForEndOfFrame();
        UIManager.Instance.ShowUIArtBlitzGame(true);
        GameArtBlitzManager.Instance.Init(data);
    }


    #region popup back list

    public bool isCloseEscape = false;
    public List<BasePanel> objListOpen = new List<BasePanel>();

    public bool IsCloseEscape
    {
        get { return this.isCloseEscape; }
        set { this.isCloseEscape = value; }
    }

    public void AddObjList(BasePanel basePanel)
    {
        if (!objListOpen.Contains(basePanel))
            objListOpen.Add(basePanel);

    }

    public void RemoveObjList(BasePanel basePanel)
    {
        if (objListOpen.Contains(basePanel))
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                IsCloseEscape = false;
            });
            objListOpen.Remove(basePanel);
        }

    }

    #endregion
}
