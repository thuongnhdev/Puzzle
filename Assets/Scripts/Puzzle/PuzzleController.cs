using DG.Tweening;
using EventDispatcher;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataCore;
using com.F4A.MobileThird;
using System;
using UnityEngine.UI;

public class PuzzleController : MonoBehaviour
{
    [SerializeField] int indexLsIndexLayer;
    [SerializeField] SpriteRenderer[] sprTargets;
    [SerializeField] SpriteRenderer[] sprSources;
    [SerializeField] LayerInfo[] layers;
    [SerializeField] int currentIndexObject;
    [SerializeField] int totalObject;
    [SerializeField] List<int> lsIndexLayer;
    [SerializeField] PuzzleLevelInitData m_PuzzleInitData;
    Action _InitCompleted;


    private bool m_InitImagePuzzleDone = false;
    public LayerInfo[] Layers { get => layers; set => layers = value; }
    public int IndexCurrentLayer { get => indexLsIndexLayer; set => indexLsIndexLayer = value; }
    public int CurrentIndexObject { get => currentIndexObject; set => currentIndexObject = value; }
    public int TotalObject { get => totalObject; set => totalObject = value; }
    public List<int> LsIndexLayer { get => lsIndexLayer; set => lsIndexLayer = value; }
    private string _placement = "";
    public string PuzzleName { get => m_PuzzleInitData.NamePuzzle; }
    public void PreInit(PuzzleLevelInitData puzzleData, string placement)
    {
        if (m_PuzzleInitData != null)
        {
            DestroyData();
        }
        _placement = placement;
        indexLsIndexLayer = 0;
        m_PuzzleInitData = Instantiate(puzzleData, Vector3.zero, Quaternion.identity, transform);
        m_PuzzleInitData.Init();
        //StartCoroutine(m_PuzzleInitData.GenarateData(() =>
        //{
        // UIManager.Instance.TrsLoading.gameObject.SetActive(false);

        //m_PuzzleInitData.BuildPuzzle();

        sprTargets = m_PuzzleInitData.SpriteTarget.ToArray();
        sprSources = m_PuzzleInitData.SpriteSource.ToArray();

        Transform GLs = m_PuzzleInitData.transform.GetChild(1).transform;
        int numberLayer = GLs.childCount;
        layers = new LayerInfo[numberLayer];
        bool isActive;
        for (int indexLayer = 0; indexLayer < numberLayer; indexLayer++)
        {
            Transform layer = GLs.GetChild(indexLayer).transform;

            layers[indexLayer] = new LayerInfo();
            layers[indexLayer].IndexLayer = indexLayer;
            layers[indexLayer].TargetObject = new TargetObject2D[layer.childCount];

            isActive = indexLayer == 0;

            DataCore.Debug.Log("Layer " + indexLayer + ": " + isActive, false);

            for (int indexTargetDrag2D = 0; indexTargetDrag2D < layer.childCount; indexTargetDrag2D++)
            {
                layers[indexLayer].TargetObject[indexTargetDrag2D] = layer.GetChild(indexTargetDrag2D).GetComponent<TargetObject2D>();
                layers[indexLayer].TargetObject[indexTargetDrag2D].gameObject.SetActive(isActive);
            }
        }

        for (int i = 0; i < sprSources.Length; i++)
        {
            sprSources[i].gameObject.SetActive(false);
        }

        DOVirtual.DelayedCall(0.5f, () => { m_InitImagePuzzleDone = true; }).SetId(this);

        //}));

    }
    public void Init(bool isResumePlay, Action completed)
    {
        _InitCompleted = completed;
        StartCoroutine(DelayInit(isResumePlay));
    }

