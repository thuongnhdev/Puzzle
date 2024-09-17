using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LeoScript.ArtBlitz
{
    public class MenuSceneManager : MonoBehaviour
    {
        public void OnStartButtonClicked(ItemPanel itemPanel)
        {
            //CirclesContainer container = itemPanel.circlesContainerGO.GetComponent<CirclesContainer>();
            //container.IsReversed = itemPanel.isReversed.isOn;
            //container.IsMagnetized = itemPanel.isMagnetized.isOn;
            //container.ShowBG = itemPanel.showBackground.isOn;

            DataHolder.texture = itemPanel.texture;
            DataHolder.isBossLevel = itemPanel.isBossLevel.isOn;
            DataHolder.layout = itemPanel.selectedLayout;

            SceneManager.LoadScene("GameScene");
        }
    }
}