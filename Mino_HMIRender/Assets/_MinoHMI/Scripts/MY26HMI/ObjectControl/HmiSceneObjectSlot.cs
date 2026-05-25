using System;
using UnityEngine;

namespace MinoHMI.MY26HMI
{
    /// <summary>
    /// 场景与对象凹槽绑定项：目标场景名与 Object 引用一一对应。
    /// </summary>
    [Serializable]
    public class HmiSceneObjectSlot
    {
        [Tooltip("Build Settings 中已加入的目标场景名称，同时作为列表项标识")]
        public string targetSceneName;

        [Tooltip("与该场景绑定的对象引用（Prefab、场景物体、材质等均可）")]
        public UnityEngine.Object storedObject;

        public bool HasTargetScene => !string.IsNullOrWhiteSpace(targetSceneName);

        public bool HasObject => storedObject != null;

        public GameObject StoredGameObject
        {
            get
            {
                TryGetBoundGameObject(out GameObject gameObject);
                return gameObject;
            }
        }

        public bool TryGetBoundGameObject(out GameObject gameObject)
        {
            gameObject = null;
            if (storedObject == null)
            {
                return false;
            }

            if (storedObject is GameObject gameObjectReference)
            {
                gameObject = gameObjectReference;
                return true;
            }

            if (storedObject is Component component)
            {
                gameObject = component.gameObject;
                return true;
            }

            return false;
        }

        public bool TrySetGameObjectActive(bool active)
        {
            if (!TryGetBoundGameObject(out GameObject gameObject))
            {
                return false;
            }

            gameObject.SetActive(active);
            return true;
        }

        public T GetStoredObject<T>() where T : UnityEngine.Object
        {
            return storedObject as T;
        }
    }
}
