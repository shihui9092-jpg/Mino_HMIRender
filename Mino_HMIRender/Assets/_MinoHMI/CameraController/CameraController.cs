using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class CameraPreset
{
	[Tooltip("机位唯一 ID，建议全局唯一。")]
	public int presetId;
	[Tooltip("机位名称，用于按名称查找和调用。")]
	public string presetName = "NewPreset";
	[Tooltip("该机位对应的快捷键，设为 None 表示不绑定。")]
	public KeyCode hotkey = KeyCode.None;
	[Tooltip("机位位置。")]
	public Vector3 pos;
	[Tooltip("机位欧拉角。")]
	public Vector3 rotateAngles;
	[Tooltip("机位高度偏移。")]
	public float height;
	[Tooltip("机位水平偏移。")]
	public float offset;
	[Tooltip("机位距离。")]
	public float distance;
}

[System.Serializable]
public class CameraKeyBindings
{
	[Header("参数微调按键")]
	public KeyCode increaseHeight = KeyCode.UpArrow;
	public KeyCode decreaseHeight = KeyCode.DownArrow;
	public KeyCode decreaseOffset = KeyCode.LeftArrow;
	public KeyCode increaseOffset = KeyCode.RightArrow;

	[Header("功能开关按键")]
	public KeyCode resetCharacterAndLighting = KeyCode.R;
	public KeyCode toggleDragObject = KeyCode.J;
	public KeyCode toggleRotateLight = KeyCode.K;
	public KeyCode toggleRimLight = KeyCode.B;
	public KeyCode toggleWetEffect = KeyCode.W;
}

public class CameraController : MonoBehaviour
{
	[Header("目标引用")]
	[Tooltip("相机环绕的目标焦点。")]
	[SerializeField] private Transform targetFocus;
	[Tooltip("可被拖拽旋转的展示对象。")]
	[SerializeField] private GameObject targetObj;
	[Tooltip("用于旋转控制的主光源。")]
	[SerializeField] private Transform mainLight;

	[Header("交互开关")]
	[Tooltip("启用后可通过鼠标拖拽物体或光源。")]
	[SerializeField] private bool enableDragObject = false;
	[Tooltip("拖拽模式下是否旋转光源，关闭时旋转物体。")]
	[SerializeField] private bool enableRotateLight = false;

	[Header("相机位姿参数")]
	[Tooltip("相机垂直偏移。")]
	[SerializeField] private float height = 0.0f;
	[Tooltip("相机水平偏移。")]
	[SerializeField] private float offset = 0.0f;
	[Tooltip("基础观察距离。")]
	[SerializeField] private float distance = 3.5f;
	[Tooltip("滚轮缩放速度。")]
	[Range(0.1f, 4f)]
	[SerializeField] private float zoomWheelSpeed = 4.0f;

	[Header("旋转与缩放限制")]
	[Tooltip("最小观察距离。")]
	[SerializeField] private float minDistance = 1f;
	[Tooltip("最大观察距离。")]
	[SerializeField] private float maxDistance = 4f;
	[Tooltip("水平旋转灵敏度。")]
	[SerializeField] private float xSpeed = 250.0f;
	[Tooltip("垂直旋转灵敏度。")]
	[SerializeField] private float ySpeed = 120.0f;
	[Tooltip("垂直旋转最小角度。")]
	[SerializeField] private float yMinLimit = -10;
	[Tooltip("垂直旋转最大角度。")]
	[SerializeField] private float yMaxLimit = 60;
	[Tooltip("拖拽物体时的旋转速度。")]
	[SerializeField] private float objRotateSpeed = 500.0f;

	[Header("按键配置")]
	[Tooltip("所有系统功能键位绑定。")]
	[SerializeField] private CameraKeyBindings keyBindings = new CameraKeyBindings();

	[Header("机位列表")]
	[Tooltip("可在 Inspector 中自由添加、删除和排序机位。")]
	[SerializeField] private List<CameraPreset> cameraPresets = new List<CameraPreset>();

	[Header("旧版本机位迁移（仅兼容）")]
	[SerializeField] private CameraPreset legacyCameraPreset1;
	[SerializeField] private CameraPreset legacyCameraPreset2;
	[SerializeField] private CameraPreset legacyCameraPreset3;
	[SerializeField] private CameraPreset legacyCameraPreset4;
	[SerializeField] private CameraPreset legacyCameraPreset5;
	[SerializeField] private CameraPreset legacyCameraPreset6;
	[SerializeField] private CameraPreset legacyCameraPreset7;

