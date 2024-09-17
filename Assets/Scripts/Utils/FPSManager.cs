using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace com.F4A.MobileThird
{
    public class FPSManager : SingletonMono<FPSManager>
    {
        private Coroutine _reduceFrameRate;
        // Start is called before the first frame update
        void Start()
        {
            Application.targetFrameRate = 30;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.touchCount > 0)
            {
                //Debug.Log("exist a touch");

                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    if (_reduceFrameRate != null) {
                        StopCoroutine(_reduceFrameRate);
                    }
                    if (Application.targetFrameRate != 60) {
                        Application.targetFrameRate = 60;
                    }                    
                    //Debug.Log($"Touch begans. targetFrameRate {Application.targetFrameRate}");
                }
                if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    //Debug.Log("Touch Ended.");
                    _reduceFrameRate = StartCoroutine(ReduceFrameRate());
                }
            }
        }
        IEnumerator ReduceFrameRate() {
            yield return new WaitForSeconds(5f);
            Application.targetFrameRate = 30;
            _reduceFrameRate = null;
            //Debug.Log($"ReduceFrameRate. {Application.targetFrameRate}");
        }
    }
}