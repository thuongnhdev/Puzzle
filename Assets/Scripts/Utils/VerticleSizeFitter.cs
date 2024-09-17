using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticleSizeFitter : MonoBehaviour
{
    public RectTransform rectTransform;
    public RectTransform children;

    private void Start()
    {
        DataCore.Debug.Log("height: " + children.sizeDelta.y);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, children.sizeDelta.y);
    }

    [ContextMenu("Test")]
    private void Test()
    {
        DataCore.Debug.Log("height: " + children.sizeDelta.y);
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, children.sizeDelta.y);
    }
}
