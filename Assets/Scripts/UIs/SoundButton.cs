using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundButton : MonoBehaviour
{
    public void OnClickSound()
    {
        SoundController.Instance.PlaySfxClick();
    }
}
