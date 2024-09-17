using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.U2D;

public class TC_ReadPuzzleJson : MonoBehaviour
{
    [SerializeField] string m_NamePuzzle;
  //  [SerializeField] private PuzzleObject m_PuzzleObject;
    [SerializeField] private List<SpriteRenderer> m_SpriteSource;
    [SerializeField] private List<SpriteRenderer> m_SpriteTarget;
    [SerializeField] private Sprite[] m_Sprites;
    [SerializeField] private SpriteAtlas m_SpriteAtlas;
    [SerializeField] private Dictionary<string, Sprite> m_DSpriteSource;

    public Vector2 ResolutionOfBG
    {
        get => m_resolutionOfBG;
    }
    public List<SpriteRenderer> SpriteSource { get => m_SpriteSource; set => m_SpriteSource = value; }
    public List<SpriteRenderer> SpriteTarget { get => m_SpriteTarget; set => m_SpriteTarget = value; }

    private Vector2 m_resolutionOfBG;

    public void Init()
    {
        m_DSpriteSource = new Dictionary<string, Sprite>();
        for (int i = 0; i < m_Sprites.Length; i++)
        {
            m_DSpriteSource.Add(m_Sprites[i].name, m_Sprites[i]);
        }

        Rect bgRect = m_DSpriteSource["BG"].rect;
        m_resolutionOfBG.x = bgRect.width;
        m_resolutionOfBG.y = bgRect.height;
    }

    public Sprite GetSprite(string key)
    {
        return m_DSpriteSource[key];
    }
}
