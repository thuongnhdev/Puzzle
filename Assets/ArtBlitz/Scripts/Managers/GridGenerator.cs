using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LeoScript.ArtBlitz
{
    public class GridGenerator : MonoBehaviour
    {
        [SerializeField] private GameObject textureCellPrefab;

        [SerializeField] private Vector2 offset;

        [SerializeField] private GameObject gameContainerPrefab;
        [SerializeField] private GameObject rowContainerPrefab;
        [SerializeField] private GameObject columnContainerPrefab;

        [SerializeField] private RectTransform gameContainerRoot;     

        public GameGrid CreateNewGrid(Vector2Int layout ,Texture2D originalTex ,Rect sampleRect)
        {     
            List<TextureCell> cells = CreateTextureCells(originalTex, layout, sampleRect);

            GameObject containerGO = Instantiate(gameContainerPrefab);
            GameGrid grid = containerGO.GetComponent<GameGrid>();
            AddTextureCellsToRowAndColumn(grid, cells, layout);
            grid.transform.SetParent(gameContainerRoot);
            grid.transform.localPosition = new Vector3(0, 50, 0); //Vector3.zero;
            grid.transform.localScale = new Vector3(1.8f, 1.8f, 1); //Vector3.one;
            grid.UpdateScaleRoot(gameContainerRoot);

            return grid;
        }

        public List<TextureCell> CreateTextureCells(Texture2D originalTex, Vector2Int layout , Rect sampleRect)
        {
            Vector2 textureResolution = new Vector2(originalTex.width, originalTex.height);

            Vector2 textureCenter = textureResolution / 2;

            List<TextureCell> list = new List<TextureCell>();

            float cellWidth = 1 / (float)layout.x;
            float cellHeight = 1 / (float)layout.y;

            Vector2 cellSize = new Vector2(cellWidth * originalTex.width, cellHeight * originalTex.height);
            
            offset = textureCenter - cellSize * 0.5f;

            Vector2 cellTextureRectSize = new Vector2(cellWidth, cellHeight);

            for (int j = 0; j < layout.y; j++)
            {
                for (int i = 0; i < layout.x; i++)
                {
                    GameObject cellGo = Instantiate(textureCellPrefab);
                    TextureCell cell = cellGo.GetComponent<TextureCell>();

                    cell.Init(originalTex, new Vector2Int(i, j), cellTextureRectSize, sampleRect);

                    list.Add(cell);
                }
            }

            foreach (TextureCell cell in list)
            {
                cell.GetComponent<RectTransform>().anchoredPosition -= offset;
            }

            return list;
        }

        private void AddTextureCellsToRowAndColumn(GameGrid grid, List<TextureCell> list, Vector2Int layout)
        {
            List<ColumnContainer> columnContainerList = new List<ColumnContainer>();
            List<RowContainer> rowContainerList = new List<RowContainer>();

            for (int i = 0; i < layout.x; i++)
            {
                GameObject columnGO = Instantiate(columnContainerPrefab);
                ColumnContainer columnContainer = columnGO.GetComponent<ColumnContainer>();
                columnContainer.SetIndex(i);
                columnContainerList.Add(columnContainer);

                foreach (TextureCell cell in list)
                {
                    if (cell.Coordinate.x == i)
                    {
                        columnContainer.AddCell(cell);
                    }
                }
            }

            for (int i = 0; i < layout.y; i++)
            {
                GameObject rowGO = Instantiate(rowContainerPrefab);
                RowContainer rowContainer = rowGO.GetComponent<RowContainer>();
                rowContainer.SetIndex(i);
                rowContainerList.Add(rowContainer);

                foreach (TextureCell cell in list)
                {
                    if (cell.Coordinate.y == i)
                    {
                        rowContainer.AddCell(cell);
                    }
                }
            }

            grid.SetupGameGrid(list, rowContainerList, columnContainerList);
        }
    }
}