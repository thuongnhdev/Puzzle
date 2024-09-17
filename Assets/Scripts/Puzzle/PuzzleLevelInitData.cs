using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class PuzzleLevelInitData : MonoBehaviour
{
    [SerializeField] string m_NamePuzzle;
    [SerializeField] private Vector2 m_resolutionOfBG;
    [SerializeField] private List<SpriteRenderer> m_SpriteSource;
    [SerializeField] private List<SpriteRenderer> m_SpriteTarget;
    [SerializeField] private Sprite[] m_Sprites;
    [SerializeField] private SpriteAtlas m_SpriteAtlas;
    [SerializeField] private Dictionary<string, Sprite> m_DSpriteSource;

    public List<SpriteRenderer> SpriteSource { get => m_SpriteSource; set => m_SpriteSource = value; }
    public List<SpriteRenderer> SpriteTarget { get => m_SpriteTarget; set => m_SpriteTarget = value; }

    public Sprite[] Sprites
    {
        get { return m_Sprites; }
        set { m_Sprites = value; }
    }

    public Vector2 ResolutionOfBG { get => m_resolutionOfBG; set => m_resolutionOfBG = value; }

    public string NamePuzzle
    {
        get => m_NamePuzzle;
        set => m_NamePuzzle = value;
    }


    public void Init()
    {
        m_DSpriteSource = new Dictionary<string, Sprite>();
        for (int i = 0; i < m_Sprites.Length; i++)
        {
            m_DSpriteSource.Add(m_Sprites[i].name, m_Sprites[i]);
        }

        for (int i = 0; i < m_SpriteSource.Count; i++)
        {
            m_SpriteSource[i].maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        for (int i = 0; i < m_SpriteTarget.Count; i++)
        {
            m_SpriteTarget[i].maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        if (!m_DSpriteSource.ContainsKey("BG"))
        {
            m_DSpriteSource.Add("BG", transform.GetChild(0).GetComponent<SpriteRenderer>().sprite);
        }

        transform.GetChild(0).GetComponent<SpriteRenderer>().maskInteraction =
            SpriteMaskInteraction.VisibleInsideMask;

        Rect bgRect = m_DSpriteSource["BG"].rect;
        m_resolutionOfBG.x = bgRect.width;
        m_resolutionOfBG.y = bgRect.height;
    }

    public Sprite GetSprite(string key)
    {
        return m_DSpriteSource[key];
    }


}
