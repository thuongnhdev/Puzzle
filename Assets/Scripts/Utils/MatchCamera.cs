using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchCamera : MonoBehaviour
{
    public float Ratio = 1;

    float lastCameraSize = -1;

    float baseScale;
    private Vector3 difPos;
    private bool isInit = false;
    private float baseCameraScale;
    private Camera target;

    public void Init(Camera targetCamera)
    {
        target = targetCamera;
        baseCameraScale = target.orthographicSize;
        baseScale = transform.localScale.x;
        difPos = target.transform.position - transform.position;
        isInit = true;
    }

    private void LateUpdate()
    {
        if (!isInit)
        {
            return;
        }

        if (target.orthographicSize != lastCameraSize)
        {
            lastCameraSize = target.orthographicSize;
            float newScale = baseScale * (1 + ((lastCameraSize / baseCameraScale) - 1) * Ratio);
            transform.localScale = new Vector3(newScale, newScale, newScale);
          
        }

        transform.position = target.transform.position - difPos*(lastCameraSize/baseCameraScale);
    }
}
