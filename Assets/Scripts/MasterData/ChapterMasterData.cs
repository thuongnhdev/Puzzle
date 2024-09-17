using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ChapterMasterData", menuName = "ChapterMasterData", order = 1)]
[PreferBinarySerialization]
public class ChapterMasterData : ScriptableObject
{
    public string ID;
    public string PartName;
    public string Description;
    public string Author;
    public ThumbnailsData Thumbnail;
    public int Price;
    public List<PuzzleLevelData> PuzzleLevels = new List<PuzzleLevelData>();
    public string Version;
    public string ChapterThumbnailLabel() {
        if (Thumbnail != null && !string.IsNullOrEmpty(Thumbnail.Thumbnail))
        {
            var fileName = Thumbnail.Thumbnail.Split('/').Last();
            if (!string.IsNullOrEmpty(fileName))
            { 
                var thumbnail = fileName.Split('.').First();
                return $"Thumbnail_Small_{thumbnail}";
            }            
        }

        return string.Empty;
    }

    public List<string> FirstPuzzleLabels() {
        var labels = new List<string>();
        var firstPuzzle = PuzzleLevels.First();
        if (firstPuzzle != null) {
            labels.AddRange(firstPuzzle.Labels());
        }
        return labels;
    }
    public List<string> RemainingPuzzleLabelsExceptFirstPuzzle() 
    {
        var labels = new List<string>();
        var labelDic = new Dictionary<string, int>();
        for (int i = 0; i < PuzzleLevels.Count(); i++)
        {
            if (i > 0) {
                var puzzle = PuzzleLevels[i];
                var puzzleLabels = puzzle.Labels();
                for (int j = 0; j < puzzleLabels.Count(); j++)
                {
                    var label = puzzleLabels[j];
                    if (!labelDic.ContainsKey(label))
                    {
                        labels.Add(label);
                        labelDic.Add(label, 1);
                    }
                }
            }
        }
        return labels;
    }

    public List<string> Labels() {
        var labels = new List<string>();
        var labelDic = new Dictionary<string, int>();
        foreach (var puzzle in PuzzleLevels)
        {
            foreach (var label in puzzle.Labels())
            {
                if (!labelDic.ContainsKey(label)) {
                    labels.Add(label);
                    labelDic.Add(label, 1);
                }
            }            
        }

        return labels;
    }
    
}