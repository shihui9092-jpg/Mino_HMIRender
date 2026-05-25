using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class WeatherControl : MonoBehaviour
{
    [Header("Rain")]
    public GameObject RainVFX;
    public GameObject RainOther;
    //public Material RainCarPaintMat = null;
    //public Material RainCarPaintGlassMat = null;
    public Material RainGroundMat = null;
    [Range(0.0f, 1.0f)]
    public float RainVFXControl = 0.0f;

    [Space(20)]
    [Header("Snow")]
    public GameObject SnowVFX;
    public GameObject SnowOther;
    public Material SnowGroundMat = null;
    [Range(0.0f, 1.0f)]
    public float SnowVFXControl = 0.0f;

    void Start()
    {      
        RainSetting();
        SnowSetting();

        if (RainVFX != null)
        {
            var particleCS = RainVFX.GetComponent<ParticleSystem>();
            particleCS.Stop();
        }
        if (SnowVFX != null)
        {
            var particleCS = SnowVFX.GetComponent<ParticleSystem>();
            particleCS.Stop();
        }      
    }

    void RainSetting()
    {
        if (RainVFX != null)
        {
            var particleCS = RainVFX.GetComponent<ParticleSystem>();
            var emission = particleCS.emission;
            emission.rateOverTimeMultiplier = RainVFXControl * 240.0f;

            if (RainVFXControl > 0.01f)
                particleCS.Play();
            else
                particleCS.Stop();

        }
        if (RainOther != null)
        {
            var particleCS = RainOther.GetComponent<ParticleSystem>();
            var emission = particleCS.emission;
            emission.rateOverTimeMultiplier = RainVFXControl * 100.0f;

            if (RainVFXControl > 0.01f)
                particleCS.Play();
            else
                particleCS.Stop();
        }
        //if (RainCarPaintMat != null)
        //{
        //    RainCarPaintMat.SetFloat("_RainControl", RainVFXControl);
        //}
        //if (RainCarPaintGlassMat != null)
        //{
        //    RainCarPaintGlassMat.SetFloat("_RainControl", RainVFXControl);
        //}
        if (RainGroundMat != null)
        {
            RainGroundMat.SetFloat("_RainControl", RainVFXControl);
        }
    }


    void SnowSetting()
    {
        if (SnowVFX != null)
        {
            var particleCS = SnowVFX.GetComponent<ParticleSystem>();
            var emission = particleCS.emission;
            emission.rateOverTimeMultiplier = SnowVFXControl * 200.0f;

            if (SnowVFXControl > 0.01f)                
                particleCS.Play();              
            else
                particleCS.Stop();        
        }
        if (SnowOther != null)
        {
            var particleCS = SnowOther.GetComponent<ParticleSystem>();
            var emission = particleCS.emission;
            emission.rateOverTimeMultiplier = SnowVFXControl * 30.0f;

            if (SnowVFXControl > 0.0f)

                particleCS.Play();

            else
                particleCS.Stop();

        }

        if (SnowGroundMat != null)
        {
            SnowGroundMat.SetFloat("_SnowControl", SnowVFXControl);
        }

    }
    void Update()
    {
        RainSetting();
        SnowSetting();

        if (SnowVFXControl > 0.01f)
        {
            SnowSetting();
        }

        if (RainVFXControl > 0.01f)
        {
            RainSetting();
        }
    }
}
