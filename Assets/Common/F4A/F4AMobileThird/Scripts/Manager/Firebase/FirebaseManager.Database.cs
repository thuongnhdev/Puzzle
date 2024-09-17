namespace com.F4A.MobileThird
{
    //#if DEFINE_FIREBASE_DATABASE
    using Firebase.Database;
    using Firebase.Extensions;
    using Firebase;
    using System;
    //#endif
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using DataCore;


    public partial class FirebaseManager
    {
        //#if DEFINE_FIREBASE_DATABASE
        DatabaseReference mDatabaseRef;

        //#endif

        public void Redeem(string redeemCode, Action<string> completed, Action failed = null)
        {
            DataCore.Debug.Log($"Redeem: {redeemCode}");
            
            if (mDatabaseRef == null)
            {
                mDatabaseRef = FirebaseDatabase.DefaultInstance.RootReference;

            }
#if UNITY_IOS || UNITY_ANDROID
            try
            {                
                if (string.IsNullOrEmpty(redeemCode)) {
                    failed?.Invoke();
                    return;
                }
                DataCore.Debug.Log($"Start Redeem: {redeemCode}");
                mDatabaseRef.Child("Redeem").Child("RedeemCode").GetValueAsync().ContinueWith(task =>
                {
                    DataCore.Debug.Log($"Redeem: {redeemCode} IsCompleted: {task.IsCompleted}");

                    if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        object value = snapshot.Child(redeemCode).GetValue(false);
                        if (value != null)
                        {
                            mDatabaseRef.Child("Redeem").Child("RedeemCode").Child(redeemCode).RemoveValueAsync();
                            
                            DataCore.Debug.Log($"Completed Redeem: {redeemCode}");
                            Dispatcher.Instance.Invoke(() =>
                            {
                                completed?.Invoke(value.ToString());
                            });
                        }
                        else
                        {
                            Dispatcher.Instance.Invoke(() =>
                            {
                                failed?.Invoke();
                            });
                        }
                    }
                    else
                    {
                        Dispatcher.Instance.Invoke(() =>
                        {
                            failed?.Invoke();
                        });
                    }
                    mDatabaseRef = null;
                });
            }
            catch (Exception e)
            {
                DataCore.Debug.Log($"Failed to Redeem: {redeemCode}. Error: {e.Message}");

                failed?.Invoke();
            }
#endif
        }

    }
}
