using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using DataCore;

public class SpinController : MonoBehaviour
{
    public bool IsSpining = false;

    [System.Serializable]
    public class WheelItem
    {
        [Header("Data")]
        [SerializeField]
        private int index;
        public int Index
        {
            get
            {
                return index;
            }
        }

        [SerializeField]
        private RewardType rewardType;
        public RewardType RewardT
        {
            get
            {
                return rewardType;
            }
        }

        [SerializeField]
        [Tooltip("Percent appear")]
        private int percent;
        public int Percent
        {
            get
            {
                return percent;
            }
        }

        [SerializeField]
        private int rewardAmount;
        public int RewardAmount
        {
            get
            {
                return rewardAmount;
            }
        }

        [Header("Components")]
        [SerializeField]
        private Image image;
        public Image Image
        {
            get
            {
                return image;
            }
        }
        [SerializeField]
        private Image moveImage;
        public Image MoveImage
        {
            get
            {
                return moveImage;
            }
        }

        [SerializeField]
        private TextMeshProUGUI text;
        public TextMeshProUGUI Text
        {
            get
            {
                return text;
            }
        }

        public void Refresh()
        {
            text.SetText(rewardAmount.ToString());
        }
    }

    [Header("Ref")]
    [SerializeField] private Transform rotationRoot;
    [SerializeField] private Transform leftMove;
    [SerializeField] private Transform rightMove;
    [SerializeField] private List<WheelItem> items;

    [Space]
    [Header("Config Data")]
    [SerializeField] private float minAngle = 360.0f;
    [SerializeField] private float maxAngle = 1080.0f;
    [SerializeField] private float rotationSpeed = 50.0f;
    [SerializeField] private float appearDelay = 0.25f;
    [SerializeField] private float itemShowDelay = 0.1f;
    [SerializeField] private float itemScaleAmount = 1.1f;
    [SerializeField] private float itemScaleDuration = 0.25f;
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float angleToProduceSound = 5.0f;

    private Coroutine _spinCoroutine;
    private Action _onRotationComplete;
    private int _totalPercent;

    public void Init()
    {
        InitData();
        StopAllCoroutines();
    }

    public void Reset()
    {
        StopAllCoroutines();
    }

    public void RegisterCompleteEvents(Action onComplete)
    {
        _onRotationComplete += onComplete;
    }

    [ContextMenu("Spin")]
    public void Spin()
    {
        if (IsSpining)
        {
            return;
        }

        if (_spinCoroutine != null)
            StopCoroutine(_spinCoroutine);

        _spinCoroutine = StartCoroutine(SpinWheel());
    }

    private IEnumerator SpinWheel()
    {
        IsSpining = true;

        foreach (var itemToReset in items)
            itemToReset.Image.transform.localScale = Vector3.one;


        int itemIndex = RandomItem();
        float randomAngle = Random.Range(minAngle, maxAngle);
        float anglePerItem = 360 / items.Count;
        var targetAngle = ((int)(randomAngle / 360)) * 360 + itemIndex * anglePerItem + Random.Range(0, anglePerItem - 10) + 5;


        var initialRotation = rotationRoot.rotation;
        var currentValue = 0.0f;
        var angleToTheNextSoundLeft = angleToProduceSound;

        var previousRotation = 0.0f;
        while (currentValue <= 1.0f)
        {
            currentValue += Time.deltaTime * rotationSpeed / targetAngle;
            var currentRotation = spinCurve.Evaluate(currentValue) * targetAngle;
            angleToTheNextSoundLeft -= Mathf.Abs(currentRotation - previousRotation);
            if (angleToTheNextSoundLeft <= 0.0f)
            {
                //rotationSource.Play(); play rotate sound
                angleToTheNextSoundLeft = angleToProduceSound;
            }

            previousRotation = currentRotation;
            rotationRoot.localEulerAngles = currentRotation * Vector3.forward;

            yield return null;
        }

        //rotationSource.Stop();
        var item = items[itemIndex];
        //var currentTimePassed = 0.0f;
        //while (currentTimePassed <= itemScaleDuration)
        //{
        //    item.Image.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * itemScaleAmount, currentTimePassed / itemScaleDuration);
        //    currentTimePassed += Time.deltaTime;

        //    yield return null;
        //}

        switch (item.RewardT)
        {
            case RewardType.HINT:
                GameData.Instance.IncreaseHint(item.RewardAmount, ConfigManager.GameData.ResourceEarnSource.lucky_draw);
                MoveInkHintToBottom(item.MoveImage, item.Image, () =>
                {
                    IsSpining = false;
                    _onRotationComplete?.Invoke();
            
                });
                break;
            case RewardType.INK:
                GameData.Instance.IncreaseInks(item.RewardAmount, ConfigManager.GameData.ResourceEarnSource.lucky_draw);
                MoveInkIconToTop(item.MoveImage, item.Image, () =>
                {
                    IsSpining = false;
                    _onRotationComplete?.Invoke();
                 
                });
                break;
        }

        _spinCoroutine = null;
   
    }