	[HideInInspector]
	[SerializeField] private bool disableSteering = false;
	[HideInInspector]
	[SerializeField] private bool isApplyingCameraPreset = false;

	public Transform TargetFocus => targetFocus;
	public GameObject TargetObj => targetObj;
	public Transform MainLight => mainLight;
	public bool EnableDragObject => enableDragObject;
	public bool EnableRotateLight => enableRotateLight;
	public float Height => height;
	public float Offset => offset;
	public float Distance => distance;
	public float ZoomWheelSpeed => zoomWheelSpeed;
	public float MinDistance => minDistance;
	public float MaxDistance => maxDistance;
	public float XSpeed => xSpeed;
	public float YSpeed => ySpeed;
	public float YMinLimit => yMinLimit;
	public float YMaxLimit => yMaxLimit;
	public float ObjRotateSpeed => objRotateSpeed;
	public bool DisableSteeringState => disableSteering;
	public bool IsApplyingCameraPreset => isApplyingCameraPreset;
	public CameraKeyBindings KeyBindings => keyBindings;
	public IReadOnlyList<CameraPreset> CameraPresets => cameraPresets;

	private float orbitX = 0.0f;
	private float orbitY = 0.0f;
	private float normalAngle = 0.0f;
	private float currentDistance = 0.0f;
	private float currentXSpeed = 0.0f;
	private float currentYSpeed = 0.0f;
	private float targetXSpeed = 0.0f;
	private float targetYSpeed = 0.0f;
	private float currentObjectRotateSpeed = 0.0f;
	private float targetObjectRotateSpeed = 0.0f;
	private bool isDraggingObject = false;
	private bool lastLeftMouseButtonState = false;
	private Collider[] surfaceColliders;
	private float boundsMaxSize = 20.0f;
	private bool isWet = false;

	private PointerEventData pointerEventData;
	private EventSystem cachedEventSystem;
	private readonly List<RaycastResult> eventSystemRaycastResults = new List<RaycastResult>(16);
	private int uiLayer = -1;
	private GameObject rimLightObject;
	private Light rimLightComponent;
	private Quaternion initialCharacterRotation;
	private Quaternion initialLightRotation;

	private void Start()
	{
		MigrateLegacyPresetsIfNeeded();

		Vector3 angles = transform.eulerAngles;
		orbitX = angles.y;
		orbitY = angles.x;
		uiLayer = LayerMask.NameToLayer("UI");

		if (targetObj != null && mainLight != null)
		{
			initialCharacterRotation = targetObj.transform.rotation;
			initialLightRotation = mainLight.rotation;
		}

		CacheRimLightComponent();
		Reset();
	}

	public void DisableSteering(bool state)
	{
		disableSteering = state;
	}

	public bool TryApplyCameraPresetById(int presetId)
	{
		CameraPreset preset = GetCameraPresetById(presetId);
		if (preset == null)
		{
			return false;
		}

		ApplyCameraPreset(preset);
		return true;
	}

	public bool TryApplyCameraPresetByName(string presetName)
	{
		CameraPreset preset = GetCameraPresetByName(presetName);
		if (preset == null)
		{
			return false;
		}

		ApplyCameraPreset(preset);
		return true;
	}

	public CameraPreset GetCameraPresetById(int presetId)
	{
		for (int index = 0; index < cameraPresets.Count; index++)
		{
			CameraPreset preset = cameraPresets[index];
			if (preset != null && preset.presetId == presetId)
			{
				return preset;
			}
		}

		return null;
	}

	public CameraPreset GetCameraPresetByName(string presetName)
	{
		if (string.IsNullOrEmpty(presetName))
		{
			return null;
		}

		for (int index = 0; index < cameraPresets.Count; index++)
		{
			CameraPreset preset = cameraPresets[index];
			if (preset != null && preset.presetName == presetName)
			{
				return preset;
			}
		}

		return null;
	}

	private void ResetCharacterAndLighting()
	{
		if (targetObj != null)
		{
			targetObj.transform.rotation = initialCharacterRotation;
		}

		if (mainLight != null)
		{
			mainLight.transform.rotation = initialLightRotation;
		}
	}

