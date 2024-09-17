#if UNITY_IOS
using System;
using System.Collections;
using System.Collections.Generic;
using com.F4A.MobileThird;
using UnityEngine.iOS;
using UnityEngine;
using DataCore;

namespace com.F4A.MobileThird
{
    public partial class AppleRateService : IRateService
    {
        private bool _isInit = false;


        public void Initialize()
        {
            _isInit = true;
        }

        public void ShowAppstore()
        {
            SocialManager.Instance.OpenRateGame();
        }

        public void ShowReviewInApp(bool forced = false)
        {
            try
            {
                DataCore.Debug.Log("ShowReviewInApp");
                bool didShowInAppReview = Device.RequestStoreReview();
                if (!didShowInAppReview && forced)
                {
                    ShowAppstore();
                }
            }
            catch (Exception ex)
            {
               DataCore.Debug.Log($"Failed to ShowReviewInApp. Error: {ex.Message}");
            }
        }
    }
}
#endif
