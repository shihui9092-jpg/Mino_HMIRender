using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MinoHMI.Rendering
{
    /// <summary>
    /// 平面反射相机组件
    /// 创建镜像相机用于渲染反射内容
    /// </summary>
    [ExecuteAlways]
    public class PlanarReflectionCamera : MonoBehaviour
    {
        [Header("反射设置")]
        [Tooltip("反射纹理分辨率")]
        public Vector2Int reflectionResolution = new Vector2Int(512, 512);
        
        [Tooltip("反射纹理质量")]
        [Range(0.25f, 1)]
        public float resolutionScale = 1.0f;
        
        [Tooltip("是否使用HDR")]
        public bool useHDR = false;
        
        [Tooltip("反射层级遮罩")]
        public LayerMask reflectionLayers = -1;
        
        [Tooltip("裁剪平面偏移")]
        public float clipPlaneOffset = 0.07f;

        [Header("性能优化")]
        [Tooltip("最大反射距离")]
        public float maxReflectionDistance = 100f;

        [Tooltip("反射更新频率(每N帧渲染一次,1=每帧)")]
        [Range(1, 10)]
        public int reflectionUpdateRate = 1;
        
        [Tooltip("是否启用反射")]
        public bool enableReflection = true;

        // 私有成员
        private Camera reflectionCamera;
        private RenderTexture reflectionTexture;
        private Camera mainCamera;
        private int reflectionRenderFrameCounter;

        // Shader 属性 ID
        private static readonly int ReflectionTexPropertyID = Shader.PropertyToID("_PlanarReflectionTexture");
        private static readonly int ReflectionVPPropertyID = Shader.PropertyToID("_PlanarReflectionVP");

        private const int MinReflectionTextureSize = 4;
        private const int DefaultReflectionTextureWidth = 512;
        private const int DefaultReflectionTextureHeight = 512;

        private void OnEnable()
        {
            reflectionRenderFrameCounter = 0;
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            CleanupResources();
        }

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            if (!enableReflection || camera.cameraType != CameraType.Game)
                return;

            // 仅 Base Camera 渲染反射，避免 Camera Stack 中 Overlay 重复渲染
            if (!IsBaseRenderCamera(camera))
                return;

            if (!ShouldRenderReflectionThisFrame())
                return;

            mainCamera = camera;
            
            // 初始化反射相机
            if (reflectionCamera == null)
            {
                CreateReflectionCamera();
            }

            SyncReflectionCameraSettings();

            if (!TryGetReflectionTextureSize(out int textureWidth, out int textureHeight))
                return;

            // 更新反射纹理
            UpdateReflectionTexture(textureWidth, textureHeight);
            
            // 渲染反射
            RenderReflection(context);
        }

        /// <summary>
        /// 是否为 URP Base 相机（非 Overlay）
        /// </summary>
        private static bool IsBaseRenderCamera(Camera camera)
        {
            if (camera == null)
                return false;

            UniversalAdditionalCameraData additionalData = camera.GetUniversalAdditionalCameraData();
            if (additionalData == null)
                return true;

            return additionalData.renderType == CameraRenderType.Base;
        }

        /// <summary>
        /// 按 reflectionUpdateRate 节流反射渲染
        /// </summary>
        private bool ShouldRenderReflectionThisFrame()
        {
            int updateRate = Mathf.Clamp(reflectionUpdateRate, 1, 10);
            reflectionRenderFrameCounter++;

            if (reflectionRenderFrameCounter < updateRate)
                return false;

            reflectionRenderFrameCounter = 0;
            return true;
        }

        /// <summary>
        /// 同步反射相机运行时参数
        /// </summary>
        private void SyncReflectionCameraSettings()
        {
            if (reflectionCamera == null)
                return;

            reflectionCamera.cullingMask = reflectionLayers;
        }

        /// <summary>
        /// 创建反射相机
        /// </summary>
        private void CreateReflectionCamera()
        {
            GameObject reflectionCameraGO = new GameObject("Planar Reflection Camera");
            reflectionCameraGO.hideFlags = HideFlags.HideAndDontSave;
            reflectionCamera = reflectionCameraGO.AddComponent<Camera>();
            
            // 配置反射相机
            reflectionCamera.enabled = false;
            reflectionCamera.clearFlags = CameraClearFlags.Color;
            // 使用透明背景，确保未命中反射物体的像素 alpha 为 0
            reflectionCamera.backgroundColor = Color.clear;
            reflectionCamera.cullingMask = reflectionLayers;
            // 与主相机保持一致，避免反射 RT 与车漆 HDR/颜色空间不一致
            reflectionCamera.allowHDR = mainCamera.allowHDR;
            reflectionCamera.allowMSAA = false;
            
            // 添加URP相机数据
            UniversalAdditionalCameraData cameraData = reflectionCameraGO.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderShadows = true;
            cameraData.requiresColorOption = CameraOverrideOption.Off;
            cameraData.requiresDepthOption = CameraOverrideOption.Off;
        }

        /// <summary>
        /// 计算有效的反射纹理尺寸，避免创建 width/height 为 0 的 RT
        /// </summary>
        private bool TryGetReflectionTextureSize(out int width, out int height)
        {
            int baseWidth = reflectionResolution.x > 0
                ? reflectionResolution.x
                : DefaultReflectionTextureWidth;
            int baseHeight = reflectionResolution.y > 0
                ? reflectionResolution.y
                : DefaultReflectionTextureHeight;

            float clampedScale = Mathf.Clamp(resolutionScale, 0.25f, 1f);
            width = Mathf.Max(MinReflectionTextureSize, Mathf.RoundToInt(baseWidth * clampedScale));
            height = Mathf.Max(MinReflectionTextureSize, Mathf.RoundToInt(baseHeight * clampedScale));
            return width > 0 && height > 0;
        }

        /// <summary>
        /// 更新反射纹理
        /// </summary>
        private void UpdateReflectionTexture(int width, int height)
        {
            if (reflectionCamera == null || width <= 0 || height <= 0)
                return;

            if (reflectionTexture == null || 
                reflectionTexture.width != width || 
                reflectionTexture.height != height)
            {
                if (reflectionTexture != null)
                {
                    reflectionTexture.Release();
                }

                RenderTextureDescriptor descriptor = new RenderTextureDescriptor(
                    width, height,
                    useHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32,
                    16
                );
                descriptor.autoGenerateMips = false;
                descriptor.useMipMap = false;

                reflectionTexture = new RenderTexture(descriptor);
                reflectionTexture.name = "PlanarReflectionTexture";
                reflectionTexture.filterMode = FilterMode.Bilinear;
                reflectionTexture.wrapMode = TextureWrapMode.Clamp;

                reflectionCamera.targetTexture = reflectionTexture;
            }
        }

        /// <summary>
        /// 渲染反射
        /// </summary>
        private void RenderReflection(ScriptableRenderContext context)
        {
            if (mainCamera == null || reflectionCamera == null || reflectionTexture == null)
                return;

            Vector3 planePos = transform.position;
            Vector3 planeNormal = transform.up.normalized;

            Matrix4x4 reflectionMatrix = CalculateReflectionMatrix(planePos, planeNormal);

            // 同步镜像相机 Transform，确保车漆等依赖 _WorldSpaceCameraPos 的 Shader 在反射里颜色正确
            Vector3 reflectedPosition = reflectionMatrix.MultiplyPoint(mainCamera.transform.position);
            Vector3 reflectedForward = reflectionMatrix.MultiplyVector(mainCamera.transform.forward).normalized;
            Vector3 reflectedUp = reflectionMatrix.MultiplyVector(mainCamera.transform.up).normalized;
            reflectionCamera.transform.SetPositionAndRotation(
                reflectedPosition,
                Quaternion.LookRotation(reflectedForward, reflectedUp));

            // 平面反射观察矩阵
            reflectionCamera.worldToCameraMatrix = mainCamera.worldToCameraMatrix * reflectionMatrix;

            reflectionCamera.fieldOfView = mainCamera.fieldOfView;
            reflectionCamera.nearClipPlane = mainCamera.nearClipPlane;
            reflectionCamera.farClipPlane = Mathf.Min(mainCamera.farClipPlane, maxReflectionDistance);
            reflectionCamera.aspect = mainCamera.aspect;
            reflectionCamera.allowHDR = mainCamera.allowHDR;

            Vector4 clipPlane = CameraSpacePlane(reflectionCamera, planePos, planeNormal, 1.0f);
            reflectionCamera.projectionMatrix = mainCamera.CalculateObliqueMatrix(clipPlane);

            Matrix4x4 reflectionVP = reflectionCamera.projectionMatrix * reflectionCamera.worldToCameraMatrix;

            Shader.SetGlobalTexture(ReflectionTexPropertyID, reflectionTexture);
            Shader.SetGlobalMatrix(ReflectionVPPropertyID, reflectionVP);

            // 反射会翻转绕序，需要反转剔除
            bool previousInvertCulling = GL.invertCulling;
            try
            {
                GL.invertCulling = true;
                UniversalRenderPipeline.RenderSingleCamera(context, reflectionCamera);
            }
            finally
            {
                GL.invertCulling = previousInvertCulling;
            }
        }

        /// <summary>
        /// 计算反射矩阵
        /// </summary>
        private Matrix4x4 CalculateReflectionMatrix(Vector3 planePos, Vector3 planeNormal)
        {
            float d = -Vector3.Dot(planeNormal, planePos);
            Vector4 reflectionPlane = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, d);

            Matrix4x4 reflection = Matrix4x4.identity;
            
            reflection.m00 = 1 - 2 * reflectionPlane.x * reflectionPlane.x;
            reflection.m01 = -2 * reflectionPlane.x * reflectionPlane.y;
            reflection.m02 = -2 * reflectionPlane.x * reflectionPlane.z;
            reflection.m03 = -2 * reflectionPlane.x * reflectionPlane.w;
            
            reflection.m10 = -2 * reflectionPlane.y * reflectionPlane.x;
            reflection.m11 = 1 - 2 * reflectionPlane.y * reflectionPlane.y;
            reflection.m12 = -2 * reflectionPlane.y * reflectionPlane.z;
            reflection.m13 = -2 * reflectionPlane.y * reflectionPlane.w;
            
            reflection.m20 = -2 * reflectionPlane.z * reflectionPlane.x;
            reflection.m21 = -2 * reflectionPlane.z * reflectionPlane.y;
            reflection.m22 = 1 - 2 * reflectionPlane.z * reflectionPlane.z;
            reflection.m23 = -2 * reflectionPlane.z * reflectionPlane.w;

            return reflection;
        }

        /// <summary>
        /// 计算相机空间裁剪平面
        /// </summary>
        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * clipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        private void CleanupResources()
        {
            if (reflectionCamera != null)
            {
                if (Application.isPlaying)
                    Destroy(reflectionCamera.gameObject);
                else
                    DestroyImmediate(reflectionCamera.gameObject);
                
                reflectionCamera = null;
            }

            if (reflectionTexture != null)
            {
                reflectionTexture.Release();
                reflectionTexture = null;
            }
        }

        private void OnDestroy()
        {
            CleanupResources();
        }

        private void OnValidate()
        {
            if (reflectionResolution.x <= 0)
            {
                reflectionResolution.x = DefaultReflectionTextureWidth;
            }

            if (reflectionResolution.y <= 0)
            {
                reflectionResolution.y = DefaultReflectionTextureHeight;
            }

            resolutionScale = Mathf.Clamp(resolutionScale, 0.25f, 1f);
            reflectionUpdateRate = Mathf.Clamp(reflectionUpdateRate, 1, 10);
        }
    }
}
