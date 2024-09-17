using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeoScript.ArtBlitz
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        private static DontDestroyOnLoad instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(instance.gameObject);
            }
        }
    }

}