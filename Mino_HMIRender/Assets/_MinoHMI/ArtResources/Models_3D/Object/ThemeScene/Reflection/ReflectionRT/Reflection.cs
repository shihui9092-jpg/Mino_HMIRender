using System;
using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// 镜子管理脚本 —— 挂在新建的Camera上
/// </summary>
[ExecuteInEditMode]
public class Reflection : MonoBehaviour
{
    public GameObject mirrorPlane;  //镜子
    public Camera mainCamera;   //主摄像机
    public RenderTexture RT;
    private Camera mirrorCamera; //镜像摄像机


    private void Start()
    {
        mirrorCamera = GetComponent<Camera>();
        RenderPipelineManager.beginFrameRendering += RenderCamera;
    }

    private void RenderCamera(ScriptableRenderContext context, Camera[] arg2)
    {
        if (null == mirrorPlane || null == mirrorCamera || null == mainCamera) return;
        
        //mirrorCamera.transform.position = mainCamera.transform.position;
        //mirrorCamera.CopyFrom(mainCamera);
        Vector3 worldSpaceViewDir = mainCamera.transform.forward;
        Vector3 worldSpaceViewUp = mainCamera.transform.up;
        Vector3 worldSpaceCamPos = mainCamera.transform.position;

        Vector3 planeSpaceViewDir = mirrorPlane.transform.InverseTransformDirection(worldSpaceViewDir);
        Vector3 planeSpaceViewUp = mirrorPlane.transform.InverseTransformDirection(worldSpaceViewUp);
        Vector3 planeSpaceCamPos = mirrorPlane.transform.InverseTransformPoint(worldSpaceCamPos);

        planeSpaceViewDir.y *= -1.0f;
        planeSpaceViewUp.y *= -1.0f;
        planeSpaceCamPos.y *= -1.0f;

        worldSpaceViewDir = mirrorPlane.transform.TransformDirection(planeSpaceViewDir);
        worldSpaceViewUp = mirrorPlane.transform.TransformDirection(planeSpaceViewUp);
        worldSpaceCamPos = mirrorPlane.transform.TransformPoint(planeSpaceCamPos);

        mirrorCamera.transform.position = worldSpaceCamPos;
        mirrorCamera.transform.LookAt(worldSpaceCamPos + worldSpaceViewDir, worldSpaceViewUp);

    }
}