	public void Reset()
	{
		lastLeftMouseButtonState = Input.GetMouseButton(0);
		disableSteering = false;

		currentDistance = distance;
		currentXSpeed = 0.0f;
		currentYSpeed = 0.0f;
		targetXSpeed = 0.0f;
		targetYSpeed = 0.0f;
		currentObjectRotateSpeed = 0.0f;
		targetObjectRotateSpeed = 0.0f;
		surfaceColliders = null;

		if (targetObj)
		{
			Renderer[] renderers = targetObj.GetComponentsInChildren<Renderer>();
			Bounds bounds = new Bounds();
			bool hasValidBounds = false;
			foreach (Renderer rendererComponent in renderers)
			{
				if (!hasValidBounds)
				{
					hasValidBounds = true;
					bounds = rendererComponent.bounds;
				}
				else
				{
					bounds.Encapsulate(rendererComponent.bounds);
				}
			}

			Vector3 size = bounds.size;
			float maxSize = size.x > size.y ? size.x : size.y;
			maxSize = size.z > maxSize ? size.z : maxSize;
			boundsMaxSize = maxSize;
			currentDistance += boundsMaxSize * 1.2f;

			surfaceColliders = targetObj.GetComponentsInChildren<Collider>();
		}
	}

	private void MigrateLegacyPresetsIfNeeded()
	{
		if (cameraPresets != null && cameraPresets.Count > 0)
		{
			return;
		}

		if (cameraPresets == null)
		{
			cameraPresets = new List<CameraPreset>();
		}

		AddLegacyPresetToList(legacyCameraPreset1, 1, KeyCode.Alpha1);
		AddLegacyPresetToList(legacyCameraPreset2, 2, KeyCode.Alpha2);
		AddLegacyPresetToList(legacyCameraPreset3, 3, KeyCode.Alpha3);
		AddLegacyPresetToList(legacyCameraPreset4, 4, KeyCode.Alpha4);
		AddLegacyPresetToList(legacyCameraPreset5, 5, KeyCode.Alpha5);
		AddLegacyPresetToList(legacyCameraPreset6, 6, KeyCode.Alpha6);
		AddLegacyPresetToList(legacyCameraPreset7, 7, KeyCode.Alpha7);
	}

	private void AddLegacyPresetToList(CameraPreset legacyPreset, int fallbackId, KeyCode fallbackHotkey)
	{
		if (legacyPreset == null)
		{
			return;
		}

		if (legacyPreset.presetId == 0)
		{
			legacyPreset.presetId = fallbackId;
		}
		if (string.IsNullOrEmpty(legacyPreset.presetName))
		{
			legacyPreset.presetName = "Preset_" + fallbackId;
		}
		if (legacyPreset.hotkey == KeyCode.None)
		{
			legacyPreset.hotkey = fallbackHotkey;
		}

		cameraPresets.Add(legacyPreset);
	}

	private void ApplyCameraPreset(CameraPreset preset)
	{
		if (preset == null)
		{
			return;
		}

		Vector3 angles = preset.rotateAngles;
		isApplyingCameraPreset = true;

		DOTween.To(() => transform.position, value => transform.position = value, preset.pos, 0.5f);
		DOTween.To(() => orbitX, value => orbitX = value, angles.y, 0.5f);
		DOTween.To(() => orbitY, value => orbitY = value, angles.x, 0.5f);
		DOTween.To(() => height, value => height = value, preset.height, 0.5f);
		DOTween.To(() => offset, value => offset = value, preset.offset, 0.5f);
		DOTween.To(() => distance, value => distance = value, preset.distance, 0.5f)
			.OnComplete(() => { isApplyingCameraPreset = false; });
	}

	private void LateUpdate()
	{
		HandlePresetHotkeys();

		bool isOverUi = IsPointerOverUIElement();
		bool isMouseOverGameWindow = IsMouseOverGameWindow();

		if (isMouseOverGameWindow)
		{
			HandleKeyboardHotkeys();
		}

		Vector3 mousePosition = Input.mousePosition;
		if (IsMouseInBlockedArea(mousePosition))
		{
			return;
		}

		if (CanHandleCameraControl(isMouseOverGameWindow, isOverUi))
		{
			HandleMouseControl();
		}

		UpdateDistanceBySurfaceOcclusion();
		ApplyCameraTransform();
	}

	private void HandlePresetHotkeys()
	{
		for (int index = 0; index < cameraPresets.Count; index++)
		{
			CameraPreset preset = cameraPresets[index];
			if (preset == null || preset.hotkey == KeyCode.None)
			{
				continue;
			}

			if (Input.GetKeyDown(preset.hotkey))
			{
				ApplyCameraPreset(preset);
				return;
			}
		}
	}

