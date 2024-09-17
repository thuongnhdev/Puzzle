using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Android;
using DG.Tweening;
using Spine.Unity;
using DataCore;

public class UIDownloadAndShare : BasePanel
{
    //public static string CACHE_IMAGE_PATH;
    //public static string DOWNLOAD_IMAGE_PATH;

    [SerializeField] private Camera shareCamera;

    [SerializeField] private GameObject btnDownload;

    [SerializeField] private GameObject btnShare;
    //private bool _isValidShareImg = false;
    //private bool _isRequestingPermission;
    private string _name;

    public override void Init()
    {
        //CACHE_IMAGE_PATH = Path.Combine(Application.temporaryCachePath, "cachedImg.png");
        //DOWNLOAD_IMAGE_PATH = Path.Combine(Application.persistentDataPath, "{0}.png");
        //border.enabled = false;
    }

    public override void SetData(object[] data)
    {
        base.SetData(data);
        //_isValidShareImg = false;
        GameManager.Instance.CapturePuzzle.ResetManual();
        if ((data != null) && (data.Length > 0))
        {
            _name = data[0].ToString();
        }
    }

    public override void Open()
    {
        GameManager.Instance.ActiveBorderPuzzleAnimation(true);
        base.Open();
        btnShare.SetActive(true);
        btnDownload.SetActive(true);
        if (GameManager.Instance.GetStepGame() < StepGameConstants.PlayPuzzleOne &&
            GameData.Instance.SavedPack.SaveData.Coin == 0)
        {
            btnShare.SetActive(false);
            btnDownload.SetActive(false);
        }
        UIManager.Instance.EnablePuzzleBorder(true);
        GameManager.Instance.ActivePuzzleAnimation(true);
        //border.enabled = true;
    }

    public override void Close()
    {
        SoundController.Instance.PlaySfxClick();
        GameManager.Instance.EnableMatchCamera(false);
        GameManager.Instance.ActivePuzzleAnimation(false);
        base.Close();
        //border.enabled = false;
    }

    //IEnumerator CaptureImage(Action onCompleted)
    //{
    //    RenderTexture shareObjectTexture = new RenderTexture(900, 1200, 16, RenderTextureFormat.ARGB32);
    //    //RenderTexture shareObjectTexture = new RenderTexture(1920, 720, 16, RenderTextureFormat.ARGB32);
    //    if (shareCamera.targetTexture != null)
    //    {
    //        shareCamera.targetTexture.Release();
    //    }

    //    //GameManager.Instance.Watermark.enabled = true;
    //    shareCamera.targetTexture = shareObjectTexture;

    //    shareCamera.enabled = true;
    //    shareCamera.orthographicSize = GameManager.Instance.ComputeCameraOthorsize();
    //    shareCamera.transform.position = new Vector3(0, 0, -48); // 0.595
    //    yield return null;

    //    RenderTexture curTarget = RenderTexture.active;
    //    RenderTexture.active = shareObjectTexture;

    //    shareCamera.Render();

    //    Texture2D shareTex =
    //        new Texture2D(shareObjectTexture.width, shareObjectTexture.height, TextureFormat.RGB24, false);
    //    shareTex.ReadPixels(new Rect(0, 0, shareObjectTexture.width, shareObjectTexture.height), 0, 0);
    //    shareTex.Apply();

    //    shareCamera.enabled = false;
    //    RenderTexture.active = curTarget;

    //    yield return null;

    //    File.WriteAllBytes(CACHE_IMAGE_PATH, shareTex.EncodeToPNG());

    //    Destroy(shareTex);
    //    shareCamera.targetTexture.Release();
    //    shareObjectTexture.Release();

    //    _isValidShareImg = true;
    //    onCompleted?.Invoke();
    //}

    //    private bool HadPermission()
    //    {
    //#if UNITY_EDITOR
    //        return true;
    //#endif

    //#if UNITY_ANDROID
    //        bool hasReadPer = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead);
    //        bool hasWritePer = Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);

    //        _isRequestingPermission = !hasReadPer || !hasWritePer;

    //        if (!hasReadPer)
    //        {
    //            Permission.RequestUserPermission(Permission.ExternalStorageRead);
    //        }
    //        if (!hasWritePer)
    //        {
    //            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
    //        }

    //        return !_isRequestingPermission;
    //#elif   UNITY_IOS
    //        return true;
    //#endif
    //    }

    public void OnShareTap()
    {
        SoundController.Instance.PlaySfxClick();

        GameManager.Instance.CapturePuzzle.SharePuzzle(_name);
        //DataCore.Debug.Log("OnShare");
        //if (!HadPermission())
        //{
        //    return;
        //}

        //DataCore.Debug.Log("OnShare: " + _isValidShareImg.ToString());
        //if (_isValidShareImg)
        //{
        //    Sharing();
        //}
        //else
        //{
        //    StartCoroutine(CaptureImage(() => { Sharing(); }));
        //}
    }

    public void OnDownLoadTap()
    {
        SoundController.Instance.PlaySfxClick();

        GameManager.Instance.CapturePuzzle.DownLoadPuzzle(_name);
        //if (!HadPermission())
        //{
        //    return;
        //}

        //if (_isValidShareImg)
        //{
        //    Downloading();
        //}
        //else
        //{
        //    StartCoroutine(CaptureImage(() =>
        //    {
        //        Downloading();
        //    }));
        //}
    }

    public void OnContinueTap()
    {
        SoundController.Instance.PlaySfxClick();
        UIManager.Instance.ShowPuzzleCompletedFinalPage();
    }

    //private void Sharing()
    //{
    //    DataCore.Debug.Log("Share: " + CACHE_IMAGE_PATH);
    //    new NativeShare().AddFile(CACHE_IMAGE_PATH).Share();
    //    Toast.instance.ShowMessage("Shared");
    //    SaveGameManager.Instance.SavedPack.UpdateChallenge(ChallengeType.SHARE_PUZZLE, 1);
    //}

    //private void Downloading()
    //{
    //    NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(CACHE_IMAGE_PATH, Application.productName, _name + ".png",
    //        (isSuccess, path) =>
    //        {
    //            if (isSuccess)
    //            {
    //                DataCore.Debug.Log("Downloaded: " + path);
    //            }
    //            else
    //            {
    //                DataCore.Debug.Log("Fail To Download: " + path);
    //            }
    //            Toast.instance.ShowMessage("Downloaded");
    //            NativeGallery.GetImageFromGallery((temp) => { });
    //        });

    //}
}
