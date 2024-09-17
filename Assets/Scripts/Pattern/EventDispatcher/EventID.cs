public enum EventID
{
    None = 0,
    BackHome,
    BackGameDetail,
    BackBookDetail,
    BackLiveEvent,
    LiveEventTimeout,
    PlayPuzzle,
    NextPuzzle,
    DestroyPuzzle,
    BackHomeResume,
    CheckShowIntertitialAds,
    BackHomeNextChapter,
    BackHomeStore,
    BackPuzzleTab,
    BackHomeCollection,

    // Gameplay Event
    OnInitPuzzleCompleted,
    UpdateProgressLayer,
    NextLayer,
    PauseTimeTutorial,
    OnUpdateSubcribeState,
    OnUpdateResumePlaying,
    OnUpdateResumeCollectionPlaying,
    UsedHint,
    OnPhysicalBack
}