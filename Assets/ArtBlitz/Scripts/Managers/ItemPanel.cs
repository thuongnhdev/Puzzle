using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LeoScript.ArtBlitz
{  
    public class ItemPanel : MonoBehaviour
    {
        public Toggle isBossLevel;

        public Texture2D texture;        
        public GameObject gameObjectGO;

        [SerializeField] private List<Vector2Int> layouts;
        public Vector2Int selectedLayout;

        private void Start()
        {
            selectedLayout = layouts[0];
        }

        public void OnDropDownValueChanged(int index)
        {
            selectedLayout = layouts[index];
        }
    }
}