	private bool IsMouseOverGameWindow()
	{
		Vector3 mousePosition = Input.mousePosition;
		return !(0 > mousePosition.x || 0 > mousePosition.y || Screen.width < mousePosition.x || Screen.height < mousePosition.y);
	}

	private static bool IsMouseInBlockedArea(Vector3 mousePosition)
	{
		return mousePosition.x < Screen.width / 3 && mousePosition.y > (Screen.height - Screen.height / 3);
	}

	private bool CanHandleCameraControl(bool isMouseOverGameWindow, bool isOverUi)
	{
		return targetObj && targetFocus && isMouseOverGameWindow && !isOverUi && !isApplyingCameraPreset;
	}

	private static bool IsKeyPressed(KeyCode keyCode)
	{
		return keyCode != KeyCode.None && Input.GetKey(keyCode);
	}

	private static bool IsKeyDown(KeyCode keyCode)
	{
		return keyCode != KeyCode.None && Input.GetKeyDown(keyCode);
	}

	private void HandleKeyboardHotkeys()
	{
		if (IsKeyPressed(keyBindings.increaseHeight))
		{
			height += 0.005f;
		}
		if (IsKeyPressed(keyBindings.decreaseHeight))
		{
			height -= 0.005f;
		}
		if (IsKeyPressed(keyBindings.decreaseOffset))
		{
			offset -= 0.005f;
		}
		if (IsKeyPressed(keyBindings.increaseOffset))
		{
			offset += 0.005f;
		}
		if (IsKeyDown(keyBindings.resetCharacterAndLighting))
		{
			ResetCharacterAndLighting();
		}
		if (IsKeyDown(keyBindings.toggleDragObject))
		{
			enableDragObject = !enableDragObject;
		}
		if (IsKeyDown(keyBindings.toggleRotateLight))
		{
			enableRotateLight = !enableRotateLight;
		}
		if (IsKeyDown(keyBindings.toggleRimLight))
		{
			CacheRimLightComponent();
			if (rimLightComponent != null)
			{
				rimLightComponent.enabled = !rimLightComponent.enabled;
			}
		}
		if (IsKeyDown(keyBindings.toggleWetEffect))
		{
			isWet = !isWet;
			Shader.SetGlobalFloat("RainGlobal", isWet ? 1.0f : 0.0f);
		}
	}

	private void HandleMouseControl()
	{
		UpdateDraggingState();
		UpdateRequestedSpeeds();
		ApplyObjectRotation();
		ApplyCameraOrbit();
	}

	private void UpdateDraggingState()
	{
		if (!lastLeftMouseButtonState && Input.GetMouseButton(0))
		{
			isDraggingObject = enableDragObject;
		}
		else if (lastLeftMouseButtonState && !Input.GetMouseButton(0))
		{
			isDraggingObject = false;
		}

		lastLeftMouseButtonState = Input.GetMouseButton(0);
	}

	private void UpdateRequestedSpeeds()
	{
		if (isDraggingObject)
		{
			if (Input.GetMouseButton(0) && !disableSteering)
			{
				targetObjectRotateSpeed += (Input.GetAxis("Mouse X") * objRotateSpeed * 0.02f - targetObjectRotateSpeed) * Time.deltaTime * 10;
			}
			else
			{
				targetObjectRotateSpeed += (0 - targetObjectRotateSpeed) * Time.deltaTime * 4;
			}

			targetXSpeed += (0 - targetXSpeed) * Time.deltaTime * 4;
			targetYSpeed += (0 - targetYSpeed) * Time.deltaTime * 4;
			return;
		}

		if (Input.GetMouseButton(0) && !disableSteering)
		{
			targetXSpeed += (Input.GetAxis("Mouse X") * xSpeed * 0.02f - targetXSpeed) * Time.deltaTime * 10;
			targetYSpeed += (Input.GetAxis("Mouse Y") * ySpeed * 0.02f - targetYSpeed) * Time.deltaTime * 10;
		}
		else
		{
			targetXSpeed += (0 - targetXSpeed) * Time.deltaTime * 4;
			targetYSpeed += (0 - targetYSpeed) * Time.deltaTime * 4;
		}

		targetObjectRotateSpeed += (0 - targetObjectRotateSpeed) * Time.deltaTime * 4;
		if (enableDragObject)
		{
			targetObjectRotateSpeed = 0.0f;
			currentObjectRotateSpeed = 0.0f;
		}
	}

