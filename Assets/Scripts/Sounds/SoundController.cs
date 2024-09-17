using DataCore;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using com.F4A.MobileThird;
using System;
using DataCore;

public class SoundController : SingletonMonoAwake<SoundController>
{
    [SerializeField]
    private AudioSource bgMusicAudioSource;
    [SerializeField]
    private AudioClip[] musics;
    [SerializeField]
    private AudioClip[] effects;

    [SerializeField]
    private AudioSource audioClick;
    [SerializeField]
    private List<string> listMusicActive = new List<string>();

    public List<string> ListMusicActive { get => listMusicActive; set => listMusicActive = value; }

    private void OnEnable()
    {
        GameData.OnMusicMute += OnMusicMute;
    }

    private void OnDisable()
    {
        GameData.OnMusicMute -= OnMusicMute;
    }

    private void OnMusicMute(bool isMute)
    {
        if (!isMute)
            StopBGMusicEffect();
        else
            PlayMainBackgroundMusic();
    }

    public void PlayMainBackgroundMusic()
    {
        if (!GameData.Instance.SavedPack.SaveData.IsMusicActive)
        {
            return;
        }
        string mainBackgroundMusicPath = "Assets/Bundles/Music/Art_Story_Theme.mp3";
        AssetManager.Instance.LoadPathAsync<AudioClip>(mainBackgroundMusicPath, (bgMusic) =>
        {
            if (bgMusic != null)
            {
                PlayBGMusic(bgMusic, mainBackgroundMusicPath, 0.7f);
            }
        });
    }

    public void PlaySfxWin()
    {
        if (!GameData.Instance.SavedPack.SaveData.IsSoundActive)
        {
            return;
        }

        GameObject go = new GameObject("SfxWin");
        go.transform.parent = transform;
        AudioSource audio = go.AddComponent<AudioSource>();
        audio.clip = MCache.Instance.SfxWin;
        audio.Play();
        Destroy(go.gameObject, 3);
    }

    public void PlaySfxClick()
    {
        if (!GameData.Instance.SavedPack.SaveData.IsSoundActive)
        {
            return;
        }
        audioClick.volume = 0.7f;
        audioClick.clip = effects[0];
        audioClick.Play();
    }

    public void PlaySfxClickTing()
    {
        if (!GameData.Instance.SavedPack.SaveData.IsSoundActive)
        {
            return;
        }
        audioClick.volume = 1.0f;
        audioClick.clip = effects[1];
        audioClick.Play();
    }

    public void StopBGMusic()
    {
        StopBGMusicEffect();
    }

    private void StopBGMusicEffect(Action onComplete = null)
    {
        DataCore.Debug.Log("StopBGMusic");
        if (bgMusicAudioSource.isPlaying)
        {
            DOVirtual.Float(1f, 0.0f, 2f, (value) =>
            {
                bgMusicAudioSource.volume = value;
            });

            bgMusicAudioSource.DOFade(0, 2f).OnComplete(() =>
             {
                 bgMusicAudioSource.Stop();
                 bgMusicAudioSource.volume = 1;
                 if (!String.IsNullOrEmpty(playingBackgroundMusicName)) {
                     playingBackgroundMusicName = string.Empty;
                     AssetManager.Instance.ReleasePath(playingBackgroundMusicName);
                 }                 
                 onComplete?.Invoke();
             });
        }
        else
            onComplete?.Invoke();
    }

    public void MuteBgMusic(bool isMute)
    {
        bgMusicAudioSource.volume = isMute == true ? 0 : 1;
    }

    private string playingBackgroundMusicName = string.Empty;
    
