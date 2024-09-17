using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;
using DG.Tweening;

namespace LeoScript.ArtBlitz
{
    public class TextureCell : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Texture2D originalTexture;

        [SerializeField] private Vector2Int coordinate;
        public Vector2Int Coordinate { get => coordinate; }

        [SerializeField] private Vector2Int currentCoordinate;
        public Vector2Int CurrentCoordinate { get => currentCoordinate; }
        [SerializeField] private RawImage rawImage;

        public event OnCell_EndDrag OnCell_EndDragEventHandler;
        public delegate void OnCell_EndDrag(TextureCell cell, PointerEventData eventData);

        public event OnCell_Drag OnCell_DragEventHandler;
        public delegate void OnCell_Drag(TextureCell cell, PointerEventData eventData);

        public event OnCell_BeginDrag OnCell_BeginDragEventHandler;
        public delegate void OnCell_BeginDrag(TextureCell cell, PointerEventData eventData);

        [SerializeField] private TMP_Text text;

        [SerializeField] private SkeletonGraphic lightAnimation;
        public void SetColor(Color c)
        {
            rawImage.color = c;
        }

        public Vector2 GetLocalPosition()
        {
            return GetComponent<RectTransform>().anchoredPosition;
        }

        public void Init(Texture2D originalTex , Vector2Int coordinate , Vector2 rectSize, Rect sampleRect)
        {
            this.originalTexture = originalTex;
            this.coordinate = coordinate;

            rawImage.texture = originalTex;

            Vector2 rectPosition = coordinate * rectSize;

            Rect rect = new Rect((rectPosition * sampleRect.size + sampleRect.position ) , rectSize * sampleRect.size);

            rawImage.uvRect = rect;

            rawImage.SetNativeSize();

            RectTransform rectTrans = rawImage.GetComponent<RectTransform>();

            rectTrans.sizeDelta = rectTrans.sizeDelta / sampleRect.size;

            gameObject.name = "cell_" + coordinate.x + "_" + coordinate.y;
           
            GetComponent<RectTransform>().anchoredPosition = rectPosition * new Vector2( rawImage.texture.width, rawImage.texture.height);
            GetComponent<RectTransform>().anchoredPosition = new Vector2((int)GetComponent<RectTransform>().anchoredPosition.x, (int)GetComponent<RectTransform>().anchoredPosition.y);
            text.text = coordinate.ToString();
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            OnCell_BeginDragEventHandler?.Invoke(this, eventData);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            OnCell_DragEventHandler?.Invoke(this, eventData);
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            OnCell_EndDragEventHandler?.Invoke(this, eventData);
        }

        public void ActiveEffect()
        {
            ActiveAnimationHint(true);
            DOVirtual.DelayedCall(2f, () =>
            {
                ActiveAnimationHint(false);
            });
        }

        public void SetXPosition(float X)
        {
            transform.position = new Vector3(X, transform.position.y, transform.position.z);
        }

        public void SetYPosition(float Y)
        {
            transform.position = new Vector3(transform.position.x , Y ,transform.position.z);
        }

        public void ActiveAnimationHint(bool isActive)
        {
            lightAnimation.gameObject.SetActive(isActive);
            if (lightAnimation.skeletonDataAsset != null)
            {
                lightAnimation.Clear();
                lightAnimation.Initialize(true);
                float widthSize = rawImage.rectTransform.sizeDelta.x/4;
                float heightSize = rawImage.rectTransform.sizeDelta.y/4;
                float indexRamdonScale = UnityEngine.Random.Range(0.1f, 0.3f);
                int indexRamdonName = UnityEngine.Random.Range(1, 3);
                float wPos = UnityEngine.Random.Range(-widthSize, widthSize);
                float hPos = UnityEngine.Random.Range(-heightSize, heightSize);
                lightAnimation.transform.localPosition = new Vector3(wPos,hPos,1);
                lightAnimation.transform.localScale = new Vector3(indexRamdonScale, indexRamdonScale, 1);
                lightAnimation.AnimationState.SetAnimation(0, indexRamdonName.ToString(), false);

            }
          
        }
    } 
}