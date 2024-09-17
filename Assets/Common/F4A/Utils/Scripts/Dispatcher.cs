using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Dispatcher : MonoBehaviour
{
    private static Dispatcher instance;

    public static Dispatcher Instance
    {
        get
        {
            return instance;
        }
    }

    public void Invoke(Action fn)
    {
        DOVirtual.DelayedCall(0.01f, () =>
        {
            fn?.Invoke();
        });           
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }
}