    IEnumerator DelayInit(bool isResumePlay)
    {
        yield return new WaitUntil(() => m_InitImagePuzzleDone);
        DataCore.Debug.Log("DelayInit m_InitImagePuzzleDone");
        try
        {
            if (Camera.main.aspect < 0.5625f)
            {
                transform.parent.localScale = Vector3.one * (Camera.main.aspect / 0.5625f);
            }
            else
            {
                transform.parent.localScale = Vector3.one;
            }
            DataCore.Debug.Log("Completed setup canera");

            lsIndexLayer = new List<int>();
            totalObject = 0;
            for (int indexLayer = 0; indexLayer < layers.Length; indexLayer++)
            {
                layers[indexLayer].Puzzle = this;
                layers[indexLayer].IndexLayer = indexLayer;
                for (int i = 0; i < layers[indexLayer].TargetObject.Length; i++)
                {
                    layers[indexLayer].TargetObject[i].Layer = layers[indexLayer];
                    totalObject++;
                }
            }

            DataCore.Debug.Log("Completed layers", false);

            currentIndexObject = 0;

            List<SpriteRenderer> sources = new List<SpriteRenderer>(sprSources);
            List<SpriteRenderer> targets = new List<SpriteRenderer>(sprTargets);

            int bookId = GameManager.Instance.CurBookId;
            string partId = GameManager.Instance.CurPartId;
            int puzzleId = GameManager.Instance._curPuzzleId;
            DataCore.Debug.Log("Call Delay Init: isResumePlay "+ isResumePlay);
            if (!isResumePlay)
            {
                if (PlayerData.Instance.IsNewer)
                {
                    DataCore.Debug.Log("RandomLayer", false);
                    RandomLayer();
                }
                else
                {
                    DataCore.Debug.Log("Add Range: " + PlayerData.Instance.UserProfile.m_LevelOldInfo.m_LsIndexLayer, false);
                    this.lsIndexLayer.AddRange(PlayerData.Instance.UserProfile.m_LevelOldInfo.m_LsIndexLayer);
                }
            }
            else
            {
                if (GameManager.Instance.PuzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
                {
                    var data = GameData.Instance.SavedPack.DataGetCurrentPuzzleCollectionData(GameManager.Instance._collectionId, GameManager.Instance._collectionIndex);
                    if (data != null)
                    {
                        var savedData = data.savedData;
                        this.lsIndexLayer.AddRange(savedData.m_LsIndexLayer);
                        indexLsIndexLayer = savedData.m_IndexCurrentLayerDone + 1;
                        currentIndexObject = savedData.m_LsObjectDone.Count;
                    }
                    else
                    {
                        DataCore.Debug.LogError("Can't find resume data");
                    }
                }
                else
                {
                    if (GameData.Instance.SavedPack.GetCurrentPuzzleData() != null)
                    {
                        var savedData = GameData.Instance.SavedPack.GetCurrentPuzzleData().savedData;
                        this.lsIndexLayer.AddRange(savedData.m_LsIndexLayer);
                        indexLsIndexLayer = savedData.m_IndexCurrentLayerDone + 1;
                        currentIndexObject = savedData.m_LsObjectDone.Count;

                    }
                    else
                    {
                        DataCore.Debug.LogError("Can't find resume data");
                    }
                }
            }
            DataCore.Debug.Log("Call Delay Init: " + gameObject.name + ", " + sprSources.Length + ", " + sprTargets.Length, false);

            UpdateProgression();
            layers[GetIndexLayer()].ShowLayer();
            UIManager.Instance.UIGameplay.SetProgressBar(GetTotalObjInCurrentLayer());

            if (isResumePlay)
            {
                int curLayerObjectLength = layers[indexLsIndexLayer].TargetObject.Length;

                LevelInfo savedData = null;
                if (GameManager.Instance.PuzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
                {
                    var item = GameData.Instance.SavedPack.DataGetCurrentPuzzleCollectionData(GameManager.Instance._collectionId, GameManager.Instance._collectionIndex);
                    if (item != null)
                    {                        
                        savedData = item.savedData;
                    }
                }
                else 
                {
                    LastPuzzlePlay lastPuzzlePlay = GameData.Instance.SavedPack.GetCurrentPuzzleData();
                    savedData = lastPuzzlePlay.savedData;
                }
                for (int i = sources.Count - 1; i >= 0; i--)
                {
                    if (savedData.m_LsObjectDone
                        .Contains(sources[i].name))
                    {
                        sources[i].gameObject.SetActive(true);
                        targets[i].gameObject.SetActive(false);

                        for (int j = 0; j < curLayerObjectLength; j++)
                        {
                            if (targets[i].name == layers[indexLsIndexLayer].TargetObject[j].transform.name)
                            {
                                layers[indexLsIndexLayer].TargetObject[j].IsDoneTarget = true;
                            }
                        }

                        sources.RemoveAt(i);
                        targets.RemoveAt(i);
                    }
                }

                // DataCore.Debug.Log("-------------- " + indexLsIndexLayer);

                for (int i = 0; i <= indexLsIndexLayer; i++)
                {
                    //DataCore.Debug.Log("-------------- set done " + i);
                    Layers[i].IsDone = true;
                }

                layers[indexLsIndexLayer].UpdateStageLayer();
            }

            if (!Tutorial.IsCompleted)
                UIManager.Instance.UIGameplay.ScrollObject.CreateObjectsItemUITutorial(this, sources.ToArray(), targets.ToArray());
            else
                UIManager.Instance.UIGameplay.ScrollObject.CreateObjectsItemUI(this, sources.ToArray(), targets.ToArray());

            DataCore.Debug.Log("OnInitPuzzleCompleted");

            this.PostEvent(EventID.OnInitPuzzleCompleted);

            if (m_PuzzleInitData != null)
            {
                if (Tutorial.IsCompleted)
                {
                    AnalyticManager.Instance.TrackStartPuzzleEvent(m_PuzzleInitData.NamePuzzle, _placement);
                    var step = GameManager.Instance.GetStepGame();
                    if (step == StepGameConstants.PlayPuzzleOne)
                    {
                        var eventName = $"{ConfigManager.TrackingEvent.EventName.start_puzzle}_1";
                        AnalyticManager.Instance.LogEvent(eventName);
                    }
                    else if (step == StepGameConstants.PlayPuzzleTwo)
                    {
                        var eventName = $"{ConfigManager.TrackingEvent.EventName.start_puzzle}_2";
                        AnalyticManager.Instance.LogEvent(eventName);
                    }
                    else if (step == StepGameConstants.PlayPuzzleThree)
                    {
                        var eventName = $"{ConfigManager.TrackingEvent.EventName.start_puzzle}_3";
                        AnalyticManager.Instance.LogEvent(eventName);
                    }
                }
                else
                {
                    var mappingTutPuzzle = new Dictionary<string, int>()
                    {
                        { "Alice_6_Tut", 1 },
                        { "Peter_Pan_19_Tut", 2 },
                        { "The_Secret_Garden_8_Tut", 3 },
                    };
                    if (mappingTutPuzzle.ContainsKey(m_PuzzleInitData.NamePuzzle))
                    {
                        var index = mappingTutPuzzle[m_PuzzleInitData.NamePuzzle];
                        AnalyticManager.Instance.TrackStartPuzzleInTutorial(index, (int)GameSession.Instance.SessionPlayedTime);
                    }
                }
            }

        }
        catch (Exception ex)
        {
            DataCore.Debug.Log($"Failed DelayInit for Error: {ex.Message}");
        }

        yield return new WaitForSeconds(0.5f);

        _InitCompleted?.Invoke();
        _InitCompleted = null;

        if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
        {
            SetActiveLayerTutorial();
        }
    }

    public void SetActiveLayerTutorial()
    {
        if (Layers.Length == 1)
            return;

        DOVirtual.DelayedCall(2.0f, () =>
        {
            GameManager.Instance.OnResetTimeShowTutorial();
            UIManager.Instance.UIGameplay.ShowTutorial();

        });
        UIManager.Instance.UIGameplay.ScrollObject.SetSizeViewPort(Layers[GetIndexLayer()].TargetObject.Length);
        var _layerIndex = GetIndexLayer() + 1;
        for (var i = 0; i < UIManager.Instance.UIGameplay.ScrollObject.ObjectItems.Length; i++)
        {
            UIManager.Instance.UIGameplay.ScrollObject.ObjectItems[i].gameObject.SetActive(false);
        }

        for (var i = 0; i < Layers[GetIndexLayer()].TargetObject.Length; i++)
        {
            string[] objName = Layers[GetIndexLayer()].TargetObject[i].name.Split('_');
            for (var j = 0; j < UIManager.Instance.UIGameplay.ScrollObject.ObjectItems.Length; j++)
            {
                if (string.Compare(objName[1], UIManager.Instance.UIGameplay.ScrollObject.ObjectItems[j].name) == 0)
                {
                    if (_layerIndex > 1)
                    {
                        var dragObject = UIManager.Instance.UIGameplay.ScrollObject.ObjectItems[j].gameObject.GetComponentInChildren<DragObject>();
                        var imageFade = dragObject.imgObjSource;
                        Color tmp = imageFade.color;
                        tmp.a = 0f;
                        imageFade.color = tmp;
                        UIManager.Instance.UIGameplay.ScrollObject.ObjectItems[j].gameObject.SetActive(true);
                        //fade in object in layer
                        DOVirtual.Float(0.0f, 1.0f, 2.5f, (x) =>
                        {
                            tmp.a = x;
                            imageFade.color = tmp;
                        }).OnComplete(() =>
                        {
                            tmp.a = 1.0f;
                            imageFade.color = tmp;
                        });
                    }
                    else
                        UIManager.Instance.UIGameplay.ScrollObject.ObjectItems[j].gameObject.SetActive(true);
                }
            }
        }




        if (Layers[GetIndexLayer()].TargetObject.Length > 3)
        {


            int indexCurrentLayer = GameManager.Instance.CurrentPuzzle.GetIndexLayer();
            var arrObjectInLayer = GameManager.Instance.CurrentPuzzle.Layers[indexCurrentLayer].TargetObject;
            TargetObject2D targetObject2D = null;
            string nameHint = "";
            for (int i = 0; i < arrObjectInLayer.Length; i++)
            {
                if (arrObjectInLayer[i].IsDoneTarget == false)
                {
                    nameHint = arrObjectInLayer[i].name;
                    targetObject2D = arrObjectInLayer[i];
                    break;
                }
            }

            string str_NameSource = GameManager.Instance.CurrentPuzzle.GetNameTarget(nameHint);

            int indexItemUI = -1;
            ObjectItem objectItemTempUI = null;
            for (int i = 0; i < UIManager.Instance.UIGameplay.ScrollObject.ObjectItems.Length; i++)
            {
                if (UIManager.Instance.UIGameplay.ScrollObject.ObjectItems[i].gameObject.activeSelf && UIManager.Instance.UIGameplay.ScrollObject.ObjectItems[i].transform.childCount > 0)
                {
                    ++indexItemUI;
                    if (UIManager.Instance.UIGameplay.ScrollObject.ObjectItems[i].name.ToString().Equals(str_NameSource))
                    {
                        objectItemTempUI = UIManager.Instance.UIGameplay.ScrollObject.ObjectItems[i];
                        break;
                    }
                }
            }

            if (indexItemUI > 2)
            {
                MCache.Instance.Hand.IndexShowHorizontal = 0;
                // UI -> World
                Vector3 v3_objectItemTempUI = Camera.main.ScreenToWorldPoint(objectItemTempUI.transform.position);

                // World -> Screen
                v3_objectItemTempUI = Camera.main.WorldToScreenPoint(v3_objectItemTempUI);
                MCache.Instance.Hand.StartMoveHorizontal(v3_objectItemTempUI);
            }
        }
    }

    public int GetIndexLayer()
    {
        //return LsIndexLayer[indexLsIndexLayer];
        return indexLsIndexLayer;
    }


    public void Show()
    {
        m_PuzzleInitData.gameObject.SetActive(true);
    }

    public void Hide()
    {
        m_PuzzleInitData.gameObject.SetActive(false);
    }

    public Vector2 GetResolutionOfBG()
    {
        return m_PuzzleInitData.ResolutionOfBG;
    }

    public Sprite GetSprite(string key)
    {
        return m_PuzzleInitData.GetSprite(key);
    }

    public int GetTotalObjInCurrentLayer()
    {
        return layers[GetIndexLayer()].TargetObject.Length;
    }
    void RandomLayer()
    {
        List<int> lsIndexLayer = new List<int>();
        for (int i = 0; i < layers.Length; i++)
        {
            lsIndexLayer.Add(i);
        }
        for (int index = 0; index < layers.Length; index++)
        {
            int indexR = UnityEngine.Random.Range(0, lsIndexLayer.Count);
            this.lsIndexLayer.Add(index /*lsIndexLayer[indexR]*/);
            lsIndexLayer.RemoveAt(indexR);
        }
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].IndexLayer = i;
        }

        PlayerData.Instance.UserProfile.m_LevelOldInfo.m_LsIndexLayer.Clear();
        PlayerData.Instance.UserProfile.m_LevelOldInfo.m_LsIndexLayer.AddRange(this.lsIndexLayer);

        if (GameManager.Instance.PuzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
        {
            var data = GameData.Instance.SavedPack.DataGetCurrentPuzzleCollectionData(GameManager.Instance._collectionId, GameManager.Instance._collectionIndex);

            if (data != null)
            {
                data.savedData.m_LsIndexLayer.AddRange(this.lsIndexLayer);
                GameData.Instance.RequestSaveGame();
            }
        }
        else
        {
            if (GameData.Instance.SavedPack.GetCurrentPuzzleData() != null)
            {
                GameData.Instance.SavedPack.GetCurrentPuzzleData().savedData.m_LsIndexLayer.AddRange(this.lsIndexLayer);
                GameData.Instance.RequestSaveGame();
            }
        }


    }

    public bool CheckDonePuzzle()
    {
        foreach (var item in layers)
        {
            if (item.IsDone == false)
                return false;
        }
        return true;
    }
    public void UpdateStagePuzzle()
    {
        if (CheckDonePuzzle() == false)
        {
            if (PlayerData.Instance.UserProfile.m_LevelOldInfo.m_IndexCurrentLayerDone != indexLsIndexLayer)
            {
                PlayerData.Instance.UserProfile.m_LevelOldInfo.m_IndexCurrentLayerDone = indexLsIndexLayer;
            }

            if (GameManager.Instance.PuzzleOpenPlacement == ConfigManager.GameData.PlayType.collection_play_puzzle)
            {
                int collectionId = GameManager.Instance._collectionId;
                var data = GameData.Instance.SavedPack.DataGetCurrentPuzzleCollectionData(GameManager.Instance._collectionId, GameManager.Instance._collectionIndex);
                if (data != null && data.savedData.m_IndexCurrentLayerDone != indexLsIndexLayer)
                {
                    data.savedData.m_IndexCurrentLayerDone = indexLsIndexLayer;
                    GameData.Instance.RequestSaveGame();
                }
            }
            else
            {
                if (GameData.Instance.SavedPack.GetCurrentPuzzleData() != null && GameData.Instance.SavedPack.GetCurrentPuzzleData().savedData.m_IndexCurrentLayerDone != indexLsIndexLayer)
                {
                    GameData.Instance.SavedPack.GetCurrentPuzzleData().savedData.m_IndexCurrentLayerDone = indexLsIndexLayer;
                    GameData.Instance.RequestSaveGame();
                }
            }



            ++indexLsIndexLayer;
            DOVirtual.DelayedCall(MCache.Instance.Config.TIME_NEXT_LAYER, () =>
            {
                layers[GetIndexLayer()].ShowLayer();
                if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
                {
                    SetActiveLayerTutorial();
                }
            });

            this.PostEvent(EventID.NextLayer);
        }
        else
        {
            var timeDelay = MCache.Instance.Config.TIME_NEXT_LAYER - 0.5f;
            //DOVirtual.DelayedCall(MCache.Instance.Config.TIME_NEXT_LAYER - 0.2f, () => { SoundController.Instance.PlaySfxWin(); });
            DOVirtual.DelayedCall(timeDelay, () =>
              {
                  // ToDO: call FinishPuzzle
                  GameManager.Instance.FinishPuzzle();
                  //GameManager.Instance.LoadReplayPuzzle();
                  //gameObject.SetActive(false);
                  //UIManager.Instance.UIGameplay.gameObject.SetActive(false);
                  //anim.gameObject.SetActive(true);
              });
        }
    }

    public void UpdateProgression()
    {
        UIManager.Instance.UIGameplay.ScrollObject.UpdateProgression(currentIndexObject, totalObject);
    }
    public string GetNameTarget(string str_Name)
    {
        if (sprTargets == null) return "";

        for (int i = 0; i < sprTargets.Length; i++)
        {
            if (sprTargets[i].name.Equals(str_Name))
            {
                return sprSources[i].name;
            }
        }
        return "";
    }

    public void DestroyData()
    {
        if (m_PuzzleInitData != null)
        {
            DataCore.Debug.Log("Detroy data: " + m_PuzzleInitData.NamePuzzle, false);
            DestroyImmediate(m_PuzzleInitData.gameObject);
        }
    }
}

[System.Serializable]
public enum PuzzleStatus
{
    NONE, LOCK, UNLOCK, COMPLETE
}