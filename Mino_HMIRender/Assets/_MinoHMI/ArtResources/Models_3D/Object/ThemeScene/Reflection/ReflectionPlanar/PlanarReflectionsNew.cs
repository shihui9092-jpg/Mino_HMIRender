using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

[ExecuteAlways]
public class PlanarReflectionsNew : MonoBehaviour
{
    [System.Serializable]
    public enum ResolutionMulltiplier
    {
        Full,
        Half,
        Third,
        Quarter
    }

    [System.Serializable]
    public class PlanarReflectionSettings
    {
        public ResolutionMulltiplier m_ResolutionMultiplier = ResolutionMulltiplier.Third;
        public float m_ClipPlaneOffset = 0.07f;
        public LayerMask m_ReflectLayers = -1;
        public bool m_shadows;
        //模糊效果相关参数
        public bool _blurOn = true;
        [Range(0.0f, 5.0f)]
        public float _blurSize = 0;
        [Range(0, 10)]
        public int _blurIterations = 4;
        [Range(1.0f, 4.0f)]
        public float _downsample = 1;
    }

    [SerializeField]
    public PlanarReflectionSettings m_settings = new PlanarReflectionSettings();

    public GameObject target;
    [FormerlySerializedAs("camOffset")] public float m_planeOffset;

    private static Camera m_ReflectionCamera;
    private RenderTexture m_ReflectionTexture = null;
    private RenderTexture m_BlurReflectionTexture = null;
    private static readonly int planarReflectionTextureID = Shader.PropertyToID("_PlanarReflectionTexture");

    // 缓存变量以减少GC
    private Vector2Int m_CurrentTextureSize;
    private ResolutionMulltiplier m_CurrentResolutionMultiplier;
    private float m_CurrentDownsample;
    private bool m_CurrentBlurOn;
    private int m_CurrentBlurIterations;
    private float m_CurrentBlurSize;

    //模糊shader
    const string k_BlurShader = "Hidden/KawaseBlur";
    private Material _blurMaterial;
    private bool m_IsInitialized;

    private void OnEnable()
    {
        if (!m_IsInitialized)
        {
            Initialize();
        }
        RenderPipelineManager.beginCameraRendering += ExecuteBeforeCameraRender;
    }

    private void Initialize()
    {
        // 初始化缓存变量
        m_CurrentResolutionMultiplier = m_settings.m_ResolutionMultiplier;
        m_CurrentDownsample = m_settings._downsample;
        m_CurrentBlurOn = m_settings._blurOn;
        m_CurrentBlurIterations = m_settings._blurIterations;
        m_CurrentBlurSize = m_settings._blurSize;
        m_IsInitialized = true;
    }

