using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif 
using System;

public class GPUSkinningExport
{

#if UNITY_EDITOR
    public static void StartExport(string prefabPath, Action<bool> onComplete = null)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if(prefab != null)
        {
            GameObject[] prefabList = new GameObject[] { prefab };
            StartExportList(prefabList, onComplete);
        }
        else
        {
            if (onComplete != null)
            {
                onComplete(false);
            }
        }

    }

    
    public static void StartExportList(GameObject[] prefabList, Action<bool> onComplete = null)
    {
        if(prefabList.Length == 0)
        {
            Debug.Log("预设列表是空的 prefabList.Length=" + prefabList.Length);
            if (onComplete != null)
            {
                onComplete(false);
            }
            return;
        }
        GameObject runGO = GameObject.Find("GPUSkinningExport");
        if(runGO != null)
        {
            if(EditorApplication.isPlaying)
            {
                Debug.Log("有正在运行的列表");
                return;
            }
            else
            {
                GameObject.DestroyImmediate(runGO);
            }
        }


        runGO = new GameObject("GPUSkinningExport");
        GPUSkinningExportList manager = runGO.AddComponent<GPUSkinningExportList>();

        Action<GameObject> onEnd = (GameObject go) =>
        {
            GameObject.DestroyImmediate(runGO);
            EditorApplication.isPlaying = false;
            Debug.Log("导出完成");
            if (onComplete != null)
            {
                onComplete(true);
            }

        };



        EditorApplication.isPlaying = true;
        manager.StartExport(prefabList, onEnd);
    }

#endif 
}
