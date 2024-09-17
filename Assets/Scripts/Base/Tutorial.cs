using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataCore;

public class Tutorial
{
    public static bool IsCompleted
    {
        get { return GameData.Instance.SavedPack.SaveData.IsTutorialCompleted; }
    }
  
    private int _curStep;
    private float currentTime = 0;
    private bool isPause = false;

    public float CurrentTime
    {
        get { return this.currentTime; }
        set { this.currentTime = value; }
    }

    public Tutorial()
    {
        _curStep = 0;
    }

    public void NextStep()
    {
        _curStep++;
        UpdateTutorial();
        DataCore.Debug.Log("Tutorial Step: " + _curStep);
    }

    public void OnUpdate(float deltaTime)
    {

        if (Input.GetMouseButton(0))
        {
            UIManager.Instance.UIGameplay.HideTutorial();
            currentTime = 0;
            return;
        }
        if (isPause || MCache.Instance.Hand.IsShowingHorizontal || MCache.Instance.Hand.IsShowing)
        {
            return;
        }

        var timeCountDownShowHand = MCache.Instance.Config.TIME_COOLDOWN_SHOW_TUTORIAL;

        if (GameManager.Instance.CurrentPuzzle.GetIndexLayer() == 0
            && GameManager.Instance.CurrentTutorial < 1)
        {
            if (currentTime == 0)
            {
                currentTime += deltaTime;
                UIManager.Instance.UIGameplay.ShowTutorial();
            }
        }
        else
        {
            if (currentTime < timeCountDownShowHand)
            {
                currentTime += deltaTime;
                if (currentTime >= timeCountDownShowHand)
                {
                    UIManager.Instance.UIGameplay.ShowTutorial();
                }
            }
        }
    
    }

    public void Pause(bool value)
    {
        isPause = value;
    }

    public void CompletedStep()
    {
        if (_curStep == MCache.Instance.Config.NUMBER_TUTORIAL_STEP)
        {
            CompletedTutorial();
            return;
        }

        NextStep();
    }

    private void CompletedTutorial()
    {
        DataCore.Debug.Log("Completed Tutorial");
        GameData.Instance.SavedPack.SaveData.IsTutorialCompleted = true;
        GameData.Instance.RequestSaveGame();
        //UIManager.Instance.UIGameplay.EnableUITutorial(false);
        GameManager.Instance.SetStepGame(StepGameConstants.Tutorial);

    }

    private void UpdateTutorial()
    {
        //UIManager.Instance.UIGameplay.ShowTutorial();
    }

    // Cheat Finish
    public void ForceComplete()
    {
        CompletedTutorial();
    }



}
