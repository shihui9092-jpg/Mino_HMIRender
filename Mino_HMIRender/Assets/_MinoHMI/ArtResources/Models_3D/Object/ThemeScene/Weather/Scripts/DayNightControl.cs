using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class DayNightControl : MonoBehaviour
{
    public Material DayNightSkyMat = null;
    public Material DayNightSceneMat = null;
    public Transform DirectionalLight = null;
    [Header("Control")]
    [Range(0.0f, 1.0f)]
    public float DayNight = 0.0f;
    public float MainLightDay;
    public float MainLightNight;
    
    private void Update()
    {
        SetDayNightMaterial();
        
    }

    private void SetDayNightMaterial()
    {
        if (DayNightSkyMat != null)
        {
            DayNightSkyMat.SetFloat("_DayNightControl", DayNight);


        }
        if (DayNightSceneMat != null)
        {
            DayNightSceneMat.SetFloat("_DayNightControl", DayNight);

        }
        if (DirectionalLight != null)
        {
            DirectionalLight.GetComponent<Light>().intensity = Mathf.Lerp(MainLightDay, MainLightNight, DayNight);
        }


    }
   
}


