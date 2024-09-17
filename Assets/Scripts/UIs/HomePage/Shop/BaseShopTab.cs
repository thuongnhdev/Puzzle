using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseShopTab : MonoBehaviour
{
    [SerializeField] protected IAPType Type;

    public virtual void OnClick()
    {

    }

    protected virtual void OnPurchaseSuccess()
    {

    }

    protected virtual void OnPurchaseFailed()
    {
        UIManager.Instance.ShowPurchaseFail(0, null);
    }
}
