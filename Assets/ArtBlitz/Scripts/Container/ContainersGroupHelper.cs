using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeoScript.ArtBlitz
{
    public class ContainersGroupHelper : MonoBehaviour
    {
        [SerializeField] private GameObject containerGroupPrefab;
        [SerializeField] private List<ContainersGroup> createdGroupList;
        public List<ContainersGroup> CreateGroupList { get => createdGroupList; }

        [SerializeField] private RectTransform groupsRoot;
        [SerializeField] private GameGrid gameGrid;

        public OnProgressUpdated OnProgressUpdatedEventHandler;
        public delegate void OnProgressUpdated(float progress, List<ContainerBase> holdingContainerList);

        public void UpdateGroupStates(List<ContainerBase> holdingContainerList = null)
        {

            foreach (ContainersGroup group in createdGroupList)
            {
                Destroy(group.gameObject);
            }

            createdGroupList.Clear();

            for (int i = 0; i < gameGrid.RowList.Count - 1; i++)
            {
                RowContainer row1 = gameGrid.RowList[i];
                RowContainer row2 = gameGrid.RowList[i + 1];

                if (row1 == null || row2 == null) return;

                if (!row1.IsHolding && !row2.IsHolding)
                {
                    if (row1.CurrentSocket == null || row2.CurrentSocket == null) return;
                    if (row1.CurrentSocket.Coordinate == null || row2.CurrentSocket.Coordinate == null) return;
                    bool isInCorrectDirection = (row2.CurrentSocket.Coordinate.y - row1.CurrentSocket.Coordinate.y) == 1;

                    if (isInCorrectDirection)
                    {
                        MergeContainers(row1, row2);
                    }
                }
            }

            for (int i = 0; i < gameGrid.ColumnList.Count - 1; i++)
            {
                ColumnContainer column1 = gameGrid.ColumnList[i];
                ColumnContainer column2 = gameGrid.ColumnList[i + 1];

                if (!column1.IsHolding && !column2.IsHolding)
                {
                    bool isInCorrectDirection = (column2.CurrentSocket.Coordinate.x - column1.CurrentSocket.Coordinate.x) == 1;

                    if (isInCorrectDirection)
                    {
                        MergeContainers(column1, column2);
                    }
                }
            }

            float progress = GetCurrentProgress();
            OnProgressUpdatedEventHandler?.Invoke(progress, holdingContainerList);
        }

        public float GetCurrentProgress()
        {
            int requiredMatches = gameGrid.RowList.Count - 1 + gameGrid.ColumnList.Count - 1;

            int matches = 0;

            foreach (ContainersGroup group in createdGroupList)
            {
                matches += (group.ContainerList.Count - 1);
            }

            float progress = ((float)matches / requiredMatches);            return progress;
         
        }

        private ContainersGroup MergeContainers(ContainerBase container1, ContainerBase container2)
        {
            ContainersGroup group1 = GetGroupForContainer(container1);
            ContainersGroup group2 = GetGroupForContainer(container2);

            if (group1 == null && group2 == null)
            {
               return CreateNewGroupForContainers(container1, container2);
            }
            else if (group1 != null && group2 == null)
            {
                group1.AddToGroup(container2);
                return group1;
            }
            else if (group1 == null && group2 != null)
            {
                group2.AddToGroup(container1);
                return group2;
            }
            else if (group1 != null && group2 != null)
            {
                throw new System.Exception("both containers have group");
            }

             throw new System.Exception("error");
        }

        private ContainersGroup CreateNewGroupForContainers(ContainerBase container1, ContainerBase container2)
        {
            GameObject containerGroupGO = Instantiate(containerGroupPrefab);

            ContainersGroup group = containerGroupGO.GetComponent<ContainersGroup>();
            if (container1 is RowContainer)
            {
                group.SetType(ContainersGroup.GroupType.Row);
            }
            else if (container1 is ColumnContainer)
            { 
                group.SetType(ContainersGroup.GroupType.Column);
            }

            group.AddToGroup(container1);
            group.AddToGroup(container2);

            createdGroupList.Add(group);

            return group;
        }

        public ContainersGroup GetGroupForContainer(ContainerBase container)
        {
            foreach (ContainersGroup group in createdGroupList)
            {
                if (group.ContainerList.Contains(container))
                {
                    return group;
                }
            }

            return null;
        }
    }
}