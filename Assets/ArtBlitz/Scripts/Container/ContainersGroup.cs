using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeoScript.ArtBlitz
{
    public class ContainersGroup : MonoBehaviour
    {
        [SerializeField] private List<ContainerBase> containerList;
        public List<ContainerBase> ContainerList { get => containerList; }

        private bool isHolding = false;
        public bool IsHolding { get => isHolding; }

        public enum GroupType
        {
            Unknown,
            Row,
            Column
        }

        [SerializeField] private GroupType groupType = GroupType.Unknown;
        public GroupType GetGroupType()
        { 
            return groupType; 
        }

        public void SetType(GroupType type)
        {
            this.groupType = type;
        }


        public void AddToGroup(ContainerBase container)
        {
            if ((container is RowContainer) && groupType == GroupType.Row)
            {
                containerList.Add(container);
            }
            else if ((container is ColumnContainer) && groupType == GroupType.Column)
            {
                containerList.Add(container);
            }
            else
            {
                Debug.LogError("wrong type");
            }
        }

        public void Hold()
        {
            foreach (ContainerBase container in containerList)
            {
                container.Hold();
            }
            isHolding = true;
        }

        public void Drop()
        {
            foreach (ContainerBase container in containerList)
            {
                container.Drop();
            }

            isHolding = false;
        }

    }
}