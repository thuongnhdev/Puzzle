using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIBookStore : HomepageTab
{
    [SerializeField] private IntroSection introSection;
    [SerializeField] private ResumePlayingSection resumePlayingSection;
    //[SerializeField] private TopPicksSection topPicksSection;
    //[SerializeField] private LatestUpdateSection latestUpdateSection;
    [SerializeField] private MyAllCollection myAllCollection;
    [SerializeField] private Transform footer;
    //[SerializeField] private FreePlayingSection freePlayingSection;

    public override void Init()
    {
        base.Init();

        DOVirtual.DelayedCall(0.0f, () =>
        {
            footer.SetAsLastSibling();
        }).Play();
    }
    private void InitComponents() {
        if (!_didInit) {
            introSection.Init();
            //resumePlayingSection.Init();
            //topPicksSection.Init();
            //latestUpdateSection.Init();
            //freePlayingSection.Init();
            myAllCollection.Init();
        }
        _didInit = true;
    }
    protected override void AfterShowed()
    {
        base.AfterShowed();
        scrollRect.verticalNormalizedPosition = 1;
        //resumePlayingSection.UpdateData();
        InitComponents();
    }
}