    // Cleanup all the objects we possibly have created
    private void OnDisable()
    {
        Cleanup();
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    void Cleanup()
    {
        RenderPipelineManager.beginCameraRendering -= ExecuteBeforeCameraRender;

        if (m_ReflectionCamera)
        {
            m_ReflectionCamera.targetTexture = null;
            SafeDestroy(m_ReflectionCamera.gameObject);
            m_ReflectionCamera = null;
        }

        ReleaseRenderTexture(ref m_ReflectionTexture);
        ReleaseRenderTexture(ref m_BlurReflectionTexture);

        SafeDestroy(_blurMaterial);

        m_IsInitialized = false;
    }

    void ReleaseRenderTexture(ref RenderTexture texture)
    {
        if (texture != null)
        {
            RenderTexture.ReleaseTemporary(texture);
            texture = null;
        }
    }

    void SafeDestroy(Object obj)
    {
        if (obj == null) return;

        if (Application.isEditor)
        {
            DestroyImmediate(obj);
        }
        else
        {
            Destroy(obj);
        }
    }

    private void UpdateCamera(Camera src, Camera dest)
    {
        if (dest == null)
            return;
        dest.CopyFrom(src);
        dest.cameraType = CameraType.Game;
        dest.useOcclusionCulling = false;
    }

    private void UpdateReflectionCamera(Camera realCamera)
    {
        if (m_ReflectionCamera == null)
            m_ReflectionCamera = CreateMirrorObjects(realCamera);

        // find out the reflection plane: position and normal in world space
        Vector3 pos = Vector3.zero;
        Vector3 normal = Vector3.up;
        if (target != null)
        {
            pos = target.transform.position + Vector3.up * m_planeOffset;
            normal = target.transform.up;
        }

        UpdateCamera(realCamera, m_ReflectionCamera);

        // Render reflection
        // Reflect camera around reflection plane
        float d = -Vector3.Dot(normal, pos) - m_settings.m_ClipPlaneOffset;
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        Matrix4x4 reflection = Matrix4x4.identity;
        reflection *= Matrix4x4.Scale(new Vector3(1, -1, 1));

        CalculateReflectionMatrix(ref reflection, reflectionPlane);
        Vector3 oldpos = realCamera.transform.position - new Vector3(0, pos.y * 2, 0);
        Vector3 newpos = ReflectPosition(oldpos);
        m_ReflectionCamera.transform.forward = Vector3.Scale(realCamera.transform.forward, new Vector3(1, -1, 1));
        m_ReflectionCamera.worldToCameraMatrix = realCamera.worldToCameraMatrix * reflection;

        // Setup oblique projection matrix so that near plane is our reflection
        // plane. This way we clip everything below/above it for free.
        Vector4 clipPlane = CameraSpacePlane(m_ReflectionCamera, pos - Vector3.up * 0.1f, normal, 1.0f);
        Matrix4x4 projection = realCamera.CalculateObliqueMatrix(clipPlane);
        m_ReflectionCamera.projectionMatrix = projection;
        m_ReflectionCamera.cullingMask = m_settings.m_ReflectLayers; // never render water layer
        m_ReflectionCamera.transform.position = newpos;
    }

    // Calculates reflection matrix around the given plane
    private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
    {
        reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
        reflectionMat.m01 = (-2F * plane[0] * plane[1]);
        reflectionMat.m02 = (-2F * plane[0] * plane[2]);
        reflectionMat.m03 = (-2F * plane[3] * plane[0]);

        reflectionMat.m10 = (-2F * plane[1] * plane[0]);
        reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
        reflectionMat.m12 = (-2F * plane[1] * plane[2]);
        reflectionMat.m13 = (-2F * plane[3] * plane[1]);

        reflectionMat.m20 = (-2F * plane[2] * plane[0]);
        reflectionMat.m21 = (-2F * plane[2] * plane[1]);
        reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
        reflectionMat.m23 = (-2F * plane[3] * plane[2]);

        reflectionMat.m30 = 0F;
        reflectionMat.m31 = 0F;
        reflectionMat.m32 = 0F;
        reflectionMat.m33 = 1F;
    }

    private static Vector3 ReflectPosition(Vector3 pos)
    {
        return new Vector3(pos.x, -pos.y, pos.z);
    }

    private float GetScaleValue()
    {
        switch (m_settings.m_ResolutionMultiplier)
        {
            case ResolutionMulltiplier.Full:
                return 1f;
            case ResolutionMulltiplier.Half:
                return 0.5f;
            case ResolutionMulltiplier.Third:
                return 0.33f;
            case ResolutionMulltiplier.Quarter:
                return 0.25f;
            default:
                return 0.5f; // default to half res
        }
    }

    // Given position/normal of the plane, calculates plane in camera space.
    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * m_settings.m_ClipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    private Camera CreateMirrorObjects(Camera currentCamera)
    {
        GameObject go =
            new GameObject($"Planar Refl Camera id{GetInstanceID().ToString()} for {currentCamera.GetInstanceID().ToString()}",
                typeof(Camera));
        UniversalAdditionalCameraData lwrpCamData =
            go.AddComponent(typeof(UniversalAdditionalCameraData)) as UniversalAdditionalCameraData;
        UniversalAdditionalCameraData lwrpCamDataCurrent = currentCamera.GetComponent<UniversalAdditionalCameraData>();
        lwrpCamData.renderShadows = m_settings.m_shadows; // turn off shadows for the reflection camera
        lwrpCamData.requiresColorOption = CameraOverrideOption.Off;
        lwrpCamData.requiresDepthOption = CameraOverrideOption.Off;
        var reflectionCamera = go.GetComponent<Camera>();
        reflectionCamera.transform.SetPositionAndRotation(transform.position, transform.rotation);
        reflectionCamera.allowMSAA = currentCamera.allowMSAA;
        reflectionCamera.depth = -10;
        reflectionCamera.enabled = false;
        reflectionCamera.allowHDR = currentCamera.allowHDR;
        go.hideFlags = HideFlags.HideAndDontSave;

        return reflectionCamera;
    }

    private Vector2Int ReflectionResolution(Camera cam, float scale)
    {
        var x = (int)(cam.pixelWidth * scale * GetScaleValue());
        var y = (int)(cam.pixelHeight * scale * GetScaleValue());
        return new Vector2Int(x, y);
    }

    private bool CheckSettingsChanged(Camera camera)
    {
        float scale = UniversalRenderPipeline.asset != null ? UniversalRenderPipeline.asset.renderScale : 1.0f;
        Vector2Int newSize = ReflectionResolution(camera, scale);

        bool settingsChanged =
            m_CurrentResolutionMultiplier != m_settings.m_ResolutionMultiplier ||
            m_CurrentDownsample != m_settings._downsample ||
            m_CurrentBlurOn != m_settings._blurOn ||
            m_CurrentBlurIterations != m_settings._blurIterations ||
            m_CurrentBlurSize != m_settings._blurSize ||
            !m_CurrentTextureSize.Equals(newSize);

        if (settingsChanged)
        {
            m_CurrentResolutionMultiplier = m_settings.m_ResolutionMultiplier;
            m_CurrentDownsample = m_settings._downsample;
            m_CurrentBlurOn = m_settings._blurOn;
            m_CurrentBlurIterations = m_settings._blurIterations;
            m_CurrentBlurSize = m_settings._blurSize;
            m_CurrentTextureSize = newSize;

            // 释放旧的纹理
            ReleaseRenderTexture(ref m_ReflectionTexture);
            ReleaseRenderTexture(ref m_BlurReflectionTexture);
        }

        return settingsChanged;
    }

    private void EnsureBlurMaterial()
    {
        if (_blurMaterial == null)
        {
            var blurShader = Shader.Find(k_BlurShader);
            if (blurShader == null)
            {
                Debug.LogError("Reflection Not Find Blur Shader");
                return;
            }
            _blurMaterial = CoreUtils.CreateEngineMaterial(blurShader);
        }
    }

    private void EnsureReflectionTexture(Camera camera)
    {
        if (m_ReflectionTexture == null)
        {
            float scale = UniversalRenderPipeline.asset != null ? UniversalRenderPipeline.asset.renderScale : 1.0f;
            Vector2Int res = ReflectionResolution(camera, scale);

            bool useHDR10 = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB111110Float);
            RenderTextureFormat hdrFormat =
                useHDR10 ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.DefaultHDR;
            m_ReflectionTexture = RenderTexture.GetTemporary(res.x, res.y, 16,
                GraphicsFormatUtility.GetGraphicsFormat(hdrFormat, true));
            m_ReflectionTexture.useMipMap = true;
            m_ReflectionTexture.autoGenerateMips = true;
            m_ReflectionTexture.name = "_PlanarReflectionTexture";
        }
    }

