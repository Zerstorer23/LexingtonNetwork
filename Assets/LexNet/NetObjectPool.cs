using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetObjectPool : MonoBehaviour
{
    private static NetObjectPool prInstance;

    public static NetObjectPool instance
    {
        get
        {
            if (!prInstance)
            {
                prInstance = FindObjectOfType<NetObjectPool>();
                if (!prInstance)
                {
                     Debug.LogWarning("There needs to be one active NetObjectPool script on a GameObject in your scene.");
                }
                else
                {
                    prInstance.Init();
                }
            }

            return prInstance;
        }
    }
    private Dictionary<string, Queue<GameObject>> objectLibrary;
    void Init()
    {
        if (objectLibrary == null)
        {
            objectLibrary = new Dictionary<string, Queue<GameObject>>();
        }
    }

    public static void SaveObject(string tag, GameObject go) {
        if (!instance.objectLibrary.ContainsKey(tag)) {
            instance.objectLibrary.Add(tag, new Queue<GameObject>());
        }
        go.SetActive(false);
        go.hideFlags = HideFlags.HideInHierarchy;
        instance.objectLibrary[tag].Enqueue(go);
    }
    public static GameObject PollObject(string prefabName, Vector3 position, Quaternion quaternion) {
        if (!instance.objectLibrary.ContainsKey(prefabName) ||
                instance.objectLibrary[prefabName].Count <= 0)
        {
            GameObject go = GameObject.Instantiate((GameObject)Resources.Load(prefabName), position, quaternion);
            go.SetActive(true);
            return go;
        }
        else {
            GameObject go = instance.objectLibrary[prefabName].Dequeue();
            go.transform.position = position;
            go.transform.rotation = quaternion;
            go.SetActive(true);
            go.hideFlags = HideFlags.None;
            return go;
        }
    }

}
