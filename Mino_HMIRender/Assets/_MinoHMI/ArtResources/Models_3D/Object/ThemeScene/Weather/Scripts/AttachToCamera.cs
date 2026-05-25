using UnityEngine;

public class AttachToCamera : MonoBehaviour
{
    [Tooltip("是否将物体设置为摄像机的子物体（挂点）")]
    public bool attachToCamera = true;
    [Tooltip("如果不需要保持原有世界位置和旋转，请勾选（重置相对偏移为零）")]
    public bool resetLocalTransform = true;

    private void Start()
    {
        if (Camera.main == null)
        {
            Debug.LogError("未找到主摄像机！");
            return;
        }
        if (attachToCamera)
        {
            transform.SetParent(Camera.main.transform);
            if (resetLocalTransform)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
    }
}