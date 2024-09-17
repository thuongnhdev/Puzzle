using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlBase
{
    const float Epsilon = 0.1f;
    protected float smoothTime = 0.5f;
    protected float autoZoomSpeed = 0.1f;

    public float MaxZoomIn { get { return maxZoomIn; } }
    public float MaxZoomOut { get { return maxZoomOut; } }

    public Action<float> OnCameraChangeSize;
    public Action<Vector3> OnCameraChangePosition;

    [SerializeField] protected Camera targetCamera;
    [SerializeField] protected Transform transform;
    protected float targetOrthoSize;

    protected bool isAutoZooming;
    protected Rect camMoveBound;
    protected Rect camViewBound;
    protected float maxZoomIn;
    protected float maxZoomOut;
    protected float moveRatio;
    protected float smoothVecX;
    protected float smoothVecY;
    protected float cameraZ;

    protected float vecSmoothSpeed = 2.45f;
    protected float vecAutozoomSmoothSpeed = 2.45f * 5;
    Vector2 vecPos;
    protected Vector2? targetPos;
    protected Vector2 smoothPosVec;

    float wRatio;
    float hRatio;

    public virtual void Active()
    {
        smoothVecX = 0;
        smoothVecY = 0;
    }

    public virtual void Deactive()
    {
    }

    public virtual void Update(ref float smoothOrthoVec)
    {
        if (targetCamera.orthographicSize != targetOrthoSize || isAutoZooming)
        {
            targetCamera.orthographicSize = Mathf.SmoothDamp(targetCamera.orthographicSize,
                targetOrthoSize, ref smoothOrthoVec, isAutoZooming ? autoZoomSpeed : smoothTime);
            OnCameraChangeSize?.Invoke(targetCamera.orthographicSize);

            if (Mathf.Abs(targetOrthoSize - targetCamera.orthographicSize) <= 0.1f)
            {
                UpdateOrthoSize(targetOrthoSize);
                isAutoZooming = false;
            }
        }

        //position
        if (targetPos != null)
        {
            Vector3 newPos;
            float smoothTime = 1.0f / (isAutoZooming ? vecAutozoomSmoothSpeed : vecSmoothSpeed);
            newPos.x = Mathf.SmoothDamp(transform.position.x, targetPos.Value.x, ref smoothPosVec.x, smoothTime);
            newPos.y = Mathf.SmoothDamp(transform.position.y, targetPos.Value.y, ref smoothPosVec.y, smoothTime);
            newPos.z = cameraZ;

            float dif = (targetPos.Value - (Vector2)newPos).magnitude;

            if (Mathf.Abs(dif) < 0.02f)
            {
                newPos.x = targetPos.Value.x;
                newPos.y = targetPos.Value.y;
                targetPos = null;
            }

            transform.position = newPos;
            OnCameraChangePosition?.Invoke(transform.position);
        }
    }

    protected virtual void OnInputMove(Vector2 vec)
    {
        if (isAutoZooming)
        {
            return;
        }

        //calculate world
        Vector3 worldMove;
        worldMove.x = vec.x * wRatio;
        worldMove.y = vec.y * hRatio;
        worldMove.z = cameraZ;

        worldMove = targetCamera.transform.position - new Vector3(worldMove.x, worldMove.y, 0);

        targetCamera.transform.position = ClaimPosition(worldMove);
        OnCameraChangePosition?.Invoke(transform.position);
        if (targetPos != null)
        {
            targetPos = null;
        }
    }

    void ReCalculateScreenRatio()
    {
        wRatio = (targetCamera.aspect * targetCamera.orthographicSize * 2) / Screen.width;
        hRatio = (targetCamera.orthographicSize * 2) / Screen.height;
    }

    public virtual void OnInputScale(float scale, Vector2 focus)
    {
        if (targetPos == null && !isAutoZooming)
        {
            scale = -(scale - 1) + 1;
            float newOrthoSize = Mathf.Clamp(targetCamera.orthographicSize * scale, maxZoomIn, maxZoomOut);
            if (newOrthoSize != targetCamera.orthographicSize)
            {
                UpdateOrthoSize(newOrthoSize, true);
            }

            //pos
            Vector3 offsetToMid;

            offsetToMid = targetCamera.ScreenToWorldPoint(focus);

            offsetToMid.x = (targetCamera.transform.position.x - offsetToMid.x);
            offsetToMid.y = (targetCamera.transform.position.y - offsetToMid.y);
            offsetToMid.z = 0;
            offsetToMid *= scale - 1;

            targetCamera.transform.position = ClaimPosition(targetCamera.transform.position + offsetToMid);
            OnCameraChangePosition?.Invoke(targetCamera.transform.position);
        }
    }

    protected virtual void UpdateOrthoSize(float size, bool force = false)
    {
        targetOrthoSize = size;

        camMoveBound.xMin = size * targetCamera.aspect + camViewBound.xMin;
        camMoveBound.xMax = camViewBound.xMax - size * targetCamera.aspect;

        if (camMoveBound.xMin > camMoveBound.xMax)
        {
            camMoveBound.xMin = camMoveBound.xMax = (camViewBound.xMin + camViewBound.xMax) / 2.0f;
        }

        camMoveBound.yMin = size + camViewBound.yMin;
        camMoveBound.yMax = camViewBound.yMax - size;

        if (camMoveBound.yMin > camMoveBound.yMax)
        {
            camMoveBound.yMin = camMoveBound.yMax = (camViewBound.yMin + camViewBound.yMax) / 2.0f;
        }

        camMoveBound.xMin = NearZero(camMoveBound.xMin);
        camMoveBound.xMax = NearZero(camMoveBound.xMax);
        camMoveBound.yMin = NearZero(camMoveBound.yMin);
        camMoveBound.yMax = NearZero(camMoveBound.yMax);

        UpdateMoveRatio();

        if (force)
        {
            targetCamera.orthographicSize = targetOrthoSize;
            OnCameraChangeSize?.Invoke(targetOrthoSize);
        }

        ReCalculateScreenRatio();
    }

    protected virtual void UpdateMoveRatio()
    {
        if ((camMoveBound.width == 0) || (camMoveBound.height == 0))
        {
            if ((camMoveBound.width == 0) && (camMoveBound.height != 0))
            {
                moveRatio = camMoveBound.height / camViewBound.height;
            }
            else if ((camMoveBound.width != 0) && (camMoveBound.height == 0))
            {
                moveRatio = camMoveBound.width / camViewBound.width;
            }
            else
            {
                moveRatio = 0;
            }
        }
        else
        {
            moveRatio = (camMoveBound.width * camMoveBound.height) / (camViewBound.width * camViewBound.height);
        }
    }

    protected float NearZero(float value)
    {
        if (Mathf.Abs(value) < Epsilon)
        {
            return 0;
        }

        return value;
    }

    protected Vector3 ClaimPosition(Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, camMoveBound.xMin, camMoveBound.xMax);
        pos.y = Mathf.Clamp(pos.y, camMoveBound.yMin, camMoveBound.yMax);

        return pos;
    }

    public void SetTargetCameraSize(float size, bool force = false)
    {
        UpdateOrthoSize(size, force);
    }

    public void SetTargetPos(Vector2 pos)
    {
        targetPos = pos;
    }
}
