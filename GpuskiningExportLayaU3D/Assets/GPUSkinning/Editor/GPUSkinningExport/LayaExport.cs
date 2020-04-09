using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class LayaExport 
{
    public static void ExportPrefab(string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        ExportPrefab(prefab);
    }


    public static void ExportPrefab(GameObject prefab)
    {
        if(prefab == null)
        {
            Debug.Log("预设为空");
            return;
        }
        GameObject go = GameObject.Instantiate<GameObject>(prefab);
        go.name = prefab.name;
        LayaAir3D.ExportResources();
        GameObject.DestroyImmediate(go);
    }
}
