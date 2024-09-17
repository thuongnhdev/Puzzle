using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UICollectionStore : HomepageTab
{
    [SerializeField] private IntroSection introSection;
    [SerializeField] private PuzzleCollectionTab puzzleCollectionTab;
    [SerializeField] private Transform footer;

    public override void Init()
    {
        base.Init();
        if (!_didInit) {
            introSection.Init();
            puzzleCollectionTab.Init();

            DOVirtual.DelayedCall(0.0f, () =>
            {
                footer.SetAsLastSibling();
            }).Play();
        }
        _didInit = true;
    }

    protected override void AfterShowed()
    {
        base.AfterShowed();
        scrollRect.verticalNormalizedPosition = 1;
    }

}
