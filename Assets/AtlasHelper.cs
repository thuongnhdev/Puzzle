using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class AtlasHelper : MonoBehaviour
{
    [SerializeField] private SpriteAtlas m_SpriteAtlas;
    [SerializeField] private Image m_Img;
    [SerializeField] private string m_NameImgInAtlas;
    private void Start()
    {
        m_Img.sprite = m_SpriteAtlas.GetSprite(m_NameImgInAtlas);
    }
}
