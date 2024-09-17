using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LeoScript.ArtBlitz
{
    public class ColumnContainer : ContainerBase
    {
        public float GetXPosition()
        {
            return TextureCellList[0].transform.position.x;
        }

        public void SetXPosition(float X)
        {
            foreach (TextureCell cell in TextureCellList)
            {
                cell.SetXPosition(X);
            }
        }

        public override bool IsOutOfCurrentSocket(Vector2 threshold)
        {
            return Mathf.Abs(GetXPosition() - CurrentSocket.GetPosition().x) > threshold.x;
        }

        public override void MoveCells(PointerEventData data ,float movementScale)
        {
            float targetX = TextureCellList[0].transform.position.x + data.delta.x * 1;
            foreach (TextureCell cell in TextureCellList)
            {
                cell.SetXPosition(targetX);
            }
        }

        public override void MoveCellsAuto(Vector2 position, float movementScale)
        {
            float targetX = TextureCellList[0].transform.position.x + position.x * 1;
            foreach (TextureCell cell in TextureCellList)
            {
                cell.SetXPosition(targetX);
            }
        }

        public override void EndMoveCells()
        {
            foreach (TextureCell cell in TextureCellList)
            {
                cell.ActiveEffect();
            }
        }

        public override void MoveToSocketImmediate(ContainerSocket socket)
        {
            StopAllCoroutines();
            foreach (TextureCell cell in TextureCellList)
            {
                cell.SetXPosition(socket.GetPosition().x);
            }
        }

        public override void MoveToCurrentSocket()
        {
            StopAllCoroutines();
            StartCoroutine(MoveCoroutine(CurrentSocket.GetPosition().x));
        }

        private IEnumerator MoveCoroutine(float targetX)
        {
            float progress = 0.0f;
            float startX = GetXPosition();
            while (progress < 1.0f)
            {
                progress += Time.deltaTime * moveSpeed;

                progress = Mathf.Clamp(0, 1, progress);

                foreach (TextureCell cell in TextureCellList)
                {
                    float yThisFrame = Mathf.Lerp(startX, targetX, progress);
                    cell.SetXPosition(yThisFrame);
                }

                Debug.Log(gameObject.name + " moving" + progress);

                yield return new WaitForEndOfFrame();
            }


            MoveToSocketImmediate(CurrentSocket);

        }
    }
}