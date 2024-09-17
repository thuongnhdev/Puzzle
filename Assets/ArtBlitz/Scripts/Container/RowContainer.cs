using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LeoScript.ArtBlitz
{
    public class RowContainer : ContainerBase
    {
        public float GetYPosition()
        {
            return TextureCellList[0].transform.position.y;
        }

        public void SetYPosition(float y)
        {
            foreach (TextureCell cell in TextureCellList)
            {
                cell.SetYPosition(y);
            }
        }

        public override bool IsOutOfCurrentSocket(Vector2 threshold)
        {
            return Mathf.Abs(GetYPosition() - CurrentSocket.GetPosition().y) > threshold.y;
        }

        public override void MoveCells(PointerEventData data, float movementScale)
        {
            float targetY = TextureCellList[0].transform.position.y + data.delta.y * 1;
            foreach (TextureCell cell in TextureCellList)
            {
                cell.SetYPosition(targetY);
            }
        }

        public override void MoveCellsAuto(Vector2 position, float movementScale)
        {
            float targetY = TextureCellList[0].transform.position.y + position.y * 1;
            foreach (TextureCell cell in TextureCellList)
            {
                cell.SetYPosition(targetY);
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
                cell.SetYPosition(socket.GetPosition().y);
            }
        }

        public override void MoveToCurrentSocket()
        {
            StopAllCoroutines();
            StartCoroutine(MoveCoroutine(CurrentSocket.GetPosition().y));
        }

        private IEnumerator MoveCoroutine(float targetY)
        {
            float progress = 0.0f;
            float startY = GetYPosition();
            while (progress < 1.0f)
            {
                progress += Time.deltaTime * moveSpeed;

                progress = Mathf.Clamp(0, 1, progress);

                foreach (TextureCell cell in TextureCellList)
                {
                    float yThisFrame = Mathf.Lerp(startY, targetY, progress);
                    cell.SetYPosition(yThisFrame);
                }

                yield return new WaitForEndOfFrame();
            }

            MoveToSocketImmediate(CurrentSocket);            
        }
    }
}