	private void ApplyObjectRotation()
	{
		currentObjectRotateSpeed += (targetObjectRotateSpeed - currentObjectRotateSpeed) * Time.deltaTime * 20;
		if (!enableDragObject)
		{
			return;
		}

		if (enableRotateLight)
		{
			if (mainLight != null)
			{
				mainLight.transform.Rotate(Vector3.up, -currentObjectRotateSpeed, Space.World);
			}

			return;
		}

		targetObj.transform.Rotate(Vector3.up, -currentObjectRotateSpeed, Space.World);
	}

	private void ApplyCameraOrbit()
	{
		currentXSpeed += (targetXSpeed - currentXSpeed) * Time.deltaTime * 20;
		currentYSpeed += (targetYSpeed - currentYSpeed) * Time.deltaTime * 20;
		orbitX += currentXSpeed;
		orbitY -= currentYSpeed;
		orbitY = ClampAngle(orbitY, yMinLimit + normalAngle, yMaxLimit + normalAngle);

		distance -= Input.GetAxis("Mouse ScrollWheel") * zoomWheelSpeed;
		distance = Mathf.Clamp(distance, minDistance, maxDistance);
	}

	private void UpdateDistanceBySurfaceOcclusion()
	{
		if (surfaceColliders != null && surfaceColliders.Length > 0)
		{
			RaycastHit hitInfo = new RaycastHit();
			Vector3 direction = Vector3.Normalize(targetFocus.position - transform.position);
			float requiredDistance = 0.01f;
			bool surfaceFound = false;

			foreach (Collider surfaceCollider in surfaceColliders)
			{
				Ray ray = new Ray(transform.position - direction * boundsMaxSize, direction);
				if (surfaceCollider.Raycast(ray, out hitInfo, Mathf.Infinity))
				{
					requiredDistance = Mathf.Max(Vector3.Distance(hitInfo.point, targetFocus.position) + distance, requiredDistance);
					surfaceFound = true;
				}
			}

			if (surfaceFound)
			{
				currentDistance += (requiredDistance - currentDistance) * Time.deltaTime * 4;
			}

			return;
		}

		currentDistance = distance;
	}

	private void ApplyCameraTransform()
	{
		Quaternion rotation = Quaternion.Euler(orbitY, orbitX, 0);
		Vector3 position = rotation * new Vector3(offset, height, -currentDistance) + targetFocus.position;
		transform.rotation = rotation;
		transform.position = position;
	}

	private static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360)
		{
			angle += 360;
		}
		if (angle > 360)
		{
			angle -= 360;
		}
		return Mathf.Clamp(angle, min, max);
	}

	public void SetNormalAngle(float angle)
	{
		normalAngle = angle;
	}

	public void set_normal_angle(float angle)
	{
		SetNormalAngle(angle);
	}

	// 返回当前鼠标是否悬停在 UI 上。
	public bool IsPointerOverUIElement()
	{
		return IsPointerOverUIElement(GetEventSystemRaycastResults());
	}

	// 根据射线检测结果判断是否命中 UI 图层。
	private bool IsPointerOverUIElement(List<RaycastResult> raycastResults)
	{
		if (raycastResults == null)
		{
			return false;
		}

		for (int index = 0; index < raycastResults.Count; index++)
		{
			if (raycastResults[index].gameObject.layer == uiLayer)
			{
				return true;
			}
		}
		return false;
	}

	// 获取当前鼠标位置的 UI 射线结果（复用对象，避免每帧 GC）。
	private List<RaycastResult> GetEventSystemRaycastResults()
	{
		EventSystem currentEventSystem = EventSystem.current;
		if (currentEventSystem == null)
		{
			return null;
		}

		if (pointerEventData == null || cachedEventSystem != currentEventSystem)
		{
			pointerEventData = new PointerEventData(currentEventSystem);
			cachedEventSystem = currentEventSystem;
		}

		pointerEventData.Reset();
		pointerEventData.position = Input.mousePosition;

		eventSystemRaycastResults.Clear();
		currentEventSystem.RaycastAll(pointerEventData, eventSystemRaycastResults);
		return eventSystemRaycastResults;
	}

	private void CacheRimLightComponent()
	{
		if (rimLightComponent != null)
		{
			return;
		}

		if (rimLightObject == null)
		{
			rimLightObject = GameObject.Find("RimLight");
		}

		if (rimLightObject != null)
		{
			rimLightComponent = rimLightObject.GetComponent<Light>();
		}
	}
}