    public void PlayBGMusic(AudioClip clip, string path ,float volume = 1)
    {
        
        if (!GameData.Instance.SavedPack.SaveData.IsMusicActive)
        {
            bgMusicAudioSource.clip = clip;
            return;
        }
        if (clip == null) return;
        if (!string.IsNullOrEmpty(playingBackgroundMusicName)) {
            DataCore.Debug.Log($"playingBackgroundMusicName: {path} clip.name: {path}", false);
            if (playingBackgroundMusicName == clip.name) return;
        }

        StopBGMusicEffect(() =>
        {
            playingBackgroundMusicName = path;
            bgMusicAudioSource.Stop();
            bgMusicAudioSource.clip = clip;
            bgMusicAudioSource.loop = true;

            bgMusicAudioSource.volume = 0;
            bgMusicAudioSource.Play();
            
            DOVirtual.Float(0.0f, volume, 5f, (value) =>
            {
                bgMusicAudioSource.volume = value;
            });
        });
    }

    public void PlayBgMusicAgain()
    {
        bgMusicAudioSource.volume = 1;
        bgMusicAudioSource.Play();
        bgMusicAudioSource.loop = true;
    }
    public void PlayVibrate()
    {
        if (!GameData.Instance.SavedPack.SaveData.IsHapticActive)
        {
            return;
        }

        DataCore.Debug.Log("Play Vibrate", false);

        // Handheld.Vibrate();
        Taptic.Light();
    }

    //public void PlayMusicBG(EMusicBGName eMusic, bool loop = true)
    //{
    //    bool onMusic = (PlayerPrefs.GetInt(ConfigVariables.SETTING_MUSIC, 1) > 0) ? true : false;
    //    if (onMusic == false) return;
    //    //if (id >= musics.Length)
    //    //    id = musics.Length - 1;
    //    DataCore.Debug.Log("PlayMusicBG");
    //    if (listMusicActive.Contains(eMusic.ToString())) return;
    //    DataCore.Debug.Log("PlayMusicBG");
    //    listMusicActive.Add(eMusic.ToString());
    //    GameObject go = new GameObject(MCache.Instance.MusicBG[eMusic.ToString()].name);
    //    go.transform.parent = transform;
    //    audioBG = go.AddComponent<AudioSource>();
    //    audioBG.clip = MCache.Instance.MusicBG[eMusic.ToString()];
    //    audioBG.volume = 1;
    //    audioBG.loop = loop;
    //    audioBG.Play();
    //}
    //public void PlayEffect(EIDSoundEffect eID, float timeSound = -1, float vol = 1)
    //{
    //    //bool onSound = (PlayerPrefs.GetInt(ConfigVariables.SETTING_SOUND, 1) > 0) ? true : false;
    //    if (onSound == false) return;
    //    GameObject go = new GameObject(eID.ToString());
    //    go.transform.parent = transform;
    //    AudioSource audio = go.AddComponent<AudioSource>();
    //    //DataCore.Debug.Log("PlayEffect =>>" + eID.ToString());
    //    audio.clip = MCache.Instance.Sfx[eID.ToString()];
    //    audio.volume = vol != 1 ? vol : ConfigVariables.SFX_VOLUMN;
    //    audio.Play();
    //    DOVirtual.DelayedCall(timeSound <= 0 ? ConfigVariables.TIME_AUTO_DESTROY_SOUND_AFFECT : timeSound, () =>
    //    {
    //        DOVirtual.Float(audio.volume, 0, timeSound <= 0 ? ConfigVariables.TIME_AUTO_DESTROY_SOUND_AFFECT : timeSound, (x) =>
    //        {
    //            audio.volume = x;
    //        }).OnComplete(() =>
    //        {
    //            Destroy(go);
    //        }).SetEase(Ease.Linear).SetId(this);
    //    }).SetId(this).SetEase(Ease.Linear);
    //}
    //public void StopMusicBG()
    //{
    //    if (audioBG == null) return;

    //    DOVirtual.Float(audioBG.volume, 0, ConfigVariables.TIME_DELAY_OFF_SOUND, (x) =>
    //    {
    //        audioBG.volume = x;
    //    }).OnComplete(() =>
    //    {
    //        listMusicActive.Clear();
    //        Destroy(audioBG.gameObject);
    //        DOTween.Kill(this);
    //    }).SetId(this);
    //}
    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
