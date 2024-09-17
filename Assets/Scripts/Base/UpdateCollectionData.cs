using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.F4A.MobileThird;
using System.Linq;
#if UNITY_EDITOR
    using UnityEditor;
#endif

public class UpdateCollectionData : SingletonMono<PuzzleDataSource>
{
    [SerializeField]
    public TextAsset jsonFile;

    [SerializeField]
    private string pathFolderCollectionAsset = "Assets/ScriptableObjects/";
    private string FileAsset = ".asset";
    private void Start()
    {        
        CreateOldCollectionData();
    }
    private void CreateOldCollectionData() {
        Debug.Log($"{jsonFile.text}");

        GetCollectionData myObject = JsonUtility.FromJson<GetCollectionData>(jsonFile.text);
        //list collection
        List<List<string>> collections = new List<List<string>>();
        var previousCollectionName = myObject.baseAbilities[0].CollectionName;
        collections.Add(new List<string>());

        for (int i = 0; i < myObject.baseAbilities.Count; i++)
        {
            if (previousCollectionName != myObject.baseAbilities[i].CollectionName) {
                collections.Add(new List<string>());
                previousCollectionName = myObject.baseAbilities[i].CollectionName;
            }
            collections.Last().Add(myObject.baseAbilities[i].PuzzleName);
        }
        var collectionPuzzlesKeyAndIndex = new SerializableDictionaryBase<string, int>();

        for (int collectionId = 0; collectionId < collections.Count; collectionId++)
        {
            var puzzles = collections[collectionId];
            for (int i = 0; i < puzzles.Count; i++)
            {
                var puzzle = puzzles[i];
                var key = $"{collectionId}_{puzzle}";
                collectionPuzzlesKeyAndIndex.Add(key, i);                
            }
        }

        var path = pathFolderCollectionAsset + "OldCollectionData" + FileAsset;
#if UNITY_EDITOR

                OldCollectionData asset = (OldCollectionData)AssetDatabase.LoadAssetAtPath(path, typeof(OldCollectionData));
                if (asset == null)
                {
                    asset = ScriptableObject.CreateInstance<OldCollectionData>();
                    AssetDatabase.CreateAsset(asset, path);
                }
                asset.collectionPuzzlesKeyAndIndex = collectionPuzzlesKeyAndIndex;
                Selection.activeObject = asset;
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
#endif
    }

}
