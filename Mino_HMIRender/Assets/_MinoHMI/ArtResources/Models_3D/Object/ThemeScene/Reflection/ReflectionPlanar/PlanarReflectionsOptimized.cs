using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

[ExecuteAlways]
public class PlanarReflectionsOptimized : MonoBehaviour
{
    [System.Serializable]
    public enum ResolutionMultiplier
    {
        Full,
        Half,
        Third,
        Quarter
    }

    [System.Serializable]
    public class PlanarReflectionSettings
    {
        public ResolutionMultiplier m_ResolutionMultiplier = ResolutionMultiplier.Third;
        public float m_ClipPlaneOffset = 0.07f;
        public LayerMask m_ReflectLayers = -1;
        public bool m_shadows;
        
        // 模糊效果相关参数
        public bool _blurOn = true;
        [Range(0.0f, 5.0f)]
        public float _blurSize = 0;
        [Range(0, 4)] // 减少最大迭代次数
        public int _blurIterations = 2; // 默认值降低
        [Range(1.0f, 4.0f)]
        public float _downsample = 2; // 增加默认降采样
    }

    [System.Serializable]
    public class OptimizationSettings
    {
        [Tooltip("每N帧更新一次反射（1=每帧，2=每隔一帧）")]
        [Range(1, 5)]
        public int updateInterval = 1;
        
        [Tooltip("超过此距离不渲染反射")]
        public float maxReflectionDistance = 50f;
        
        [Tooltip("启用距离剔除")]
        public bool enableDistanceCulling = true;
        
        [Tooltip("使用Mipmap（提高远距离质量但增加开销）")]
        public bool useMipmap = false;
        
        [Tooltip("根据距离动态调整分辨率")]
        public bool dynamicResolution = true;
        
        [Tooltip("视锥体剔除边界扩展")]
        public float frustumPadding = 1.2f;
    }

    [SerializeField]
    public PlanarReflectionSettings m_settings = new PlanarReflectionSettings();
    
    [SerializeField]
    public OptimizationSettings m_optimizationSettings = new OptimizationSettings();

    public GameObject target;
    [FormerlySerializedAs("camOffset")] public float m_planeOffset;

    private static Camera m_ReflectionCamera;
    private RenderTexture m_ReflectionTexture;
    private RenderTexture m_BlurReflectionTexture;
    private static readonly int planarReflectionTextureID = Shader.PropertyToID("_PlanarReflectionTexture");

    // 缓存变量以减少GC和重复计算
    private Vector2Int m_CurrentTextureSize;
    private ResolutionMultiplier m_CurrentResolutionMultiplier;
    private float m_CurrentDownsample;
    private bool m_CurrentBlurOn;
    private int m_CurrentBlurIterations;
    private float m_CurrentBlurSize;

    // 反射矩阵缓存
    private Matrix4x4 m_CachedReflectionMatrix;
    private Vector3 m_LastTargetPosition;
    private Quaternion m_LastTargetRotation;
    private Vector3 m_LastCameraPosition;
    private bool m_NeedRecalculate = true;

    // 帧计数器
    private int m_FrameCount;

    // 模糊shader
    private const string k_BlurShader = "Hidden/KawaseBlur";
    private Material _blurMaterial;
    private bool m_IsInitialized;

    // 临时RT ID缓存（避免每帧PropertyToID调用）
    private static readonly int s_TempBlurRT1 = Shader.PropertyToID("_TempBlur1");
    private static readonly int s_TempBlurRT2 = Shader.PropertyToID("_TempBlur2");
    private static readonly int s_BlurOffsetID = Shader.PropertyToID("_BlurOffset");

