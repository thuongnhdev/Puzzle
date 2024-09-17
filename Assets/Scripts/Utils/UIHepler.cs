using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHepler : MonoBehaviour
{
    private void Awake()
    {
        FixedSize();
    }

    void Start()
    {
        //FixedSize();
    }


    public void FixedSize()
    {
        if (Camera.main.aspect < 0.5625f)
        {
            transform.localScale = Vector3.one * (Camera.main.aspect / 0.5625f);
        }
    }
}