    public void ExecuteBeforeCameraRender(ScriptableRenderContext context, Camera camera)
    {
        if (!enabled || camera.cameraType != CameraType.Game)
            return;

        // 检查设置是否更改，如果需要则释放旧纹理
        CheckSettingsChanged(camera);

        GL.invertCulling = true;
        RenderSettings.fog = false;
        var max = QualitySettings.maximumLODLevel;
        var bias = QualitySettings.lodBias;
        QualitySettings.maximumLODLevel = 1;
        QualitySettings.lodBias = bias * 0.5f;

        UpdateReflectionCamera(camera);

        EnsureReflectionTexture(camera);
        m_ReflectionCamera.targetTexture = m_ReflectionTexture;

        UniversalRenderPipeline.RenderSingleCamera(context, m_ReflectionCamera);

        GL.invertCulling = false;
        RenderSettings.fog = true;
        QualitySettings.maximumLODLevel = max;
        QualitySettings.lodBias = bias;

        // 模糊处理
        if (m_settings._blurOn)
        {
            EnsureBlurMaterial();
            if (_blurMaterial == null)
            {
                // 如果模糊材质创建失败，使用非模糊纹理
                Shader.SetGlobalTexture(planarReflectionTextureID, m_ReflectionTexture);
                return;
            }

            if (m_BlurReflectionTexture == null)
            {
                var sourceDesc = m_ReflectionTexture.descriptor;
                sourceDesc.msaaSamples = 1;
                sourceDesc.depthBufferBits = 0;
                m_BlurReflectionTexture = RenderTexture.GetTemporary(sourceDesc);
                m_BlurReflectionTexture.name = "Blur ReflectionTex";
            }

            ApplyBlurEffect(context, m_ReflectionTexture, m_BlurReflectionTexture);
            Shader.SetGlobalTexture(planarReflectionTextureID, m_BlurReflectionTexture);
        }
        else
        {
            Shader.SetGlobalTexture(planarReflectionTextureID, m_ReflectionTexture);
        }
    }

    private void ApplyBlurEffect(ScriptableRenderContext context, RenderTexture source, RenderTexture destination)
    {
        var buf = CommandBufferPool.Get("Blur Reflection");
        try
        {
            float width = source.width;
            float height = source.height;
            var sourceDesc = source.descriptor;
            sourceDesc.msaaSamples = 1;
            sourceDesc.depthBufferBits = 0;
            sourceDesc.width = Mathf.RoundToInt(width / m_settings._downsample);
            sourceDesc.height = Mathf.RoundToInt(height / m_settings._downsample);

            int blurredID = Shader.PropertyToID("_Temp1");
            int blurredID2 = Shader.PropertyToID("_Temp2");
            buf.GetTemporaryRT(blurredID, sourceDesc);
            buf.GetTemporaryRT(blurredID2, sourceDesc);

            buf.SetGlobalFloat("_BlurOffset", 1.0f + m_settings._blurSize);
            buf.Blit(source, blurredID, _blurMaterial, 0);

            for (int i = 1; i < m_settings._blurIterations; i++)
            {
                float iterationOffs = i;
                buf.SetGlobalFloat("_BlurOffset", iterationOffs + m_settings._blurSize);
                buf.Blit(blurredID, blurredID2, _blurMaterial, 0);

                // pingpong
                (blurredID, blurredID2) = (blurredID2, blurredID);
            }

            buf.SetGlobalFloat("_BlurOffset", m_settings._blurIterations + m_settings._blurSize);
            buf.Blit(blurredID, destination, _blurMaterial, 0);

            buf.ReleaseTemporaryRT(blurredID);
            buf.ReleaseTemporaryRT(blurredID2);

            context.ExecuteCommandBuffer(buf);
        }
        finally
        {
            CommandBufferPool.Release(buf);
        }
    }
}