using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleAnimator", menuName = "PuzzleAnimator", order = 1)]
public class PuzzleAnimator : ScriptableObject
{
    public string ID;
    public Animator animator;
}