    private void InitData()
    {
        _totalPercent = 0;
        int len = items.Count;
        for (int i = 0; i < len; i++)
        {
            items[i].Refresh();
            _totalPercent += items[i].Percent;
        }
    }

    private int RandomItem()
    {
        int randomPercent = Random.Range(0, _totalPercent + 1);
        int len = items.Count;
        int curPercent = 0;
        for (int i = 0; i < len; i++)
        {
            curPercent += items[i].Percent;
            if (randomPercent <= curPercent)
            {
                return i;
            }
        }

        return 0;
    }

    private void MoveInkIconToTop(Image inkIconMoving, Image inkIcon, Action onComplete)
    {
        inkIconMoving.enabled = true;
        inkIconMoving.transform.localScale = Vector3.one;
        inkIconMoving.transform.position = inkIcon.transform.position;
        inkIconMoving.transform.localEulerAngles = Vector3.zero;
        inkIconMoving.transform.DOScale(Vector3.one * 0.5f, 1.0f);
        inkIconMoving.transform.DOLocalRotate(new Vector3(0, 0, -inkIcon.transform.localEulerAngles.z - rotationRoot.localEulerAngles.z), 0.5f);
        inkIcon.enabled = false;
        UIManager.Instance.ActiveLockTouch(true);
        inkIconMoving.transform.DOMove(ShareUIManager.Instance.InkTopCurrencyIconPos, 1.0f).SetEase(Ease.InBack).OnComplete(() =>
        {
            // inkPanelParent.SetActive(false);
            ShareUIManager.Instance.PlayAnimInkIcon();
            ShareUIManager.Instance.UpdateCurrencyData(1.0f, () =>
            {
                UIManager.Instance.ActiveLockTouch(false);
                inkIconMoving.enabled = false;
                inkIcon.enabled = true;
                onComplete?.Invoke();
            });

        });
    }

    private void MoveInkHintToBottom(Image iconMoving, Image icon, Action onComplete)
    {
        iconMoving.enabled = true;
        iconMoving.transform.position = icon.transform.position;
        iconMoving.transform.localEulerAngles = Vector3.zero;
        iconMoving.transform.DOLocalRotate(new Vector3(0, 0, -icon.transform.localEulerAngles.z - rotationRoot.localEulerAngles.z - 90), 0.5f);

        icon.enabled = false;
        UIManager.Instance.ActiveLockTouch(true);


        Vector3 targetPos;

        if (icon.transform.position.x > Screen.width / 2)
        {
            targetPos = rightMove.position;
        }
        else
        {
            targetPos = leftMove.position;
        }

        iconMoving.transform.DOMove(targetPos, 1.0f).SetEase(Ease.InQuart).OnComplete(() =>
        {
            UIManager.Instance.ActiveLockTouch(false);
            iconMoving.enabled = false;
            icon.enabled = true;
            onComplete?.Invoke();
        });
    }
}

[Serializable]
public enum RewardType
{
    INK, HINT
}