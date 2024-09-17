using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace com.F4A.MobileThird
{
    [AddComponentMenu("F4A/IAPButton")]
    public class IAPButton : MonoBehaviour
    {
#pragma warning disable 414

        [SerializeField]
        private string productIdAndroid = "", productIdIOS;


        public string GetProductId()
        {
#if UNITY_ANDROID
            return productIdAndroid;
#elif UNITY_IOS
            return productIdIOS;
#else
            return "";
#endif
        }

        [SerializeField]
        private TextMeshProUGUI textPrices = null;

        [SerializeField]
        private TextMeshProUGUI tmpValuePackage = null;


        [SerializeField]
        private Button btnBuy = null;

        private IAPProductInfo productInfo = null;

        protected virtual void Start()
        {
            productIdAndroid = productIdAndroid.ToLower();
            productIdIOS = productIdIOS.ToLower();

            if (!btnBuy && transform.Find("Buy")) btnBuy = transform.Find("Buy").GetComponent<Button>();
            if (btnBuy)
            {
                btnBuy.onClick.RemoveAllListeners();
                btnBuy.onClick.AddListener(HandleBtnBuy_Click);
            }

            DOVirtual.DelayedCall(0.1f, () =>
            {
                Init();
            });
        }



        /// <summary>
        /// 
        /// </summary>
        public virtual void Init()
        {
#if DEFINE_IAP
            if (!string.IsNullOrEmpty(GetProductId()))
            {
                productInfo = IAPManager.Instance.GetProductInfoById(GetProductId());

                if (productInfo != null)
                {
                    textPrices.text = IAPManager.Instance.GetProductPriceStringById(productInfo.Id);
                    if (tmpValuePackage != null)
                        tmpValuePackage.text = "x" + productInfo.Coin.ToString();
#if UNITY_ANDROID
                    productIdAndroid = productInfo.Id;
#elif UNITY_IOS
                    productIdIOS = productInfo.Id;
#endif
                }
            }
            else
            {

            }
#endif
        }

        /// <summary>
        /// Handles the button buy click.
        /// </summary>
        private void HandleBtnBuy_Click()
        {
            SoundController.Instance.PlaySfxClick();
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                UIManager.Instance.ShowPopupNoInternet();
                return;
            }
#if DEFINE_IAP
            if (productInfo != null)
            {
                IAPManager.Instance.BuyProductByID(productInfo.Id);
            }
            else
            {
                UIManager.Instance.ShowPurchaseFail(1, () => { });
            }
#endif
        }
    }
}
