using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AutoScale : MonoBehaviour
{
    private void Start()
    {
        RectTransform rect = GetComponent<RectTransform>();
        //if (rect.sizeDelta.y > 250)
        //    rect.sizeDelta = new Vector2(rect.sizeDelta.x - (rect.sizeDelta.y - 250), 250);
        while (rect.sizeDelta.y > 250)
        {
            rect.sizeDelta = new Vector2(rect.sizeDelta.x - 1.0f, rect.sizeDelta.y - 1.0f);
        }
    }
}
