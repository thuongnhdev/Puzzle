using System;
using System.Collections;
using System.Collections.Generic;
using com.F4A.MobileThird;

#if UNITY_ANDROID
using Google.Play.Review;

using UnityEngine;

namespace com.F4A.MobileThird
{
    public partial class GoogleRateService : IRateService
    {
        private bool _isInit = false;
        private ReviewManager _reviewManager;


        public void Initialize()
        {
            _isInit = false;

            InitialReviewManager();
        }

        private void InitialReviewManager()
        {
#if !UNITY_EDITOR
            _reviewManager = new ReviewManager();
            _isInit = true;
#endif
        }

        public bool IsInitialized()
        {
            return _isInit;
        }



        public void ShowAppstore()
        {
            SocialManager.Instance.OpenRateGame();
        }

        public void ShowReviewInApp(bool forced = false)
        {
#if !UNITY_EDITOR
            RequestPlayReviewInfo(forced);
#endif
        }

        private void RequestPlayReviewInfo(bool forced)
        {
            DataCore.Debug.Log($"RequestPlayReviewInfo {forced}");
            var requestFlowOperation = _reviewManager.RequestReviewFlow();
            if (forced)
            {
                ShowAppstore();
            }
            else {
                requestFlowOperation.Completed += (Google.Play.Common.PlayAsyncOperation<PlayReviewInfo, ReviewErrorCode> obj) => {
                    if (obj.Error != ReviewErrorCode.NoError)
                    {
                        if (forced)
                        {
                            ShowAppstore();
                        }
                        DataCore.Debug.Log($"Failed RequestPlayReviewInfo. Error: {requestFlowOperation.Error}");
                    }
                    else
                    {
                        var playReviewInfo = obj.GetResult();
                        DataCore.Debug.Log($"RequestReviewFlow {playReviewInfo}");
                        var launchFlowOperation = _reviewManager.LaunchReviewFlow(playReviewInfo);
                        launchFlowOperation.Completed += (Google.Play.Common.PlayAsyncOperation<Google.Play.Common.VoidResult, ReviewErrorCode> obj) => {
                            if (obj.Error != ReviewErrorCode.NoError)
                            {
                                if (forced)
                                {
                                    ShowAppstore();
                                }
                                DataCore.Debug.Log($"Failed RequestPlayReviewInfo. Error: {launchFlowOperation.Error}");
                            }
                            else
                            {
                                DataCore.Debug.Log($"LaunchReviewFlow {obj.Error}");
                            }

                        };
                    }
                };
            }

        }        
    }
}
#endif