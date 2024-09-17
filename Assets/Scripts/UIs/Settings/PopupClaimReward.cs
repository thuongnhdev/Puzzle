using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupClaimReward : BasePopup
{
    [SerializeField] private TextMeshProUGUI amountTxt;
    [SerializeField] private Image iconImg;

    [SerializeField] private Sprite[] iconSprites;

    private Action _onClose;

    public override void SetData(object[] data)
    {
        base.SetData(data);

        if (data.Length > 0)
        {
            amountTxt.SetText(data[0].ToString());
            switch ((RewardType) data[1])
            {
                case RewardType.HINT:
                    iconImg.sprite = iconSprites[0];
                    break;
                case RewardType.INK:
                    iconImg.sprite = iconSprites[1];
                    break;
            }

            _onClose = (Action) data[2];
        }
    }

    public override void Close()
    {
        base.Close();
        _onClose?.Invoke();
    }

}
