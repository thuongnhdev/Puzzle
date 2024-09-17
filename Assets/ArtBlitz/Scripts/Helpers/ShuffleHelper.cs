using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeoScript.ArtBlitz
{
    public class ShuffleHelper : MonoBehaviour
    {
        [SerializeField] private SocketsHelper socketsHelper;
        [SerializeField] private ContainersGroupHelper containersGroupHelper;

        public void Shuffle(int times, List<RowContainer> rowList, List<ColumnContainer> columnList)
        {
            for (int i = 0; i < times; i++)
            {
                bool isRow = Random.Range(0, 2) == 0;

                if (isRow)
                {
                    Vector2Int indices = GetRandomRangeFromList(rowList);
                    socketsHelper.SwitchPositionOfContainers(rowList[indices.x], rowList[indices.y]);
                }
                else
                {
                    Vector2Int indices = GetRandomRangeFromList(columnList);
                    socketsHelper.SwitchPositionOfContainers(columnList[indices.x], columnList[indices.y]);
                }

                containersGroupHelper.UpdateGroupStates();
                float progress = containersGroupHelper.GetCurrentProgress();
                if (progress == 0.0f)
                {
                    return;
                }
            }
        }

        private Vector2Int GetRandomRangeFromList<T>(List<T> list)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < list.Count; i++)
            {
                indices.Add(i);
            }

            int randomIndex = Random.Range(0, indices.Count);

            int firstIndex = indices[randomIndex];
            indices.RemoveAt(firstIndex);

            randomIndex = Random.Range(0, indices.Count);
            int secondIndex = indices[randomIndex];

            return new Vector2Int(firstIndex, secondIndex);
        }

    }
}