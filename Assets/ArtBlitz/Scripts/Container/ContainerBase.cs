using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LeoScript.ArtBlitz
{
    public abstract class ContainerBase : MonoBehaviour
    {
        [SerializeField] private int index ;
        public int Index { get => index; }

        [SerializeField] private List<TextureCell> textureCellList;
        public List<TextureCell> TextureCellList { get => textureCellList; }

        [SerializeField] private RectTransform cellsRoot;

        [SerializeField] private ContainerSocket currentSocket;
        public ContainerSocket CurrentSocket { get => currentSocket; }

        private bool isHolding = false;
        public bool IsHolding { get => isHolding; }

        protected float moveSpeed = 20;

        public void SetSocket(ContainerSocket socket)
        {
            this.currentSocket = socket;
        }

        public void SetIndex(int index)
        {
            this.index = index;
            if (this is RowContainer)
            {
                gameObject.name = "RowContainer_" + index;
            }
            else if (this is ColumnContainer)
            {
                gameObject.name = "ColumnContainer_" + index;
            }
        
        }

        public abstract bool IsOutOfCurrentSocket(Vector2 threshold);

        public void SetupTransformRelationship(RectTransform parent ,RectTransform cellsRoot)
        {
            this.cellsRoot = cellsRoot;
            transform.SetParent(parent);
        }

        public void AddCell(TextureCell cell)
        {
            textureCellList.Add(cell);
        }

        public abstract void MoveToSocketImmediate(ContainerSocket socket);

        public abstract void MoveToCurrentSocket();     

        public void Hold()
        {
            StopAllCoroutines();
            foreach (TextureCell cell in textureCellList)
            {
                cell.transform.SetParent(transform);
            }

            isHolding = true;
        }

        public void Drop()
        {
            foreach (TextureCell cell in textureCellList)
            {
                cell.transform.SetParent(cellsRoot);
            }

            isHolding = false;
        }

        public abstract void MoveCells(PointerEventData data , float movementScale);

        public abstract void MoveCellsAuto(Vector2 position, float movementScale);

        public abstract void EndMoveCells();

        public void SetColor(Color color)
        {
            foreach (TextureCell cell in textureCellList)
            {
                cell.GetComponent<RawImage>().color = color;
            }
        }
    }
}