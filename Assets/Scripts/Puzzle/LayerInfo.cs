using UnityEngine;

[System.Serializable]
public class LayerInfo
{
    [SerializeField] int indexLayer = -1;
    [SerializeField] bool isDone = false;
    [SerializeField] TargetObject2D[] targetObject;
    PuzzleController puzzle;
    public TargetObject2D[] TargetObject { get => targetObject; set => targetObject = value; }
    public bool IsDone { get => isDone; set => isDone = value; }
    public PuzzleController Puzzle { get => puzzle; set => puzzle = value; }
    public int IndexLayer { get => indexLayer; set => indexLayer = value; }

    public void UpdateStageLayer()
    {
        bool isDoneTemp = true;
        DataCore.Debug.Log("Layer: " + indexLayer + ", All Object: " + targetObject.Length, false);
        foreach (var item in targetObject)
        {
            if (item.IsDoneTarget == false)
            {
                isDoneTemp = false;
                break;
            }
        }

        UpdateProgressData();

        this.IsDone = isDoneTemp;
        if (isDoneTemp)
            puzzle.UpdateStagePuzzle();
    }

    private void UpdateProgressData()
    {
        LayerProgressData data = new LayerProgressData();
        data.Layer = indexLayer;
        data.Total = TargetObject.Length;
        int countComplete = 0;
        foreach (var item in targetObject)
        {
            if (item.IsDoneTarget)
            {
                countComplete++;
            }
        }

        data.NumComplete = countComplete;

        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.UpdateProgressLayer, data.NumComplete);

    }

    public void ShowLayer()
    {
        foreach (var item in targetObject)
        {
            item.Show();
        }
    }
}


public struct LayerProgressData
{
    public int Layer;
    public int NumComplete;
    public int Total;
}