    #region Lifecycle

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
        m_FrameCount = 0;
        m_NeedRecalculate = true;
        m_IsInitialized = true;
    }

    private void OnDisable()
    {
        Cleanup();
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void Cleanup()
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

    #endregion

    #region Utility Methods

    private void ReleaseRenderTexture(ref RenderTexture texture)
    {
        if (texture != null)
        {
            RenderTexture.ReleaseTemporary(texture);
            texture = null;
        }
    }

    private void SafeDestroy(Object obj)
    {
        if (obj == null) return;

#if UNITY_EDITOR
        if (Application.isPlaying)
            Destroy(obj);
        else
            DestroyImmediate(obj);
#else
        Destroy(obj);
#endif
    }

    private float GetScaleValue()
    {
        switch (m_settings.m_ResolutionMultiplier)
        {
            case ResolutionMultiplier.Full:
                return 1f;
            case ResolutionMultiplier.Half:
                return 0.5f;
            case ResolutionMultiplier.Third:
                return 0.33f;
            case ResolutionMultiplier.Quarter:
                return 0.25f;
            default:
                return 0.5f;
        }
    }

    private Vector2Int ReflectionResolution(Camera cam, float scale, float distanceFactor = 1f)
    {
        float finalScale = GetScaleValue() * scale * distanceFactor;
        var x = Mathf.Max(64, (int)(cam.pixelWidth * finalScale));
        var y = Mathf.Max(64, (int)(cam.pixelHeight * finalScale));
        return new Vector2Int(x, y);
    }

    #endregion

    #region Camera Setup

    private void UpdateCamera(Camera src, Camera dest)
    {
        if (dest == null)
            return;
        dest.CopyFrom(src);
        dest.cameraType = CameraType.Game;
        dest.useOcclusionCulling = false;
    }

    private Camera CreateMirrorObjects(Camera currentCamera)
    {
        GameObject go = new GameObject("Planar Refl Camera", typeof(Camera));
        
        UniversalAdditionalCameraData cameraData =
            go.AddComponent<UniversalAdditionalCameraData>();
        
        cameraData.renderShadows = m_settings.m_shadows;
        cameraData.requiresColorOption = CameraOverrideOption.Off;
        cameraData.requiresDepthOption = CameraOverrideOption.Off;
        
        var reflectionCamera = go.GetComponent<Camera>();
        reflectionCamera.transform.SetPositionAndRotation(transform.position, transform.rotation);
        reflectionCamera.allowMSAA = currentCamera.allowMSAA;
        reflectionCamera.depth = -10;
        reflectionCamera.enabled = false;
        reflectionCamera.allowHDR = currentCamera.allowHDR;
        go.hideFlags = HideFlags.HideAndDontSave;

        return reflectionCamera;
    }

    private void UpdateReflectionCamera(Camera realCamera)
    {
        if (m_ReflectionCamera == null)
            m_ReflectionCamera = CreateMirrorObjects(realCamera);

        // 计算反射平面位置和法线
        Vector3 pos = Vector3.zero;
        Vector3 normal = Vector3.up;
        
        if (target != null)
        {
            pos = target.transform.position + Vector3.up * m_planeOffset;
            normal = target.transform.up;
            
            // 检查目标是否移动或旋转
            if (!Approximately(m_LastTargetPosition, target.transform.position) || 
                !Approximately(m_LastTargetRotation, target.transform.rotation))
            {
                m_NeedRecalculate = true;
                m_LastTargetPosition = target.transform.position;
                m_LastTargetRotation = target.transform.rotation;
            }
        }

        UpdateCamera(realCamera, m_ReflectionCamera);

        // 只在需要时重新计算反射矩阵
        if (m_NeedRecalculate)
        {
            float d = -Vector3.Dot(normal, pos) - m_settings.m_ClipPlaneOffset;
            Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

            m_CachedReflectionMatrix = Matrix4x4.Scale(new Vector3(1, -1, 1));
            CalculateReflectionMatrix(ref m_CachedReflectionMatrix, reflectionPlane);
            
            m_NeedRecalculate = false;
        }

        // 设置反射相机的变换
        Vector3 oldpos = realCamera.transform.position - new Vector3(0, pos.y * 2, 0);
        Vector3 newpos = ReflectPosition(oldpos);
        
        m_ReflectionCamera.transform.forward = Vector3.Scale(realCamera.transform.forward, new Vector3(1, -1, 1));
        m_ReflectionCamera.worldToCameraMatrix = realCamera.worldToCameraMatrix * m_CachedReflectionMatrix;

        // 设置斜投影矩阵
        Vector4 clipPlane = CameraSpacePlane(m_ReflectionCamera, pos - Vector3.up * 0.1f, normal, 1.0f);
        Matrix4x4 projection = realCamera.CalculateObliqueMatrix(clipPlane);
        m_ReflectionCamera.projectionMatrix = projection;
        m_ReflectionCamera.cullingMask = m_settings.m_ReflectLayers;
        m_ReflectionCamera.transform.position = newpos;
        
        m_LastCameraPosition = realCamera.transform.position;
    }

    #endregion

    #region Reflection Math

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

    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * m_settings.m_ClipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

    private bool Approximately(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.0001f;
    }

    private bool Approximately(Quaternion a, Quaternion b)
    {
        return Quaternion.Dot(a, b) > 0.9999f;
    }

    #endregion

    #region Texture Management

    private bool CheckSettingsChanged(Camera camera, float distanceFactor)
    {
        float scale = UniversalRenderPipeline.asset != null ? UniversalRenderPipeline.asset.renderScale : 1.0f;
        Vector2Int newSize = ReflectionResolution(camera, scale, distanceFactor);

        bool sizeChanged = !m_CurrentTextureSize.Equals(newSize);
        bool settingsChanged =
            m_CurrentResolutionMultiplier != m_settings.m_ResolutionMultiplier ||
            m_CurrentDownsample != m_settings._downsample ||
            m_CurrentBlurOn != m_settings._blurOn ||
            m_CurrentBlurIterations != m_settings._blurIterations ||
            m_CurrentBlurSize != m_settings._blurSize;

        if (sizeChanged || settingsChanged)
        {
            m_CurrentResolutionMultiplier = m_settings.m_ResolutionMultiplier;
            m_CurrentDownsample = m_settings._downsample;
            m_CurrentBlurOn = m_settings._blurOn;
            m_CurrentBlurIterations = m_settings._blurIterations;
            m_CurrentBlurSize = m_settings._blurSize;
            m_CurrentTextureSize = newSize;

            // 只在尺寸改变时释放纹理
            if (sizeChanged)
            {
                ReleaseRenderTexture(ref m_ReflectionTexture);
                ReleaseRenderTexture(ref m_BlurReflectionTexture);
            }

            return true;
        }

        return false;
    }

    private void EnsureReflectionTexture(Camera camera, float distanceFactor)
    {
        if (m_ReflectionTexture == null)
        {
            float scale = UniversalRenderPipeline.asset != null ? UniversalRenderPipeline.asset.renderScale : 1.0f;
            Vector2Int res = ReflectionResolution(camera, scale, distanceFactor);

            bool useHDR10 = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.RGB111110Float);
            RenderTextureFormat hdrFormat = useHDR10 ? RenderTextureFormat.RGB111110Float : RenderTextureFormat.DefaultHDR;
            
            m_ReflectionTexture = RenderTexture.GetTemporary(res.x, res.y, 16,
                GraphicsFormatUtility.GetGraphicsFormat(hdrFormat, true));
            
            m_ReflectionTexture.useMipMap = m_optimizationSettings.useMipmap;
            m_ReflectionTexture.autoGenerateMips = m_optimizationSettings.useMipmap;
            m_ReflectionTexture.name = "_PlanarReflectionTexture";
        }
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

    #endregion

    #region Optimization Helpers

    private bool ShouldUpdateThisFrame()
    {
        return m_FrameCount % m_optimizationSettings.updateInterval == 0;
    }

    private float CalculateDistanceFactor(Camera camera)
    {
        if (!m_optimizationSettings.dynamicResolution || target == null)
            return 1f;

        float distance = Vector3.Distance(camera.transform.position, target.transform.position);
        float maxDist = m_optimizationSettings.maxReflectionDistance;
        
        // 距离越远，分辨率越低
        float factor = Mathf.Clamp01(1f - (distance / maxDist));
        return Mathf.Max(0.25f, factor); // 最低保持25%分辨率
    }

    private bool IsWithinRenderDistance(Camera camera)
    {
        if (!m_optimizationSettings.enableDistanceCulling || target == null)
            return true;

        float distance = Vector3.Distance(camera.transform.position, target.transform.position);
        return distance <= m_optimizationSettings.maxReflectionDistance;
    }

    #endregion

    #region Main Rendering

    public void ExecuteBeforeCameraRender(ScriptableRenderContext context, Camera camera)
    {
        if (!enabled || camera.cameraType != CameraType.Game)
            return;

        // 帧率控制
        m_FrameCount++;
        if (!ShouldUpdateThisFrame())
        {
            // 即使不更新也要设置全局纹理，避免材质丢失引用
            if (m_settings._blurOn && m_BlurReflectionTexture != null)
                Shader.SetGlobalTexture(planarReflectionTextureID, m_BlurReflectionTexture);
            else if (m_ReflectionTexture != null)
                Shader.SetGlobalTexture(planarReflectionTextureID, m_ReflectionTexture);
            return;
        }

        // 距离剔除
        if (!IsWithinRenderDistance(camera))
            return;

        // 计算距离因子用于动态分辨率
        float distanceFactor = CalculateDistanceFactor(camera);

        // 检查设置是否更改
        CheckSettingsChanged(camera, distanceFactor);

        // 渲染设置
        GL.invertCulling = true;
        bool previousFog = RenderSettings.fog;
        RenderSettings.fog = false;
        
        int previousMaxLOD = QualitySettings.maximumLODLevel;
        float previousLODBias = QualitySettings.lodBias;
        QualitySettings.maximumLODLevel = 1;
        QualitySettings.lodBias = previousLODBias * 0.5f;

        try
        {
            UpdateReflectionCamera(camera);
            EnsureReflectionTexture(camera, distanceFactor);
            
            m_ReflectionCamera.targetTexture = m_ReflectionTexture;
            UniversalRenderPipeline.RenderSingleCamera(context, m_ReflectionCamera);

            // 模糊处理
            if (m_settings._blurOn)
            {
                EnsureBlurMaterial();
                
                if (_blurMaterial != null)
                {
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
            else
            {
                Shader.SetGlobalTexture(planarReflectionTextureID, m_ReflectionTexture);
            }
        }
        finally
        {
            // 恢复渲染设置
            GL.invertCulling = false;
            RenderSettings.fog = previousFog;
            QualitySettings.maximumLODLevel = previousMaxLOD;
            QualitySettings.lodBias = previousLODBias;
        }
    }

    #endregion

    #region Blur Effect

    private void ApplyBlurEffect(ScriptableRenderContext context, RenderTexture source, RenderTexture destination)
    {
        if (m_settings._blurIterations <= 0)
        {
            Graphics.Blit(source, destination);
            return;
        }

        var buf = CommandBufferPool.Get("Blur Reflection");
        try
        {
            float width = source.width;
            float height = source.height;
            
            var sourceDesc = source.descriptor;
            sourceDesc.msaaSamples = 1;
            sourceDesc.depthBufferBits = 0;
            sourceDesc.width = Mathf.Max(1, Mathf.RoundToInt(width / m_settings._downsample));
            sourceDesc.height = Mathf.Max(1, Mathf.RoundToInt(height / m_settings._downsample));

            buf.GetTemporaryRT(s_TempBlurRT1, sourceDesc, FilterMode.Bilinear);
            buf.GetTemporaryRT(s_TempBlurRT2, sourceDesc, FilterMode.Bilinear);

            // 第一次模糊
            buf.SetGlobalFloat(s_BlurOffsetID, 1.0f + m_settings._blurSize);
            buf.Blit(source, s_TempBlurRT1, _blurMaterial, 0);

            // 迭代模糊（乒乓缓冲）
            int currentRT = s_TempBlurRT1;
            int nextRT = s_TempBlurRT2;
            
            for (int i = 1; i < m_settings._blurIterations; i++)
            {
                buf.SetGlobalFloat(s_BlurOffsetID, i + m_settings._blurSize);
                buf.Blit(currentRT, nextRT, _blurMaterial, 0);

                // 交换RT
                int temp = currentRT;
                currentRT = nextRT;
                nextRT = temp;
            }

            // 最终输出
            buf.SetGlobalFloat(s_BlurOffsetID, m_settings._blurIterations + m_settings._blurSize);
            buf.Blit(currentRT, destination, _blurMaterial, 0);

            buf.ReleaseTemporaryRT(s_TempBlurRT1);
            buf.ReleaseTemporaryRT(s_TempBlurRT2);

            context.ExecuteCommandBuffer(buf);
        }
        finally
        {
            CommandBufferPool.Release(buf);
        }
    }

    #endregion

#if UNITY_EDITOR
    // 编辑器调试信息
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;

        Vector3 pos = target.transform.position + Vector3.up * m_planeOffset;
        Vector3 normal = target.transform.up;

        // 绘制反射平面
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(pos, new Vector3(5, 0.01f, 5));
        Gizmos.DrawRay(pos, normal * 2f);

        // 绘制渲染距离
        if (m_optimizationSettings.enableDistanceCulling)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.transform.position, m_optimizationSettings.maxReflectionDistance);
        }
    }
#endif
}
