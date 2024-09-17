using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataCore;

public class MCache : MonoBehaviour
{
    public static MCache Instance;
    [SerializeField] ConfigVariables config;
    [SerializeField] FxFillController fxFill;
    [SerializeField] HandController m_HandController;
    [SerializeField] Sprite[] arrSprite;
    [SerializeField] AudioClip sfxWin;

    public ConfigVariables Config { get => config; set => config = value; }
    public FxFillController FxFill { get => fxFill; set => fxFill = value; }
    public Sprite[] ArrSprite { get => arrSprite; set => arrSprite = value; }
    public AudioClip SfxWin { get => sfxWin; set => sfxWin = value; }
    public AudioClip SfxClick { get => sfxWin; set => sfxWin = value; }
    public HandController Hand { get => m_HandController; set => m_HandController = value; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
