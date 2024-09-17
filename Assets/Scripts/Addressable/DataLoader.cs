using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

public abstract class DataLoader : IEnumerator
{
    public float progress { get; set; } //progress of this dataloader
    public string Error { get; set; }
    public string LoaderName { get; set; }
    public long fileSize { get; set; }


    #region IEnumerator implementation

    public bool MoveNext()
    {
        return !IsDone();
    }

    public void Reset()
    {

    }

    public object Current
    {
        get
        {
            return null;
        }
    }

    public abstract bool IsDone();
    public abstract bool Update();

    public virtual void Release()
    {

    }

    #endregion
} 