using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "PuzzleLevelData", menuName = "PuzzleLevelData", order = 1)]
[PreferBinarySerialization]
public class PuzzleLevelData : ScriptableObject
{
    public int ID;
    public int Level;
    public string Name;
    public string Desc;
    public int Ink;
    public ThumbnailsData Thumbnail;
    public CompletePuzzleImageData CompletePuzzleImage;
    public string PuzzlePrefabAddress;
    public BackgroundMusicData BGMusic;
    public SpineData Animation;
    public string Version;
    public string PuzzleSpriteAtlasAddress { get { return $"Assets/Bundles/Puzzles/{Name}/Sprites.spriteatlas"; } }
    public string ThumbnailLabel() {
        if (Thumbnail != null && !string.IsNullOrEmpty(Thumbnail.Thumbnail)) { 
            var fileName = Thumbnail.Thumbnail.Split('/').Last();
            if (!string.IsNullOrEmpty(fileName))
            {
                var thumbnail = fileName.Split('.').First();
                return $"Thumbnail_Small_{thumbnail}";
            }
        }

        return string.Empty;
    }
    public List<string> Labels() {
        var labels = new List<string>();
        if (!string.IsNullOrEmpty(Name)) {
            labels.Add($"Puzzle_{Name}");
            labels.Add($"Spine_{Name}");
            labels.Add($"Thumbnail_Large_{Name}");
        }
        var musicLabel = BGMusic.GetLabel();
        if (!string.IsNullOrEmpty(musicLabel)) {
            labels.Add(BGMusic.GetLabel());
        }                
        return labels;
    }
}
