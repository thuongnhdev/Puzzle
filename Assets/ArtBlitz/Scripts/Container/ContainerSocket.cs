using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeoScript.ArtBlitz
{
    public class ContainerSocket : MonoBehaviour
    {
        private Vector2Int coordinate;
        public Vector2Int Coordinate { get => coordinate; }

        [SerializeField] private RectTransform trans;

        [SerializeField] private ContainerBase currentContainer;
        public ContainerBase CurrentContainer { get => currentContainer; }


        public void SetContainer(ContainerBase container)
        {
            currentContainer = container;
        }  

        public Vector2 GetPosition()
        {
            return trans.position;
        }

        public void Setup(ContainerBase containerBase, Vector2Int coordinate, Vector2 position)
        {
            this.currentContainer = containerBase;
            this.coordinate = coordinate;
            gameObject.name = "Socket " + coordinate.ToString();
            containerBase.SetSocket(this);
            GetComponent<RectTransform>().localPosition = position;
        }


    }
}