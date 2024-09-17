using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TC_Models : MonoBehaviour
{
   
}
public class PuzzleData
{
    public string SpriteBG; //SpriteObject
    public List<string> SpriteGLs; // List<LayerData>
    public List<string> SpriteOBJs;
    public PuzzleData()
    {
        SpriteBG = "";
        SpriteGLs = new List<string>();
        SpriteOBJs = new List<string>();
    }
}
[System.Serializable]
public class LayerData
{
    public List<string> Sprites; // List<SpriteObject>
    public LayerData()
    {
        Sprites = new List<string>();
    }
}
[System.Serializable]
public class SpriteObject
{
    public string Name;
    public int Layer;
    public int OrderLayer;
    public Vector3 Position;
    public Quaternion Rotate;
    public Vector3 Scale;
}
[System.Serializable]
public class LayerObject
{
    public List<SpriteObject> Sprites;
    public LayerObject()
    {
        Sprites = new List<SpriteObject>();
    }
}
[System.Serializable]
public class PuzzleObject
{
    [SerializeField] private SpriteObject m_SpriteBG;
    [SerializeField] private List<LayerObject> m_SpriteGLs;
    [SerializeField] private List<LayerObject> m_SpriteOBJs;
    public SpriteObject SpriteBG { get => m_SpriteBG; set => m_SpriteBG = value; }
    public List<LayerObject> SpriteGLs { get => m_SpriteGLs; set => m_SpriteGLs = value; }
    public List<LayerObject> SpriteOBJs { get => m_SpriteOBJs; set => m_SpriteOBJs = value; }
    public PuzzleObject()
    {
        m_SpriteBG = new SpriteObject();
        m_SpriteGLs = new List<LayerObject>();
        m_SpriteOBJs = new List<LayerObject>();
    }
    public PuzzleObject GetFromJson(PuzzleData puzzleData)
    {
        m_SpriteBG = new SpriteObject();
        m_SpriteGLs = new List<LayerObject>();
        m_SpriteOBJs = new List<LayerObject>();

        PuzzleObject puzzleObjectTemp = new PuzzleObject();

        JsonUtility.FromJsonOverwrite(puzzleData.SpriteBG, SpriteBG);

        for (int indexLayer = 0; indexLayer < puzzleData.SpriteGLs.Count; indexLayer++)
        {
            LayerObject layerObject = new LayerObject();
            JsonUtility.FromJsonOverwrite(puzzleData.SpriteGLs[indexLayer], layerObject);
            SpriteGLs.Add(layerObject);
        }

        for (int indexLayer = 0; indexLayer < puzzleData.SpriteOBJs.Count; indexLayer++)
        {
            LayerObject layerObject = new LayerObject();
            JsonUtility.FromJsonOverwrite(puzzleData.SpriteOBJs[indexLayer], layerObject);
            SpriteOBJs.Add(layerObject);
        }

        return puzzleObjectTemp;
    }
}
