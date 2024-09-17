using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LeoScript.ArtBlitz
{
    public class GameGrid : MonoBehaviour
    {
        [SerializeField] private List<RowContainer> rowList;
        public List<RowContainer> RowList { get => rowList; }

        [SerializeField] private List<ColumnContainer> columnList;
        public List<ColumnContainer> ColumnList { get => columnList; }

        [SerializeField] private List<TextureCell> cellsList;
        public List<TextureCell> CellsList { get => cellsList; }

        [SerializeField] private RectTransform cellsRoot;

        [SerializeField] private RectTransform rowsRoot;
        [SerializeField] private RectTransform columnsRoot;

        [SerializeField] private SocketsHelper socketsHelper;

        [SerializeField] private Vector2 cellSize;
        public Vector2 CellSize { get => cellSize; }

        [SerializeField] private List<ContainerBase> holdingContainerList;

        [SerializeField] private DraggingState draggingState;

        [SerializeField] private ContainersGroupHelper containersGroupHelper;

        [SerializeField] private ShuffleHelper shuffleHelper;

        public OnProgressUpdated OnProgressUpdatedEventHandler;
        public delegate void OnProgressUpdated(float progress, List<ContainerBase> holdingContainerList);

        private bool isLocked = false;
        [SerializeField] private Color lockColor;

        public enum DraggingState
        {
            Nothing,
            Row,
            Column
        }

        public void SetupGameGrid(List<TextureCell> list, List<RowContainer> rowList, List<ColumnContainer> columnList)
        {
            containersGroupHelper.OnProgressUpdatedEventHandler += GroupsHelper_OnProgressUpdated;

            this.cellsList = list;
            this.rowList = rowList;
            this.columnList = columnList;

            foreach (TextureCell cell in list)
            {
                cell.transform.SetParent(cellsRoot);
                cell.transform.localScale = Vector3.one;

                cell.OnCell_BeginDragEventHandler += Cell_OnCell_BeginDragEventHandler;
                cell.OnCell_DragEventHandler += Cell_OnCell_DragEventHandler;
                cell.OnCell_EndDragEventHandler += Cell_OnCell_EndDragEventHandler;
            }

            foreach (RowContainer row in rowList)
            {
                row.SetupTransformRelationship(rowsRoot, cellsRoot);
            }

            foreach (ColumnContainer column in columnList)
            {
                column.SetupTransformRelationship(columnsRoot, cellsRoot);
            }

            cellSize = list[0].GetComponent<RectTransform>().rect.size;
            socketsHelper.CreateSockets(rowList, columnList);
        }

        private void GroupsHelper_OnProgressUpdated(float progress, List<ContainerBase> holdingContainerList)
        {
            OnProgressUpdatedEventHandler?.Invoke(progress, holdingContainerList);
        }

        public void UpdateScaleRoot(RectTransform trans)
        {
            socketsHelper.UpdateScaleRoot(trans);
        }

        public void Shuffle()
        {
            float progress = 1.0f;
            int max = 50;
            int count = 0;
            while (progress != 0.0f && count < max)
            {
                count++;
                shuffleHelper.Shuffle(50, rowList, columnList);

                containersGroupHelper.UpdateGroupStates();
                progress = containersGroupHelper.GetCurrentProgress();

                Debug.Log("shufflnig " + count + " : " + progress);
            }
        }

        private void Cell_OnCell_BeginDragEventHandler(TextureCell cell, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (isLocked) { return; }
            bool isMovingRow = Mathf.Abs(eventData.delta.x) < Mathf.Abs(eventData.delta.y);

            ContainerBase container;
            ContainersGroup group;
            
            if (isMovingRow)
            {
                draggingState = DraggingState.Row;
                container = GetRowForTextureCell(cell);
            }
            else
            {
                draggingState = DraggingState.Column;
                container = GetColumnForTextureCell(cell);
            }

            group = containersGroupHelper.GetGroupForContainer(container);

            if (group != null)
            {
                holdingContainerList.AddRange(group.ContainerList);
            }
            else
            {
                holdingContainerList.Add(container);
            }

            foreach (ContainerBase c in holdingContainerList)
            {
                c.Hold();
            }
        }

        private void Cell_OnCell_DragEventHandler(TextureCell cell, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (Input.touchCount > 1) return;
            if (socketsHelper == null) return;
            if (holdingContainerList == null) return;
            if ((cell.transform.position.x) < 0 || cell.transform.position.x > (Screen.width)) return;
            if (cell.transform.position.y < 100 || cell.transform.position.y > (Screen.height-100)) return;
            if (holdingContainerList.Count > 0)
            {
                foreach (ContainerBase container in holdingContainerList)
                {
                    container.MoveCells(eventData, GetHierarchyScale());
                }
                socketsHelper.UpdateSocket(holdingContainerList);
            }
        }

        private void Cell_OnCell_EndDragEventHandler(TextureCell cell, UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (holdingContainerList.Count == 0) return;
            if (holdingContainerList.Count > 0)
            {
                foreach (ContainerBase container in holdingContainerList)
                {
                    DropContainer(container);
                }

                containersGroupHelper.UpdateGroupStates(holdingContainerList);
            }
            holdingContainerList.Clear();

            draggingState = DraggingState.Nothing;
        }

        private void DropContainer(ContainerBase container)
        {
            socketsHelper.MoveContainerToCurrentSocket(container);
            container.Drop();
        }

        private RowContainer GetRowForTextureCell(TextureCell cell)
        {
            return rowList.Find(t => t.TextureCellList.Contains(cell));
        }

        private ColumnContainer GetColumnForTextureCell(TextureCell cell)
        {
            return columnList.Find(t => t.TextureCellList.Contains(cell));
        }

        private float GetHierarchyScale()
        {
            return GetComponentInParent<Canvas>().scaleFactor;
        }

        public void Lock()
        {
            isLocked = true;
            foreach (TextureCell cell in cellsList)
            {
                cell.SetColor(lockColor);
            }
        }

        public void Unlock()
        {
            isLocked = false;
            foreach (TextureCell cell in cellsList)
            {
                cell.SetColor(Color.white);
            }
            transform.SetAsLastSibling();
        }

        public void Complete()
        {
            isLocked = true;
        }
    }
}