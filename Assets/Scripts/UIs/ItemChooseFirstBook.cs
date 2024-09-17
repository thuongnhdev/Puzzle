using DataCore;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemChooseFirstBook : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI txtBookName;
    [SerializeField] private TextMeshProUGUI txtBookAuthor;
    [SerializeField] private Image imgThumbnail;


    private BookMasterData masterData;

    public void Init(BookMasterData data)
    {
        masterData = data;
        UpdateUI();
    }

    private void UpdateUI()
    {
        txtBookName.text = masterData.BookName;
        txtBookAuthor.text = "by " + masterData.Author;
        AssetManager.Instance.LoadPathAsync<Sprite>(masterData.Thumbnail.Thumbnail, (thumb) =>
        {
            if (thumb != null && imgThumbnail != null)
            {
                imgThumbnail.sprite = thumb;
            }
        });
    }
}
