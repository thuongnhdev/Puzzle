using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NestedScrollParent : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private NestedScrollChildren[] childrens;

    private void Awake()
    {
        for (int i = 0; i < childrens.Length; i++)
        {
            childrens[i].SetParent(scrollRect);
        }
    }
}
