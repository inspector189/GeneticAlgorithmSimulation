using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeDilationBtn : MonoBehaviour
{
    [SerializeField]
    private float timeScale = 1f;
    
    public void BtnClick()
    {
        Time.timeScale = timeScale;
    }
}
