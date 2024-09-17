using System;
using System.Collections;
using System.Collections.Generic;
using com.F4A.MobileThird;
using DataCore;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    [SerializeField] private UILoading loading;

    [SerializeField] private UIWelcome uIWelcome;

    // Start is called before the first frame update
    void Start()
    {
        InitData();
        ShowLoading();
    }

    private void InitData()
    {
        loading.Init();
        InitCalendarCrashFix();
        GameData.Instance.LoadGameData(GameData.LoadType.LOCAL);        
    }

    public static void InitCalendarCrashFix()
    {
        try
        {
            // Two Letter ISO Language
            string strTwoLetterISOLanguage = System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
            DataCore.Debug.Log($"[CurrentCulture.strTwoLetterISOLanguage] {strTwoLetterISOLanguage}");
            if (strTwoLetterISOLanguage == "ar")
            {
                new System.Globalization.UmAlQuraCalendar();
            }
            else if (strTwoLetterISOLanguage == "th") {
                new System.Globalization.ThaiBuddhistCalendar();
            }
        }
        catch (Exception ex)
        {
            DataCore.Debug.Log($"Failed InitCalendarCrashFix. {ex.Message}");
        }
    }

    private void ShowLoading()
    {
        loading.Open();                
        AssetManager.Instance.Init(progress: (progress) => {
            var value = progress * 0.2f;
            loading.UpdateProgress(value);
        }, completed: () =>
        {
            DataCore.Debug.Log("Start Load Main Scene");
            StartCoroutine(AdsService.Instance.Initialize());
            if (!GameData.Instance.SavedPack.SaveData.IsTutorialCompleted)
            {
                uIWelcome.gameObject.SetActive(true);
                ShowUiWelcome(() =>
                {
                    StartCoroutine(LoadScene());
                });
            }
            else
                StartCoroutine(LoadScene());
        });
    }

    public void ShowUiWelcome(Action onComplete = null)
    {
        uIWelcome.SetData(new object[] { onComplete });
        uIWelcome.Open();
    }
    IEnumerator LoadScene()
    {
        yield return new WaitForEndOfFrame();        
        
        var timer = 0;
        while (timer < 20)
        {
            timer += 1;
            var value = 0.2f + timer * 0.01f;
            loading.UpdateProgress(value);
            //DataCore.Debug.Log($"loading value: {value}", false);
            yield return new WaitForSeconds(0.1f);
        }

        if (Tutorial.IsCompleted)
        {            
            if (FirebaseManager.Instance.FirebaseCheckedDependencies)
            {
                //Check if the device can reach the internet via a carrier data network
                if (SocialManager.Instance.isConnectionNetwork() && FirebaseManager.Instance.FirebaseInitialized && GameData.Instance.IsUserLogin())
                {
                    GameData.Instance.LoadGameData(GameData.LoadType.ONLINE);
                }
            }

            timer = 0;
            while (timer < 20)
            {
                timer += 1;
                var value = 0.4f + timer * 0.01f;
                loading.UpdateProgress(value);
                //DataCore.Debug.Log($"loading value: {value}", false);
                yield return new WaitForSeconds(0.1f);
            }            
        }
        GameData.Instance.MigrateIAPAndPremium();

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("MainV2", LoadSceneMode.Single);
        asyncOperation.allowSceneActivation = false;
        //DataCore.Debug.Log("Pro :" + asyncOperation.progress);
        //When the load is still in progress, output the Text and progress bar
        while (!asyncOperation.isDone)
        {
            var value = asyncOperation.progress * 0.2f + 0.6f;
            loading.UpdateProgress(value);

            if (asyncOperation.progress >= 0.9f)
            {
                loading.UpdateProgress(1f);
                GameSession.Instance.IsCompletedFirstSessionLoading = true;
                yield return new WaitForSeconds(0.1f);
                asyncOperation.allowSceneActivation = true;
                yield break;                
            }

            yield return null;
        }
    }

}
