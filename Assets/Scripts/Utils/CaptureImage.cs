using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Android;
using DataCore;
using com.F4A.MobileThird;


public class CaptureImage : MonoBehaviour
{

    [SerializeField] private Camera shareCamera;

    private string CACHE_IMAGE_PATH;
    private string DOWNLOAD_IMAGE_PATH;

    private bool _isRequestingPermission;
    private bool _isValidShareImg = false;
    private string _name;

    public void Init()
    {
        CACHE_IMAGE_PATH = Path.Combine(Application.temporaryCachePath, "cachedImg.png");
        DOWNLOAD_IMAGE_PATH = Path.Combine(Application.persistentDataPath, "{0}.png");
        _isValidShareImg = false;
    }

    public void ResetManual()
    {
        _isValidShareImg = false;
    }

    IEnumerator Capture(Action onCompleted)
    {
        GameManager.Instance.ResetScaleSpawnPoint();
        RenderTexture shareObjectTexture = new RenderTexture(900, 1200, 16, RenderTextureFormat.ARGB32);
        //RenderTexture shareObjectTexture = new RenderTexture(1920, 720, 16, RenderTextureFormat.ARGB32);
        if (shareCamera.targetTexture != null)
        {
            shareCamera.targetTexture.Release();
        }

        //GameManager.Instance.Watermark.enabled = true;
        shareCamera.targetTexture = shareObjectTexture;

        shareCamera.enabled = true;
        shareCamera.orthographicSize = GameManager.Instance.ComputeCameraOthorsize();
        shareCamera.transform.position = new Vector3(0, 0, -48); // 0.595
        yield return null;

        RenderTexture curTarget = RenderTexture.active;
        RenderTexture.active = shareObjectTexture;

        shareCamera.Render();

        Texture2D shareTex =
            new Texture2D(shareObjectTexture.width, shareObjectTexture.height, TextureFormat.RGB24, false);
        shareTex.ReadPixels(new Rect(0, 0, shareObjectTexture.width, shareObjectTexture.height), 0, 0);
        shareTex.Apply();

        shareCamera.enabled = false;
        RenderTexture.active = curTarget;

        yield return null;

        File.WriteAllBytes(CACHE_IMAGE_PATH, shareTex.EncodeToPNG());

        Destroy(shareTex);
        shareCamera.targetTexture.Release();
        shareObjectTexture.Release();

        onCompleted?.Invoke();
    }

    public void SharePuzzle(string name)
    {
        _name = name;
        DataCore.Debug.Log("OnShare");

        if (_isValidShareImg)
        {
            Sharing();
        }
        else
        {
            StartCoroutine(Capture(() => { Sharing(); }));
        }

    }


    public void DownLoadPuzzle(string name)
    {
        _name = name;

        if (_isValidShareImg)
        {
            Downloading();
        }
        else
        {
            StartCoroutine(Capture(() =>
            {
                Downloading();
            }));
        }
    }

    private void Sharing()
    {
        DataCore.Debug.Log("Share: " + CACHE_IMAGE_PATH);
        new NativeShare().AddFile(CACHE_IMAGE_PATH).Share();
        Toast.instance.ShowMessage("Shared");
        if (!string.IsNullOrEmpty(_name))
        {
            AnalyticManager.Instance.TrackSharePuzzleEvent(_name);
        }

        GameData.Instance.SavedPack.UpdateChallenge(ChallengeType.SHARE_PUZZLE, 1);
        GameData.Instance.RequestSaveGame();
    }

    private void Downloading()
    {

        NativeGallery.Permission permission = NativeGallery.RequestPermission(NativeGallery.PermissionType.Write);
        
        if (permission == NativeGallery.Permission.Granted) {
            NativeGallery.SaveImageToGallery(CACHE_IMAGE_PATH, Application.productName, _name + ".png",
            (isSuccess, path) =>
            {
                if (isSuccess)
                {
                    DataCore.Debug.Log("Downloaded: " + path);
                    if (!string.IsNullOrEmpty(_name))
                    {
                        AnalyticManager.Instance.TrackDownloadPuzzleEvent(_name);
                    }
                }
                else
                {
                    DataCore.Debug.Log("Fail To Download: " + path);
                }
                Toast.instance.ShowMessage("Downloaded");
            });
        }else {
            UIManager.Instance.ShowPopupNotice("To download photo here, Art Story needs access to your device's storage.\nTo change this settings, go to the Permissions area of Art Story's app info.");
        }         
    }
}
