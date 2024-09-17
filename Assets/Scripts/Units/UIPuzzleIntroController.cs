using System.Collections;
using System.Collections.Generic;
using DataCore;
using DG.Tweening;
using EventDispatcher;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using com.F4A.MobileThird;

public class UIPuzzleIntroController : BasePanel, IPointerDownHandler, IPointerUpHandler
{

    [SerializeField] private UITextWriter txtWriter;
    [SerializeField] private TextMeshProUGUI txtPuzzleName;
    [SerializeField] private TextMeshProUGUI txtPuzzleFooter;

    private PuzzleLevelData currentData;

    public override void Init()
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        DataCore.Debug.Log("Touch on PuzzleController");
        txtWriter.SetTimePerChar(40.0f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        DataCore.Debug.Log("OnPointerUp PuzzleController");
        txtWriter.SetTimePerChar(20.0f);
    }

    public override void SetData(object[] data)
    {
        base.SetData(data);

        if (data.Length == 0)
        {
            return;
        }

        currentData = (PuzzleLevelData)data[0];
        UpdateUI();
    }

    public override void Open()
    {
        base.Open();
        UIManager.Instance.EnablePuzzleBorder(false);
        var step = GameManager.Instance.GetStepGame();
        if (step == StepGameConstants.PlayPuzzleOne) {            
            AnalyticManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.show_puzzle_description_1);
        }
    }

    private void UpdateUI()
    {
        var txtDesc = currentData.Desc.Trim();
        txtWriter.SetText(txtDesc, 20.0f);
        var name = currentData.name.Replace("_", " ");
        var book = MasterDataStore.Instance.GetBookByID(GameManager.Instance.CurBookId);
        var chapter = MasterDataStore.Instance.GetPartById(GameManager.Instance.CurBookId, GameManager.Instance.CurPartId);
        txtPuzzleName.text = book.BookName;
        txtPuzzleFooter.text = chapter.PartName + " - Puzzle " + currentData.ID + "/" + chapter.PuzzleLevels.Count;
    }

    public void OnContinueClick()
    {
        SoundController.Instance.PlaySfxClick();
        Close();

        if (AdsLogic.IsBannerAds())
        {
            AdsService.Instance.ShowBanner();
        }
        //this.PostEvent(EventID.PlayPuzzle);
        var step = GameManager.Instance.GetStepGame();
        if (step == StepGameConstants.PlayPuzzleOne)
        {
            AnalyticManager.Instance.LogEvent(ConfigManager.TrackingEvent.EventName.completed_puzzle_description_1);
        }
        if (GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
        {
            UIManager.Instance.UIGameplay.OnShowPopupHint();
        }
    }
  

}
