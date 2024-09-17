using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LeoScript.ArtBlitz
{
    public class SocketsHelper : MonoBehaviour
    {
        [SerializeField] private GameGrid grid;

        [SerializeField] private List<ContainerSocket> rowSocketList;
        [SerializeField] private List<ContainerSocket> columnSocketList;

        [SerializeField] private GameObject socketPrefab;

        [SerializeField] private RectTransform rowSocketsRoot;
        [SerializeField] private RectTransform columnScoketsRoot;

        [SerializeField] private RectTransform gameScaleRectTrans;

        [SerializeField] private ContainersGroupHelper containersGroupHelper;

        [SerializeField] private Vector2 snapThreshold;

        public void CreateSockets(List<RowContainer> rowList, List<ColumnContainer> columnList)
        {
            List<ContainerBase> list = new List<ContainerBase>(rowList);
            rowSocketList =  CreateSocketList(list, rowSocketsRoot, Color.green);

            list = new List<ContainerBase>(columnList);
            columnSocketList = CreateSocketList(list, columnScoketsRoot, Color.yellow);
        }

        private List<ContainerSocket> CreateSocketList(List<ContainerBase> containerList , Transform indicatorsRoot,Color indicatorColor)
        {
            List<ContainerSocket> socketList = new List<ContainerSocket>();

            foreach (ContainerBase container in containerList)
            {
                TextureCell cell = container.TextureCellList[0];
                GameObject socketGO = Instantiate(socketPrefab, indicatorsRoot);

                ContainerSocket socket = socketGO.GetComponent<ContainerSocket>();

                socket.Setup(container, cell.Coordinate, cell.GetLocalPosition());

                socket.GetComponent<Image>().color = indicatorColor;
                socketList.Add(socket);
            }

            return socketList;
        }

        public void UpdateScaleRoot(RectTransform trans)
        {
            gameScaleRectTrans = trans;
            snapThreshold = grid.CellSize * 0.5f * gameScaleRectTrans.localScale.x;
        }

        public void MoveContainerToCurrentSocket(ContainerBase container)
        {
            container.MoveToCurrentSocket();
        }

        private ContainerSocket GetCorrectSocketForContainer(ContainerBase container )
        {
            float minDistance = float.MaxValue;
            ContainerSocket correctSocket = null;
            List<ContainerSocket> socketList = null;

            if (container is RowContainer)
            {
                socketList = rowSocketList;
            }
            else if (container is ColumnContainer)
            {
                socketList = columnSocketList;
            }

            foreach (ContainerSocket socket in socketList)
            {
                float distance = GetDistanceBetweenSocketAndContainer(socket, container);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    correctSocket = socket;
                }
            }

            return correctSocket;
        }

        public void UpdateSocket(List<ContainerBase> holdingContainerList)
        {
            if (IsReadyToUpdateSockets(holdingContainerList))
            {
                List<ContainerSocket> emptySockets = ClearSocketsOfContainers(holdingContainerList);

                ContainerBase looseContainer = null;

                foreach (ContainerBase container in holdingContainerList)
                {
                    ContainerSocket newSocket = GetCorrectSocketForContainer(container);

                    bool isSocketEmpty = newSocket.CurrentContainer != null;
                    if (isSocketEmpty)
                    {
                        looseContainer = newSocket.CurrentContainer;
                        looseContainer.SetSocket(null);
                    }
                    else
                    {
                        emptySockets.Remove(newSocket);
                    }

                    container.SetSocket(newSocket);
                    newSocket.SetContainer(container);
                }
               
                if (emptySockets.Count == 1)
                {
                    ContainerSocket emptySocket = emptySockets[0];

                    looseContainer.SetSocket(emptySocket);
                    looseContainer.MoveToCurrentSocket();
                    emptySocket.SetContainer(looseContainer);

                    containersGroupHelper.UpdateGroupStates();
                }
                else
                {                    
                    Debug.LogError("found more than one empty sockets");
                }
            }
        }

        private static List<ContainerSocket> ClearSocketsOfContainers(List<ContainerBase> outOfCurrentSocketContainerList)
        {
            List<ContainerSocket> emptySockets = new List<ContainerSocket>();
            foreach (ContainerBase container in outOfCurrentSocketContainerList)
            {
                emptySockets.Add(container.CurrentSocket);
                container.CurrentSocket.SetContainer(null);
                container.SetSocket(null);
            }

            return emptySockets;
        }

        private bool IsReadyToUpdateSockets(List<ContainerBase> holdingContainerList)
        {
            if (!holdingContainerList[0].IsOutOfCurrentSocket(snapThreshold))
            {
                return false;
            }

            List<ContainerSocket> targetSockets = new List<ContainerSocket>();

            foreach (ContainerBase container in holdingContainerList)
            {
                ContainerSocket newSocket = GetCorrectSocketForContainer(container);

                if (targetSockets.Contains(newSocket) || newSocket == container.CurrentSocket)
                {
                    return false;
                }
                else
                {
                    targetSockets.Add(newSocket);
                }                
            }

            return true;
        }

        public void SwitchPositionOfContainers(ContainerBase container1 , ContainerBase container2)
        {            
            ContainerSocket socket1 = container1.CurrentSocket;
            ContainerSocket socket2 = container2.CurrentSocket;
            
            socket1.SetContainer(container2);
            container2.SetSocket(socket1);
            container2.MoveToSocketImmediate(socket1);

            socket2.SetContainer(container1);
            container1.SetSocket(socket2);
            container1.MoveToSocketImmediate(socket2);  
        }

        private float GetDistanceBetweenSocketAndContainer(ContainerSocket socket, ContainerBase container)
        {
            if (container is RowContainer)
            {
                RowContainer row = container as RowContainer;
                return Mathf.Abs(socket.transform.position.y - row.GetYPosition());
            }
            else if (container is ColumnContainer)
            {
                ColumnContainer column = container as ColumnContainer;
                return Mathf.Abs(socket.transform.position.x - column.GetXPosition());
            }
            else
            {
                throw new System.Exception("wrong type");
            }
        }
    }

}